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
    public Camera customizationCamera;          // Ŀ���͸���¡ ī�޶�

    [Header("Scene References")]
    public Transform mainSceneSpawnPoint;       // (��� ���ο� ���� �����Ӱ�)

    [Header("UI Elements")]
    public Dropdown raceDropdown;
    public Dropdown wardrobeDropdown;
    public Button createCharacterButton;
    // public Button applyWardrobeButton;      // ApplyWardrobeButton ����
    public Button clearWardrobeButton;
    public Button exportButton;

    // (����) Export �� ���� ĳ���͸� ������ ���� �ʹٸ� ���
    [Header("Exported Characters Positioning")]
    public Vector3 exportPositionOffset = new Vector3(2.0f, 0, 0);
    private int exportedCharacterCount = 0;

    // UMA ���� ���� ����
    private List<DynamicCharacterAvatar> allCharacters = new List<DynamicCharacterAvatar>();
    private DynamicCharacterAvatar currentAvatar = null;
    private string currentRace = "HumanMale";    // �ʱⰪ
    private Dictionary<string, RaceData> raceOptions = new Dictionary<string, RaceData>();  // RaceName -> RaceData
    private Dictionary<string, string> wardrobeRecipes = new Dictionary<string, string>();  // DisplayName -> RecipeName

    void Start()
    {
        // �ʱ� ī�޶� ����
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (customizationCamera != null) customizationCamera.gameObject.SetActive(true);

        // �ʼ� üũ
        CheckRequiredReferences();

        // UI �̺�Ʈ ����
        createCharacterButton.onClick.AddListener(OnCreateCharacterButton);
        clearWardrobeButton.onClick.AddListener(ClearWardrobe);
        exportButton.onClick.AddListener(ExportCharacter);

        raceDropdown.onValueChanged.AddListener(OnRaceChanged);
        // Wardrobe ��Ӵٿ�: ���� �� �ٷ� ApplyWardrobe
        wardrobeDropdown.onValueChanged.AddListener(delegate { ApplyWardrobe(); });

        // ��Ӵٿ� �ʱ�ȭ
        InitializeRaceDropdown();
        InitializeWardrobeDropdown();

        Debug.Log("FullCharacterCustomizer Start() �Ϸ�");
    }

    void Update()
    {
        // �ʿ� �� �ٸ� ���� �߰� ����
        // (���콺 �巡��/ȸ�� ����� CharacterControllerCombined�� �����ϹǷ� ���⼭�� ����)
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

        // (4) Collider �߰� (BuildCharacter ���Ŀ� �߰�)
        if (newAvatar.GetComponent<Collider>() == null)
        {
            Collider collider = newAvatar.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            Debug.Log($"Collider�� ���� {newAvatar.name}�� BoxCollider�� �߰��߽��ϴ�.");
        }

        // (5) ���� Ŀ���͸���¡ ��� ����
        currentAvatar = newAvatar;
        allCharacters.Add(newAvatar);

        Debug.Log($"�� ĳ���� ���� �Ϸ�: {newAvatar.name} (���� ĳ���� ��: {allCharacters.Count})");
    }

    /// <summary>
    /// ���̽��� ���� �ӿ��� ������� �ִ� �޼���
    /// (HumanMale, HumanFemale)
    /// </summary>
    private void AssignUnderwear(DynamicCharacterAvatar avatar, string raceName)
    {
        // ����� �α�: ���� Race �̸�
        Debug.Log($"AssignUnderwear ȣ���. Race: {raceName}");

        try
        {
            switch (raceName)
            {
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
    /// "Apply Wardrobe" �� ��Ӵٿ�� ������ �� ����
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

        currentAvatar.SetSlot(wardrobeRecipe.wardrobeSlot, wardrobeRecipe.name);
        currentAvatar.BuildCharacter();
        Debug.Log($"Wardrobe �����: {selected}");
    }

    /// <summary>
    /// "Clear Wardrobe" �� ���� Wardrobe ��� ����
    /// </summary>
    void ClearWardrobe()
    {
        if (currentAvatar == null) return;

        currentAvatar.ClearSlots();
        currentAvatar.BuildCharacter();

        Debug.Log("Wardrobe ��� ���ŵ�.");
    }

    /// <summary>
    /// "Export" ��ư �� Ŀ���͸���¡�� ���� ĳ���͸� ���� ī�޶� ��ġ�� �̵� + CharacterControllerCombined ����
    /// </summary>
    void ExportCharacter()
    {
        if (currentAvatar == null)
        {
            Debug.LogError("������ ĳ���Ͱ� �����ϴ�.");
            return;
        }

        // (1) ĳ���͸� ���� ī�޶� '��'�� ��ġ
        Vector3 newPosition = mainCamera.transform.position + mainCamera.transform.forward * 2.0f;
        currentAvatar.transform.position = newPosition;

        // (2) ī�޶��� ����(Y) ȸ���� ��������
        //     Camera.main.transform.eulerAngles.y�� ���ͼ� ���
        float cameraY = mainCamera.transform.eulerAngles.y;
        currentAvatar.transform.rotation = Quaternion.Euler(0f, cameraY, 0f);

        // Collider�� ������ �߰�
        if (currentAvatar.GetComponent<Collider>() == null)
        {
            Collider collider = currentAvatar.gameObject.AddComponent<BoxCollider>();
            collider.isTrigger = false;
            Debug.Log($"Collider�� ���� {currentAvatar.name}�� BoxCollider�� �߰��߽��ϴ�.");
        }

        // (3) ĳ���� ���� ��ũ��Ʈ(CharacterControllerCombined) ����
        if (currentAvatar.GetComponent<CharacterControllerCombined>() == null)
        {
            currentAvatar.gameObject.AddComponent<CharacterControllerCombined>();
        }

        // ī�޶� ��ȯ
        if (customizationCamera != null) customizationCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        Debug.Log($"{currentAvatar.name} ĳ���͸� ���� ī�޶� ������ �����½��ϴ�. ��ġ: {newPosition}");
    }
}

    // ---------------------------------------------------------------------------
    // CharacterControllerCombined
    //  - ���� Ŭ�� �� �巡��
    //  - ���� Ŭ�� �� ȸ��
    //  - Delete Ű �� ĳ���� ����
    // ---------------------------------------------------------------------------
    public class CharacterControllerCombined : MonoBehaviour
{
    // ���� Ŭ�� ���� ����
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // ���� Ŭ�� �ν� �ð� (��)

    // ȸ�� ���� ����
    private bool isRotating = false;
    private Vector3 lastMousePosition;
    [Range(0.1f, 10f)]
    public float rotationSpeed = 1f; // ȸ�� �ӵ� ����

    // �巡�� ���� ����
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Plane dragPlane;

    private Camera mainCamera;

    void Start()
    {
        // ���� ī�޶� ã�� (Camera.main)
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("���� ī�޶� �����ϴ�. Camera.main�� Ȯ���ϼ���.");
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

        // Delete Ű �Է� ����
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
                // Ŭ���� ��ü�� �ڱ� �ڽ����� Ȯ��
                if (hit.transform == this.transform)
                {
                    // ���� Ŭ�� Ȯ��
                    if (timeSinceLastClick <= doubleClickThreshold)
                    {
                        // ���� Ŭ�� �� ȸ�� ���
                        isRotating = true;
                        isDragging = false;
                        lastMousePosition = Input.mousePosition;
                        return;
                    }
                    else
                    {
                        // ���� Ŭ�� �� �巡�� ���
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

        // ���� ȸ��
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

            // Y���� ���� (���ϸ� �����Ӱ� ����)
            targetPosition.y = transform.position.y;

            transform.position = targetPosition;
        }
    }

    void DestroyCharacter()
    {
        Debug.Log($"{gameObject.name}��(��) �����Ǿ����ϴ�.");
        Destroy(gameObject);
    }
}

