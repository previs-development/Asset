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
    }

    void Update()
    {
        // ESC 키를 눌렀을 때 메인 카메라로 전환
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchToMainCamera();
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
}