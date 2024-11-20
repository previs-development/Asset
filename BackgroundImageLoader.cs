using UnityEngine;
using UnityEngine.UI;
using SFB; // Standalone File Browser 네임스페이스 추가

public class BackgroundImageLoader : MonoBehaviour
{
    public RawImage displayImage; // LOWIMAGE에 해당하는 UI RawImage 컴포넌트

    void Start()
    {
        // 시작 시 RawImage를 비활성화하여 화면에 표시되지 않도록 함
        displayImage.gameObject.SetActive(false);
    }

    // 파일 선택 창을 열고 이미지를 로드하는 메서드
    public void OnClickOpen()
    {
        Debug.Log("Open File button clicked"); // 버튼 클릭 확인용 메시지
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Image", "", new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") }, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            LoadImage(paths[0]);
        }
    }

    // 선택한 이미지를 텍스처로 불러와 화면에 표시
    private void LoadImage(string filePath)
    {
        var fileData = System.IO.File.ReadAllBytes(filePath); // 파일의 바이트 데이터를 읽어옴
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData); // 텍스처에 이미지 데이터 로드

        // 텍스처를 RawImage에 적용
        displayImage.texture = texture;

        // 이미지의 비율을 맞추기 위해 RectTransform 조정
        AdjustImageAspectRatio(texture);

        // 이미지가 로드된 후 RawImage를 활성화하여 화면에 표시
        displayImage.gameObject.SetActive(true);
    }

    // 이미지 비율을 맞추기 위해 RectTransform을 조정하는 메서드
    private void AdjustImageAspectRatio(Texture2D texture)
    {
        RectTransform rt = displayImage.GetComponent<RectTransform>();

        // 화면의 비율과 이미지 비율을 계산
        float screenAspect = (float)Screen.width / Screen.height;
        float imageAspect = (float)texture.width / texture.height;

        // 화면과 이미지의 비율에 따라 크기 조정
        if (imageAspect > screenAspect)
        {
            // 이미지가 화면보다 넓을 경우 높이를 기준으로 맞춤
            rt.sizeDelta = new Vector2(Screen.height * imageAspect, Screen.height);
        }
        else
        {
            // 이미지가 화면보다 좁을 경우 너비를 기준으로 맞춤
            rt.sizeDelta = new Vector2(Screen.width, Screen.width / imageAspect);
        }

        // 이미지가 회전되어 있다면 RectTransform의 회전을 조정할 수 있음 (필요시 사용)
        rt.localEulerAngles = Vector3.zero; // 이미지 회전을 0으로 설정
    }
}


