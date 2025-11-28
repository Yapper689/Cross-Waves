using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Waves_Controller : MonoBehaviour
{
    public static Waves_Controller instance;

    [Header("Waves Settings")]
    public float amplitude = 50f;
    public float width = 150f;
    public float speed = 10f;

    [Header("Spawner Settings")]
    public float spawnInterval = 150f;
    public int maxWaves = 3; // how many waves can be active

    private float spawnTimer = 0f;

    // Each waves has its own center position
    private class Wave
    {
        public float center; // moves along X
    }

    private List<Wave> waves = new List<Wave>();


    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    private void Start()
    {
        SpawnWave();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime * speed;

        // Spawn new wave at interval
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnWave();
        }

        // Move waves forward
        for (int i = 0; i < waves.Count; i++)
        {
            waves[i].center += Time.deltaTime * speed;
        }
    }

    private void SpawnWave()
    {
        // If we reached max waves, remove the oldest one
        if (waves.Count >= maxWaves)
        {
            waves.RemoveAt(0); // remove FIRST, not last
        }

        Wave p = new Wave();
        p.center = 0f;
        waves.Add(p);
    }

    // Combine wave heights from all active waves
    public float GetWaveHeight(float x)
    {
        float height = 0f;

        foreach (Wave p in waves)
        {
            float dx = x - p.center;
            // Gaussian Waves
            float gauss = Mathf.Exp(-(dx * dx) / width);
            // Sine Waves: amplitude* Mathf.Sin(_x / length + offset);
            height += amplitude * gauss;
        }

        return height;
    }
}

