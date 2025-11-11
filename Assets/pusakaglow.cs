using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PusakaGlow : MonoBehaviour
{
    public Light pusakaLight;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null && pusakaLight != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (pusakaLight != null)
            pusakaLight.enabled = false;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (pusakaLight != null)
            pusakaLight.enabled = true;
    }
}
