using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeToBlack : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1f;
    private bool isFading = false;

    public void StartFade()
    {
        isFading = true;
    }

    void Update()
    {
        if (isFading && fadeImage)
        {
            Color c = fadeImage.color;
            c.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = c;

            if (c.a >= 1f)
            {
                isFading = false;
                SceneManager.LoadScene("UImenu");
                // TODO: Load scene game over atau restart
            }
        }
    }
}
