using UnityEngine;

public class MainMenuBGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _mainMenuBGM;

    private void OnEnable()
    {
        if (OOTechSoundManager.Inst != null && _mainMenuBGM != null)
        {
            OOTechSoundManager.Inst.PlayBGM(_mainMenuBGM);
        }
    }

    private void OnDisable()
    {
        if (OOTechSoundManager.Inst != null)
        {
            OOTechSoundManager.Inst.StopBGM();
        }
    }
}