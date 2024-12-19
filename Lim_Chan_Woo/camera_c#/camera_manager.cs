using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // 모든 카메라를 저장할 리스트
    private List<Camera> cameras = new List<Camera>();

    // 메인 카메라
    public Camera mainCamera;

    // 현재 활성화된 카메라의 인덱스
    private int currentCameraIndex = -1;

    // 카메라 전환 이벤트
    public delegate void CameraSwitched(Camera newCamera);
    public event CameraSwitched OnCameraSwitched;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera가 설정되지 않았습니다.");
                return;
            }
        }

        // 메인 카메라를 리스트에 추가하고 활성화
        cameras.Add(mainCamera);
        mainCamera.enabled = true;
        currentCameraIndex = 0;

        // 이벤트 발생
        OnCameraSwitched?.Invoke(mainCamera);
    }

    void Update()
    {
        // ESC 키를 눌렀을 때 메인 카메라로 전환
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchToMainCamera();
        }

        // Delete 키를 눌렀을 때 현재 활성화된 카메라 삭제
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            RemoveCurrentCamera();
        }
    }

    // 특정 카메라로 전환하는 메서드
    public void SwitchToCamera(Camera newCamera)
    {
        if (newCamera == null || !cameras.Contains(newCamera))
            return;

        // 현재 활성화된 카메라 비활성화
        if (currentCameraIndex >= 0 && currentCameraIndex < cameras.Count)
        {
            cameras[currentCameraIndex].enabled = false;
        }

        // 새로운 카메라 활성화
        newCamera.enabled = true;
        currentCameraIndex = cameras.IndexOf(newCamera);

        // 이벤트 발생
        OnCameraSwitched?.Invoke(newCamera);
    }

    // 메인 카메라로 전환하는 메서드
    public void SwitchToMainCamera()
    {
        if (mainCamera == null)
            return;

        if (currentCameraIndex != 0)
        {
            // 현재 활성화된 카메라 비활성화
            if (currentCameraIndex >= 0 && currentCameraIndex < cameras.Count)
            {
                cameras[currentCameraIndex].enabled = false;
            }

            // 메인 카메라 활성화
            mainCamera.enabled = true;
            currentCameraIndex = 0;

            // 이벤트 발생
            OnCameraSwitched?.Invoke(mainCamera);
        }
    }

    // 새로운 카메라를 리스트에 추가하는 메서드
    public void AddCamera(Camera newCamera)
    {
        if (newCamera != null && !cameras.Contains(newCamera))
        {
            cameras.Add(newCamera);
        }
    }

    // 현재 활성화된 카메라를 삭제하고 메인 카메라로 전환하는 메서드
    public void RemoveCurrentCamera()
    {
        // 메인 카메라는 삭제할 수 없음
        if (currentCameraIndex <= 0 || currentCameraIndex >= cameras.Count)
        {
            Debug.LogWarning("삭제할 수 있는 활성 카메라가 없습니다. (메인 카메라는 삭제할 수 없습니다.)");
            return;
        }

        Camera cameraToRemove = cameras[currentCameraIndex];

        if (cameraToRemove != null)
        {
            // 카메라 비활성화 및 삭제
            cameraToRemove.enabled = false;
            cameras.RemoveAt(currentCameraIndex);
            Destroy(cameraToRemove.gameObject);
            Debug.Log($"카메라 '{cameraToRemove.name}'가 삭제되었습니다.");

            // 메인 카메라로 전환
            SwitchToMainCamera();
        }
        else
        {
            Debug.LogError("삭제하려는 카메라가 존재하지 않습니다.");
        }
    }

    // 현재 활성화된 카메라 가져오기
    public Camera GetCurrentCamera()
    {
        if (currentCameraIndex >= 0 && currentCameraIndex < cameras.Count)
        {
            return cameras[currentCameraIndex];
        }

        Debug.LogWarning("현재 활성화된 카메라가 없습니다.");
        return null;
    }
}
