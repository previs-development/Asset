using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PropSpawner : MonoBehaviour
{
    // 소환할 프리팹들을 리스트로 관리
    public List<GameObject> propPrefabs;

    // 드롭다운 컴포넌트
    public Dropdown propDropdown;

    // Add Prop 버튼
    public Button addPropButton;

    // 소환 위치를 기준으로 사용할 Transform (메인 카메라)
    public Transform spawnPoint;

    void Start()
    {
        // 드롭다운 초기화
        if (propDropdown != null)
        {
            // 드롭다운 옵션 클리어
            propDropdown.ClearOptions();

            // 프리팹 이름을 드롭다운 옵션으로 추가
            List<string> options = new List<string>();
            foreach (GameObject prefab in propPrefabs)
            {
                options.Add(prefab.name);
            }
            propDropdown.AddOptions(options);
        }
        else
        {
            Debug.LogError("Dropdown 컴포넌트가 할당되지 않았습니다.");
        }

        // Add Prop 버튼에 클릭 이벤트 리스너 추가
        if (addPropButton != null)
        {
            addPropButton.onClick.AddListener(SpawnSelectedProp);
        }
        else
        {
            Debug.LogError("Add Prop 버튼이 할당되지 않았습니다.");
        }

        // spawnPoint가 지정되지 않았다면 메인 카메라의 위치 사용
        if (spawnPoint == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                spawnPoint = mainCamera.transform;
            }
            else
            {
                Debug.LogError("메인 카메라를 찾을 수 없습니다. spawnPoint를 할당하세요.");
            }
        }
    }

    void SpawnSelectedProp()
    {
        if (propPrefabs.Count == 0)
        {
            Debug.LogError("프리팹 리스트가 비어 있습니다.");
            return;
        }

        if (propDropdown != null)
        {
            int selectedIndex = propDropdown.value;
            if (selectedIndex >= 0 && selectedIndex < propPrefabs.Count)
            {
                GameObject selectedPrefab = propPrefabs[selectedIndex];
                if (selectedPrefab != null)
                {
                    // 메인 카메라의 앞쪽에 소환 (옵션 1)
                    Vector3 spawnPosition = spawnPoint.position + spawnPoint.forward * 2f; // 2미터 앞
                    Instantiate(selectedPrefab, spawnPosition, spawnPoint.rotation);
                }
                else
                {
                    Debug.LogError("선택된 프리팹이 null입니다.");
                }
            }
            else
            {
                Debug.LogError("선택된 인덱스가 유효하지 않습니다.");
            }
        }
        else
        {
            Debug.LogError("Dropdown 컴포넌트가 할당되지 않았습니다.");
        }
    }
}
