using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;

public class CharacterRaceDropdown : MonoBehaviour
{
    public DynamicCharacterAvatar avatar;  // UMA 캐릭터 참조
    public Dropdown raceDropdown;          // Race 선택 드롭다운
    private Dictionary<string, RaceData> raceOptions = new Dictionary<string, RaceData>(); // Race 데이터 저장

    void Start()
    {
        if (avatar == null || raceDropdown == null)
        {
            Debug.LogError("DynamicCharacterAvatar 또는 Dropdown이 설정되지 않았습니다.");
            return;
        }

        // 드롭다운 초기화
        InitializeRaceDropdown();

        // 드롭다운 변경 이벤트 연결
        raceDropdown.onValueChanged.AddListener(OnRaceSelected);
    }

    private void InitializeRaceDropdown()
    {
        raceDropdown.options.Clear();

        // UMA Global Library에서 Race 데이터 가져오기
        UMAAssetIndexer assetIndexer = UMAAssetIndexer.Instance;
        if (assetIndexer == null)
        {
            Debug.LogError("UMA Global Library (UMAAssetIndexer)를 찾을 수 없습니다.");
            return;
        }

        // 모든 Race 데이터 가져오기
        var allRaces = assetIndexer.GetAllAssets<RaceData>();
        foreach (var race in allRaces)
        {
            if (IsAllowedRace(race.raceName)) // 제외된 Race 필터링
            {
                raceOptions.Add(race.raceName, race);
                raceDropdown.options.Add(new Dropdown.OptionData(race.raceName));
            }
            else
            {
                Debug.Log($"제외된 Race: {race.raceName}");
            }
        }

        // O3n Race 데이터가 없으면 수동 추가
        AddO3nRaces(assetIndexer);

        if (raceOptions.Count > 0)
        {
            raceDropdown.value = 0;
            raceDropdown.RefreshShownValue();
        }
        else
        {
            Debug.LogError("Race 데이터가 비어 있습니다.");
        }
    }

    private bool IsAllowedRace(string raceName)
    {
        // 제외할 Race 목록
        List<string> excludedRaces = new List<string>
        { 
            "Human Boy",
            "Human Girl",
            "HumanFemale",
            "HumanMale",
            "DNAConverterControllerExampleRace",
            "HumanFemaleHigh",
            "HumanFemaleHigh",
            "Human Female HighPoly_Bare",
            "Elf Female",
            "Elf Male",
            "HumanFemaleHighPoly",
            "HumanMaleHighPoly",
            "ToonFemale",
            "Werewolf",
            "SkyCar"
        };

        // Race가 제외 목록에 포함되어 있으면 false 반환
        return !excludedRaces.Contains(raceName);
    }

    private void AddO3nRaces(UMAAssetIndexer assetIndexer)
    {
        // O3n Race 데이터 수동 추가
        string[] o3nRaceNames = { "o3nMaleRace", "o3nFemaleRace" };
        foreach (var raceName in o3nRaceNames)
        {
            if (!raceOptions.ContainsKey(raceName))
            {
                var race = assetIndexer.GetAsset<RaceData>(raceName);
                if (race != null)
                {
                    raceOptions.Add(raceName, race);
                    raceDropdown.options.Add(new Dropdown.OptionData(raceName));
                    Debug.Log($"O3n Race 추가: {raceName}");
                }
                else
                {
                    Debug.LogWarning($"RaceData {raceName}를 찾을 수 없습니다. UMA_DCS.prefab 또는 Global Library를 확인하세요.");
                }
            }
        }
    }

    private void OnRaceSelected(int index)
    {
        if (index < 0 || index >= raceDropdown.options.Count)
        {
            Debug.LogError("유효하지 않은 Race 선택입니다.");
            return;
        }

        string selectedRaceName = raceDropdown.options[index].text;

        if (raceOptions.ContainsKey(selectedRaceName))
        {
            // 선택한 Race로 변경
            avatar.ChangeRace(selectedRaceName);
            avatar.BuildCharacter();
            Debug.Log($"Race가 {selectedRaceName}(으)로 변경되었습니다.");
        }
        else
        {
            Debug.LogError($"선택한 Race({selectedRaceName})를 찾을 수 없습니다.");
        }
    }
}



