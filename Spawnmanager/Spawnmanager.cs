using UnityEngine;
using UnityEngine.UI;

public class AssetManager : MonoBehaviour
{
    public Dropdown dropdown;          // 드롭다운 UI
    public Button applyButton;        // Apply 버튼
    public Button clearButton;        // Clear 버튼         
    public Transform spawnParent;     // 소환된 오브젝트의 부모
    public string assetFolderPath = "Prefabs";    // 에셋이 저장된 폴더 경로 (Resources 폴더 내부)

    private GameObject spawnedObject; // 현재 소환된 오브젝트

    void Start()
    {
        // 버튼에 이벤트 리스너 추가
        applyButton.onClick.AddListener(SpawnSelectedAsset);
        clearButton.onClick.AddListener(ClearSpawnedAsset);
    }

    void SpawnSelectedAsset()
    {
        string selectedAssetName = dropdown.options[dropdown.value].text;

        // 올바른 경로를 확인
        string assetPath = $"{assetFolderPath}/{selectedAssetName}";
        Debug.Log($"로드하려는 경로: {assetPath}");

        // 기존 소환된 오브젝트 제거
        ClearSpawnedAsset();

        // Resources에서 에셋 로드
        GameObject asset = Resources.Load<GameObject>(assetPath);
        if (asset != null)
        {
            Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
            spawnedObject = Instantiate(asset, spawnPosition, Quaternion.identity, spawnParent);
            Debug.Log($"'{selectedAssetName}' 소환 완료.");
        }
        else
        {
            Debug.LogError($"'{assetPath}' 경로에서 에셋을 찾을 수 없습니다!");
        }
    }

    void ClearSpawnedAsset()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
            Debug.Log("소환된 오브젝트를 삭제했습니다.");
        }
    }
}

