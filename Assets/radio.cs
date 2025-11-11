using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(XRGrabInteractable))]
public class RadioController : MonoBehaviour
{
    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;

    [Header("Radio Sound")]
    public AudioClip radioSound; // suara yang mau diputar

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;

        // Dengarkan event grab
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        if (radioSound == null) return;

        // Restart suara setiap kali dipencet
        audioSource.Stop();
        audioSource.clip = radioSound;
        audioSource.Play();
    }
}
