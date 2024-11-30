using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;

public class CharacterWardrobeCustomization : MonoBehaviour
{
    public DynamicCharacterAvatar avatar;  // UMA ĳ���� ����
    public Dropdown wardrobeDropdown;      // �ǻ� ���� ��Ӵٿ�
    public Dropdown raceDropdown;          // Race ���� ��Ӵٿ�
    public Button applyButton;             // �ǻ� ���� ��ư
    public Button clearButton;             // �ǻ� ���� ��ư

    private Dictionary<string, string> wardrobeRecipes = new Dictionary<string, string>(); // �ǻ� ������ ����
    private string currentRace;            // ���� ���õ� Race �̸�

    void Start()
    {
        if (avatar == null || wardrobeDropdown == null || raceDropdown == null || applyButton == null || clearButton == null)
        {
            Debug.LogError("�ʼ� ������Ʈ�� �������� �ʾҽ��ϴ�.");
            return;
        }

        // Race ��Ӵٿ� ���� �̺�Ʈ ����
        raceDropdown.onValueChanged.AddListener(OnRaceChanged);

        // ��Ӵٿ� �ʱ�ȭ
        InitializeRaceDropdown();

        // ��ư �̺�Ʈ ����
        applyButton.onClick.AddListener(ApplyWardrobe);
        clearButton.onClick.AddListener(ClearWardrobe);
    }

    private void InitializeRaceDropdown()
    {
        raceDropdown.options.Clear();

        // Race �����͸� ���� �߰�
        List<string> races = new List<string> { "o3n Female", "o3n Male", "HumanGirl", "HumanBoy" };
        foreach (var race in races)
        {
            raceDropdown.options.Add(new Dropdown.OptionData(race));
        }

        raceDropdown.value = 0;
        raceDropdown.RefreshShownValue();

        // �ʱ� Race ����
        currentRace = raceDropdown.options[raceDropdown.value].text;
        InitializeWardrobeDropdown(); // �ʱ� Wardrobe ��Ӵٿ� ����
    }

    private void InitializeWardrobeDropdown()
    {
        wardrobeDropdown.options.Clear();
        wardrobeRecipes.Clear();

        // Global Library���� Wardrobe Recipe ������ ��������
        AddWardrobeRecipesFromGlobalLibrary();

        // ��Ӵٿ� �ɼ� �߰�
        foreach (var item in wardrobeRecipes)
        {
            wardrobeDropdown.options.Add(new Dropdown.OptionData(item.Key)); // ��Ӵٿ ǥ�õ� �̸�
        }

        wardrobeDropdown.value = 0;
        wardrobeDropdown.RefreshShownValue();
    }

    private void AddWardrobeRecipesFromGlobalLibrary()
    {
        // UMA Asset Indexer���� ��� Wardrobe Recipe ��������
        var recipes = UMAAssetIndexer.Instance.GetAllAssets<UMAWardrobeRecipe>();
        foreach (var recipe in recipes)
        {
            // ���� Race�� ȣȯ�Ǵ� �����Ǹ� �߰�
            if (recipe.compatibleRaces.Contains(currentRace))
            {
                wardrobeRecipes.Add(recipe.DisplayValue, recipe.name);
                Debug.Log($"Wardrobe Recipe �߰���: {recipe.DisplayValue} ({recipe.name})");
            }
        }
    }

    private void OnRaceChanged(int index)
    {
        if (index < 0 || index >= raceDropdown.options.Count)
        {
            Debug.LogError("��ȿ���� ���� Race �����Դϴ�.");
            return;
        }

        // Race ����
        string selectedRace = raceDropdown.options[index].text;
        currentRace = selectedRace;

        // Avatar Race ����
        avatar.ChangeRace(currentRace);
        avatar.BuildCharacter();

        // Wardrobe ��Ӵٿ� ������Ʈ
        InitializeWardrobeDropdown();
        Debug.Log($"Race �����: {currentRace}");
    }

    private void ApplyWardrobe()
    {
        int selectedIndex = wardrobeDropdown.value;
        string selectedOption = wardrobeDropdown.options[selectedIndex].text;

        if (!wardrobeRecipes.ContainsKey(selectedOption))
        {
            Debug.LogError($"������ �ɼ�({selectedOption})�� ���� Wardrobe Recipe�� ã�� �� �����ϴ�.");
            return;
        }

        string recipeName = wardrobeRecipes[selectedOption];

        if (!string.IsNullOrEmpty(recipeName))
        {
            // Wardrobe Recipe���� ���� �̸� ��������
            var wardrobeRecipe = UMAAssetIndexer.Instance.GetAsset<UMAWardrobeRecipe>(recipeName);
            if (wardrobeRecipe == null)
            {
                Debug.LogError($"Wardrobe Recipe '{recipeName}'�� ã�� �� �����ϴ�.");
                return;
            }

            // ���� �̸� �ڵ� ����
            string slotName = wardrobeRecipe.wardrobeSlot;

            // ���Կ� �ǻ� ����
            avatar.SetSlot(slotName, recipeName);

            Debug.Log($"���� '{slotName}'�� �ǻ� '{recipeName}'�� ���������� ����Ǿ����ϴ�.");
        }
        else
        {
            // ���Կ��� �ǻ� ����
            ClearWardrobe();
        }

        avatar.BuildCharacter(); // ���� ���� ����
    }

    private void ClearWardrobe()
    {
        avatar.ClearSlots();
        avatar.BuildCharacter();
        Debug.Log("��� ���Կ��� �ǻ��� ���ŵǾ����ϴ�.");
    }
}




