public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject videoUI; // panel / raw image

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Video selesai");

        // Option 1: Hide video
        videoUI.SetActive(false);

        // Option 2: Pindah scene
        SceneManager.LoadScene("UImenu");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // tombol skip
        {
            SkipVideo();
        }
    }

    public void SkipVideo()
    {
        videoPlayer.Stop();
        videoUI.SetActive(false);
        SceneManager.LoadScene("UImenu");
    }
}