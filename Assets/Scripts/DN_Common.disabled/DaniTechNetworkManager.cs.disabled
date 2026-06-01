using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class DaniTechNetworkManager : MonoBehaviour
{
    public static DaniTechNetworkManager Inst { get; set; }

    private void Awake()
    {
        Inst = this;
    }

    // 파일 저장 경로 설정 (C:/Users/이름/.../projectName/save.json)
    private string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, "DaniTechSaveData.json");
    } 

    // 세이브 기능 구현
    public void RequstSaveData(DaniTechPlayerModel data)
    {
        // prettyPrint = true는 JSON을 보기 좋게 정렬
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(), json); // 파일 쓰기는 상당한 비용이 소모됨!
        Debug.Log($"저장 완료: {GetPath()}");
    }

    // 로드 기능
    public DaniTechPlayerModel RequstLoadSaveData()
    {
        string path = GetPath();
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            DaniTechPlayerModel data = JsonUtility.FromJson<DaniTechPlayerModel>(json);
            Debug.Log("데이터를 불러왔습니다.");
            return data;
        }
        else
        {
            Debug.LogWarning("세이브 파일이 없습니다. 새 데이터를 생성합니다.");
            var playerData = GetDefaultPlayerData();
            RequstSaveData(GetDefaultPlayerData());
            return playerData;
        }
    }

    public DaniTechPlayerModel GetDefaultPlayerData()
    {
        var newPlayerData = new DaniTechPlayerModel();
        newPlayerData.PlayerName = "NoName";
        newPlayerData.PlayerTotalExp = 0;
        return newPlayerData;
    }
}
