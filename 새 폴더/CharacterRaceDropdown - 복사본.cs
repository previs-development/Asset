using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;

public class CharacterRaceDropdown : MonoBehaviour
{
    public DynamicCharacterAvatar avatar;  // UMA ĳ���� ����
    public Dropdown raceDropdown;          // Race ���� ��Ӵٿ�
    private Dictionary<string, RaceData> raceOptions = new Dictionary<string, RaceData>(); // Race ������ ����

    void Start()
    {
        if (avatar == null || raceDropdown == null)
        {
            Debug.LogError("DynamicCharacterAvatar �Ǵ� Dropdown�� �������� �ʾҽ��ϴ�.");
            return;
        }

        // ��Ӵٿ� �ʱ�ȭ
        InitializeRaceDropdown();

        // ��Ӵٿ� ���� �̺�Ʈ ����
        raceDropdown.onValueChanged.AddListener(OnRaceSelected);
    }

    private void InitializeRaceDropdown()
    {
        raceDropdown.options.Clear();

        // UMA Global Library���� Race ������ ��������
        UMAAssetIndexer assetIndexer = UMAAssetIndexer.Instance;
        if (assetIndexer == null)
        {
            Debug.LogError("UMA Global Library (UMAAssetIndexer)�� ã�� �� �����ϴ�.");
            return;
        }

        // ��� Race ������ ��������
        var allRaces = assetIndexer.GetAllAssets<RaceData>();
        foreach (var race in allRaces)
        {
            if (IsAllowedRace(race.raceName)) // ���ܵ� Race ���͸�
            {
                raceOptions.Add(race.raceName, race);
                raceDropdown.options.Add(new Dropdown.OptionData(race.raceName));
            }
            else
            {
                Debug.Log($"���ܵ� Race: {race.raceName}");
            }
        }

        // O3n Race �����Ͱ� ������ ���� �߰�
        AddO3nRaces(assetIndexer);

        if (raceOptions.Count > 0)
        {
            raceDropdown.value = 0;
            raceDropdown.RefreshShownValue();
        }
        else
        {
            Debug.LogError("Race �����Ͱ� ��� �ֽ��ϴ�.");
        }
    }

    private bool IsAllowedRace(string raceName)
    {
        // ������ Race ���
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

        // Race�� ���� ��Ͽ� ���ԵǾ� ������ false ��ȯ
        return !excludedRaces.Contains(raceName);
    }

    private void AddO3nRaces(UMAAssetIndexer assetIndexer)
    {
        // O3n Race ������ ���� �߰�
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
                    Debug.Log($"O3n Race �߰�: {raceName}");
                }
                else
                {
                    Debug.LogWarning($"RaceData {raceName}�� ã�� �� �����ϴ�. UMA_DCS.prefab �Ǵ� Global Library�� Ȯ���ϼ���.");
                }
            }
        }
    }

    private void OnRaceSelected(int index)
    {
        if (index < 0 || index >= raceDropdown.options.Count)
        {
            Debug.LogError("��ȿ���� ���� Race �����Դϴ�.");
            return;
        }

        string selectedRaceName = raceDropdown.options[index].text;

        if (raceOptions.ContainsKey(selectedRaceName))
        {
            // ������ Race�� ����
            avatar.ChangeRace(selectedRaceName);
            avatar.BuildCharacter();
            Debug.Log($"Race�� {selectedRaceName}(��)�� ����Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogError($"������ Race({selectedRaceName})�� ã�� �� �����ϴ�.");
        }
    }
}



