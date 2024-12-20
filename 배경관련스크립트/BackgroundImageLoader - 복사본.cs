using UnityEngine;
using UnityEngine.UI;
using SFB; // Standalone File Browser ���ӽ����̽� �߰�

public class BackgroundImageLoader : MonoBehaviour
{
    public RawImage displayImage; // LOWIMAGE�� �ش��ϴ� UI RawImage ������Ʈ

    void Start()
    {
        // ���� �� RawImage�� ��Ȱ��ȭ�Ͽ� ȭ�鿡 ǥ�õ��� �ʵ��� ��
        displayImage.gameObject.SetActive(false);
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

    // ������ �̹����� �ؽ�ó�� �ҷ��� ȭ�鿡 ǥ��
    private void LoadImage(string filePath)
    {
        var fileData = System.IO.File.ReadAllBytes(filePath); // ������ ����Ʈ �����͸� �о��
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData); // �ؽ�ó�� �̹��� ������ �ε�

        // �ؽ�ó�� RawImage�� ����
        displayImage.texture = texture;

        // �̹����� ������ ���߱� ���� RectTransform ����
        AdjustImageAspectRatio(texture);

        // �̹����� �ε�� �� RawImage�� Ȱ��ȭ�Ͽ� ȭ�鿡 ǥ��
        displayImage.gameObject.SetActive(true);
    }

    // �̹��� ������ ���߱� ���� RectTransform�� �����ϴ� �޼���
    private void AdjustImageAspectRatio(Texture2D texture)
    {
        RectTransform rt = displayImage.GetComponent<RectTransform>();

        // ȭ���� ������ �̹��� ������ ���
        float screenAspect = (float)Screen.width / Screen.height;
        float imageAspect = (float)texture.width / texture.height;

        // ȭ��� �̹����� ������ ���� ũ�� ����
        if (imageAspect > screenAspect)
        {
            // �̹����� ȭ�麸�� ���� ��� ���̸� �������� ����
            rt.sizeDelta = new Vector2(Screen.height * imageAspect, Screen.height);
        }
        else
        {
            // �̹����� ȭ�麸�� ���� ��� �ʺ� �������� ����
            rt.sizeDelta = new Vector2(Screen.width, Screen.width / imageAspect);
        }

        // �̹����� ȸ���Ǿ� �ִٸ� RectTransform�� ȸ���� ������ �� ���� (�ʿ�� ���)
        rt.localEulerAngles = Vector3.zero; // �̹��� ȸ���� 0���� ����
    }
}


