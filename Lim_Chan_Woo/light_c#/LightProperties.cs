using UnityEngine;

public class LightProperties : MonoBehaviour
{
    private Light targetLight;
    private Color lastUserColor = Color.white; // 기본 색상을 흰색으로 설정

    // 조명을 설정하는 메서드
    public void SetLight(Light light)
    {
        targetLight = light;
        if (targetLight != null)
        {
            // 조명이 할당될 때 현재 색상을 기록
            lastUserColor = targetLight.color;
            Debug.Log($"LightProperties: Light set to {light.name}, initial color: {lastUserColor}");
        }
        else
        {
            Debug.LogWarning("LightProperties: targetLight is not set.");
        }
    }

    // 색상을 업데이트하는 메서드
    public void UpdateColor(Color color)
    {
        if (targetLight != null)
        {
            targetLight.color = color;
            lastUserColor = color; // 변경된 색상을 기록
            Debug.Log($"LightProperties: Light color updated to {color}");
        }
        else
        {
            Debug.LogWarning("LightProperties: targetLight is not set.");
        }
    }

    // 현재 색상을 반환하는 메서드
    public Color GetCurrentColor()
    {
        return lastUserColor;
    }
}
