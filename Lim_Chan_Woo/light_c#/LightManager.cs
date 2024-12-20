using UnityEngine;

public class LightManager : MonoBehaviour
{
    public CameraManager cameraManager;

    public void DeleteLightAndCamera()
    {
        // 현재 활성화된 카메라 가져오기
        Camera currentCamera = cameraManager.GetCurrentCamera();
        if (currentCamera != null && currentCamera != cameraManager.mainCamera)
        {
            // 카메라와 연결된 GameObject 삭제
            Destroy(currentCamera.gameObject);

            // 메인 카메라로 전환
            cameraManager.SwitchToMainCamera();
        }
        else
        {
            Debug.LogWarning("삭제할 수 있는 활성 카메라가 없습니다. (메인 카메라는 삭제할 수 없습니다.)");
        }
    }
}