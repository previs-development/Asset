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
    public Camera customizationCamera;         // 커스터마이징 카메라

    [Header("Scene References")]
    public Transform mainSceneSpawnPoint;       // 내보내기(Export) 시 이동할 위치

    [Header("UI Elements")]
    public Dropdown raceDropdown;
    public Dropdown wardrobeDropdown;
    public Button createCharacterButton;
    // public Button applyWardrobeButton;      // ApplyWardrobeButton 제거
    public Button clearWardrobeButton;
    public Button exportButton;

    [Header("Movement for Exported Characters")]
    public float characterMoveSpeed = 3f;       // 캐릭터 이동 속도

    [Header("Exported Characters Positioning")]
    public Vector3 exportPositionOffset = new Vector3(2.0f, 0, 0); // 각 캐릭터 간의 간격
    private int exportedCharacterCount = 0;     // 내보낸 캐릭터 수 추적

    // UMA 관련 내부 변수
    private List<DynamicCharacterAvatar> allCharacters = new List<DynamicCharacterAvatar>();
    private DynamicCharacterAvatar currentAvatar = null;
    private string currentRace = "HumanMale";    // 초기값
    private Dictionary<string, RaceData> raceOptions = new Dictionary<string, RaceData>();  // RaceName -> RaceData
    private Dictionary<string, string> wardrobeRecipes = new Dictionary<string, string>();    // DisplayName -> RecipeName

    // 선택된(Export된) 캐릭터: 마우스로 클릭하면 여기에 할당
    private DynamicCharacterAvatar selectedAvatar = null;

    void Start()
    {
        // 초기 카메라 설정
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (customizationCamera != null) customizationCamera.gameObject.SetActive(true);

        // 필수 체크
        CheckRequiredReferences();

        // UI 이벤트 연결
        createCharacterButton.onClick.AddListener(OnCreateCharacterButton);
        // applyWardrobeButton.onClick.AddListener(ApplyWardrobe); // ApplyWardrobeButton 제거
        clearWardrobeButton.onClick.AddListener(ClearWardrobe);
        exportButton.onClick.AddListener(ExportCharacter);

        raceDropdown.onValueChanged.AddListener(OnRaceChanged);
        wardrobeDropdown.onValueChanged.AddListener(delegate { ApplyWardrobe(); }); // 드롭다운 선택 시 ApplyWardrobe 호출

        // 드롭다운 초기화
        InitializeRaceDropdown();
        InitializeWardrobeDropdown();

        Debug.Log("FullCharacterCustomizer Start() 완료");
    }

    void Update()
    {
        // (1) 메인 카메라가 켜져 있는 상태에서만 마우스 클릭 + WASD 이동 처리
        if (mainCamera != null && mainCamera.gameObject.activeInHierarchy)
        {
            // 1) 마우스 왼클릭으로 캐릭터 선택
            if (Input.GetMouseButtonDown(0))
            {
                SelectAvatarByMouseClick();
            }

            // 2) 선택된 캐릭터를 WASD로 이동
            if (selectedAvatar != null)
            {
                float horizontal = Input.GetAxis("Horizontal"); // A(-1) <-> D(+1)
                float vertical = Input.GetAxis("Vertical");     // W(+1) <-> S(-1)

                Vector3 move = new Vector3(horizontal, 0, vertical)
                               * characterMoveSpeed
                               * Time.deltaTime;

                // 충돌 검사를 간단히 처리하려면 CharacterController 등으로 이동해도 됨
                selectedAvatar.transform.Translate(move, Space.World);
            }
        }
    }

    /// <summary>
    /// 마우스 클릭 → 캐릭터 선택
    /// </summary>
    private void SelectAvatarByMouseClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 캐릭터나 자식 객체에 Collider가 있어야 Raycast에 걸립니다.
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            var clickedAvatar = hit.collider.GetComponentInParent<DynamicCharacterAvatar>();
            if (clickedAvatar != null)
            {
                selectedAvatar = clickedAvatar;
                Debug.Log($"{selectedAvatar.name} 캐릭터를 선택했습니다.");
            }
            else
            {
                Debug.Log("캐릭터가 아닌 객체를 클릭했습니다.");
            }
        }
        else
        {
            Debug.Log("어떤 캐릭터도 클릭되지 않았습니다.");
        }
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

        // (4) 현재 커스터마이징 대상 지정
        currentAvatar = newAvatar;
        allCharacters.Add(newAvatar);

        Debug.Log($"새 캐릭터 생성 완료: {newAvatar.name} (현재 캐릭터 수: {allCharacters.Count})");
    }

    /// <summary>
    /// 레이스에 맞춰 속옷을 착용시켜 주는 메서드
    /// (o3n_female, o3n_maled, HumanMale, HumanFemale)
    /// </summary>
    private void AssignUnderwear(DynamicCharacterAvatar avatar, string raceName)
    {
        // 디버깅 로그: 현재 Race 이름
        Debug.Log($"AssignUnderwear 호출됨. Race: {raceName}");

        try
        {
            switch (raceName)
            {
                case "o3n_Female":
                    avatar.SetSlot("UnderwearTop", "o3n_female_bra_01_recipe");
                    avatar.SetSlot("UnderwearBottom", "o3n_female_undies_01_recipe");
                    Debug.Log("o3n_Female용 속옷 설정 완료.");
                    break;

                case "o3n_Male":
                    avatar.SetSlot("UnderwearBottom", "o3n_male_undies_01_recipe");
                    Debug.Log("o3n_Male용 속옷 설정 완료.");
                    break;

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
    /// "Apply Wardrobe" 버튼 → 선택한 옷 적용
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

        currentAvatar.SetSlot(wardrobeRecipe.wardrobeSlot, recipeName);
        currentAvatar.BuildCharacter();
        Debug.Log($"Wardrobe 적용됨: {selected}");
    }

    /// <summary>
    /// "Clear Wardrobe" 버튼 → 현재 Wardrobe 모두 제거
    /// </summary>
    void ClearWardrobe()
    {
        if (currentAvatar == null) return;

        currentAvatar.ClearSlots();
        currentAvatar.BuildCharacter();

        // 다시 속옷만 착용하게 하려면 아래 호출
        // AssignUnderwear(currentAvatar, currentRace);
    }

    /// <summary>
    /// "Export" 버튼 → 커스터마이징이 끝난 캐릭터를 메인씬으로 이동
    /// </summary>
    void ExportCharacter()
    {
        if (currentAvatar == null)
        {
            Debug.LogError("내보낼 캐릭터가 없습니다.");
            return;
        }

        // 각 캐릭터를 고유한 위치에 배치
        Vector3 newPosition = mainSceneSpawnPoint.position + exportPositionOffset * exportedCharacterCount;
        currentAvatar.transform.position = newPosition;
        currentAvatar.transform.rotation = mainSceneSpawnPoint.rotation;

        // Collider가 없으면 추가
        if (currentAvatar.GetComponent<Collider>() == null)
        {
            Collider collider = currentAvatar.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            Debug.Log($"Collider가 없는 {currentAvatar.name}에 BoxCollider를 추가했습니다.");
        }

        // 캐릭터 수 증가
        exportedCharacterCount++;

        // 카메라 전환: 커스터마이징 카메라 끄고, 메인 카메라 켜기
        if (customizationCamera != null) customizationCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        Debug.Log($"{currentAvatar.name} 캐릭터를 메인씬으로 내보냈습니다. 위치: {newPosition}");
    }
}









