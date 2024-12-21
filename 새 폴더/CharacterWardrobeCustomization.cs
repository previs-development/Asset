using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;
using System.Linq;

public class FullCharacterCustomizer : MonoBehaviour
{
    [Header("UMA Setup")]
    public DynamicCharacterAvatar avatarPrefab; // UMA 캐릭터 프리팹
    public Transform workspace;                 // 새 캐릭터 생성 위치(부모)

    [Header("Cameras")]
    public Camera mainCamera;                   // 메인 카메라
    public Camera customizationCamera;          // 커스터마이징 카메라

    [Header("Scene References")]
    public Transform mainSceneSpawnPoint;       // (사용 여부에 따라 자유롭게)

    [Header("UI Elements")]
    public Dropdown raceDropdown;
    public Dropdown wardrobeDropdown;
    public Button createCharacterButton;
    // public Button applyWardrobeButton;      // ApplyWardrobeButton 제거
    public Button clearWardrobeButton;
    public Button exportButton;

    // (기존) Export 시 여러 캐릭터를 나란히 놓고 싶다면 사용
    [Header("Exported Characters Positioning")]
    public Vector3 exportPositionOffset = new Vector3(2.0f, 0, 0);
    private int exportedCharacterCount = 0;

    // UMA 관련 내부 변수
    private List<DynamicCharacterAvatar> allCharacters = new List<DynamicCharacterAvatar>();
    private DynamicCharacterAvatar currentAvatar = null;
    private string currentRace = "HumanMale";    // 초기값
    private Dictionary<string, RaceData> raceOptions = new Dictionary<string, RaceData>();  // RaceName -> RaceData
    private Dictionary<string, string> wardrobeRecipes = new Dictionary<string, string>();  // DisplayName -> RecipeName

    void Start()
    {
        // 초기 카메라 설정
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (customizationCamera != null) customizationCamera.gameObject.SetActive(true);

        // 필수 체크
        CheckRequiredReferences();

        // UI 이벤트 연결
        createCharacterButton.onClick.AddListener(OnCreateCharacterButton);
        clearWardrobeButton.onClick.AddListener(ClearWardrobe);
        exportButton.onClick.AddListener(ExportCharacter);

        raceDropdown.onValueChanged.AddListener(OnRaceChanged);
        // Wardrobe 드롭다운: 선택 시 바로 ApplyWardrobe
        wardrobeDropdown.onValueChanged.AddListener(delegate { ApplyWardrobe(); });

        // 드롭다운 초기화
        InitializeRaceDropdown();
        InitializeWardrobeDropdown();

        Debug.Log("FullCharacterCustomizer Start() 완료");
    }

    void Update()
    {
        // 필요 시 다른 로직 추가 가능
        // (마우스 드래그/회전 기능은 CharacterControllerCombined가 전담하므로 여기서는 제거)
    }

    /// <summary>
    /// 필수 참조가 빠져있는지 검사
    /// </summary>
    void CheckRequiredReferences()
    {
        if (!avatarPrefab || !workspace || !mainSceneSpawnPoint ||
            !mainCamera || !customizationCamera ||
            !raceDropdown || !wardrobeDropdown ||
            !createCharacterButton || /* !applyWardrobeButton || */ !clearWardrobeButton || !exportButton)
        {
            Debug.LogError("필수 오브젝트나 컴포넌트가 할당되지 않았습니다.");
        }
    }

    /// <summary>
    /// "Create Character" 버튼 클릭 시
    /// - 기존 캐릭터 유지
    /// - 커스터마이징 카메라 활성화
    /// - 새 캐릭터 생성 후 레이스별 속옷 착용
    /// </summary>
    void OnCreateCharacterButton()
    {
        // (1) 메인 카메라 끄고, 커스터마이징 카메라 켜기
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (customizationCamera != null) customizationCamera.gameObject.SetActive(true);

        // (2) 새 캐릭터 생성
        var newAvatar = Instantiate(avatarPrefab, workspace);
        newAvatar.transform.localPosition = Vector3.zero;
        newAvatar.transform.localRotation = Quaternion.identity;

        // 레이스 변경
        if (raceOptions.ContainsKey(currentRace))
            newAvatar.ChangeRace(raceOptions[currentRace]);
        newAvatar.BuildCharacter();

        // (3) 레이스별 속옷 착용
        AssignUnderwear(newAvatar, currentRace);

        // (4) Collider 추가 (BuildCharacter 이후에 추가)
        if (newAvatar.GetComponent<Collider>() == null)
        {
            Collider collider = newAvatar.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            Debug.Log($"Collider가 없는 {newAvatar.name}에 BoxCollider를 추가했습니다.");
        }

        // (5) 현재 커스터마이징 대상 지정
        currentAvatar = newAvatar;
        allCharacters.Add(newAvatar);

        Debug.Log($"새 캐릭터 생성 완료: {newAvatar.name} (현재 캐릭터 수: {allCharacters.Count})");
    }

