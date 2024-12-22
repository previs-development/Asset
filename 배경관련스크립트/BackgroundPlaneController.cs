using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // UI �̺�Ʈ ó���� ���� �ʿ�
using SFB; // Standalone File Browser ���ӽ����̽� �߰�
using System.Collections.Generic;

public class BackgroundPlaneController : MonoBehaviour
{
    [Header("Background Plane Settings")]
    public GameObject backgroundPlanePrefab; // ��� Plane�� Prefab
    public Transform planesParent; // ��� ��� Plane�� ���� �θ� Transform

    [Header("UI Elements")]
    public GameObject planeListContent; // Plane ����Ʈ�� ���� ScrollView�� Content
    public GameObject planeListItemPrefab; // Plane ����Ʈ �������� Prefab (��ư ��)
    public Text errorMessageText; // ���� �޽����� ǥ���� UI Text

    private List<GameObject> backgroundPlanes = new List<GameObject>(); // ������ ��� Plane ����Ʈ
    private List<Material> backgroundMaterials = new List<Material>(); // �� Plane�� Material ����Ʈ

    private GameObject selectedPlane = null; // ���� ���õ� Plane
    private int planeCount = 0; // Plane �̸��� ���� ī����

    // ���� ������ �⺻ ������ ��
    public Vector3 defaultPlaneScale = new Vector3(0.6f, 2.4f, 0.4f); // ������ ������
    public float planeSpacing = 5f; // Plane ���� ����

    // ������ ���� �ӵ�
    public float scaleSpeed = 0.1f; // ��ü ���� ���� �ӵ�
    public float scaleSpeedX = 0.1f; // ���� ���� ���� �ӵ�

    void Start()
    {
        // �ʱ� Plane ���� (�ʿ��ϸ� �ּ� ����)
        // CreateNewBackgroundPlane();
    }

    /// <summary>
    /// �� ��� Plane�� �����ϰ� ����Ʈ�� �߰��մϴ�.
    /// </summary>
    public void CreateNewBackgroundPlane()
    {
        if (backgroundPlanePrefab == null)
        {
            Debug.LogError("Background Plane Prefab�� �Ҵ���� �ʾҽ��ϴ�!");
            ShowError("Background Plane Prefab�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // ī�޶��� ���� ��ġ�� ������ �������� Plane ��ġ ����
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("���� ī�޶� �����ϴ�!");
            ShowError("���� ī�޶� �����ϴ�!");
            return;
        }

        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 5f;

        // Plane�� ī�޶� �ٶ󺸵��� ȸ�� ����
        Quaternion spawnRotation = Quaternion.LookRotation(-mainCamera.transform.forward, Vector3.up);

        // Prefab �ν��Ͻ�ȭ
        GameObject newPlane = Instantiate(backgroundPlanePrefab, spawnPosition, spawnRotation, planesParent);

        // Plane�� ������ ����
        newPlane.transform.localScale = defaultPlaneScale; // ������ �����Ϸ� ����

        // �����̼� X�� -90���� ����
        newPlane.transform.Rotate(-90f, 0f, 0f, Space.Self);

        // ����ũ�� �̸� �ο�
        planeCount++;
        newPlane.name = $"Plane_{planeCount}";

        // Material ���� �� ����
        Renderer renderer = newPlane.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material newMaterial = new Material(Shader.Find("Standard"));
            renderer.material = newMaterial;
            backgroundMaterials.Add(newMaterial);
        }
        else
        {
            Debug.LogWarning("������ Plane�� Renderer�� �����ϴ�.");
            ShowError("������ Plane�� Renderer�� �����ϴ�.");
        }

        // PropMover ������Ʈ �߰�
        if (newPlane.GetComponent<PropMover>() == null)
        {
            newPlane.AddComponent<PropMover>();
        }

        // ����Ʈ�� �߰�
        backgroundPlanes.Add(newPlane);

        // UI ����Ʈ�� �߰�
        AddPlaneToUI(newPlane);
    }

