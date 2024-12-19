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
            Debug.LogError("CameraManager가 연결되지 않았습니다.");
            return;
        }

        // 메인 카메라의 정확한 위치와 회전을 가져옴
        Vector3 spawnPosition = cameraManager.mainCamera.transform.position;
        Quaternion spawnRotation = cameraManager.mainCamera.transform.rotation;

        // 조명 생성
        GameObject lightObject = Instantiate(lightPrefab, spawnPosition, spawnRotation);
        Light lightComponent = lightObject.GetComponent<Light>();
        if (lightComponent == null)
        {
            Debug.LogError("조명 프리팹에 Light 컴포넌트가 없습니다.");
            Destroy(lightObject);
            return;
        }

        // 시각적 프리팹 생성
        GameObject visualObject = Instantiate(visualPrefab, spawnPosition, spawnRotation);
        visualObject.transform.localRotation = Quaternion.identity; // 기본 회전 유지

        // 카메라 생성 및 위치 설정
        GameObject cameraObject = Instantiate(cameraPrefab, spawnPosition, spawnRotation);
        Camera attachedCamera = cameraObject.GetComponent<Camera>();

        if (attachedCamera != null)
        {
            // 조명과 시각적 프리팹을 카메라의 자식으로 설정하여 카메라와 함께 이동하도록 함
            lightObject.transform.SetParent(cameraObject.transform, true);
            visualObject.transform.SetParent(cameraObject.transform, true);

            // Visual Prefab과 조명 연동
            LightSelector lightSelector = visualObject.GetComponent<LightSelector>();
            if (lightSelector != null)
            {
                lightSelector.cameraManager = cameraManager;
                lightSelector.targetCamera = attachedCamera;
            }
            else
            {
                Debug.LogError("시각적 프리팹에 LightSelector 스크립트가 없습니다.");
            }

            // LightProperties 연동
            LightProperties lightProperties = visualObject.GetComponent<LightProperties>();
            if (lightProperties != null)
            {
                lightProperties.SetLight(lightComponent);
            }
            else
            {
                Debug.LogError("시각적 프리팹에 LightProperties 스크립트가 없습니다.");
            }

            // CameraManager에 새 카메라 추가
            cameraManager.AddCamera(attachedCamera);

            // LightProperties가 설정된 후에 카메라 전환
            cameraManager.SwitchToCamera(attachedCamera);
        }
        else
        {
            Debug.LogError("생성된 카메라 프리팹에 Camera 컴포넌트가 없습니다.");
            Destroy(cameraObject);
            return;
        }
    }
}
