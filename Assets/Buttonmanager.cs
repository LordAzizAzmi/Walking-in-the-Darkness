using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtonManager : MonoBehaviour
{
    public void StartAreaA()
    {
        SceneManager.LoadScene("AreaA");
    }

    public void StartAreaB()
    {
        SceneManager.LoadScene("AreaB");
    }

    public void StartAreaC()
    {
        SceneManager.LoadScene("AreaC");
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
