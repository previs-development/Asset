using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // UI 이벤트 처리를 위해 필요
using SFB; // Standalone File Browser 네임스페이스 추가
using System.Collections.Generic;

public class BackgroundPlaneController : MonoBehaviour
{
    [Header("Background Plane Settings")]
    public GameObject backgroundPlanePrefab; // 배경 Plane의 Prefab
    public Transform planesParent; // 모든 배경 Plane을 담을 부모 Transform

    [Header("UI Elements")]
    public GameObject planeListContent; // Plane 리스트를 담을 ScrollView의 Content
    public GameObject planeListItemPrefab; // Plane 리스트 아이템의 Prefab (버튼 등)
    public Text errorMessageText; // 에러 메시지를 표시할 UI Text

    private List<GameObject> backgroundPlanes = new List<GameObject>(); // 생성된 배경 Plane 리스트
    private List<Material> backgroundMaterials = new List<Material>(); // 각 Plane의 Material 리스트

    private GameObject selectedPlane = null; // 현재 선택된 Plane
    private int planeCount = 0; // Plane 이름을 위한 카운터

    // 설정 가능한 기본 스케일 값
    public Vector3 defaultPlaneScale = new Vector3(0.6f, 2.4f, 0.4f); // 수정된 스케일
    public float planeSpacing = 5f; // Plane 간의 간격

    // 스케일 조절 속도
    public float scaleSpeed = 0.1f; // 전체 비율 조절 속도
    public float scaleSpeedX = 0.1f; // 가로 비율 조절 속도

    void Start()
    {
        // 초기 Plane 생성 (필요하면 주석 해제)
        // CreateNewBackgroundPlane();
    }

    /// <summary>
    /// 새 배경 Plane을 생성하고 리스트에 추가합니다.
    /// </summary>
    public void CreateNewBackgroundPlane()
    {
        if (backgroundPlanePrefab == null)
        {
            Debug.LogError("Background Plane Prefab이 할당되지 않았습니다!");
            ShowError("Background Plane Prefab이 할당되지 않았습니다!");
            return;
        }

        // 카메라의 현재 위치와 방향을 기준으로 Plane 위치 설정
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라가 없습니다!");
            ShowError("메인 카메라가 없습니다!");
            return;
        }

        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 5f;

        // Plane이 카메라를 바라보도록 회전 설정
        Quaternion spawnRotation = Quaternion.LookRotation(-mainCamera.transform.forward, Vector3.up);

        // Prefab 인스턴스화
        GameObject newPlane = Instantiate(backgroundPlanePrefab, spawnPosition, spawnRotation, planesParent);

        // Plane의 스케일 설정
        newPlane.transform.localScale = defaultPlaneScale; // 지정된 스케일로 설정

        // 로테이션 X를 -90도로 설정
        newPlane.transform.Rotate(-90f, 0f, 0f, Space.Self);

        // 유니크한 이름 부여
        planeCount++;
        newPlane.name = $"Plane_{planeCount}";

