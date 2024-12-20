using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // All cameras list
    private List<Camera> cameras = new List<Camera>();

    // Main camera
    public Camera mainCamera;

    // Current active camera index
    private int currentCameraIndex = -1;

    // Camera switch event
    public delegate void CameraSwitched(Camera newCamera);
    public event CameraSwitched OnCameraSwitched;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("CameraManager: Main Camera is not set.");
                return;
            }
        }

        // Add main camera to list and activate it
        cameras.Add(mainCamera);
        mainCamera.enabled = true;
        currentCameraIndex = 0;

        // Invoke event
        OnCameraSwitched?.Invoke(mainCamera);
        Debug.Log("CameraManager: OnCameraSwitched 이벤트 발생 - Main Camera");
    }

    void Update()
    {
        // Press ESC to switch to main camera
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchToMainCamera();
        }

        // Press Delete to remove current camera
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            RemoveCurrentCamera();
        }
    }

    // Switch to specific camera
    public void SwitchToCamera(Camera newCamera)
    {
        if (newCamera == null || !cameras.Contains(newCamera))
            return;

        // Disable current camera
        if (currentCameraIndex >= 0 && currentCameraIndex < cameras.Count)
        {
            cameras[currentCameraIndex].enabled = false;
        }

        // Enable new camera
        newCamera.enabled = true;
        currentCameraIndex = cameras.IndexOf(newCamera);

        // Invoke event and log
        OnCameraSwitched?.Invoke(newCamera);
        Debug.Log($"CameraManager: OnCameraSwitched 이벤트 발생 - {newCamera.name}");
    }

    // Switch to main camera
    public void SwitchToMainCamera()
    {
        if (mainCamera == null)
            return;

        if (currentCameraIndex != 0)
        {
            // Disable current camera
            if (currentCameraIndex >= 0 && currentCameraIndex < cameras.Count)
            {
                cameras[currentCameraIndex].enabled = false;
            }

            // Enable main camera
            mainCamera.enabled = true;
            currentCameraIndex = 0;

            // Invoke event and log
            OnCameraSwitched?.Invoke(mainCamera);
            Debug.Log("CameraManager: OnCameraSwitched 이벤트 발생 - Main Camera");
        }
    }

    // Add new camera to list
    public void AddCamera(Camera newCamera)
    {
        if (newCamera != null && !cameras.Contains(newCamera))
        {
            cameras.Add(newCamera);
            Debug.Log($"CameraManager: Camera '{newCamera.name}' added to the camera list.");
        }
    }

    // Remove current camera and switch to main
    public void RemoveCurrentCamera()
    {
        // Cannot remove main camera
        if (currentCameraIndex <= 0 || currentCameraIndex >= cameras.Count)
        {
            Debug.LogWarning("CameraManager: No active camera to remove. (Cannot remove Main Camera.)");
            return;
        }

        Camera cameraToRemove = cameras[currentCameraIndex];

        if (cameraToRemove != null)
        {
            // Disable and remove camera
            cameraToRemove.enabled = false;
            cameras.RemoveAt(currentCameraIndex);
            Destroy(cameraToRemove.gameObject);
            Debug.Log($"CameraManager: Camera '{cameraToRemove.name}' has been removed.");

            // Switch to main camera
            SwitchToMainCamera();
        }
        else
        {
            Debug.LogError("CameraManager: Camera to remove does not exist.");
        }
    }

    // Get current active camera
    public Camera GetCurrentCamera()
    {
        if (currentCameraIndex >= 0 && currentCameraIndex < cameras.Count)
        {
            return cameras[currentCameraIndex];
        }

        Debug.LogWarning("CameraManager: No active camera.");
        return null;
    }
}
