using UnityEngine;
using SFB; // Standalone File Browser 네임스페이스 추가

public class BackgroundPlaneController : MonoBehaviour
{
    public GameObject backgroundPlane; // 배경으로 사용할 Plane 객체
    private Material backgroundMaterial; // Plane의 Material

    void Start()
    {
        // Plane의 위치와 스케일 설정
        if (backgroundPlane != null)
        {
            // Plane의 위치를 카메라 앞에 설정
            backgroundPlane.transform.position = new Vector3(0, 0, 5); // 카메라보다 앞쪽에 배치
            backgroundPlane.transform.localScale = new Vector3(10, 10, 1); // 적당한 크기로 스케일 조정

            Renderer renderer = backgroundPlane.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 새 Material 생성 및 적용
                backgroundMaterial = new Material(Shader.Find("Standard"));
                renderer.material = backgroundMaterial;
            }
        }
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

    // 선택한 이미지를 텍스처로 불러와 Plane에 적용
    private void LoadImage(string filePath)
    {
        var fileData = System.IO.File.ReadAllBytes(filePath); // 파일의 바이트 데이터를 읽어옴
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData); // 텍스처에 이미지 데이터 로드

        // 텍스처를 수직으로 뒤집기
        Texture2D flippedTexture = FlipTextureVertically(texture);

        if (backgroundMaterial != null)
        {
            backgroundMaterial.mainTexture = flippedTexture; // Plane의 Material에 텍스처 적용
        }
    }

    // 텍스처를 수직으로 뒤집는 메서드
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
        // 키보드와 마우스로 배경 객체를 이동 및 회전
        if (backgroundPlane != null)
        {
            // 마우스 휠로 크기 조절
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                // Ctrl + 마우스 휠: 가로 크기 조절
                backgroundPlane.transform.localScale += new Vector3(scroll, 0, 0);
            }
            else
            {
                // 일반 마우스 휠: 전체 크기 조절
                backgroundPlane.transform.localScale += Vector3.one * scroll;
            }

            // 키보드 방향키로 이동
            float moveSpeed = 5.0f * Time.deltaTime;
            if (Input.GetKey(KeyCode.W)) backgroundPlane.transform.Translate(Vector3.forward * moveSpeed);
            if (Input.GetKey(KeyCode.S)) backgroundPlane.transform.Translate(Vector3.back * moveSpeed);
            if (Input.GetKey(KeyCode.A)) backgroundPlane.transform.Translate(Vector3.left * moveSpeed);
            if (Input.GetKey(KeyCode.D)) backgroundPlane.transform.Translate(Vector3.right * moveSpeed);

            // 마우스 드래그로 회전
            if (Input.GetMouseButton(1)) // 마우스 오른쪽 버튼 클릭 시
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
