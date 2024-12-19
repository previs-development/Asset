using UnityEngine;

public class LightSpawner : MonoBehaviour
{
    public GameObject lightPrefab;        // 조명 프리팹
    public GameObject visualPrefab;       // 시각적 프리팹
    public GameObject cameraPrefab;       // 카메라 프리팹
    public CameraManager cameraManager;   // 카메라 관리 스크립트

    public void SpawnLight()
    {
        if (cameraManager == null)
        {
            Debug.LogError("LightSpawner: CameraManager is not assigned.");
            return;
        }

        // Get main camera's position and rotation
        Vector3 spawnPosition = cameraManager.mainCamera.transform.position;
        Quaternion spawnRotation = cameraManager.mainCamera.transform.rotation;

        // Instantiate light
        GameObject lightObject = Instantiate(lightPrefab, spawnPosition, spawnRotation);
        Light lightComponent = lightObject.GetComponent<Light>();
        if (lightComponent == null)
        {
            Debug.LogError("LightSpawner: lightPrefab does not have a Light component.");
            Destroy(lightObject);
            return;
        }
        else
        {
            Debug.Log($"LightSpawner: Light instantiated at {spawnPosition}");
        }

        // Instantiate visual prefab
        GameObject visualObject = Instantiate(visualPrefab, spawnPosition, spawnRotation);
        visualObject.transform.localRotation = Quaternion.identity; // Reset rotation
        Debug.Log("LightSpawner: VisualPrefab instantiated.");

        // Instantiate camera
        GameObject cameraObject = Instantiate(cameraPrefab, spawnPosition, spawnRotation);
        Camera attachedCamera = cameraObject.GetComponent<Camera>();

        if (attachedCamera != null)
        {
            // Tag the new camera as LightCamera
            cameraObject.tag = "LightCamera";
            Debug.Log("LightSpawner: New camera tagged as LightCamera.");

            // Parent light and visual prefab to the new camera
            lightObject.transform.SetParent(cameraObject.transform, true);
            visualObject.transform.SetParent(cameraObject.transform, true);
            Debug.Log("LightSpawner: Light and VisualPrefab parented to LightCamera.");

            // Set LightSelector references
            LightSelector lightSelector = visualObject.GetComponent<LightSelector>();
            if (lightSelector != null)
            {
                lightSelector.cameraManager = cameraManager;
                lightSelector.targetCamera = attachedCamera;
                Debug.Log("LightSpawner: LightSelector references set.");
            }
            else
            {
                Debug.LogError("LightSpawner: visualPrefab does not have a LightSelector component.");
            }

            // Set LightProperties
            LightProperties lightProperties = visualObject.GetComponent<LightProperties>();
            if (lightProperties != null)
            {
                lightProperties.SetLight(lightComponent);
                Debug.Log("LightSpawner: LightProperties set.");
            }
            else
            {
                Debug.LogError("LightSpawner: visualPrefab does not have a LightProperties component.");
            }

            // Add the new camera to CameraManager
            cameraManager.AddCamera(attachedCamera);
            Debug.Log("LightSpawner: New camera added to CameraManager.");

            // Switch to the new camera after LightProperties is set
            cameraManager.SwitchToCamera(attachedCamera);
            Debug.Log("LightSpawner: Switched to new LightCamera.");
        }
        else
        {
            Debug.LogError("LightSpawner: cameraPrefab does not have a Camera component.");
            Destroy(cameraObject);
            return;
        }
    }
}
