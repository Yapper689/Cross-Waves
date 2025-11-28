using UnityEngine;
using UnityEngine.UI;

// This script should be attached to a UI object (like the Canvas or the Slider itself)
public class Pitch_Meter_UI : MonoBehaviour
{
    [Tooltip("The Rigidbody component of the boat, used to read the current Z-rotation.")]
    public Rigidbody boatRigidbody;

    [Tooltip("The UI Slider component that visually represents the boat's pitch.")]
    public Slider pitchSlider;

    [Tooltip("Reference to the Boat_Controller script to get the safe tilt limit.")]
    public Boat_Controller boatController;

    // Internal variable to store the game over angle for mapping
    private float tiltLimit;

    void Start()
    {
        // Safety checks
        if (boatRigidbody == null || pitchSlider == null || boatController == null)
        {
            Debug.LogError("Pitch_Meter_UI requires references to the Boat Rigidbody, Slider, and Boat_Controller script in the Inspector.");
            enabled = false;
            return;
        }

        // Get the tilt limit from the controller for dynamic UI scaling
        tiltLimit = boatController.tiltLimitAngle;

        // Configure the slider to match our range
        // The bar will represent the range from -tiltLimit to +tiltLimit
        pitchSlider.minValue = 0f;
        pitchSlider.maxValue = 1f;

        // Set the safe zone (Green) and danger zones (Red) on the Slider background.
        // This visual setup is done in the Unity editor by coloring the fill area,
        // but the script provides the mapping logic.
    }

    void Update()
    {
        if (boatRigidbody == null || pitchSlider == null || boatController == null) return;

        // 1. Get the current Z rotation angle from the Rigidbody
        float currentRotation = boatRigidbody.rotation.eulerAngles.z;

        // 2. Normalize the angle to the standard [-180, 180] range
        // This is necessary because Unity's eulerAngles returns [0, 360].
        if (currentRotation > 180f)
        {
            currentRotation -= 360f;
        }

        // 3. Clamp the rotation to the safety limits for mapping
        // We only care about the rotation up to the tilt limit (e.g., -70 to +70)
        float clampedRotation = Mathf.Clamp(currentRotation, -tiltLimit, tiltLimit);

        // 4. Map the clamped rotation from [-tiltLimit, tiltLimit] to [0, 1]
        // This is the value that drives the slider's fill.
        // Formula: (Current_Value - Min_Range) / (Max_Range - Min_Range)
        float normalizedValue = (clampedRotation + tiltLimit) / (2f * tiltLimit);

        // 5. Update the Slider UI
        pitchSlider.value = normalizedValue;
    }
}