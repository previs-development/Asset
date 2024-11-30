using UnityEngine;

public class CharacterControllerCombined : MonoBehaviour
{
    // 더블 클릭 감지 변수
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // 더블 클릭 인식 시간 (초)

    // 회전 제어 변수
    private bool isRotating = false;
    private Vector3 lastMousePosition;
    [Range(0.1f, 10f)]
    public float rotationSpeed = 1f; // 회전 속도 조절 (기본값을 1f로 설정)

    // 드래그 제어 변수
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Plane dragPlane;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라가 없습니다. Camera.main을 확인하세요.");
        }

        // 초기 rotationSpeed 값 출력 (디버그 로그 제거)
        // Debug.Log("초기 Rotation Speed: " + rotationSpeed);
    }

    void Update()
    {
        HandleMouseInput();

        if (isRotating && Input.GetMouseButton(0))
        {
            RotateCharacter();
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            DragCharacter();
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == this.transform)
                {
                    if (timeSinceLastClick <= doubleClickThreshold)
                    {
                        // 더블 클릭으로 인식
                        // Debug.Log("더블 클릭 감지");
                        isRotating = true;
                        isDragging = false; // 드래그 모드 비활성화
                        lastMousePosition = Input.mousePosition;

                        // 현재 rotationSpeed 값 출력 (디버그 로그 제거)
                        // Debug.Log("현재 Rotation Speed (더블 클릭 시): " + rotationSpeed);

                        return;
                    }
                    else
                    {
                        // 단일 클릭으로 드래그 모드 시도
                        // Debug.Log("단일 클릭 감지 - 드래그 시작");
                        isDragging = true;
                        isRotating = false; // 회전 모드 비활성화
                        dragPlane = new Plane(Vector3.up, transform.position);
                        float enter = 0.0f;
                        if (dragPlane.Raycast(ray, out enter))
                        {
                            Vector3 hitPoint = ray.GetPoint(enter);
                            dragOffset = transform.position - hitPoint;
                        }

                        // 현재 rotationSpeed 값 출력 (디버그 로그 제거)
                        // Debug.Log("현재 Rotation Speed (드래그 시작 시): " + rotationSpeed);
                    }
                }
            }

            lastClickTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isRotating)
            {
                // Debug.Log("회전 모드 종료");
            }
            if (isDragging)
            {
                // Debug.Log("드래그 모드 종료");
            }
            isRotating = false;
            isDragging = false;

            // 현재 rotationSpeed 값 출력 (디버그 로그 제거)
            // Debug.Log("현재 Rotation Speed (마우스 버튼 업 시): " + rotationSpeed);
        }
    }

    void RotateCharacter()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        float deltaX = currentMousePosition.x - lastMousePosition.x;

        // Y축 회전 (Time.deltaTime 제거)
        transform.Rotate(0, deltaX * rotationSpeed, 0, Space.World);

        // 회전 속도 및 deltaX 값 출력 (디버그 로그 제거)
        // Debug.Log("DeltaX: " + deltaX + ", RotationSpeed: " + rotationSpeed);

        lastMousePosition = currentMousePosition;
    }

    void DragCharacter()
    {
        Ray camRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;

        if (dragPlane.Raycast(camRay, out enter))
        {
            Vector3 hitPoint = camRay.GetPoint(enter);
            Vector3 targetPosition = hitPoint + dragOffset;

            // Y축 고정
            targetPosition.y = transform.position.y;

            // 위치 업데이트
            transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);

            // 드래그 중 현재 rotationSpeed 값 출력 (디버그 로그 제거)
            // Debug.Log("드래그 중 Rotation Speed: " + rotationSpeed);
        }
    }
}