    /// <summary>
    /// "�� ��� �����" ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    /// </summary>
    public void OnClickCreateNewBackground()
    {
        Debug.Log("Create New Background button clicked");
        CreateNewBackgroundPlane();
    }

    /// <summary>
    /// "�̹��� ����" ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    /// </summary>
    public void OnClickOpen()
    {
        Debug.Log("Open File button clicked");
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            if (selectedPlane != null)
            {
                LoadImage(paths[0], selectedPlane);
            }
            else
            {
                Debug.LogWarning("������ ��� Plane�� ���õ��� �ʾҽ��ϴ�. ���� Plane�� �����ϼ���.");
                ShowError("������ ��� Plane�� ���õ��� �ʾҽ��ϴ�. ���� Plane�� �����ϼ���.");
            }
        }
    }

    /// <summary>
    /// ���õ� Plane�� �̹����� ����
    /// </summary>
    private void LoadImage(string filePath, GameObject plane)
    {
        try
        {
            var fileData = System.IO.File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(fileData))
            {
                ShowError("�̹����� �ε��� �� �����ϴ�.");
                return;
            }

            Texture2D flippedTexture = FlipTextureVertically(texture);

            Renderer renderer = plane.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = flippedTexture;

                // ����ڰ� �������� ������ �� �ֵ��� Plane�� �������� �������� �ʽ��ϴ�.
            }
            else
            {
                ShowError("Plane�� Renderer�� �������� �ʽ��ϴ�.");
            }
        }
        catch (System.Exception ex)
        {
            ShowError($"�̹��� �ε� �� ���� �߻�: {ex.Message}");
            Debug.LogError($"�̹��� �ε� �� ���� �߻�: {ex.Message}");
        }
    }

    /// <summary>
    /// �ؽ�ó ���� ������
    /// </summary>
    private Texture2D FlipTextureVertically(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);
        Color[] originalPixels = original.GetPixels();
        Color[] flippedPixels = new Color[originalPixels.Length];

        for (int y = 0; y < original.height; y++)
        {
            for (int x = 0; x < original.width; x++)
            {
                flippedPixels[y * original.width + x] = originalPixels[(original.height - y - 1) * original.width + x];
            }
        }

        flipped.SetPixels(flippedPixels);
        flipped.Apply();
        return flipped;
    }

    void Update()
    {
        // UI ������ ���콺 Ŭ�� �� Plane ���� ���� ����
        if (Input.GetMouseButtonDown(0))
        {
            // �����Ͱ� UI ���� ������ Plane ���� Raycast�� �� ��
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // UI Ŭ���̹Ƿ� Plane ���� ������ ��ŵ
            }
            else
            {
                // UI�� �ƴ� �� Ŭ�� �� Plane ���� �õ�
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    GameObject clickedPlane = hit.collider.gameObject;
                    if (backgroundPlanes.Contains(clickedPlane))
                    {
                        SelectPlane(clickedPlane);
                    }
                }
            }
        }

        // ������ ���� ����
        if (selectedPlane != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    // Ctrl + ���콺 ��: ���� ũ�⸸ ����
                    Vector3 currentScale = selectedPlane.transform.localScale;
                    currentScale.x += scroll * scaleSpeedX;
                    selectedPlane.transform.localScale = new Vector3(
                        Mathf.Max(currentScale.x, 0.1f), // �ּ� ������ ����
                        currentScale.y,
                        currentScale.z
                    );
                }
                else
                {
                    // �׳� ���콺 ��: ��ü ũ�� ���� ����
                    Vector3 currentScale = selectedPlane.transform.localScale;
                    currentScale += Vector3.one * scroll * scaleSpeed;
                    // �ּ� ������ ����
                    currentScale.x = Mathf.Max(currentScale.x, 0.1f);
                    currentScale.y = Mathf.Max(currentScale.y, 0.1f);
                    currentScale.z = Mathf.Max(currentScale.z, 0.1f);
                    selectedPlane.transform.localScale = currentScale;
                }
            }
        }

        // Esc Ű�� Plane ���� ����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectPlane();
        }
    }

    /// <summary>
    /// Plane ����Ʈ UI�� Plane �߰�
    /// </summary>
    private void AddPlaneToUI(GameObject plane)
    {
        if (planeListContent == null || planeListItemPrefab == null)
        {
            Debug.LogError("Plane List UI ��Ұ� �Ҵ���� �ʾҽ��ϴ�!");
            ShowError("Plane List UI ��Ұ� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        GameObject listItem = Instantiate(planeListItemPrefab, planeListContent.transform);
        Text buttonText = listItem.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = plane.name;
        }

        Button button = listItem.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => SelectPlane(plane));
        }

        // Plane�� UI ����Ʈ ������ ���� ������ ���� �߰����� ������ ������ �� �ֽ��ϴ�.
    }

    /// <summary>
    /// Plane ����
    /// </summary>
    public void SelectPlane(GameObject plane)
    {
        if (selectedPlane != null)
        {
            RemoveHighlight(selectedPlane);
        }

        selectedPlane = plane;
        HighlightPlane(selectedPlane);
        Debug.Log($"Plane '{selectedPlane.name}'��(��) ���õǾ����ϴ�.");
    }

    /// <summary>
    /// Plane ���� ����
    /// </summary>
    public void DeselectPlane()
    {
        if (selectedPlane != null)
        {
            RemoveHighlight(selectedPlane);
            Debug.Log($"Plane '{selectedPlane.name}'��(��) ���� �����Ǿ����ϴ�.");
            selectedPlane = null;
        }
    }

    /// <summary>
    /// Plane ���̶���Ʈ
    /// </summary>
    private void HighlightPlane(GameObject plane)
    {
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (!renderer.material.HasProperty("_OriginalColor"))
            {
                renderer.material.SetColor("_OriginalColor", renderer.material.color);
            }
            renderer.material.color = Color.yellow;
        }
    }

    /// <summary>
    /// Plane ���̶���Ʈ ����
    /// </summary>
    private void RemoveHighlight(GameObject plane)
    {
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null && renderer.material.HasProperty("_OriginalColor"))
        {
            Color originalColor = renderer.material.GetColor("_OriginalColor");
            renderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// "Plane ����" ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    /// </summary>
    public void OnClickDeleteSelectedPlane()
    {
        if (selectedPlane != null)
        {
            Debug.Log($"Plane '{selectedPlane.name}'��(��) �����մϴ�.");

            RemovePlaneFromUI(selectedPlane);
            backgroundPlanes.Remove(selectedPlane);
            Destroy(selectedPlane);
            selectedPlane = null;
        }
        else
        {
            Debug.LogWarning("������ Plane�� ���õ��� �ʾҽ��ϴ�.");
            ShowError("������ Plane�� ���õ��� �ʾҽ��ϴ�.");
        }
    }

    /// <summary>
    /// UI ����Ʈ���� �ش� Plane ����
    /// </summary>
    private void RemovePlaneFromUI(GameObject plane)
    {
        foreach (Transform child in planeListContent.transform)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                Text buttonText = button.GetComponentInChildren<Text>();
                if (buttonText != null && buttonText.text == plane.name)
                {
                    Destroy(child.gameObject);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// ���� �޽��� ǥ��
    /// </summary>
    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            StartCoroutine(ClearErrorMessageAfterDelay(3f));
        }
        Debug.LogError(message);
    }

    /// <summary>
    /// ���� �ð� �� ���� �޽��� �ʱ�ȭ
    /// </summary>
    private System.Collections.IEnumerator ClearErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorMessageText != null)
        {
            errorMessageText.text = "";
        }
    }
}
