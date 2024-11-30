using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;

public class CharacterCustomization : MonoBehaviour
{
    public DynamicCharacterAvatar avatar; // UMA ĳ���� ����

    // �����̴�
    public Slider heightSlider;
    public Slider weightSlider; // ������ ���� �����̴�

    void Start()
    {
        // �����̴� �ʱ�ȭ
        heightSlider.onValueChanged.AddListener(AdjustHeight);
        weightSlider.onValueChanged.AddListener(AdjustWeight);
    }

    // Ű ���� (height, neckHeight DNA)
    public void AdjustHeight(float value)
    {
        avatar.SetDNA("height", value);       // ��ü Ű ����
        avatar.SetDNA("neckHeight", value);   // �� ���� ����
        avatar.BuildCharacter();
        Debug.Log($"Height adjusted to: {value}");
    }

    // ������ ���� (�ϳ��� �����̴��� lowerWeight, Overweight, upperWeight ��� ����)
    public void AdjustWeight(float value)
    {
        avatar.SetDNA("lowerWeight", value * 0.8f); // ��ü�� 80% ����
        avatar.SetDNA("Overweight", value);         // ��ü�� 100% ����
        avatar.SetDNA("upperWeight", value * 0.9f); // ��ü�� 90% ����
        avatar.BuildCharacter();
        Debug.Log($"Weight adjusted to: {value}");
    }
}
