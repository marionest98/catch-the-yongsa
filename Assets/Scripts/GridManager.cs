using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("프리팹 연결")]
    public GameObject cellPrefab; // Inspector에서 프리팹 연결하는 슬롯
    public GameObject wallPrefab; // Inspector에서 Wall 프리팹 연결하는 슬롯
    public GameObject relicPrefab; // Inspector에서 Relic 프리팹 연결하는 슬롯

    // 각 칸의 상태 타입 (숫자 대신 이름으로 관리)
    public enum CellType
    {
        Empty,   // 빈 칸
        Wall,    // 벽/장애물
        Yongsa,  // 용사
        FanClub, // 팬클럽
        Relic    // 성물
    }

    // 2차원 배열로 그리드 상태 저장 (엑셀 표처럼 가로x세로)
    private CellType[,] grid;
    // 성물 오브젝트 추적 (좌표 → 오브젝트)
    private Dictionary<Vector2Int, GameObject> relicObjects = new Dictionary<Vector2Int, GameObject>();
    // 성물 사용 개수 카운트 - 클리어 시 별 판정에 사용
    public int relicUsedCount { get; private set; } = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitGrid();
    }

    void InitGrid()
    {
        StageData stageData = StageManager.Instance.stageData;
        grid = new CellType[stageData.width, stageData.height];

        // Cell과 Wall을 묶어줄 부모 오브젝트 생성
        GameObject cellHolder = new GameObject("CellHolder");
        GameObject wallHolder = new GameObject("WallHolder");

        // StageData의 rows 문자열을 읽어서 grid 데이터 설정
        for (int y = 0; y < stageData.height; y++)
        {
            for (int x = 0; x < stageData.width; x++)
            {
                if (stageData.rows[stageData.height - 1 - y][x] == '1')
                {
                    grid[x, y] = CellType.Wall;
                }
                else if (stageData.rows[stageData.height - 1 - y][x] == '2')
                {
                    grid[x, y] = CellType.Empty; // 용사 위치는 빈칸으로 처리
                    stageData.yongsaStartPos = new Vector2Int(x, y); // 용사 시작 위치 자동 설정
                }
                else
                {
                    grid[x, y] = CellType.Empty;
                }
            }
        }

        // grid 데이터 기반으로 오브젝트 생성
        for (int x = 0; x < stageData.width; x++)
        {
            for (int y = 0; y < stageData.height; y++)
            {
                Vector3 pos = new Vector3((x - (stageData.width - 1) / 2f), (y - (stageData.height - 1) / 2f), 0);

                // 모든 칸에 바닥(Cell) 생성 후 CellHolder 아래로 묶기
                GameObject cell = Instantiate(cellPrefab, pos, Quaternion.identity);
                cell.transform.SetParent(cellHolder.transform);

                // 벽인 칸엔 Wall 생성 후 WallHolder 아래로 묶기
                if (grid[x, y] == CellType.Wall)
                {
                    GameObject wall = Instantiate(wallPrefab, new Vector3(pos.x, pos.y, -1), Quaternion.identity);
                    wall.transform.SetParent(wallHolder.transform);
                }
            }
        }

        Debug.Log("그리드 초기화 완료! 크기: " + stageData.width + "x" + stageData.height);
    }

    // 성물 배치 함수 - 플레이어가 클릭한 칸에 성물 생성
    public void PlaceRelic(int x, int y)
    {
        // 승리 판정 먼저 체크 (4방향 막힘)
        if (WinConditionChecker.Instance.CheckWin())
        {
            WinConditionChecker.Instance.TriggerWin();
            return;
        }

        if (grid[x, y] != CellType.Empty) { return; } // 빈 칸이 아니면 배치 불가

        // 임시로 성물 배치 후 경로 확인
        grid[x, y] = CellType.Relic;

        Vector2Int yongsaPos = CharacterManager.Instance.yongsaPos;

        // 벽 또는 팬클럽으로 막혀 경로 없으면 배치 불가
        if (!AIManager.Instance.HasPathToRelic(yongsaPos, new Vector2Int(x, y)))
        {
            grid[x, y] = CellType.Empty;
            Debug.Log("배치 불가: 경로 없음");
            return;
        }

        // 경로 있으면 정식 배치
        Vector3 pos = new Vector3((x - (StageManager.Instance.stageData.width - 1) / 2f),
                                  (y - (StageManager.Instance.stageData.height - 1) / 2f), -1);
        GameObject relicObj = Instantiate(relicPrefab, pos, Quaternion.identity);
        relicObjects[new Vector2Int(x, y)] = relicObj;
        
        relicUsedCount++;// 성물 사용 횟수 증가
        Debug.Log("성물 사용 횟수: " + relicUsedCount);

        Debug.Log("성물 배치: " + x + ", " + y);
        TurnManager.Instance.EndPlayerTurn();
    }

    // 캐릭터 위치 업데이트 - CharacterManager에서 이동 후 호출
    public void UpdateCharacterPositions(Vector2Int yongsaPos, List<Vector2Int> fanClubPositions)
    {
        // 기존 캐릭터 위치 초기화 (벽이랑 성물 제외하고 전부 Empty로)
        for (int x = 0; x < StageManager.Instance.stageData.width; x++)
        {
            for (int y = 0; y < StageManager.Instance.stageData.height; y++)
            {
                if (grid[x, y] == CellType.Yongsa || grid[x, y] == CellType.FanClub)
                {
                    grid[x, y] = CellType.Empty;
                }
            }
        }

        // 새 위치 반영
        grid[yongsaPos.x, yongsaPos.y] = CellType.Yongsa;
        foreach (Vector2Int pos in fanClubPositions)
        {
            grid[pos.x, pos.y] = CellType.FanClub;
        }
    }

    // 특정 위치의 타입 반환 - AIManager에서 경로 계산할 때 사용
    public CellType GetCell(int x, int y)
    {
        return grid[x, y];
    }

    // 특정 위치가 이동 가능한지 확인 - AIManager에서 사용
    public bool IsWalkable(int x, int y)
    {
        if (x < 0 || x >= StageManager.Instance.stageData.width) { return false; }
        if (y < 0 || y >= StageManager.Instance.stageData.height) { return false; }
        return grid[x, y] == CellType.Empty || grid[x, y] == CellType.Relic;
    }

    // 성물 제거 - 용사가 성물 먹었을 때 호출
    public void RemoveRelic(int x, int y)
    {
        grid[x, y] = CellType.Empty;
        Vector2Int key = new Vector2Int(x, y);
        if (relicObjects.ContainsKey(key))
        {
            Destroy(relicObjects[key]); // 오브젝트 제거
            relicObjects.Remove(key);   // 딕셔너리에서도 제거
        }
        Debug.Log("성물 제거: " + x + ", " + y);
    }
}