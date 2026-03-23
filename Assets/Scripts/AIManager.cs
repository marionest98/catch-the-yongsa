using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // 용사 이동 실행 - TurnManager에서 코루틴으로 호출
    // 플레이어 턴은 용사가 성물을 먹은 후에만 시작됨
    public IEnumerator MoveYongsaCoroutine()
    {
        StageData stageData = StageManager.Instance.stageData;
        // 무한루프 안전장치 - 이게 터지면 AI 버그
        int maxIterations = stageData.width * stageData.height * 2;
        int iterations = 0;

        while (true)
        {
            iterations++;
            if (iterations > maxIterations)
            {
                Debug.LogError("최대 반복 초과 - AI 버그 확인 필요");
                break;
            }

            Vector2Int yongsaPos = CharacterManager.Instance.yongsaPos;
            Vector2Int? relicPos = FindNearestRelic(yongsaPos);

            if (relicPos == null)
            {
                Debug.Log("이동할 성물 없음");
                break;
            }

            HashSet<Vector2Int> previouslyVisited = CharacterManager.Instance.GetVisitedTiles();
            bool hasUnvisited = false;
            for (int x = 0; x < stageData.width; x++)
            {
                for (int y = 0; y < stageData.height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (GridManager.Instance.IsWalkable(x, y) &&
                        !previouslyVisited.Contains(pos) &&
                        pos != relicPos.Value)
                    {
                        hasUnvisited = true;
                        break;
                    }
                }
                if (hasUnvisited) { break; }
            }

            Vector2Int[] dirs = {
                new Vector2Int(0, 1), new Vector2Int(0, -1),
                new Vector2Int(-1, 0), new Vector2Int(1, 0)
            };

            bool ateRelic = false;
            if (!hasUnvisited)
            {
                foreach (Vector2Int dir in dirs)
                {
                    Vector2Int adjacent = yongsaPos + dir;
                    if (adjacent == relicPos.Value)
                    {
                        GridManager.Instance.RemoveRelic(adjacent.x, adjacent.y);
                        CharacterManager.Instance.AddFanClub();
                        CharacterManager.Instance.MoveYongsa(adjacent);
                        // Lerp 완료까지 대기
                        yield return new WaitUntil(() => !CharacterManager.Instance.isMoving);
                        CharacterManager.Instance.ResetVisitedTiles();
                        Debug.Log("성물 획득!");
                        ateRelic = true;
                        break;
                    }
                }
            }
            if (ateRelic) { break; }

            List<Vector2Int> bestPath = FindLongestPath(yongsaPos, relicPos);

            if (bestPath.Count <= 1)
            {
                if (WinConditionChecker.Instance.CheckWin())
                {
                    WinConditionChecker.Instance.TriggerWin();
                    yield break;
                }
                break;
            }

            Vector2Int nextMove = bestPath[1];

            if (GridManager.Instance.GetCell(nextMove.x, nextMove.y) == GridManager.CellType.Relic)
            {
                GridManager.Instance.RemoveRelic(nextMove.x, nextMove.y);
                CharacterManager.Instance.AddFanClub();
                CharacterManager.Instance.MoveYongsa(nextMove);
                // Lerp 완료까지 대기
                yield return new WaitUntil(() => !CharacterManager.Instance.isMoving);
                CharacterManager.Instance.ResetVisitedTiles();
                Debug.Log("성물 획득!");
                break;
            }

            // 일반 이동
            CharacterManager.Instance.MoveYongsa(nextMove);
            // Lerp 완료까지 대기
            yield return new WaitUntil(() => !CharacterManager.Instance.isMoving);
        }

        // 승리 판정
        if (WinConditionChecker.Instance.CheckWin())
        {
            WinConditionChecker.Instance.TriggerWin();
            yield break;
        }

        TurnManager.Instance.EndYongsaTurn();
    }

    // 가장 가까운 성물 위치 찾기
    private Vector2Int? FindNearestRelic(Vector2Int from)
    {
        StageData stageData = StageManager.Instance.stageData;
        Vector2Int? nearest = null;
        int minDist = int.MaxValue;

        for (int x = 0; x < stageData.width; x++)
        {
            for (int y = 0; y < stageData.height; y++)
            {
                if (GridManager.Instance.GetCell(x, y) == GridManager.CellType.Relic)
                {
                    int dist = Mathf.Abs(x - from.x) + Mathf.Abs(y - from.y);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = new Vector2Int(x, y);
                    }
                }
            }
        }

        return nearest;
    }

    // 용사와 팬클럽이 이동했을 때의 가상 grid 생성
    // 시작 팬클럽 생성도 시뮬레이션에 포함
    private GridManager.CellType[,] SimulateMove(Vector2Int newYongsaPos, List<Vector2Int> currentFanPositions)
    {
        StageData stageData = StageManager.Instance.stageData;
        GridManager.CellType[,] simGrid = new GridManager.CellType[stageData.width, stageData.height];

        // 현재 grid 상태 복사
        for (int x = 0; x < stageData.width; x++)
        {
            for (int y = 0; y < stageData.height; y++)
            {
                simGrid[x, y] = GridManager.Instance.GetCell(x, y);
            }
        }

        // 기존 용사/팬클럽 위치 초기화
        for (int x = 0; x < stageData.width; x++)
        {
            for (int y = 0; y < stageData.height; y++)
            {
                if (simGrid[x, y] == GridManager.CellType.Yongsa || simGrid[x, y] == GridManager.CellType.FanClub)
                {
                    simGrid[x, y] = GridManager.CellType.Empty;
                }
            }
        }

        // 팬클럽 이동 시뮬레이션 (꼬리부터 역순)
        List<Vector2Int> newFanPositions = new List<Vector2Int>(currentFanPositions);
        for (int i = newFanPositions.Count - 1; i > 0; i--)
        {
            newFanPositions[i] = newFanPositions[i - 1];
        }
        if (newFanPositions.Count > 0)
        {
            newFanPositions[0] = CharacterManager.Instance.yongsaPos;
        }

        // 시작 팬클럽이 남아있으면 시작 위치에 팬클럽 추가
        int remainingStartFan = CharacterManager.Instance.GetRemainingStartFanCount();
        Vector2Int yongsaStartPos = CharacterManager.Instance.GetYongsaStartPos();
        if (remainingStartFan > 0)
        {
            newFanPositions.Add(yongsaStartPos);
        }

        // 새 위치 반영
        simGrid[newYongsaPos.x, newYongsaPos.y] = GridManager.CellType.Yongsa;
        foreach (Vector2Int pos in newFanPositions)
        {
            simGrid[pos.x, pos.y] = GridManager.CellType.FanClub;
        }

        return simGrid;
    }

    // 가상 grid 기반 Flood Fill - 이동 후 열린 공간 크기 계산
    private int FloodFillOnGrid(Vector2Int start, GridManager.CellType[,] simGrid)
    {
        StageData stageData = StageManager.Instance.stageData;
        bool[,] visited = new bool[stageData.width, stageData.height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        int count = 0;

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(-1, 0), new Vector2Int(1, 0)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            count++;

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;

                if (next.x < 0 || next.x >= stageData.width) { continue; }
                if (next.y < 0 || next.y >= stageData.height) { continue; }
                if (visited[next.x, next.y]) { continue; }

                GridManager.CellType cell = simGrid[next.x, next.y];
                if (cell == GridManager.CellType.Wall || cell == GridManager.CellType.FanClub || cell == GridManager.CellType.Yongsa) { continue; }

                visited[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }

        return count;
    }

    // 각 이동 방향 시뮬레이션 후 열린 공간 가장 넓은 방향 선택
    // 공간이 같으면 성물에 가까운 쪽 선택 (-distToRelic으로 자연스럽게 수렴)
    private List<Vector2Int> FindLongestPath(Vector2Int start, Vector2Int? relicPos)
    {
        StageData stageData = StageManager.Instance.stageData;
        List<Vector2Int> currentFanPositions = CharacterManager.Instance.GetFanClubPositions();

        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(-1, 0), new Vector2Int(1, 0)
        };

        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(start);

        if (!relicPos.HasValue) { return path; }

        Vector2Int bestMove = start;
        int bestScore = -1;

        foreach (Vector2Int dir in directions)
        {
            Vector2Int next = start + dir;

            // 맵 범위 벗어나면 건너뜀
            if (next.x < 0 || next.x >= stageData.width) { continue; }
            if (next.y < 0 || next.y >= stageData.height) { continue; }

            // 이동 불가능한 칸 건너뜀
            if (!GridManager.Instance.IsWalkable(next.x, next.y)) { continue; }

            // 이동 후 상태 시뮬레이션
            GridManager.CellType[,] simGrid = SimulateMove(next, currentFanPositions);

            // 시뮬레이션 후 성물까지 경로 없으면 건너뜀
            if (BFSDistanceOnGrid(next, relicPos.Value, simGrid) < 0) { continue; }

            // 이동 후 열린 공간 크기 (클수록 좋음)
            int openSpace = FloodFillOnGrid(next, simGrid);

            // 성물까지 거리 (멀수록 도망, 가까울수록 수렴)
            int distToRelic = Mathf.Abs(next.x - relicPos.Value.x) + Mathf.Abs(next.y - relicPos.Value.y);

            // 점수 계산: 열린 공간 최우선, 공간 같으면 성물에 가까운 쪽
            // openSpace * 100: 공간이 거리보다 항상 우선되도록
            // -distToRelic: 멀수록 점수 낮아져서 자연스럽게 성물 방향으로 수렴
            int score = (openSpace * 100) - distToRelic;

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = next;
            }
        }

        if (bestMove != start)
        {
            path.Add(bestMove);
        }
        return path;
    }

    // 시뮬레이션 grid 기반 BFS 거리 계산
    // 경로 있으면 거리 반환, 없으면 -1 반환
    private int BFSDistanceOnGrid(Vector2Int start, Vector2Int target, GridManager.CellType[,] simGrid)
    {
        if (start == target) { return 0; }

        StageData stageData = StageManager.Instance.stageData;
        bool[,] visited = new bool[stageData.width, stageData.height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> dist = new Dictionary<Vector2Int, int>();

        queue.Enqueue(start);
        visited[start.x, start.y] = true;
        dist[start] = 0;

        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(-1, 0), new Vector2Int(1, 0)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;

                if (next.x < 0 || next.x >= stageData.width) { continue; }
                if (next.y < 0 || next.y >= stageData.height) { continue; }
                if (visited[next.x, next.y]) { continue; }

                GridManager.CellType cell = simGrid[next.x, next.y];
                if (cell != GridManager.CellType.Empty && cell != GridManager.CellType.Relic) { continue; }

                visited[next.x, next.y] = true;
                dist[next] = dist[current] + 1;

                if (next == target) { return dist[next]; }

                queue.Enqueue(next);
            }
        }

        return -1;
    }

    // 벽 + 팬클럽 기준으로 용사가 성물까지 갈 수 있는지 확인
    // 경로 있으면 true, 막히면 false
    public bool HasPathToRelic(Vector2Int from, Vector2Int target)
    {
        if (from == target) { return true; }

        StageData stageData = StageManager.Instance.stageData;
        bool[,] visited = new bool[stageData.width, stageData.height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(from);
        visited[from.x, from.y] = true;

        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(-1, 0), new Vector2Int(1, 0)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;

                if (next.x < 0 || next.x >= stageData.width) { continue; }
                if (next.y < 0 || next.y >= stageData.height) { continue; }
                if (visited[next.x, next.y]) { continue; }

                // 벽 + 팬클럽 모두 막힘으로 처리
                GridManager.CellType cell = GridManager.Instance.GetCell(next.x, next.y);
                if (cell == GridManager.CellType.Wall || cell == GridManager.CellType.FanClub) { continue; }

                visited[next.x, next.y] = true;

                if (next == target) { return true; }

                queue.Enqueue(next);
            }
        }

        return false;
    }
}