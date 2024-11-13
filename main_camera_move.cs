using UnityEngine;

public class CameraController : MonoBehaviour
{
    // 이동 속도
    public float moveSpeed = 10f;
    // 마우스 감도
    public float lookSensitivity = 2f;

    // 현재 카메라의 회전 값
    private float yaw = 0f;
    private float pitch = 0f;

    // 카메라 제어 활성화 상태
    private bool isCameraControlActive = false;

    void Update()
    {
        HandleInput();

        if (isCameraControlActive)
        {
            HandleMovement();
            HandleMouseLook();
        }
    }

    void HandleInput()
    {
        // 우클릭 상태 체크
        if (Input.GetMouseButtonDown(1))
        {
            ActivateCameraControl();
        }
        if (Input.GetMouseButtonUp(1))
        {
            DeactivateCameraControl();
        }
    }

    void ActivateCameraControl()
    {
        isCameraControlActive = true;
        // 마우스 커서를 숨기고 잠급니다.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void DeactivateCameraControl()
    {
        isCameraControlActive = false;
        // 마우스 커서를 다시 보이게 하고 잠금을 해제합니다.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HandleMovement()
    {
        // 입력 값 받기
        float moveForward = Input.GetAxis("Vertical");   // W/S 또는 ↑/↓
        float moveRight = Input.GetAxis("Horizontal");   // A/D 또는 ←/→
        float moveUp = 0f;

        if (Input.GetKey(KeyCode.Space))
        {
            moveUp += 1f;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            moveUp -= 1f;
        }

        // 이동 벡터 계산
        Vector3 move = transform.forward * moveForward + transform.right * moveRight + transform.up * moveUp;
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void HandleMouseLook()
    {
        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        // Yaw (좌우 회전)
        yaw += mouseX;
        // Pitch (상하 회전), 제한을 두어 카메라가 너무 많이 회전하지 않도록 함
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        // 회전 적용
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }
}
