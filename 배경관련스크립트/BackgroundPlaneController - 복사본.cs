using UnityEngine;
using SFB; // Standalone File Browser ���ӽ����̽� �߰�
using System.Collections.Generic;
using UnityEngine.UI;

public class BackgroundPlaneController : MonoBehaviour
{
    [Header("Background Plane Settings")]
    public GameObject backgroundPlanePrefab; // ��� Plane�� Prefab
    public Transform planesParent; // ��� ��� Plane�� ���� �θ� Transform

    [Header("UI Elements")]
    public GameObject planeListContent; // Plane ����Ʈ�� ���� ScrollView�� Content
    public GameObject planeListItemPrefab; // Plane ����Ʈ �������� Prefab (��ư ��)

    [Header("Error UI")]
    public Text errorMessageText; // ���� �޽����� ǥ���� UI Text

    private List<GameObject> backgroundPlanes = new List<GameObject>(); // ������ ��� Plane ����Ʈ
    private List<Material> backgroundMaterials = new List<Material>(); // �� Plane�� Material ����Ʈ

    private GameObject selectedPlane = null; // ���� ���õ� Plane

    private int planeCount = 0; // Plane �̸��� ���� ī����

    // ���� ������ �⺻ ������ ��
    public Vector3 defaultPlaneScale = new Vector3(10, 3, 1);
    public float planeSpacing = 5f; // Plane ���� ����

    void Start()
    {
        // �ʱ� Plane ���� (�ʿ� ��)
        // CreateNewBackgroundPlane(new Vector3(0, 0, 5), defaultPlaneScale);
    }

