using UnityEngine;

public class DistanceCameraShake : MonoBehaviour
{
    public string incomingTag = "WaveAudio";
    public Transform player;

    public float maxShake = 0.5f;
    public float maxRange = 50f;
    public float minRange = 10f;

    private Vector3 baseLocalPos;
    private Transform incomingObject;

    void Start()
    {
        baseLocalPos = transform.localPosition;

        GameObject found = GameObject.FindGameObjectWithTag(incomingTag);
        if (found != null)
            incomingObject = found.transform;
    }

    void LateUpdate()
    {
        if (incomingObject == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag(incomingTag);
            if (found != null)
                incomingObject = found.transform;
            else
                return;
        }

        if (player == null) return;

        // X-axis only distance
        float dist = Mathf.Abs(player.position.x - incomingObject.position.x);

        float intensity = Mathf.InverseLerp(maxRange, minRange, dist);

        if (intensity > 0f)
        {
            Vector3 offset = Random.insideUnitSphere * (intensity * maxShake);
            transform.localPosition = baseLocalPos + offset;
        }
        else
        {
            transform.localPosition = baseLocalPos;
        }
    }
}
