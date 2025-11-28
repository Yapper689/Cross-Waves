using System.Collections;
using UnityEngine;

public class AudioSpawner : MonoBehaviour
{
    private float speed = 5f;

    private float spawnInterval = 30f;

    [SerializeField] private GameObject waveAudioPrefab;
    void Start()
    {
        StartCoroutine(SpawnMovingAudio());
    }

    private IEnumerator SpawnMovingAudio()
    {
        while (true)
        {
            // spawn
            GameObject audioObj = Instantiate(waveAudioPrefab, Vector3.zero, Quaternion.identity);

            var a = audioObj.GetComponent<AudioSource>();
            if (a != null)
            {
                a.Play();
            }

            StartCoroutine(MoveAudioObject(audioObj));

            // wait for next spawn
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator MoveAudioObject(GameObject obj)
    {
        Vector3 pos = obj.transform.position;

        while (pos.x < 300f)
        {
            pos.x += speed * Time.deltaTime;
            obj.transform.position = pos;
            yield return null;
        }

        Destroy(obj);
    }
}
