using UnityEngine;
using System.Collections;

public class RadioLoopSound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip radioSound;
    public float minInterval = 5f;   // jeda minimal (detik)
    public float maxInterval = 15f;  // jeda maksimal (detik)
    public float playDuration = 10f; // berapa lama suara diputar sebelum berhenti

    [Header("3D Sound Settings")]
    public float minDistance = 2f;   // radius suara terdengar jelas
    public float maxDistance = 20f;  // sejauh ini suara masih kedengaran samar
    public float volume = 0.8f;

    private AudioSource audioSource;

    void Start()
    {
        // Tambahkan AudioSource ke asset radio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = radioSound;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = volume;
        audioSource.rolloffMode = AudioRolloffMode.Linear; // penting!


        // mulai loop
        StartCoroutine(RadioLoopCoroutine());
    }

    IEnumerator RadioLoopCoroutine()
    {
        while (true)
        {
            // Play suara radio
            audioSource.Play();

            // tunggu selama playDuration
            yield return new WaitForSeconds(playDuration);

            // Stop suara radio
            audioSource.Stop();

            // tunggu interval acak sebelum play lagi
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
