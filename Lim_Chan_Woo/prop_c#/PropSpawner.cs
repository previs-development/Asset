using UnityEngine;
using UnityEngine.UI;

public class PropSpawner : MonoBehaviour
{
    public GameObject propPrefab; // 소환할 프리팹
    public Button addPropButton;   // Add Prop 버튼
    public Vector3 offset = new Vector3(0, 0, 0); // 카메라 위치에 대한 오프셋

    void Start()
    {
        if (addPropButton != null)
        {
            addPropButton.onClick.AddListener(SpawnPropAtCamera);
        }
        else
        {
            Debug.LogError("Add Prop 버튼이 할당되지 않았습니다.");
        }
    }

    void SpawnPropAtCamera()
    {
        if (propPrefab != null)
        {
            // 메인 카메라의 위치와 회전을 기준으로 소환
            Transform cameraTransform = Camera.main.transform;
            Vector3 spawnPosition = cameraTransform.position + offset;
            Quaternion spawnRotation = cameraTransform.rotation;

            Instantiate(propPrefab, spawnPosition, spawnRotation);
        }
        else
        {
            Debug.LogError("Prop Prefab이 할당되지 않았습니다.");
        }
    }
}
