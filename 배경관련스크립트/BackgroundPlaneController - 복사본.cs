using UnityEngine;
using SFB; // Standalone File Browser 네임스페이스 추가
using System.Collections.Generic;
using UnityEngine.UI;

public class BackgroundPlaneController : MonoBehaviour
{
    [Header("Background Plane Settings")]
    public GameObject backgroundPlanePrefab; // 배경 Plane의 Prefab
    public Transform planesParent; // 모든 배경 Plane을 담을 부모 Transform

    [Header("UI Elements")]
    public GameObject planeListContent; // Plane 리스트를 담을 ScrollView의 Content
    public GameObject planeListItemPrefab; // Plane 리스트 아이템의 Prefab (버튼 등)

    [Header("Error UI")]
    public Text errorMessageText; // 에러 메시지를 표시할 UI Text

    private List<GameObject> backgroundPlanes = new List<GameObject>(); // 생성된 배경 Plane 리스트
    private List<Material> backgroundMaterials = new List<Material>(); // 각 Plane의 Material 리스트

    private GameObject selectedPlane = null; // 현재 선택된 Plane

    private int planeCount = 0; // Plane 이름을 위한 카운터

    // 설정 가능한 기본 스케일 값
    public Vector3 defaultPlaneScale = new Vector3(10, 3, 1);
    public float planeSpacing = 5f; // Plane 간의 간격

    void Start()
    {
        // 초기 Plane 생성 (필요 시)
        // CreateNewBackgroundPlane(new Vector3(0, 0, 5), defaultPlaneScale);
    }

    /// <summary>
    /// 새 배경 Plane을 생성하고 리스트에 추가합니다.
    /// </summary>
    /// <param name="position">생성 위치</param>
    /// <param name="scale">스케일</param>
    public void CreateNewBackgroundPlane(Vector3 position, Vector3 scale)
    {
        if (backgroundPlanePrefab == null)
        {
            Debug.LogError("Background Plane Prefab이 할당되지 않았습니다!");
            ShowError("Background Plane Prefab이 할당되지 않았습니다!");
            return;
        }

        // Prefab 인스턴스화
        GameObject newPlane = Instantiate(backgroundPlanePrefab, planesParent);

        // Plane의 위치와 스케일 설정
        // 카메라의 중앙 위치를 기준으로 배경 생성
        Vector3 cameraPosition = Camera.main.transform.position;
        newPlane.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z + 5); // 카메라 앞 Z=5 위치
        newPlane.transform.localScale = new Vector3(1.0f, 2.5f, 0.5f); // 적절한 크기 설정

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
        Debug.Log("Create New Background button clicked"); // 버튼 클릭 확인용 메시지

        // 새 Plane의 위치는 기존 Plane들에 따라 조정 (예: Z축을 기준으로 앞으로)
        Vector3 newPosition = Vector3.zero;
        if (backgroundPlanes.Count > 0)
        {
            // 마지막 Plane의 Z 위치를 기준으로 앞쪽으로 일정 거리 배치
            GameObject lastPlane = backgroundPlanes[backgroundPlanes.Count - 1];
            newPosition = lastPlane.transform.position + new Vector3(0, 0, planeSpacing);
        }
        else
        {
            // 첫 Plane의 기본 위치 설정 (카메라 앞쪽)
            newPosition = new Vector3(0, 0, 5f); // 카메라 위치가 (0,0,0)이라 가정
        }

