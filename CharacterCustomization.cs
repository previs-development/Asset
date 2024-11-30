using UnityEngine;
using UnityEngine.UI;
using UMA;
using UMA.CharacterSystem;

public class CharacterCustomization : MonoBehaviour
{
    public DynamicCharacterAvatar avatar; // UMA 캐릭터 참조

    // 슬라이더
    public Slider heightSlider;
    public Slider weightSlider; // 몸무게 조절 슬라이더

    void Start()
    {
        // 슬라이더 초기화
        heightSlider.onValueChanged.AddListener(AdjustHeight);
        weightSlider.onValueChanged.AddListener(AdjustWeight);
    }

    // 키 조정 (height, neckHeight DNA)
    public void AdjustHeight(float value)
    {
        avatar.SetDNA("height", value);       // 전체 키 조정
        avatar.SetDNA("neckHeight", value);   // 목 높이 조정
        avatar.BuildCharacter();
        Debug.Log($"Height adjusted to: {value}");
    }

    // 몸무게 조정 (하나의 슬라이더로 lowerWeight, Overweight, upperWeight 비례 조정)
    public void AdjustWeight(float value)
    {
        avatar.SetDNA("lowerWeight", value * 0.8f); // 하체는 80% 비율
        avatar.SetDNA("Overweight", value);         // 전체는 100% 비율
        avatar.SetDNA("upperWeight", value * 0.9f); // 상체는 90% 비율
        avatar.BuildCharacter();
        Debug.Log($"Weight adjusted to: {value}");
    }
}
