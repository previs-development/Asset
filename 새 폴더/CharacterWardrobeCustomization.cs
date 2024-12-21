using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;
using System.Linq;

public class FullCharacterCustomizer : MonoBehaviour
{
    [Header("UMA Setup")]
    public DynamicCharacterAvatar avatarPrefab; // UMA ĳ���� ������
    public Transform workspace;                 // �� ĳ���� ���� ��ġ(�θ�)

    [Header("Cameras")]
    public Camera mainCamera;                   // ���� ī�޶�
    public Camera customizationCamera;         // Ŀ���͸���¡ ī�޶�

    [Header("Scene References")]
    public Transform mainSceneSpawnPoint;       // ��������(Export) �� �̵��� ��ġ

    [Header("UI Elements")]
    public Dropdown raceDropdown;
    public Dropdown wardrobeDropdown;
    public Button createCharacterButton;
    // public Button applyWardrobeButton;      // ApplyWardrobeButton ����
    public Button clearWardrobeButton;
    public Button exportButton;

    [Header("Movement for Exported Characters")]
    public float characterMoveSpeed = 3f;       // ĳ���� �̵� �ӵ�

    [Header("Exported Characters Positioning")]
    public Vector3 exportPositionOffset = new Vector3(2.0f, 0, 0); // �� ĳ���� ���� ����
    private int exportedCharacterCount = 0;     // ������ ĳ���� �� ����

    // UMA ���� ���� ����
    private List<DynamicCharacterAvatar> allCharacters = new List<DynamicCharacterAvatar>();
    private DynamicCharacterAvatar currentAvatar = null;
    private string currentRace = "HumanMale";    // �ʱⰪ
    private Dictionary<string, RaceData> raceOptions = new Dictionary<string, RaceData>();  // RaceName -> RaceData
    private Dictionary<string, string> wardrobeRecipes = new Dictionary<string, string>();    // DisplayName -> RecipeName

    // ���õ�(Export��) ĳ����: ���콺�� Ŭ���ϸ� ���⿡ �Ҵ�
    private DynamicCharacterAvatar selectedAvatar = null;

    void Start()
    {
        // �ʱ� ī�޶� ����
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (customizationCamera != null) customizationCamera.gameObject.SetActive(true);

        // �ʼ� üũ
        CheckRequiredReferences();

        // UI �̺�Ʈ ����
        createCharacterButton.onClick.AddListener(OnCreateCharacterButton);
        // applyWardrobeButton.onClick.AddListener(ApplyWardrobe); // ApplyWardrobeButton ����
        clearWardrobeButton.onClick.AddListener(ClearWardrobe);
        exportButton.onClick.AddListener(ExportCharacter);

        raceDropdown.onValueChanged.AddListener(OnRaceChanged);
        wardrobeDropdown.onValueChanged.AddListener(delegate { ApplyWardrobe(); }); // ��Ӵٿ� ���� �� ApplyWardrobe ȣ��

        // ��Ӵٿ� �ʱ�ȭ
        InitializeRaceDropdown();
        InitializeWardrobeDropdown();

        Debug.Log("FullCharacterCustomizer Start() �Ϸ�");
    }

    void Update()
    {
        // (1) ���� ī�޶� ���� �ִ� ���¿����� ���콺 Ŭ�� + WASD �̵� ó��
        if (mainCamera != null && mainCamera.gameObject.activeInHierarchy)
        {
            // 1) ���콺 ��Ŭ������ ĳ���� ����
            if (Input.GetMouseButtonDown(0))
            {
                SelectAvatarByMouseClick();
            }

            // 2) ���õ� ĳ���͸� WASD�� �̵�
            if (selectedAvatar != null)
            {
                float horizontal = Input.GetAxis("Horizontal"); // A(-1) <-> D(+1)
                float vertical = Input.GetAxis("Vertical");     // W(+1) <-> S(-1)

                Vector3 move = new Vector3(horizontal, 0, vertical)
                               * characterMoveSpeed
                               * Time.deltaTime;

                // �浹 �˻縦 ������ ó���Ϸ��� CharacterController ������ �̵��ص� ��
                selectedAvatar.transform.Translate(move, Space.World);
            }
        }
    }

