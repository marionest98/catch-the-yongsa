using UnityEngine;
[CreateAssetMenu(fileName = "StageData", menuName = "Yongsa/StageData")]
public class StageData : ScriptableObject
{
    public int width = 5;   // 맵 가로 크기
    public int height = 5;  // 맵 세로 크기
    // 맵 데이터를 문자열 배열로 저장
    // 1=벽, 0=빈칸 / Inspector에서 직접 입력 가능
    // 예시: "11111" / "00000" / "11111"
    public string[] rows;
    public Vector2Int yongsaStartPos; // 용사 시작 위치 (Inspector에서 입력)
    public int startFanCount = 0; // 시작 팬클럽 수 (0이면 없음)

    [Header("3별 기준 (성물 사용 개수)")]
    public int star3Threshold; // 이 개수 이하면 3성
    public int star2Threshold; // 이 개수 이하면 2성 (초과면 1성)
}