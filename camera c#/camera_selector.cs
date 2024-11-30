using UnityEngine;

public class CameraSelector : MonoBehaviour
{
    public Camera targetCamera;

    void OnMouseDown()
    {
        if (targetCamera != null)
        {
            CameraManager manager = FindObjectOfType<CameraManager>();
            if (manager != null)
            {
                manager.SwitchToCamera(targetCamera);
            }
        }
    }
}
