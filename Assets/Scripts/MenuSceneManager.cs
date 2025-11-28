using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ScreenManager : MonoBehaviour
{
    public GameObject boat;
    public float amplitude = 0.1f;
    public float frequency = 1f;
    private float startY;

    void Start()
    {
        // Set the game to target 60 FPS
        Application.targetFrameRate = 60;
        startY = boat.transform.position.y;
    }
    void Update()
    {
        BoatBuoy();
        if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.xKey.wasPressedThisFrame)
        { Application.Quit(); }
    }
    private void BoatBuoy()
    {
        // Re-check for safety in case the object was unassigned later
        if (boat == null) return;

        // 1. Calculate the Y offset using a sine wave
        // Mathf.Sin(Time.time * frequency) returns a value between -1 and 1.
        float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;

        // 2. Determine the new position for the 'boat' GameObject
        Vector3 newPosition = new Vector3(
            // Keep X and Z positions the same
            boat.transform.position.x,
            // Calculate the new Y position: starting height + the wave offset
            startY + yOffset,
            // Keep Z position the same
            boat.transform.position.z
        );

        // 3. Apply the new position to the boat
        boat.transform.position = newPosition;
    }

    public void Play()
    {
        SceneManager.LoadScene(1);
    }
}