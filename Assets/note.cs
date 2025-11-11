using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PaperGrab : MonoBehaviour
{
    public GameObject paperUI;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }

        if (paperUI != null)
            paperUI.SetActive(false); // UI dimatikan awalnya
    }

    private void OnGrab(SelectEnterEventArgs arg)
    {
        if (paperUI != null)
            paperUI.SetActive(true); // Muncul saat di-Grab
    }

    private void OnRelease(SelectExitEventArgs arg)
    {
        if (paperUI != null)
            paperUI.SetActive(false); // Hilang lagi saat dilepas
    }
}
