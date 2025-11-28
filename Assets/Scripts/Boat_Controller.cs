using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boat_Controller : MonoBehaviour
{
    public Rigidbody rb;
    public TMP_Text timeSurvived;
    private float survivalTime;

    public AudioSource boatBreakSFX;

    public ParticleSystem movingWaterSplashFX;

    private bool isMovingWaterSplashPlaying = false; // Tracks the state of the moving splash effect
    private const float RISE_THRESHOLD = 10f; // The Y-coordinate threshold for the effect
                                              // ...

    public GameObject waterSplashFX;
    public AudioSource splashSFX;
    private float peakY;  // Tracks the highest Y reached
    public float splashThreshold = 10f; // Minimum fall distance to trigger splash

    [Header("Wave Pitch Configuration")]
    [Tooltip("The factor to scale the calculated pitch angle by. Higher value means more aggressive tilting.")]
    public float pitchFactor = 3f;
    [Tooltip("Speed multiplier for smoothing the rotation.")]
    public float rotationSmoothness = 1f;
    [Tooltip("Offset angle to apply when the boat is on the wave's centerline (e.g., to level the boat).")]
    public float angleOffset = 0f;

    [Header("Rotation Limits")]
    [Tooltip("The maximum absolute Z-rotation angle (e.g., 90 for a full tilt).")]
    public float maxRotationAngle = 80f;

    // The angle at which the game ends (e.g., 70 degrees).
    [Tooltip("The absolute Z-rotation angle at which the game-over condition is met.")]
    public float tiltLimitAngle = 70f;

    [Header("Player Input")]
    [Tooltip("The rate at which the player can manually tilt the boat (degrees per second).")]
    public float playerPitchRate = 10f;
    [Tooltip("The rate at which manual pitch returns to neutral (0 degrees) when no keys are pressed.")]
    public float manualPitchDecayRate = 10f;

    [Tooltip("Controls how much the wave's effect is reduced when player input is active (0 = fully ignore wave, 1 = no change).")]
    public float waveDampeningOnInput = 0.2f;

    // Internal state to track the manual rotation applied by the player
    private float targetManualPitch = 0f;
    private bool isGameOver = false; // Flag to prevent repeated game over calls

    // NEW: Variable to store the time when the game started
    private float startTime;

    public GameObject gameOverScreen;
    public Pitch_Meter_UI pitchMeterUI;

    private void Start()
    {
        // Record the time when the game/scene starts
        startTime = Time.time;
        // Ensure the game time scale is 1 when the game starts
        Time.timeScale = 1f;
        peakY = transform.position.y; // Initialize peak height

    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape) == true || Input.GetKeyDown(KeyCode.X) == true)
        {
            SceneManager.LoadScene(0);
        }

        // Rigidbody motion (including rotation) should happen in FixedUpdate.
        if (rb == null || Waves_Controller.instance == null) return;

        HandleMovingSplashFX();

        // --- 1. HANDLE PLAYER INPUT (Manual Pitch) ---
        float inputDirection = 0f;
        bool isInputActive = false;

        if (!isGameOver)
        {
            autoSplash();
            if (Input.GetKey(KeyCode.W) || Input.GetMouseButton(0))
            {
                inputDirection = 1f;
                isInputActive = true;
            }
        }

        if (isInputActive)
        {
            targetManualPitch += inputDirection * playerPitchRate * Time.fixedDeltaTime;
        }
        else
        {
            targetManualPitch = Mathf.MoveTowards(
                targetManualPitch,
                0f,
                manualPitchDecayRate * Time.fixedDeltaTime
            );
        }

        // Clamp the manual pitch
        targetManualPitch = Mathf.Clamp(
            targetManualPitch,
            -maxRotationAngle,
            maxRotationAngle
        );


        // --- 2. Calculate Wave Pitch ---
        float effectivePitchFactor = pitchFactor;
        if (isInputActive)
        {
            effectivePitchFactor *= waveDampeningOnInput;
        }

        float waveHeight = Waves_Controller.instance.GetWaveHeight(transform.position.x);

        float wavePitch = (-waveHeight * effectivePitchFactor) + angleOffset;

        // --- 3. Combine Wave Pitch and Manual Pitch ---
        float combinedPitch = wavePitch + targetManualPitch;

        // --- 4. Clamp the FINAL combined Angle ---
        float finalTargetPitchAngle = Mathf.Clamp(
            combinedPitch,
            -maxRotationAngle,
            maxRotationAngle
        );

        // 5. Apply the rotation
        Quaternion targetRotation = Quaternion.Euler(
            rb.rotation.eulerAngles.x,
            rb.rotation.eulerAngles.y,
            finalTargetPitchAngle
        );

        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSmoothness));

        // --- 6. GAME OVER CHECK ---
        // Get the current Z rotation angle of the Rigidbody
        float currentZRotation = rb.rotation.eulerAngles.z;

        // Normalize the angle to the range [-180, 180] for easier comparison
        // Unity's eulerAngles returns [0, 360], so we convert 270 degrees to -90 degrees, for example.
        currentZRotation = Mathf.Abs(currentZRotation) > 180 ? (currentZRotation - 360) : currentZRotation;

        // Check if the boat's rotation exceeds the limit
        if (Mathf.Abs(currentZRotation) >= tiltLimitAngle)
        {
            GameOver();
        }
    }

    private void autoSplash()
    {
        float currentY = transform.position.y;

        // Update peakY if the boat is going up
        if (currentY > peakY)
            peakY = currentY;

        // Check for big fall
        if (currentY <= 0f && peakY - currentY >= splashThreshold)
        {
            if (splashSFX != null)
            {
                splashSFX.Play();
                Instantiate(waterSplashFX, transform.position, Quaternion.identity);

                // Reset peakY so we only trigger once per fall
                peakY = currentY;
            }
        }
    }

    private void GameOver()
    {
        if (isGameOver) return; // Prevent multiple calls

        isGameOver = true;

        boatBreakSFX.Play();

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.enabled = false;

        pitchMeterUI.pitchSlider.gameObject.SetActive(false);

        // NEW: Calculate survival time and format for display
        survivalTime = Time.time - startTime;
        string timeString = survivalTime.ToString("F0");

        // Update the time survived text
        if (timeSurvived != null)
        {
            timeSurvived.text = $"You survived for {timeString} seconds.";
        }
        else
        {
            Debug.LogWarning("Time Survived Text (TMP_Text) component is not assigned in the Inspector. Time not displayed.");
        }

        // Show the game over screen
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true); // Show the game over screen UI
        }
        else
        {
            Debug.LogError("GameOverScreen is not assigned in the Inspector!");
        }
    }

    private void HandleMovingSplashFX()
    {
        float currentY = transform.position.y;
        float verticalVelocity = rb.linearVelocity.y; // Get the boat's vertical velocity

        // --- CONDITION TO START PLAYING (RISING FROM BELOW 10 TO ABOVE 10) ---
        // 1. Is the effect currently NOT playing?
        // 2. Is the boat at or above the RISE_THRESHOLD (Y=10)?
        // 3. Is the boat moving upwards (verticalVelocity > 0)?
        if (!isMovingWaterSplashPlaying && currentY >= RISE_THRESHOLD && verticalVelocity > 0)
        {
            movingWaterSplashFX.Play();
            isMovingWaterSplashPlaying = true;
        }
        // --- CONDITION TO STOP PLAYING (BOAT STOPS RISING) ---
        // 1. Is the effect currently playing?
        // 2. Is the boat at or above the RISE_THRESHOLD (Y=10)? (Prevent stopping if the boat drops below 10)
        // 3. Is the boat no longer moving upwards (verticalVelocity <= 0)?
        else if (isMovingWaterSplashPlaying && currentY >= RISE_THRESHOLD && verticalVelocity <= 0)
        {
            movingWaterSplashFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            isMovingWaterSplashPlaying = false;
        }
        // --- CONDITION TO STOP PLAYING (BOAT DROPS SIGNIFICANTLY BELOW 10) ---
        // This is a safety check to ensure it stops if the boat drops back down (e.g., to Y=5)
        else if (isMovingWaterSplashPlaying && currentY < RISE_THRESHOLD)
        {
            movingWaterSplashFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            isMovingWaterSplashPlaying = false;
        }
    }
    public void restartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}