        // 새 Plane 생성
        CreateNewBackgroundPlane(newPosition, defaultPlaneScale);
    }

    /// <summary>
    /// "이미지 열기" 버튼 클릭 시 호출되는 메서드
    /// </summary>
    public void OnClickOpen()
    {
        Debug.Log("Open File button clicked"); // 버튼 클릭 확인용 메시지
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            // 현재 활성화된(선택된) Plane에 이미지를 로드
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
    /// 선택된 Plane에 이미지를 로드하고 적용합니다.
    /// </summary>
    /// <param name="filePath">이미지 파일 경로</param>
    /// <param name="plane">적용할 Plane</param>
    private void LoadImage(string filePath, GameObject plane)
    {
        try
        {
            var fileData = System.IO.File.ReadAllBytes(filePath); // 파일의 바이트 데이터를 읽어옴
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(fileData))
            {
                ShowError("이미지를 로드할 수 없습니다.");
                return;
            }

            // 텍스처를 수직으로 뒤집기
            Texture2D flippedTexture = FlipTextureVertically(texture);

            Renderer renderer = plane.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = flippedTexture; // Plane의 Material에 텍스처 적용

                // Plane의 스케일을 이미지 비율에 맞게 조정
                float aspectRatio = (float)flippedTexture.width / flippedTexture.height;
                plane.transform.localScale = new Vector3(defaultPlaneScale.x * aspectRatio, defaultPlaneScale.y, defaultPlaneScale.z); // 예시: 기본 크기에 비율 적용
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
    /// 텍스처를 수직으로 뒤집는 메서드
    /// </summary>
    /// <param name="original">원본 텍스처</param>
    /// <returns>수직으로 뒤집힌 텍스처</returns>
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
        // 선택된 Plane에 대해서만 입력 처리
        if (selectedPlane != null)
        {
            HandlePlaneControls(selectedPlane);
        }

        // 키보드 입력으로 Plane 선택 해제 (예: Esc 키)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectPlane();
        }

        // 마우스 클릭으로 Plane 선택 (추가 기능)
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼 클릭 시
        {
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

    /// <summary>
    /// Plane 리스트 UI에 Plane을 추가합니다.
    /// </summary>
    /// <param name="plane">추가할 Plane</param>
    private void AddPlaneToUI(GameObject plane)
    {
        if (planeListContent == null || planeListItemPrefab == null)
        {
            Debug.LogError("Plane List UI 요소가 할당되지 않았습니다!");
            ShowError("Plane List UI 요소가 할당되지 않았습니다!");
            return;
        }

        // Plane 리스트 아이템 인스턴스화
        GameObject listItem = Instantiate(planeListItemPrefab, planeListContent.transform);

        // 리스트 아이템의 버튼 텍스트 설정 (예: Plane 이름)
        Text buttonText = listItem.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = plane.name;
        }

        // 버튼 클릭 이벤트 추가
        Button button = listItem.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => SelectPlane(plane));
        }
    }

    /// <summary>
    /// Plane을 선택합니다.
    /// </summary>
    /// <param name="plane">선택할 Plane</param>
    public void SelectPlane(GameObject plane)
    {
        if (selectedPlane != null)
        {
            // 이전에 선택된 Plane의 하이라이트 해제
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
    /// Plane을 하이라이트하여 선택되었음을 시각적으로 표시합니다.
    /// </summary>
    /// <param name="plane">하이라이트할 Plane</param>
    private void HighlightPlane(GameObject plane)
    {
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 원래 색상을 저장
            if (!renderer.material.HasProperty("_OriginalColor"))
            {
                renderer.material.SetColor("_OriginalColor", renderer.material.color);
            }

            // 색상 변경으로 하이라이트
            renderer.material.color = Color.yellow;
        }
    }

    /// <summary>
    /// Plane의 하이라이트를 제거합니다.
    /// </summary>
    /// <param name="plane">하이라이트 제거할 Plane</param>
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

            // UI 리스트에서 삭제
            RemovePlaneFromUI(selectedPlane);

            // Plane 삭제
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
    /// Plane 리스트 UI에서 Plane을 제거합니다.
    /// </summary>
    /// <param name="plane">제거할 Plane</param>
    private void RemovePlaneFromUI(GameObject plane)
    {
        // Plane 리스트 아이템 중 Plane과 연관된 것을 찾아 삭제
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
    /// 에러 메시지를 UI에 표시합니다.
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            // 일정 시간 후 에러 메시지 초기화
            StartCoroutine(ClearErrorMessageAfterDelay(3f));
        }
        Debug.LogError(message);
    }

    /// <summary>
    /// 일정 시간 후 에러 메시지를 초기화하는 코루틴
    /// </summary>
    /// <param name="delay">지연 시간(초)</param>
    /// <returns></returns>
    private System.Collections.IEnumerator ClearErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorMessageText != null)
        {
            errorMessageText.text = "";
        }
    }

    /// <summary>
    /// 개별 Plane에 대한 이동 및 회전 제어
    /// </summary>
    /// <param name="plane">제어할 Plane</param>
    private void HandlePlaneControls(GameObject plane)
    {
        // 마우스 휠로 크기 조절
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // Ctrl + 마우스 휠: 가로 크기 조절
            plane.transform.localScale += new Vector3(scroll, 0, 0);
        }
        else
        {
            // 일반 마우스 휠: 전체 크기 조절
            plane.transform.localScale += Vector3.one * scroll;
        }

        // 키보드 방향키로 이동
        float moveSpeed = 5.0f * Time.deltaTime;
        if (Input.GetKey(KeyCode.W)) plane.transform.Translate(Vector3.forward * moveSpeed);
        if (Input.GetKey(KeyCode.S)) plane.transform.Translate(Vector3.back * moveSpeed);
        if (Input.GetKey(KeyCode.A)) plane.transform.Translate(Vector3.left * moveSpeed);
        if (Input.GetKey(KeyCode.D)) plane.transform.Translate(Vector3.right * moveSpeed);

        // 마우스 드래그로 회전
        if (Input.GetMouseButton(1)) // 마우스 오른쪽 버튼 클릭 시
        {
            float rotationSpeed = 100.0f * Time.deltaTime;
            float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;
            plane.transform.Rotate(Vector3.up, -rotationX, Space.World);
            plane.transform.Rotate(Vector3.right, rotationY, Space.Self);
        }
    }
}

