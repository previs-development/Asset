using UnityEngine;

public class LightControllerUI : MonoBehaviour
{
    public FlexibleColorPicker colorPicker; // Flexible Color Picker
    public LightProperties lightProperties; // LightProperties 스크립트

    void Start()
    {
        if (colorPicker != null)
        {
            // Color Picker의 색상 변경 이벤트 연결
            colorPicker.onColorChange.AddListener(UpdateLightColor);
        }
    }

    void UpdateLightColor(Color newColor)
    {
        if (lightProperties != null)
        {
            lightProperties.UpdateColor(newColor); // 조명 색상 업데이트
        }
    }

    // UI 슬라이더나 드롭다운 등에서 호출할 수 있는 메서드들 추가
    public void SetIntensity(float intensity)
    {
        if (lightProperties != null)
        {
            lightProperties.UpdateIntensity(intensity); // 조명 강도 업데이트
        }
    }

    public void SetLightMode(int mode)
    {
        if (lightProperties != null)
        {
            LightType lightType = LightType.Point;
            switch (mode)
            {
                case 0:
                    lightType = LightType.Point;
                    break;
                case 1:
                    lightType = LightType.Spot;
                    break;
                case 2:
                    lightType = LightType.Directional;
                    break;
            }
            lightProperties.UpdateMode(lightType); // 조명 모드 업데이트
        }
    }
}
