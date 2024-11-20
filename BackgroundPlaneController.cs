using UnityEngine;
using SFB; // Standalone File Browser ���ӽ����̽� �߰�

public class BackgroundPlaneController : MonoBehaviour
{
    public GameObject backgroundPlane; // ������� ����� Plane ��ü
    private Material backgroundMaterial; // Plane�� Material

    void Start()
    {
        // Plane�� ��ġ�� ������ ����
        if (backgroundPlane != null)
        {
            // Plane�� ��ġ�� ī�޶� �տ� ����
            backgroundPlane.transform.position = new Vector3(0, 0, 5); // ī�޶󺸴� ���ʿ� ��ġ
            backgroundPlane.transform.localScale = new Vector3(10, 10, 1); // ������ ũ��� ������ ����

            Renderer renderer = backgroundPlane.GetComponent<Renderer>();
            if (renderer != null)
            {
                // �� Material ���� �� ����
                backgroundMaterial = new Material(Shader.Find("Standard"));
                renderer.material = backgroundMaterial;
            }
        }
    }

    // ���� ���� â�� ���� �̹����� �ε��ϴ� �޼���
    public void OnClickOpen()
    {
        Debug.Log("Open File button clicked"); // ��ư Ŭ�� Ȯ�ο� �޽���
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            LoadImage(paths[0]);
        }
    }

    // ������ �̹����� �ؽ�ó�� �ҷ��� Plane�� ����
    private void LoadImage(string filePath)
    {
        var fileData = System.IO.File.ReadAllBytes(filePath); // ������ ����Ʈ �����͸� �о��
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData); // �ؽ�ó�� �̹��� ������ �ε�

        // �ؽ�ó�� �������� ������
        Texture2D flippedTexture = FlipTextureVertically(texture);

        if (backgroundMaterial != null)
        {
            backgroundMaterial.mainTexture = flippedTexture; // Plane�� Material�� �ؽ�ó ����
        }
    }

    // �ؽ�ó�� �������� ������ �޼���
    private Texture2D FlipTextureVertically(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height);
        for (int y = 0; y < original.height; y++)
        {
            for (int x = 0; x < original.width; x++)
            {
                flipped.SetPixel(x, original.height - 1 - y, original.GetPixel(x, y));
            }
        }
        flipped.Apply();
        return flipped;
    }

    void Update()
    {
        // Ű����� ���콺�� ��� ��ü�� �̵� �� ȸ��
        if (backgroundPlane != null)
        {
            // ���콺 �ٷ� ũ�� ����
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                // Ctrl + ���콺 ��: ���� ũ�� ����
                backgroundPlane.transform.localScale += new Vector3(scroll, 0, 0);
            }
            else
            {
                // �Ϲ� ���콺 ��: ��ü ũ�� ����
                backgroundPlane.transform.localScale += Vector3.one * scroll;
            }

            // Ű���� ����Ű�� �̵�
            float moveSpeed = 5.0f * Time.deltaTime;
            if (Input.GetKey(KeyCode.W)) backgroundPlane.transform.Translate(Vector3.forward * moveSpeed);
            if (Input.GetKey(KeyCode.S)) backgroundPlane.transform.Translate(Vector3.back * moveSpeed);
            if (Input.GetKey(KeyCode.A)) backgroundPlane.transform.Translate(Vector3.left * moveSpeed);
            if (Input.GetKey(KeyCode.D)) backgroundPlane.transform.Translate(Vector3.right * moveSpeed);

            // ���콺 �巡�׷� ȸ��
            if (Input.GetMouseButton(1)) // ���콺 ������ ��ư Ŭ�� ��
            {
                float rotationSpeed = 100.0f * Time.deltaTime;
                float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
                float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;
                backgroundPlane.transform.Rotate(Vector3.up, -rotationX, Space.World);
                backgroundPlane.transform.Rotate(Vector3.right, rotationY, Space.Self);
            }
        }
    }
}
