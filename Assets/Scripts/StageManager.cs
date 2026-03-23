using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("스테이지 데이터 연결 (Inspector에서 10개 순서대로 연결)")]
    public StageData[] stageDataList; // 스테이지 데이터 배열

    // 현재 스테이지 데이터 - 다른 매니저들이 여기서 읽어감
    public StageData stageData { get; private set; }

    void Awake()
    {
        Instance = this;
        LoadStage();
    }

    // PlayerPrefs에서 선택된 스테이지 번호 읽어서 stageData 세팅
    // PlayerPrefs: 유니티 내장 저장소, 앱 종료해도 유지됨
    // "SelectedStage" 키로 저장된 값 읽음, 없으면 기본값 0 (1스테이지)
    private void LoadStage()
    {
        int stageIndex = PlayerPrefs.GetInt("SelectedStage", 0);
        stageData = stageDataList[stageIndex];
        Debug.Log("스테이지 로드: " + (stageIndex + 1) + "스테이지");
    }
}