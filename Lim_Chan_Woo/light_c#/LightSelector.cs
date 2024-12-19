using UnityEngine;

public class LightSelector : MonoBehaviour
{
    public CameraManager cameraManager;
    public Camera targetCamera;

    private void OnMouseDown()
    {
        if (targetCamera != null && cameraManager != null)
        {
            cameraManager.SwitchToCamera(targetCamera);
        }
    }
}
