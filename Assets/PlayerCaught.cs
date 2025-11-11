using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    public DeathCameraFall deathCam;
    public FadeToBlack fadeScript;
    public MonoBehaviour playerMovement; // script movement player, bisa XR Rig controller atau FirstPersonController
    public float delayBeforeFade = 0.5f;
    public float delayBeforeGameOver = 2f;

    private bool isDead = false;

    public void PlayerCaught()
    {
        if (isDead) return; // supaya tidak double trigger
        isDead = true;

        GetComponent<PlayerDeathHandler>().PlayerCaught();

        // Matikan movement
        if (playerMovement != null) playerMovement.enabled = false;

        // Kamera jatuh
        if (deathCam != null) deathCam.KillPlayer();

        // Fade setelah sedikit delay
        Invoke(nameof(StartFade), delayBeforeFade);

        // Bisa tambahkan pindah scene game over
        Invoke(nameof(GoToGameOver), delayBeforeGameOver);
    }

    private void StartFade()
    {
        if (fadeScript != null) fadeScript.StartFade();
    }

    private void GoToGameOver()
    {
        Debug.Log("Game Over");
        // Contoh: UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
    }
}
