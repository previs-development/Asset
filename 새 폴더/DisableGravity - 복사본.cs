using UnityEngine;

public class DisableGravity : MonoBehaviour
{
    void Update()
    {
        // 중력이 필요 없다면 Transform을 사용하여 고정
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }
}