    /// <summary>
    /// ���콺 Ŭ�� �� ĳ���� ����
    /// </summary>
    private void SelectAvatarByMouseClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // ĳ���ͳ� �ڽ� ��ü�� Collider�� �־�� Raycast�� �ɸ��ϴ�.
        if (Physics.Raycast(ray, out hit, 1000f))
        {
            var clickedAvatar = hit.collider.GetComponentInParent<DynamicCharacterAvatar>();
            if (clickedAvatar != null)
            {
                selectedAvatar = clickedAvatar;
                Debug.Log($"{selectedAvatar.name} ĳ���͸� �����߽��ϴ�.");
            }
            else
            {
                Debug.Log("ĳ���Ͱ� �ƴ� ��ü�� Ŭ���߽��ϴ�.");
            }
        }
        else
        {
            Debug.Log("� ĳ���͵� Ŭ������ �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// �ʼ� ������ �����ִ��� �˻�
    /// </summary>
    void CheckRequiredReferences()
    {
        if (!avatarPrefab || !workspace || !mainSceneSpawnPoint ||
            !mainCamera || !customizationCamera ||
            !raceDropdown || !wardrobeDropdown ||
            !createCharacterButton || /* !applyWardrobeButton || */ !clearWardrobeButton || !exportButton)
        {
            Debug.LogError("�ʼ� ������Ʈ�� ������Ʈ�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// "Create Character" ��ư Ŭ�� ��
    /// - ���� ĳ���� ����
    /// - Ŀ���͸���¡ ī�޶� Ȱ��ȭ
    /// - �� ĳ���� ���� �� ���̽��� �ӿ� ����
    /// </summary>
    void OnCreateCharacterButton()
    {
        // (1) ���� ī�޶� ����, Ŀ���͸���¡ ī�޶� �ѱ�
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (customizationCamera != null) customizationCamera.gameObject.SetActive(true);

        // (2) �� ĳ���� ����
        var newAvatar = Instantiate(avatarPrefab, workspace);
        newAvatar.transform.localPosition = Vector3.zero;
        newAvatar.transform.localRotation = Quaternion.identity;

        // ���̽� ����
        if (raceOptions.ContainsKey(currentRace))
            newAvatar.ChangeRace(raceOptions[currentRace]);
        newAvatar.BuildCharacter();

        // (3) ���̽��� �ӿ� ����
        AssignUnderwear(newAvatar, currentRace);

        // (4) ���� Ŀ���͸���¡ ��� ����
        currentAvatar = newAvatar;
        allCharacters.Add(newAvatar);

        Debug.Log($"�� ĳ���� ���� �Ϸ�: {newAvatar.name} (���� ĳ���� ��: {allCharacters.Count})");
    }

    /// <summary>
    /// ���̽��� ���� �ӿ��� ������� �ִ� �޼���
    /// (o3n_female, o3n_maled, HumanMale, HumanFemale)
    /// </summary>
    private void AssignUnderwear(DynamicCharacterAvatar avatar, string raceName)
    {
        // ����� �α�: ���� Race �̸�
        Debug.Log($"AssignUnderwear ȣ���. Race: {raceName}");

        try
        {
            switch (raceName)
            {
                case "o3n_Female":
                    avatar.SetSlot("UnderwearTop", "o3n_female_bra_01_recipe");
                    avatar.SetSlot("UnderwearBottom", "o3n_female_undies_01_recipe");
                    Debug.Log("o3n_Female�� �ӿ� ���� �Ϸ�.");
                    break;

                case "o3n_Male":
                    avatar.SetSlot("UnderwearBottom", "o3n_male_undies_01_recipe");
                    Debug.Log("o3n_Male�� �ӿ� ���� �Ϸ�.");
                    break;

                case "HumanMale":
                    avatar.SetSlot("Underwear", "MaleDefaultUnderwear");
                    Debug.Log("HumanMale�� �ӿ� ���� �Ϸ�.");
                    break;

                case "HumanFemale":
                    avatar.SetSlot("Underwear", "FemaleUndies");
                    Debug.Log("HumanFemale�� �ӿ� ���� �Ϸ�.");
                    break;

                default:
                    Debug.LogWarning($"AssignUnderwear: {raceName}�� ���� �ӿ� ������ ���ǵ��� �ʾҽ��ϴ�.");
                    break;
            }

            // ĳ���� ����
            avatar.BuildCharacter();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"AssignUnderwear �� ���� �߻�: {ex.Message}");
        }
    }

    /// <summary>
    /// Race Dropdown �ʱ�ȭ & ������ ����
    /// </summary>
    void InitializeRaceDropdown()
    {
        raceDropdown.options.Clear();
        raceOptions.Clear();

        var assetIndexer = UMAAssetIndexer.Instance;
        if (assetIndexer == null)
        {
            Debug.LogError("UMAAssetIndexer�� ã�� �� �����ϴ�.");
            return;
        }

        // ��� RaceData ��������
        var allRaces = assetIndexer.GetAllAssets<RaceData>();
        foreach (var race in allRaces)
        {
            // ���ϴ� ���̽��� ����ϰ� �ʹٸ� ���⼭ ���͸�
            if (IsAllowedRace(race.raceName))
            {
                raceOptions[race.raceName] = race;
                raceDropdown.options.Add(new Dropdown.OptionData(race.raceName));
            }
        }

        // ù ��Ӵٿ� ��
        if (raceDropdown.options.Count > 0)
        {
            raceDropdown.value = 0;
            raceDropdown.RefreshShownValue();
            currentRace = raceDropdown.options[0].text;
        }
        else
        {
            Debug.LogWarning("��� ������ ���̽��� �����ϴ�.");
        }
    }

    /// <summary>
    /// Ư�� ���̽��� �����ϴ� ���� (��: �ʿ� ���� ���̽� ����)
    /// </summary>
    bool IsAllowedRace(string raceName)
    {
        // ���⼭ ������ ���̽� �̸���
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
    /// RaceDropdown ���� ����Ǹ� ȣ��
    /// </summary>
    void OnRaceChanged(int index)
    {
        if (index < 0 || index >= raceDropdown.options.Count)
            return;

        currentRace = raceDropdown.options[index].text;

        // ���� Ŀ���͸���¡ ���� ĳ���Ͱ� �ִٸ� ��� ���̽� ���� + �ӿ� ������
        if (currentAvatar != null && raceOptions.ContainsKey(currentRace))
        {
            currentAvatar.ChangeRace(raceOptions[currentRace]);
            currentAvatar.BuildCharacter();
            AssignUnderwear(currentAvatar, currentRace);
        }

        // ���̽� ���� �� Wardrobe ��� �籸��
        InitializeWardrobeDropdown();
    }

    /// <summary>
    /// Wardrobe Dropdown �ʱ�ȭ
    /// </summary>
    void InitializeWardrobeDropdown()
    {
        wardrobeDropdown.options.Clear();
        wardrobeRecipes.Clear();

        // ù �ɼ� = None
        wardrobeDropdown.options.Add(new Dropdown.OptionData("None"));

        var recipes = UMAAssetIndexer.Instance.GetAllAssets<UMAWardrobeRecipe>();
        foreach (var recipe in recipes)
        {
            // ���� ���̽��� ȣȯ�Ǵ� Recipe�� �߰�
            if (recipe.compatibleRaces.Contains(currentRace))
            {
                if (!wardrobeRecipes.ContainsKey(recipe.DisplayValue))
                {
                    // DisplayValue: ��Ӵٿ ǥ�õǴ� �̸�
                    // name: ���� Recipe Asset �̸�
                    wardrobeRecipes[recipe.DisplayValue] = recipe.name;
                }
            }
        }

        // ��Ӵٿ� ä���
        foreach (var item in wardrobeRecipes)
        {
            wardrobeDropdown.options.Add(new Dropdown.OptionData(item.Key));
        }

        wardrobeDropdown.value = 0;
        wardrobeDropdown.RefreshShownValue();
    }

    /// <summary>
    /// "Apply Wardrobe" ��ư �� ������ �� ����
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
            Debug.LogError($"WardrobeRecipe {selected}�� ã�� �� �����ϴ�.");
            return;
        }

        string recipeName = wardrobeRecipes[selected];
        var wardrobeRecipe = UMAAssetIndexer.Instance.GetAsset<UMAWardrobeRecipe>(recipeName);
        if (wardrobeRecipe == null)
        {
            Debug.LogError($"���� Recipe �ε� ����: {recipeName}");
            return;
        }

        currentAvatar.SetSlot(wardrobeRecipe.wardrobeSlot, recipeName);
        currentAvatar.BuildCharacter();
        Debug.Log($"Wardrobe �����: {selected}");
    }

    /// <summary>
    /// "Clear Wardrobe" ��ư �� ���� Wardrobe ��� ����
    /// </summary>
    void ClearWardrobe()
    {
        if (currentAvatar == null) return;

        currentAvatar.ClearSlots();
        currentAvatar.BuildCharacter();

        // �ٽ� �ӿʸ� �����ϰ� �Ϸ��� �Ʒ� ȣ��
        // AssignUnderwear(currentAvatar, currentRace);
    }

    /// <summary>
    /// "Export" ��ư �� Ŀ���͸���¡�� ���� ĳ���͸� ���ξ����� �̵�
    /// </summary>
    void ExportCharacter()
    {
        if (currentAvatar == null)
        {
            Debug.LogError("������ ĳ���Ͱ� �����ϴ�.");
            return;
        }

        // �� ĳ���͸� ������ ��ġ�� ��ġ
        Vector3 newPosition = mainSceneSpawnPoint.position + exportPositionOffset * exportedCharacterCount;
        currentAvatar.transform.position = newPosition;
        currentAvatar.transform.rotation = mainSceneSpawnPoint.rotation;

        // Collider�� ������ �߰�
        if (currentAvatar.GetComponent<Collider>() == null)
        {
            Collider collider = currentAvatar.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            Debug.Log($"Collider�� ���� {currentAvatar.name}�� BoxCollider�� �߰��߽��ϴ�.");
        }

        // ĳ���� �� ����
        exportedCharacterCount++;

        // ī�޶� ��ȯ: Ŀ���͸���¡ ī�޶� ����, ���� ī�޶� �ѱ�
        if (customizationCamera != null) customizationCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        Debug.Log($"{currentAvatar.name} ĳ���͸� ���ξ����� �����½��ϴ�. ��ġ: {newPosition}");
    }
}









