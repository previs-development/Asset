using UnityEngine;

public class LightProperties : MonoBehaviour
{
    private Light targetLight;

    public void SetLight(Light light)
    {
        targetLight = light;
    }

    public void UpdateIntensity(float intensity)
    {
        if (targetLight != null)
        {
            targetLight.intensity = intensity;
        }
    }

    public void UpdateColor(Color color)
    {
        if (targetLight != null)
        {
            targetLight.color = color;
        }
    }

    public void UpdateMode(LightType type)
    {
        if (targetLight != null)
        {
            targetLight.type = type;
        }
    }
}
