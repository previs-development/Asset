using UnityEngine;

public class LightControllerUI : MonoBehaviour
{
    public FlexibleColorPicker colorPicker; // Flexible Color Picker
    public LightProperties lightProperties; // LightProperties 스크립트
    public GameObject lightControlPanel;    // LightControlPanel UI 패널

    private CameraManager cameraManager;

    void OnEnable()
    {
        cameraManager = FindObjectOfType<CameraManager>();
        if (cameraManager != null)
        {
            cameraManager.OnCameraSwitched += UpdateLightProperties;
        }

        // Ensure LightControlPanel is hidden initially
        if (lightControlPanel != null)
        {
            lightControlPanel.SetActive(false);
        }
    }

    void OnDisable()
    {
        if (cameraManager != null)
        {
            cameraManager.OnCameraSwitched -= UpdateLightProperties;
        }
    }

    void Start()
    {
        if (colorPicker != null)
        {
            // Connect color picker event
            colorPicker.onColorChange.AddListener(UpdateLightColor);
        }

        // Set initial panel state based on current camera
        if (cameraManager != null)
        {
            Camera currentCamera = cameraManager.GetCurrentCamera();
            UpdateLightProperties(currentCamera);
        }
    }

    void UpdateLightProperties(Camera newCamera)
    {
        if (newCamera != null)
        {
            // Check if the new camera is a LightCamera
            if (newCamera.CompareTag("LightCamera"))
            {
                // Find LightProperties in the new camera's children
                LightProperties lp = newCamera.GetComponentInChildren<LightProperties>();
                if (lp != null)
                {
                    lightProperties = lp;
                    Debug.Log($"LightControllerUI: LightProperties updated - {lp.gameObject.name}");

                    // Show the LightControlPanel
                    if (lightControlPanel != null)
                    {
                        lightControlPanel.SetActive(true);
                    }
                }
                else
                {
                    Debug.LogWarning("LightControllerUI: No LightProperties found in the new camera.");
                    lightProperties = null;

                    // Hide the LightControlPanel
                    if (lightControlPanel != null)
                    {
                        lightControlPanel.SetActive(false);
                    }
                }
            }
            else
            {
                // If not a LightCamera, hide the LightControlPanel
                lightProperties = null;
                if (lightControlPanel != null)
                {
                    lightControlPanel.SetActive(false);
                }
            }
        }
    }

    void UpdateLightColor(Color newColor)
    {
        if (lightProperties != null)
        {
            lightProperties.UpdateColor(newColor); // Update light color
        }
        else
        {
            Debug.LogWarning("LightControllerUI: LightProperties not set.");
        }
    }
}
