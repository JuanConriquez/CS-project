using UnityEngine;

public class FlashlightToggle : MonoBehaviour
{
    public Light flashlight;      // Assign your flashlight Light component here
    public KeyCode toggleKey = KeyCode.F;  // Key to toggle flashlight

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            flashlight.enabled = !flashlight.enabled;
        }
    }
}