        // Material 생성 및 적용
        Renderer renderer = newPlane.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material newMaterial = new Material(Shader.Find("Standard"));
            renderer.material = newMaterial;
            backgroundMaterials.Add(newMaterial);
        }
        else
        {
            Debug.LogWarning("생성된 Plane에 Renderer가 없습니다.");
            ShowError("생성된 Plane에 Renderer가 없습니다.");
        }

        // PropMover 컴포넌트 추가
        if (newPlane.GetComponent<PropMover>() == null)
        {
            newPlane.AddComponent<PropMover>();
        }

        // 리스트에 추가
        backgroundPlanes.Add(newPlane);

        // UI 리스트에 추가
        AddPlaneToUI(newPlane);
    }

    /// <summary>
    /// "새 배경 만들기" 버튼 클릭 시 호출되는 메서드
    /// </summary>
    public void OnClickCreateNewBackground()
    {
        Debug.Log("Create New Background button clicked");
        CreateNewBackgroundPlane();
    }

    /// <summary>
    /// "이미지 열기" 버튼 클릭 시 호출되는 메서드
    /// </summary>
    public void OnClickOpen()
    {
        Debug.Log("Open File button clicked");
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            if (selectedPlane != null)
            {
                LoadImage(paths[0], selectedPlane);
            }
            else
            {
                Debug.LogWarning("적용할 배경 Plane이 선택되지 않았습니다. 먼저 Plane을 선택하세요.");
                ShowError("적용할 배경 Plane이 선택되지 않았습니다. 먼저 Plane을 선택하세요.");
            }
        }
    }

    /// <summary>
    /// 선택된 Plane에 이미지를 적용
    /// </summary>
    private void LoadImage(string filePath, GameObject plane)
    {
        try
        {
            var fileData = System.IO.File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(fileData))
            {
                ShowError("이미지를 로드할 수 없습니다.");
                return;
            }

            Texture2D flippedTexture = FlipTextureVertically(texture);

            Renderer renderer = plane.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = flippedTexture;

                // 사용자가 스케일을 조정할 수 있도록 Plane의 스케일을 변경하지 않습니다.
            }
            else
            {
                ShowError("Plane의 Renderer가 존재하지 않습니다.");
            }
        }
        catch (System.Exception ex)
        {
            ShowError($"이미지 로드 중 오류 발생: {ex.Message}");
            Debug.LogError($"이미지 로드 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// 텍스처 수직 뒤집기
    /// </summary>
    private Texture2D FlipTextureVertically(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);
        Color[] originalPixels = original.GetPixels();
        Color[] flippedPixels = new Color[originalPixels.Length];

        for (int y = 0; y < original.height; y++)
        {
            for (int x = 0; x < original.width; x++)
            {
                flippedPixels[y * original.width + x] = originalPixels[(original.height - y - 1) * original.width + x];
            }
        }

        flipped.SetPixels(flippedPixels);
        flipped.Apply();
        return flipped;
    }

    void Update()
    {
        // UI 위에서 마우스 클릭 시 Plane 선택 로직 무시
        if (Input.GetMouseButtonDown(0))
        {
            // 포인터가 UI 위에 있으면 Plane 선택 Raycast를 안 함
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // UI 클릭이므로 Plane 선택 로직을 스킵
            }
            else
            {
                // UI가 아닌 곳 클릭 시 Plane 선택 시도
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject clickedPlane = hit.collider.gameObject;
                    if (backgroundPlanes.Contains(clickedPlane))
                    {
                        SelectPlane(clickedPlane);
                    }
                }
            }
        }

        // 스케일 조절 로직
        if (selectedPlane != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    // Ctrl + 마우스 휠: 가로 크기만 변경
                    Vector3 currentScale = selectedPlane.transform.localScale;
                    currentScale.x += scroll * scaleSpeedX;
                    selectedPlane.transform.localScale = new Vector3(
                        Mathf.Max(currentScale.x, 0.1f), // 최소 스케일 제한
                        currentScale.y,
                        currentScale.z
                    );
                }
                else
                {
                    // 그냥 마우스 휠: 전체 크기 비율 변경
                    Vector3 currentScale = selectedPlane.transform.localScale;
                    currentScale += Vector3.one * scroll * scaleSpeed;
                    // 최소 스케일 제한
                    currentScale.x = Mathf.Max(currentScale.x, 0.1f);
                    currentScale.y = Mathf.Max(currentScale.y, 0.1f);
                    currentScale.z = Mathf.Max(currentScale.z, 0.1f);
                    selectedPlane.transform.localScale = currentScale;
                }
            }
        }

        // Esc 키로 Plane 선택 해제
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectPlane();
        }
    }

    /// <summary>
    /// Plane 리스트 UI에 Plane 추가
    /// </summary>
    private void AddPlaneToUI(GameObject plane)
    {
        if (planeListContent == null || planeListItemPrefab == null)
        {
            Debug.LogError("Plane List UI 요소가 할당되지 않았습니다!");
            ShowError("Plane List UI 요소가 할당되지 않았습니다!");
            return;
        }

        GameObject listItem = Instantiate(planeListItemPrefab, planeListContent.transform);
        Text buttonText = listItem.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = plane.name;
        }

        Button button = listItem.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => SelectPlane(plane));
        }

        // Plane과 UI 리스트 아이템 간의 매핑을 위해 추가적인 로직을 구현할 수 있습니다.
    }

    /// <summary>
    /// Plane 선택
    /// </summary>
    public void SelectPlane(GameObject plane)
    {
        if (selectedPlane != null)
        {
            RemoveHighlight(selectedPlane);
        }

        selectedPlane = plane;
        HighlightPlane(selectedPlane);
        Debug.Log($"Plane '{selectedPlane.name}'이(가) 선택되었습니다.");
    }

    /// <summary>
    /// Plane 선택 해제
    /// </summary>
    public void DeselectPlane()
    {
        if (selectedPlane != null)
        {
            RemoveHighlight(selectedPlane);
            Debug.Log($"Plane '{selectedPlane.name}'이(가) 선택 해제되었습니다.");
            selectedPlane = null;
        }
    }

    /// <summary>
    /// Plane 하이라이트
    /// </summary>
    private void HighlightPlane(GameObject plane)
    {
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (!renderer.material.HasProperty("_OriginalColor"))
            {
                renderer.material.SetColor("_OriginalColor", renderer.material.color);
            }
            renderer.material.color = Color.yellow;
        }
    }

    /// <summary>
    /// Plane 하이라이트 제거
    /// </summary>
    private void RemoveHighlight(GameObject plane)
    {
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null && renderer.material.HasProperty("_OriginalColor"))
        {
            Color originalColor = renderer.material.GetColor("_OriginalColor");
            renderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// "Plane 삭제" 버튼 클릭 시 호출되는 메서드
    /// </summary>
    public void OnClickDeleteSelectedPlane()
    {
        if (selectedPlane != null)
        {
            Debug.Log($"Plane '{selectedPlane.name}'을(를) 삭제합니다.");

            RemovePlaneFromUI(selectedPlane);
            backgroundPlanes.Remove(selectedPlane);
            Destroy(selectedPlane);
            selectedPlane = null;
        }
        else
        {
            Debug.LogWarning("삭제할 Plane이 선택되지 않았습니다.");
            ShowError("삭제할 Plane이 선택되지 않았습니다.");
        }
    }

    /// <summary>
    /// UI 리스트에서 해당 Plane 제거
    /// </summary>
    private void RemovePlaneFromUI(GameObject plane)
    {
        foreach (Transform child in planeListContent.transform)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                Text buttonText = button.GetComponentInChildren<Text>();
                if (buttonText != null && buttonText.text == plane.name)
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 에러 메시지 표시
    /// </summary>
    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            StartCoroutine(ClearErrorMessageAfterDelay(3f));
        }
        Debug.LogError(message);
    }

    /// <summary>
    /// 일정 시간 후 에러 메시지 초기화
    /// </summary>
    private System.Collections.IEnumerator ClearErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorMessageText != null)
        {
            errorMessageText.text = "";
        }
    }
}
