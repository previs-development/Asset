using UnityEngine;

public class AddCamera : MonoBehaviour
{
    public GameObject cameraPrefab; // 카메라 프리팹

    // CameraManager를 참조하기 위한 변수
    public CameraManager cameraManager;

    // 카메라 시각적 프리팹 (새로운 카메라 디자인 프리팹)
    public GameObject cameraVisualPrefab;

    public void SpawnCamera()
    {
        if (cameraManager == null)
        {
            Debug.LogError("CameraManager가 할당되지 않았습니다.");
            return;
        }

        // 메인 카메라의 현재 위치와 회전을 가져옴
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

        // 메인 카메라가 아니면 시점 전환 및 시각적 표시 프리팹 생성
        if (newCamera != cameraManager.mainCamera)
        {
            // 시점 전환
            cameraManager.SwitchToCamera(newCamera);

            // 시각적 표시 프리팹 추가
            if (cameraVisualPrefab != null)
            {
                // 시각적 프리팹을 새로운 카메라의 자식으로 생성
                GameObject visual = Instantiate(cameraVisualPrefab, cameraObj.transform);
                visual.transform.localPosition = Vector3.zero; // 카메라 위치에 시각적 프리팹 배치
                visual.transform.localRotation = Quaternion.identity; // 회전 초기화

                // CameraSelector의 targetCamera 필드 설정
                CameraSelector selector = visual.GetComponent<CameraSelector>();
                if (selector != null)
                {
                    selector.targetCamera = newCamera;
                }
                else
                {
                    Debug.LogError("시각적 프리팹에 CameraSelector 스크립트가 없습니다.");
                }
            }
        }
    }
}
