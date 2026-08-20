using UnityEngine;
using System.Collections;

public class BandarBirdAmbience : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] birdClips;
    [SerializeField] private float minDelay = 3f;
    [SerializeField] private float maxDelay = 8f;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        StartCoroutine(PlayBirdSounds());
    }

    private IEnumerator PlayBirdSounds()
    {
        while (true)
        {
            float waitTime = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);

            if (audioSource != null && birdClips != null && birdClips.Length > 0)
            {
                int index = Random.Range(0, birdClips.Length);
                audioSource.PlayOneShot(birdClips[index]);
            }
        }
    }
}