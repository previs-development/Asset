using UnityEngine;
using UnityEngine.UI;

public class AssetManager : MonoBehaviour
{
    public Dropdown dropdown;          // ��Ӵٿ� UI
    public Button applyButton;        // Apply ��ư
    public Button clearButton;        // Clear ��ư         
    public Transform spawnParent;     // ��ȯ�� ������Ʈ�� �θ�
    public string assetFolderPath = "Prefabs";    // ������ ����� ���� ��� (Resources ���� ����)

    private GameObject spawnedObject; // ���� ��ȯ�� ������Ʈ

    void Start()
    {
        // ��ư�� �̺�Ʈ ������ �߰�
        applyButton.onClick.AddListener(SpawnSelectedAsset);
        clearButton.onClick.AddListener(ClearSpawnedAsset);
    }

    void SpawnSelectedAsset()
    {
        string selectedAssetName = dropdown.options[dropdown.value].text;

        // �ùٸ� ��θ� Ȯ��
        string assetPath = $"{assetFolderPath}/{selectedAssetName}";
        Debug.Log($"�ε��Ϸ��� ���: {assetPath}");

        // ���� ��ȯ�� ������Ʈ ����
        ClearSpawnedAsset();

        // Resources���� ���� �ε�
        GameObject asset = Resources.Load<GameObject>(assetPath);
        if (asset != null)
        {
            Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
            spawnedObject = Instantiate(asset, spawnPosition, Quaternion.identity, spawnParent);
            Debug.Log($"'{selectedAssetName}' ��ȯ �Ϸ�.");
        }
        else
        {
            Debug.LogError($"'{assetPath}' ��ο��� ������ ã�� �� �����ϴ�!");
        }
    }

    void ClearSpawnedAsset()
    {
        if (spawnedObject != null)
        {
            Destroy(spawnedObject);
            spawnedObject = null;
            Debug.Log("��ȯ�� ������Ʈ�� �����߽��ϴ�.");
        }
    }
}