    /// <summary>
    /// �� ��� Plane�� �����ϰ� ����Ʈ�� �߰��մϴ�.
    /// </summary>
    /// <param name="position">���� ��ġ</param>
    /// <param name="scale">������</param>
    public void CreateNewBackgroundPlane(Vector3 position, Vector3 scale)
    {
        if (backgroundPlanePrefab == null)
        {
            Debug.LogError("Background Plane Prefab�� �Ҵ���� �ʾҽ��ϴ�!");
            ShowError("Background Plane Prefab�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // Prefab �ν��Ͻ�ȭ
        GameObject newPlane = Instantiate(backgroundPlanePrefab, planesParent);

        // Plane�� ��ġ�� ������ ����
        // ī�޶��� �߾� ��ġ�� �������� ��� ����
        Vector3 cameraPosition = Camera.main.transform.position;
        newPlane.transform.position = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z + 5); // ī�޶� �� Z=5 ��ġ
        newPlane.transform.localScale = new Vector3(1.0f, 2.5f, 0.5f); // ������ ũ�� ����

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
        Debug.Log("Create New Background button clicked"); // ��ư Ŭ�� Ȯ�ο� �޽���

        // �� Plane�� ��ġ�� ���� Plane�鿡 ���� ���� (��: Z���� �������� ������)
        Vector3 newPosition = Vector3.zero;
        if (backgroundPlanes.Count > 0)
        {
            // ������ Plane�� Z ��ġ�� �������� �������� ���� �Ÿ� ��ġ
            GameObject lastPlane = backgroundPlanes[backgroundPlanes.Count - 1];
            newPosition = lastPlane.transform.position + new Vector3(0, 0, planeSpacing);
        }
        else
        {
            // ù Plane�� �⺻ ��ġ ���� (ī�޶� ����)
            newPosition = new Vector3(0, 0, 5f); // ī�޶� ��ġ�� (0,0,0)�̶� ����
        }

        // �� Plane ����
        CreateNewBackgroundPlane(newPosition, defaultPlaneScale);
    }

    /// <summary>
    /// "�̹��� ����" ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    /// </summary>
    public void OnClickOpen()
    {
        Debug.Log("Open File button clicked"); // ��ư Ŭ�� Ȯ�ο� �޽���
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            // ���� Ȱ��ȭ��(���õ�) Plane�� �̹����� �ε�
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
    /// ���õ� Plane�� �̹����� �ε��ϰ� �����մϴ�.
    /// </summary>
    /// <param name="filePath">�̹��� ���� ���</param>
    /// <param name="plane">������ Plane</param>
    private void LoadImage(string filePath, GameObject plane)
    {
        try
        {
            var fileData = System.IO.File.ReadAllBytes(filePath); // ������ ����Ʈ �����͸� �о��
            Texture2D texture = new Texture2D(2, 2);
            if (!texture.LoadImage(fileData))
            {
                ShowError("�̹����� �ε��� �� �����ϴ�.");
                return;
            }

            // �ؽ�ó�� �������� ������
            Texture2D flippedTexture = FlipTextureVertically(texture);

            Renderer renderer = plane.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = flippedTexture; // Plane�� Material�� �ؽ�ó ����

                // Plane�� �������� �̹��� ������ �°� ����
                float aspectRatio = (float)flippedTexture.width / flippedTexture.height;
                plane.transform.localScale = new Vector3(defaultPlaneScale.x * aspectRatio, defaultPlaneScale.y, defaultPlaneScale.z); // ����: �⺻ ũ�⿡ ���� ����
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
    /// �ؽ�ó�� �������� ������ �޼���
    /// </summary>
    /// <param name="original">���� �ؽ�ó</param>
    /// <returns>�������� ������ �ؽ�ó</returns>
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
        // ���õ� Plane�� ���ؼ��� �Է� ó��
        if (selectedPlane != null)
        {
            HandlePlaneControls(selectedPlane);
        }

        // Ű���� �Է����� Plane ���� ���� (��: Esc Ű)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectPlane();
        }

        // ���콺 Ŭ������ Plane ���� (�߰� ���)
        if (Input.GetMouseButtonDown(0)) // ���콺 ���� ��ư Ŭ�� ��
        {
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

    /// <summary>
    /// Plane ����Ʈ UI�� Plane�� �߰��մϴ�.
    /// </summary>
    /// <param name="plane">�߰��� Plane</param>
    private void AddPlaneToUI(GameObject plane)
    {
        if (planeListContent == null || planeListItemPrefab == null)
        {
            Debug.LogError("Plane List UI ��Ұ� �Ҵ���� �ʾҽ��ϴ�!");
            ShowError("Plane List UI ��Ұ� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // Plane ����Ʈ ������ �ν��Ͻ�ȭ
        GameObject listItem = Instantiate(planeListItemPrefab, planeListContent.transform);

        // ����Ʈ �������� ��ư �ؽ�Ʈ ���� (��: Plane �̸�)
        Text buttonText = listItem.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = plane.name;
        }

        // ��ư Ŭ�� �̺�Ʈ �߰�
        Button button = listItem.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => SelectPlane(plane));
        }
    }

    /// <summary>
    /// Plane�� �����մϴ�.
    /// </summary>
    /// <param name="plane">������ Plane</param>
    public void SelectPlane(GameObject plane)
    {
        if (selectedPlane != null)
        {
            // ������ ���õ� Plane�� ���̶���Ʈ ����
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
    /// Plane�� ���̶���Ʈ�Ͽ� ���õǾ����� �ð������� ǥ���մϴ�.
    /// </summary>
    /// <param name="plane">���̶���Ʈ�� Plane</param>
    private void HighlightPlane(GameObject plane)
    {
        Renderer renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            // ���� ������ ����
            if (!renderer.material.HasProperty("_OriginalColor"))
            {
                renderer.material.SetColor("_OriginalColor", renderer.material.color);
            }

            // ���� �������� ���̶���Ʈ
            renderer.material.color = Color.yellow;
        }
    }

    /// <summary>
    /// Plane�� ���̶���Ʈ�� �����մϴ�.
    /// </summary>
    /// <param name="plane">���̶���Ʈ ������ Plane</param>
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

            // UI ����Ʈ���� ����
            RemovePlaneFromUI(selectedPlane);

            // Plane ����
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
    /// Plane ����Ʈ UI���� Plane�� �����մϴ�.
    /// </summary>
    /// <param name="plane">������ Plane</param>
    private void RemovePlaneFromUI(GameObject plane)
    {
        // Plane ����Ʈ ������ �� Plane�� ������ ���� ã�� ����
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
    /// ���� �޽����� UI�� ǥ���մϴ�.
    /// </summary>
    /// <param name="message">ǥ���� �޽���</param>
    private void ShowError(string message)
    {
        if (errorMessageText != null)
        {
            errorMessageText.text = message;
            // ���� �ð� �� ���� �޽��� �ʱ�ȭ
            StartCoroutine(ClearErrorMessageAfterDelay(3f));
        }
        Debug.LogError(message);
    }

    /// <summary>
    /// ���� �ð� �� ���� �޽����� �ʱ�ȭ�ϴ� �ڷ�ƾ
    /// </summary>
    /// <param name="delay">���� �ð�(��)</param>
    /// <returns></returns>
    private System.Collections.IEnumerator ClearErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorMessageText != null)
        {
            errorMessageText.text = "";
        }
    }

    /// <summary>
    /// ���� Plane�� ���� �̵� �� ȸ�� ����
    /// </summary>
    /// <param name="plane">������ Plane</param>
    private void HandlePlaneControls(GameObject plane)
    {
        // ���콺 �ٷ� ũ�� ����
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // Ctrl + ���콺 ��: ���� ũ�� ����
            plane.transform.localScale += new Vector3(scroll, 0, 0);
        }
        else
        {
            // �Ϲ� ���콺 ��: ��ü ũ�� ����
            plane.transform.localScale += Vector3.one * scroll;
        }

        // Ű���� ����Ű�� �̵�
        float moveSpeed = 5.0f * Time.deltaTime;
        if (Input.GetKey(KeyCode.W)) plane.transform.Translate(Vector3.forward * moveSpeed);
        if (Input.GetKey(KeyCode.S)) plane.transform.Translate(Vector3.back * moveSpeed);
        if (Input.GetKey(KeyCode.A)) plane.transform.Translate(Vector3.left * moveSpeed);
        if (Input.GetKey(KeyCode.D)) plane.transform.Translate(Vector3.right * moveSpeed);

        // ���콺 �巡�׷� ȸ��
        if (Input.GetMouseButton(1)) // ���콺 ������ ��ư Ŭ�� ��
        {
            float rotationSpeed = 100.0f * Time.deltaTime;
            float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;
            plane.transform.Rotate(Vector3.up, -rotationX, Space.World);
            plane.transform.Rotate(Vector3.right, rotationY, Space.Self);
        }
    }
}

