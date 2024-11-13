using UnityEngine;

public class PropMover : MonoBehaviour
{
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f;

    private bool isRotating = false;
    private Vector3 lastMousePosition;
    public float rotationSpeed = 1f;

    private bool isDragging = false;
    private Vector3 dragOffset;
    private Plane dragPlane;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라가 없습니다.");
        }
    }

    void Update()
    {
        HandleMouseInput();

        if (isRotating && Input.GetMouseButton(0))
        {
            RotateProp();
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            DragProp();
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
                        isRotating = true;
                        isDragging = false;
                        lastMousePosition = Input.mousePosition;
                        return;
                    }
                    else
                    {
                        isDragging = true;
                        isRotating = false;
                        dragPlane = new Plane(Vector3.up, transform.position);
                        float enter = 0.0f;
                        if (dragPlane.Raycast(ray, out enter))
                        {
                            Vector3 hitPoint = ray.GetPoint(enter);
                            dragOffset = transform.position - hitPoint;
                        }
                    }
                }
            }

            lastClickTime = Time.time;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
            isDragging = false;
        }
    }

    void RotateProp()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        float deltaX = currentMousePosition.x - lastMousePosition.x;

        transform.Rotate(0, deltaX * rotationSpeed, 0, Space.World);

        lastMousePosition = currentMousePosition;
    }

    void DragProp()
    {
        Ray camRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;

        if (dragPlane.Raycast(camRay, out enter))
        {
            Vector3 hitPoint = camRay.GetPoint(enter);
            Vector3 targetPosition = hitPoint + dragOffset;

            targetPosition.y = transform.position.y;

            transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
        }
    }
}
