using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TriggerJumpscare : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player; // XR Origin / Main Camera

    [Header("UI Settings")]
    public Canvas jumpscareCanvas; // Canvas yang berisi Image
    public Image jumpscareImage;   // Gambar jumpscare

    [Header("Audio Settings")]
    public AudioSource scareSound; // AudioSource (suara jumpscare)

    [Header("Effect Settings")]
    public float scareDuration = 2f; // Lama muncul jumpscare
    public bool oneTime = true;      // Sekali saja atau bisa berulang

    private bool triggered = false;

    void Start()
    {
        // Pastikan canvas mati saat awal
        if (jumpscareCanvas) jumpscareCanvas.enabled = false;

        // Pastikan gambar transparan
        if (jumpscareImage)
        {
            Color c = jumpscareImage.color;
            c.a = 0;
            jumpscareImage.color = c;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (oneTime && triggered) return;

        if (other.transform == player || other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(DoJumpscare());
        }
    }

    IEnumerator DoJumpscare()
    {
        if (jumpscareCanvas) jumpscareCanvas.enabled = true;
        if (scareSound) scareSound.Play();

        // Fade-in cepat
        if (jumpscareImage)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 4f;
                Color c = jumpscareImage.color;
                c.a = Mathf.Lerp(0, 1, t);
                jumpscareImage.color = c;
                yield return null;
            }
        }

        yield return new WaitForSeconds(scareDuration);

        // Fade-out perlahan
        if (jumpscareImage)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 2f;
                Color c = jumpscareImage.color;
                c.a = Mathf.Lerp(1, 0, t);
                jumpscareImage.color = c;
                yield return null;
            }
        }

        if (jumpscareCanvas) jumpscareCanvas.enabled = false;
    }
}
