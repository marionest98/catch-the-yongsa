using UnityEngine;

public class WinConditionChecker : MonoBehaviour
{
    public static WinConditionChecker Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // 용사 4방향이 전부 막혔는지 확인
    public bool CheckWin()
    {
        Vector2Int yongsaPos = CharacterManager.Instance.yongsaPos;

        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int nextPos = yongsaPos + dir;
            if (GridManager.Instance.IsWalkable(nextPos.x, nextPos.y))
            {
                return false; // 이동 가능한 방향이 하나라도 있으면 승리 아님
            }
        }

        return true; // 4방향 전부 막히면 승리
    }

    // 승리 처리 - 모든 승리 조건에서 이 함수 호출
    public void TriggerWin()
    {
        Debug.Log("승리!");

        // 현재 스테이지 클리어 저장
        int currentStage = PlayerPrefs.GetInt("SelectedStage", 0);
        PlayerPrefs.SetInt("StageClear_" + currentStage, 1);

        // 별 판정 - 성물 사용 개수 기준
        int relicUsed = GridManager.Instance.relicUsedCount;
        StageData stageData = StageManager.Instance.stageData;
        int stars;
        if (relicUsed <= stageData.star3Threshold)
        {
            stars = 3;
        }
        else if (relicUsed <= stageData.star2Threshold)
        {
            stars = 2;
        }
        else
        {
            stars = 1;
        }

        // 기존 별 수보다 높을 때만 저장 (최고 기록 유지)
        int prevStars = PlayerPrefs.GetInt("StarCount_" + currentStage, 0);
        if (stars > prevStars)
        {
            PlayerPrefs.SetInt("StarCount_" + currentStage, stars);
        }

        PlayerPrefs.Save();
        Debug.Log((currentStage + 1) + "스테이지 클리어! 별: " + stars + "개 (성물 " + relicUsed + "개 사용)");

        TurnManager.Instance.enabled = false; // 턴 진행 중지
        UIManager.Instance.ShowClearPopup(stars); // 클리어 팝업에 별 수 전달
    }
}