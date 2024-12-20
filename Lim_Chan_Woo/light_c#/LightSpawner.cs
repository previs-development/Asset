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

        // 메인 카메라의 위치와 회전을 가져옴
        Vector3 spawnPosition = cameraManager.mainCamera.transform.position;
        Quaternion spawnRotation = cameraManager.mainCamera.transform.rotation;

        // 조명 생성
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
            // 기본 색상을 흰색으로 설정
            lightComponent.color = Color.white;
            Debug.Log($"LightSpawner: Light instantiated at {spawnPosition} with color white.");
        }

        // 시각적 프리팹 생성
        GameObject visualObject = Instantiate(visualPrefab, spawnPosition, spawnRotation);
        visualObject.transform.localRotation = Quaternion.identity; // 회전 초기화
        Debug.Log("LightSpawner: VisualPrefab instantiated.");

        // 카메라 생성
        GameObject cameraObject = Instantiate(cameraPrefab, spawnPosition, spawnRotation);
        Camera attachedCamera = cameraObject.GetComponent<Camera>();

        if (attachedCamera != null)
        {
            // 새 카메라에 LightCamera 태그 할당
            cameraObject.tag = "LightCamera";
            Debug.Log("LightSpawner: New camera tagged as LightCamera.");

            // 조명과 시각적 프리팹을 새 카메라의 자식으로 설정
            lightObject.transform.SetParent(cameraObject.transform, true);
            visualObject.transform.SetParent(cameraObject.transform, true);
            Debug.Log("LightSpawner: Light and VisualPrefab parented to LightCamera.");

            // LightSelector 참조 설정
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

            // LightProperties 참조 설정
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

            // 새 카메라를 CameraManager에 추가
            cameraManager.AddCamera(attachedCamera);
            Debug.Log("LightSpawner: New camera added to CameraManager.");

            // LightProperties 설정 후 카메라 전환
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
