using UnityEngine;

public class LightProperties : MonoBehaviour
{
    private Light targetLight;

    public void SetLight(Light light)
    {
        targetLight = light;
        Debug.Log($"LightProperties: Light set to {light.name}");
    }

    public void UpdateColor(Color color)
    {
        if (targetLight != null)
        {
            targetLight.color = color;
            Debug.Log($"LightProperties: Light color updated to {color}");
        }
        else
        {
            Debug.LogWarning("LightProperties: targetLight is not set.");
        }
    }
}
