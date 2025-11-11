using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FlashlightToggle : MonoBehaviour
{
    public Light flashlightLight;

    private void Start()
    {
        if (flashlightLight == null)
            flashlightLight = GetComponentInChildren<Light>();  // Cari otomatis
    }

    public void Toggle()
    {
        if (flashlightLight != null)
            flashlightLight.enabled = !flashlightLight.enabled;
    }
}
