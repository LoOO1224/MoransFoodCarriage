using UnityEngine;
using UnityEngine.UI;

public class DaniTech_JukeBoxUI : MonoBehaviour
{
    [SerializeField] private InputField InputField_SoundDataId;
    [SerializeField] private DaniTechUIButton Button_PlaySFX;
    [SerializeField] private DaniTechUIButton Button_PlayBgm;

    private void Awake()
    {
        Button_PlayBgm.BindOnClickButtonEvent(OnClick_PlayBgm);
        Button_PlaySFX.BindOnClickButtonEvent(OnClick_PlaySFX);
    }

    public void OnClick_PlayBgm()
    {
        string soundDataId = InputField_SoundDataId.text;
        if(string.IsNullOrEmpty(soundDataId) == true)
        {
            Debug.LogWarning("공백문자입니다.");
            return;
        }

        DaniTechSoundManager.Inst.PlayBGM(soundDataId);
    }


    public void OnClick_PlaySFX()
    {
        string soundDataId = InputField_SoundDataId.text;
        if (string.IsNullOrEmpty(soundDataId) == true)
        {
            Debug.LogWarning("공백문자입니다.");
            return;
        }

        DaniTechSoundManager.Inst.PlaySFX(soundDataId);
    }
}
