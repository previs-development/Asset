using UnityEngine;

public class LightControllerUI : MonoBehaviour
{
    public FlexibleColorPicker colorPicker; // Flexible Color Picker
    public LightProperties lightProperties; // LightProperties 스크립트
    public GameObject lightControlPanel;    // LightControlPanel UI 패널

    private CameraManager cameraManager;
    private bool isSettingColor = false; // 프로그램적으로 색상 설정 중인지 여부

    void OnEnable()
    {
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager != null)
        {
            cameraManager.OnCameraSwitched += UpdateLightProperties;
            Debug.Log("LightControllerUI: Subscribed to OnCameraSwitched event.");
        }
        else
        {
            Debug.LogError("LightControllerUI: CameraManager not found.");
        }

        // LightControlPanel 비활성화
        if (lightControlPanel != null)
        {
            lightControlPanel.SetActive(false);
            Debug.Log("LightControllerUI: LightControlPanel set to inactive.");
        }

        // 현재 활성 카메라의 LightProperties 업데이트
        if (cameraManager != null)
        {
            Camera currentCamera = cameraManager.GetCurrentCamera();
            UpdateLightProperties(currentCamera);
        }
    }

    void OnDisable()
    {
        if (cameraManager != null)
        {
            cameraManager.OnCameraSwitched -= UpdateLightProperties;
            Debug.Log("LightControllerUI: Unsubscribed from OnCameraSwitched event.");
        }
    }

    void Start()
    {
        if (colorPicker != null)
        {
            // ColorPicker의 색상 변경 이벤트 연결
            colorPicker.onColorChange.AddListener(UpdateLightColor);
            Debug.Log("LightControllerUI: Connected UpdateLightColor to colorPicker.");
        }
    }

    void UpdateLightProperties(Camera newCamera)
    {
        if (newCamera == null) return;

        if (newCamera.CompareTag("LightCamera"))
        {
            LightProperties lp = newCamera.GetComponentInChildren<LightProperties>();
            if (lp != null)
            {
                lightProperties = lp;
                Debug.Log($"LightControllerUI: LightProperties updated - {lp.gameObject.name}");

                if (lightControlPanel != null)
                {
                    lightControlPanel.SetActive(true);
                    Debug.Log("LightControllerUI: LightControlPanel set to active.");
                }

                // ColorPicker 색상 설정 시 이벤트 제거 및 재등록
                if (colorPicker != null && lightProperties != null)
                {
                    // 이벤트 제거
                    colorPicker.onColorChange.RemoveListener(UpdateLightColor);

                    // 프로그램적으로 색상 설정
                    isSettingColor = true;
                    colorPicker.color = lightProperties.GetCurrentColor();
                    isSettingColor = false;

                    // 이벤트 재등록
                    colorPicker.onColorChange.AddListener(UpdateLightColor);

                    Debug.Log($"LightControllerUI: ColorPicker color set to {lightProperties.GetCurrentColor()}");
                }
            }
            else
            {
                lightProperties = null;
                if (lightControlPanel != null)
                {
                    lightControlPanel.SetActive(false);
                    Debug.Log("LightControllerUI: LightControlPanel set to inactive.");
                }
            }
        }
        else
        {
            lightProperties = null;
            if (lightControlPanel != null)
            {
                lightControlPanel.SetActive(false);
                Debug.Log("LightControllerUI: LightControlPanel set to inactive (non-LightCamera).");
            }
        }
    }

    void UpdateLightColor(Color newColor)
    {
        if (isSettingColor) return; // 프로그램적으로 설정 중이면 무시

        if (lightProperties != null)
        {
            lightProperties.UpdateColor(newColor);
            Debug.Log($"LightControllerUI: Updated light color to {newColor}");
        }
        else
        {
            Debug.LogWarning("LightControllerUI: LightProperties not set.");
        }
    }
}
