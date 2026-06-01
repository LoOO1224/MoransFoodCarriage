using UnityEngine;
using UnityEngine.UI;

public class DaniTech_GameTestUI : MonoBehaviour
{
    [SerializeField] private InputField InputField_ConsoleCommand;
    // [SerializeField] private Button Button_AAA;
    // [SerializeField] private DaniTechUIButton Button_BBB;

    private void OnEnable()
    {
        // UGUI꺼
            // Button_AAA.onClick.AddListener(OnClick_DataLoadTest);
        // 제가만든 거
            // Button_BBB.BindOnClickButtonEvent(OnClick_DataLoadTest);

        InputField_ConsoleCommand.onSubmit.AddListener(OnSubmit_Input);
        InputField_ConsoleCommand.onValueChanged.AddListener(OnValueChanged_Input);
    }

    private void OnDisable()
    {
        InputField_ConsoleCommand.onSubmit.RemoveAllListeners();
        InputField_ConsoleCommand.onValueChanged.RemoveAllListeners();
    }

    public void OnSubmit_Input(string str)
    {
        string inputedText = InputField_ConsoleCommand.text;
        var characterData = DaniTechGameDataManager.Instance.GetCharacterData(inputedText);
        if (characterData != null) 
        {
            Debug.LogWarning(characterData.Name);
        }

        Debug.LogWarning(str);
    }

    public void OnValueChanged_Input(string str)
    {
        Debug.Log(str);
    }

    public void OnClick_DataLoadTest()
    {
        GameDataTester.StartDataTest();
    }
   
    public void OnClick_SelectTestBtn()
    {
        DaniTechGameObjectManager.Inst.RequestSpawnEnemy();
    }
}
