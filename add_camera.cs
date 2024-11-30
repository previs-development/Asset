using UnityEngine;

public class AddCamera : MonoBehaviour
{
    public GameObject cameraPrefab; // 카메라 프리팹
    // public Vector3 spawnPosition = Vector3.zero; // 기존의 고정 위치는 삭제 또는 사용 안 함

    // CameraManager를 참조하기 위한 변수
    public CameraManager cameraManager;

    // 카메라 큐브 프리팹 (검정색 큐브)
    public GameObject cameraVisualPrefab;

    public void SpawnCamera()
    {
        if (cameraManager == null)
        {
            Debug.LogError("CameraManager가 할당되지 않았습니다.");
            return;
        }

        // 메인 카메라의 현재 위치를 가져옴
        Vector3 spawnPosition = cameraManager.mainCamera.transform.position;
        Quaternion spawnRotation = cameraManager.mainCamera.transform.rotation;

        // 카메라 프리팹을 사용하여 새로운 카메라 생성
        GameObject cameraObj = Instantiate(cameraPrefab, spawnPosition, spawnRotation);
        Camera newCamera = cameraObj.GetComponent<Camera>();

        if (newCamera == null)
        {
            Debug.LogError("카메라 프리팹에 Camera 컴포넌트가 없습니다.");
            Destroy(cameraObj);
            return;
        }

        // CameraManager에 새 카메라 추가
        cameraManager.AddCamera(newCamera);

        // 메인 카메라가 아니면 시점 전환 및 큐브 생성
        if (newCamera != cameraManager.mainCamera)
        {
            // 시점 전환
            cameraManager.SwitchToCamera(newCamera);

            // 검정색 큐브 시각적 표시 추가
            if (cameraVisualPrefab != null)
            {
                GameObject visual = Instantiate(cameraVisualPrefab, cameraObj.transform.position, cameraObj.transform.rotation, cameraObj.transform);
                visual.transform.localScale = Vector3.one * 0.5f; // 큐브 크기 조정

                // 큐브에 클릭 이벤트를 위한 컴포넌트 추가
                BoxCollider collider = visual.GetComponent<BoxCollider>();
                if (collider == null)
                {
                    collider = visual.AddComponent<BoxCollider>();
                }

                // 클릭 이벤트 처리 스크립트 추가
                CameraSelector selector = visual.AddComponent<CameraSelector>();
                selector.targetCamera = newCamera;
            }
        }
    }
}
