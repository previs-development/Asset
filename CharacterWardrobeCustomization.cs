using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;

public class CharacterWardrobeCustomization : MonoBehaviour
{
    public DynamicCharacterAvatar avatar;  // UMA 캐릭터 참조
    public Dropdown wardrobeDropdown;      // 의상 선택 드롭다운
    public Dropdown raceDropdown;          // Race 선택 드롭다운
    public Button applyButton;             // 의상 적용 버튼
    public Button clearButton;             // 의상 제거 버튼

    private Dictionary<string, string> wardrobeRecipes = new Dictionary<string, string>(); // 의상 레시피 저장
    private string currentRace;            // 현재 선택된 Race 이름

    void Start()
    {
        if (avatar == null || wardrobeDropdown == null || raceDropdown == null || applyButton == null || clearButton == null)
        {
            Debug.LogError("필수 컴포넌트가 설정되지 않았습니다.");
            return;
        }

        // Race 드롭다운 변경 이벤트 연결
        raceDropdown.onValueChanged.AddListener(OnRaceChanged);

        // 드롭다운 초기화
        InitializeRaceDropdown();

        // 버튼 이벤트 연결
        applyButton.onClick.AddListener(ApplyWardrobe);
        clearButton.onClick.AddListener(ClearWardrobe);
    }

    private void InitializeRaceDropdown()
    {
        raceDropdown.options.Clear();

        // Race 데이터를 직접 추가
        List<string> races = new List<string> { "o3n Female", "o3n Male", "HumanGirl", "HumanBoy" };
        foreach (var race in races)
        {
            raceDropdown.options.Add(new Dropdown.OptionData(race));
        }

        raceDropdown.value = 0;
        raceDropdown.RefreshShownValue();

        // 초기 Race 설정
        currentRace = raceDropdown.options[raceDropdown.value].text;
        InitializeWardrobeDropdown(); // 초기 Wardrobe 드롭다운 설정
    }

    private void InitializeWardrobeDropdown()
    {
        wardrobeDropdown.options.Clear();
        wardrobeRecipes.Clear();

        // Global Library에서 Wardrobe Recipe 데이터 가져오기
        AddWardrobeRecipesFromGlobalLibrary();

        // 드롭다운 옵션 추가
        foreach (var item in wardrobeRecipes)
        {
            wardrobeDropdown.options.Add(new Dropdown.OptionData(item.Key)); // 드롭다운에 표시될 이름
        }

        wardrobeDropdown.value = 0;
        wardrobeDropdown.RefreshShownValue();
    }

    private void AddWardrobeRecipesFromGlobalLibrary()
    {
        // UMA Asset Indexer에서 모든 Wardrobe Recipe 가져오기
        var recipes = UMAAssetIndexer.Instance.GetAllAssets<UMAWardrobeRecipe>();
        foreach (var recipe in recipes)
        {
            // 현재 Race와 호환되는 레시피만 추가
            if (recipe.compatibleRaces.Contains(currentRace))
            {
                wardrobeRecipes.Add(recipe.DisplayValue, recipe.name);
                Debug.Log($"Wardrobe Recipe 추가됨: {recipe.DisplayValue} ({recipe.name})");
            }
        }
    }

    private void OnRaceChanged(int index)
    {
        if (index < 0 || index >= raceDropdown.options.Count)
        {
            Debug.LogError("유효하지 않은 Race 선택입니다.");
            return;
        }

        // Race 변경
        string selectedRace = raceDropdown.options[index].text;
        currentRace = selectedRace;

        // Avatar Race 변경
        avatar.ChangeRace(currentRace);
        avatar.BuildCharacter();

        // Wardrobe 드롭다운 업데이트
        InitializeWardrobeDropdown();
        Debug.Log($"Race 변경됨: {currentRace}");
    }

    private void ApplyWardrobe()
    {
        int selectedIndex = wardrobeDropdown.value;
        string selectedOption = wardrobeDropdown.options[selectedIndex].text;

        if (!wardrobeRecipes.ContainsKey(selectedOption))
        {
            Debug.LogError($"선택한 옵션({selectedOption})에 대한 Wardrobe Recipe를 찾을 수 없습니다.");
            return;
        }

        string recipeName = wardrobeRecipes[selectedOption];

        if (!string.IsNullOrEmpty(recipeName))
        {
            // Wardrobe Recipe에서 슬롯 이름 가져오기
            var wardrobeRecipe = UMAAssetIndexer.Instance.GetAsset<UMAWardrobeRecipe>(recipeName);
            if (wardrobeRecipe == null)
            {
                Debug.LogError($"Wardrobe Recipe '{recipeName}'를 찾을 수 없습니다.");
                return;
            }

            // 슬롯 이름 자동 감지
            string slotName = wardrobeRecipe.wardrobeSlot;

            // 슬롯에 의상 설정
            avatar.SetSlot(slotName, recipeName);

            Debug.Log($"슬롯 '{slotName}'에 의상 '{recipeName}'가 성공적으로 적용되었습니다.");
        }
        else
        {
            // 슬롯에서 의상 제거
            ClearWardrobe();
        }

        avatar.BuildCharacter(); // 변경 사항 적용
    }

    private void ClearWardrobe()
    {
        avatar.ClearSlots();
        avatar.BuildCharacter();
        Debug.Log("모든 슬롯에서 의상이 제거되었습니다.");
    }
}




