using UnityEngine;

public class DisableGravity : MonoBehaviour
{
    void Update()
    {
        // �߷��� �ʿ� ���ٸ� Transform�� ����Ͽ� ����
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }
}

