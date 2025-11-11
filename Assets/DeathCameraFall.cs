using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

public class DeathCameraFall : MonoBehaviour
{
    [Header("Camera & Motion")]
    public Transform cameraTransform;
    public float fallDuration = 2f;
    public Vector3 fallOffset = new Vector3(0, -0.6f, 0.5f);
    public Vector3 fallRotation = new Vector3(80, 0, 0);

    [Header("XR Controls")]
    public ActionBasedContinuousMoveProvider moveProvider;
    public ActionBasedContinuousTurnProvider turnProvider;

    [Header("Audio & Fade")]
    public AudioSource deathAudio;
    public FadeToBlack fadeEffect;

    [Header("Detection")]
    public string ghostTag = "Ghost";

    private bool isDead = false;
    private Vector3 startPos;
    private Quaternion startRot;
    private float timer;

    void OnTriggerEnter(Collider other)
    {
        if (!isDead && other.CompareTag(ghostTag))
        {
            KillPlayer();
        }
    }

    public void KillPlayer()
    {
        if (isDead) return;
        isDead = true;

        // Matikan kontrol gerak
        if (moveProvider) moveProvider.enabled = false;
        if (turnProvider) turnProvider.enabled = false;

        // Posisi & rotasi awal
        startPos = cameraTransform.localPosition;
        startRot = cameraTransform.localRotation;
        timer = 0f;

        // Mainkan suara kematian
        if (deathAudio) deathAudio.Play();
    }

    void Update()
    {
        if (isDead)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fallDuration);

            // Gerakan jatuh
            cameraTransform.localPosition = Vector3.Lerp(startPos, startPos + fallOffset, t);
            cameraTransform.localRotation = Quaternion.Slerp(startRot, Quaternion.Euler(fallRotation), t);

            // Setelah jatuh selesai → fade
            if (t >= 1f && fadeEffect)
            {
                fadeEffect.StartFade();
             
            }
        }
    }
}