    /// <summary>
    /// 레이스에 맞춰 속옷을 착용시켜 주는 메서드
    /// (HumanMale, HumanFemale)
    /// </summary>
    private void AssignUnderwear(DynamicCharacterAvatar avatar, string raceName)
    {
        // 디버깅 로그: 현재 Race 이름
        Debug.Log($"AssignUnderwear 호출됨. Race: {raceName}");

        try
        {
            switch (raceName)
            {
                case "HumanMale":
                    avatar.SetSlot("Underwear", "MaleDefaultUnderwear");
                    Debug.Log("HumanMale용 속옷 설정 완료.");
                    break;

                case "HumanFemale":
                    avatar.SetSlot("Underwear", "FemaleUndies");
                    Debug.Log("HumanFemale용 속옷 설정 완료.");
                    break;

                default:
                    Debug.LogWarning($"AssignUnderwear: {raceName}에 대한 속옷 설정이 정의되지 않았습니다.");
                    break;
            }

            // 캐릭터 빌드
            avatar.BuildCharacter();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"AssignUnderwear 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// Race Dropdown 초기화 & 데이터 수집
    /// </summary>
    void InitializeRaceDropdown()
    {
        raceDropdown.options.Clear();
        raceOptions.Clear();

        var assetIndexer = UMAAssetIndexer.Instance;
        if (assetIndexer == null)
        {
            Debug.LogError("UMAAssetIndexer를 찾을 수 없습니다.");
            return;
        }

        // 모든 RaceData 가져오기
        var allRaces = assetIndexer.GetAllAssets<RaceData>();
        foreach (var race in allRaces)
        {
            // 원하는 레이스만 사용하고 싶다면 여기서 필터링
            if (IsAllowedRace(race.raceName))
            {
                raceOptions[race.raceName] = race;
                raceDropdown.options.Add(new Dropdown.OptionData(race.raceName));
            }
        }

        // 첫 드롭다운 값
        if (raceDropdown.options.Count > 0)
        {
            raceDropdown.value = 0;
            raceDropdown.RefreshShownValue();
            currentRace = raceDropdown.options[0].text;
        }
        else
        {
            Debug.LogWarning("사용 가능한 레이스가 없습니다.");
        }
    }

    /// <summary>
    /// 특정 레이스를 제외하는 필터 (예: 필요 없는 레이스 제외)
    /// </summary>
    bool IsAllowedRace(string raceName)
    {
        // 여기서 제외할 레이스 이름들
        List<string> excluded = new List<string>
        {
            "o3n Female",
            "o3n Male",
            "HumanBoy",
            "HumanGirl",
            "DNAConverterControllerExampleRace",
            "HumanFemaleHigh",
            "Human Female HighPoly_Bare",
            "Elf Female",
            "Elf Male",
            "HumanFemaleHighPoly",
            "HumanMaleHighPoly",
            "ToonFemale",
            "SkyCar"
        };
        return !excluded.Contains(raceName);
    }

    /// <summary>
    /// RaceDropdown 값이 변경되면 호출
    /// </summary>
    void OnRaceChanged(int index)
    {
        if (index < 0 || index >= raceDropdown.options.Count)
            return;

        currentRace = raceDropdown.options[index].text;

        // 현재 커스터마이징 중인 캐릭터가 있다면 즉시 레이스 변경 + 속옷 재적용
        if (currentAvatar != null && raceOptions.ContainsKey(currentRace))
        {
            currentAvatar.ChangeRace(raceOptions[currentRace]);
            currentAvatar.BuildCharacter();
            AssignUnderwear(currentAvatar, currentRace);
        }

        // 레이스 변경 시 Wardrobe 목록 재구성
        InitializeWardrobeDropdown();
    }

    /// <summary>
    /// Wardrobe Dropdown 초기화
    /// </summary>
    void InitializeWardrobeDropdown()
    {
        wardrobeDropdown.options.Clear();
        wardrobeRecipes.Clear();

        // 첫 옵션 = None
        wardrobeDropdown.options.Add(new Dropdown.OptionData("None"));

        var recipes = UMAAssetIndexer.Instance.GetAllAssets<UMAWardrobeRecipe>();
        foreach (var recipe in recipes)
        {
            // 현재 레이스와 호환되는 Recipe만 추가
            if (recipe.compatibleRaces.Contains(currentRace))
            {
                if (!wardrobeRecipes.ContainsKey(recipe.DisplayValue))
                {
                    // DisplayValue: 드롭다운에 표시되는 이름
                    // name: 실제 Recipe Asset 이름
                    wardrobeRecipes[recipe.DisplayValue] = recipe.name;
                }
            }
        }

        // 드롭다운 채우기
        foreach (var item in wardrobeRecipes)
        {
            wardrobeDropdown.options.Add(new Dropdown.OptionData(item.Key));
        }

        wardrobeDropdown.value = 0;
        wardrobeDropdown.RefreshShownValue();
    }

    /// <summary>
    /// "Apply Wardrobe" → 드롭다운에서 선택한 옷 적용
    /// </summary>
    void ApplyWardrobe()
    {
        if (currentAvatar == null) return;

        int idx = wardrobeDropdown.value;
        string selected = wardrobeDropdown.options[idx].text;

        if (selected == "None")
        {
            ClearWardrobe();
            return;
        }

        if (!wardrobeRecipes.ContainsKey(selected))
        {
            Debug.LogError($"WardrobeRecipe {selected}를 찾을 수 없습니다.");
            return;
        }

        string recipeName = wardrobeRecipes[selected];
        var wardrobeRecipe = UMAAssetIndexer.Instance.GetAsset<UMAWardrobeRecipe>(recipeName);
        if (wardrobeRecipe == null)
        {
            Debug.LogError($"실제 Recipe 로드 실패: {recipeName}");
            return;
        }

        currentAvatar.SetSlot(wardrobeRecipe.wardrobeSlot, wardrobeRecipe.name);
        currentAvatar.BuildCharacter();
        Debug.Log($"Wardrobe 적용됨: {selected}");
    }

    /// <summary>
    /// "Clear Wardrobe" → 현재 Wardrobe 모두 제거
    /// </summary>
    void ClearWardrobe()
    {
        if (currentAvatar == null) return;

        currentAvatar.ClearSlots();
        currentAvatar.BuildCharacter();

        Debug.Log("Wardrobe 모두 제거됨.");
    }

    /// <summary>
    /// "Export" 버튼 → 커스터마이징이 끝난 캐릭터를 메인 카메라 위치로 이동 + CharacterControllerCombined 부착
    /// </summary>
    void ExportCharacter()
    {
        if (currentAvatar == null)
        {
            Debug.LogError("내보낼 캐릭터가 없습니다.");
            return;
        }

        // (1) 캐릭터를 메인 카메라 '앞'에 배치
        Vector3 newPosition = mainCamera.transform.position + mainCamera.transform.forward * 2.0f;
        currentAvatar.transform.position = newPosition;

        // (2) 카메라의 수평(Y) 회전만 가져오기
        //     Camera.main.transform.eulerAngles.y만 떼와서 사용
        float cameraY = mainCamera.transform.eulerAngles.y;
        currentAvatar.transform.rotation = Quaternion.Euler(0f, cameraY, 0f);

        // Collider가 없으면 추가
        if (currentAvatar.GetComponent<Collider>() == null)
        {
            Collider collider = currentAvatar.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            Debug.Log($"Collider가 없는 {currentAvatar.name}에 BoxCollider를 추가했습니다.");
        }

        // (3) 캐릭터 제어 스크립트(CharacterControllerCombined) 부착
        if (currentAvatar.GetComponent<CharacterControllerCombined>() == null)
        {
            currentAvatar.gameObject.AddComponent<CharacterControllerCombined>();
        }

        // 카메라 전환
        if (customizationCamera != null) customizationCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        Debug.Log($"{currentAvatar.name} 캐릭터를 메인 카메라 앞으로 내보냈습니다. 위치: {newPosition}");
    }
}

    // ---------------------------------------------------------------------------
    // CharacterControllerCombined
    //  - 단일 클릭 → 드래그
    //  - 더블 클릭 → 회전
    //  - Delete 키 → 캐릭터 삭제
    // ---------------------------------------------------------------------------
    public class CharacterControllerCombined : MonoBehaviour
{
    // 더블 클릭 감지 변수
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // 더블 클릭 인식 시간 (초)

    // 회전 제어 변수
    private bool isRotating = false;
    private Vector3 lastMousePosition;
    [Range(0.1f, 10f)]
    public float rotationSpeed = 1f; // 회전 속도 조절

    // 드래그 제어 변수
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Plane dragPlane;

    private Camera mainCamera;

    void Start()
    {
        // 메인 카메라 찾기 (Camera.main)
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라가 없습니다. Camera.main을 확인하세요.");
        }
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

        // Delete 키 입력 감지
        if ((isRotating || isDragging) && Input.GetKeyDown(KeyCode.Delete))
        {
            DestroyCharacter();
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
                // 클릭한 객체가 자기 자신인지 확인
                if (hit.transform == this.transform)
                {
                    // 더블 클릭 확인
                    if (timeSinceLastClick <= doubleClickThreshold)
                    {
                        // 더블 클릭 → 회전 모드
                        isRotating = true;
                        isDragging = false;
                        lastMousePosition = Input.mousePosition;
                        return;
                    }
                    else
                    {
                        // 단일 클릭 → 드래그 모드
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

    void RotateCharacter()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        float deltaX = currentMousePosition.x - lastMousePosition.x;

        // 수평 회전
        transform.Rotate(0, deltaX * rotationSpeed, 0, Space.World);

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

            // Y축은 고정 (원하면 자유롭게 조절)
            targetPosition.y = transform.position.y;

            transform.position = targetPosition;
        }
    }

    void DestroyCharacter()
    {
        Debug.Log($"{gameObject.name}이(가) 삭제되었습니다.");
        Destroy(gameObject);
    }
}

