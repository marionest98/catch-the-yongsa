using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("프리팹 연결")]
    public GameObject yongsaPrefab;
    public GameObject fanClubPrefab;
    private GameObject yongsaObject;

    public Vector2Int yongsaPos { get; private set; }
    private int remainingStartFanCount;
    private Vector2Int yongsaStartPos;

    private HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();
    private List<Vector2Int> fanClubPositions = new List<Vector2Int>();
    private List<GameObject> fanClubObjects = new List<GameObject>();

    // 이동 속도 (초/칸)
    public float moveSpeed = 0.5f;

    // 이동 중 여부 - AIManager에서 대기할 때 사용
    public bool isMoving { get; private set; } = false;

    // 되돌리기용 이전 상태 저장 변수
    private Vector2Int prevYongsaPos;
    private List<Vector2Int> prevFanClubPositions;
    private int prevRemainingStartFanCount;
    private HashSet<Vector2Int> prevVisitedTiles;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnYongsa();
    }

    void SpawnYongsa()
    {
        StageData stageData = StageManager.Instance.stageData;
        yongsaPos = stageData.yongsaStartPos;
        float x = yongsaPos.x - (stageData.width - 1) / 2f;
        float y = yongsaPos.y - (stageData.height - 1) / 2f;
        yongsaObject = Instantiate(yongsaPrefab, new Vector3(x, y, -1), Quaternion.identity);
        Debug.Log("용사 생성 완료! 위치: " + yongsaPos);

        remainingStartFanCount = stageData.startFanCount;
        yongsaStartPos = yongsaPos;
        visitedTiles.Add(yongsaPos);
    }

    // 용사 이동 함수 - grid 상태 업데이트 + 오브젝트 이동 코루틴 시작
    public void MoveYongsa(Vector2Int newPos)
    {
        StageData stageData = StageManager.Instance.stageData;

        // 이동 방향에 따라 스프라이트 좌우 반전
        // newPos.x > yongsaPos.x = 오른쪽 이동, Flip X 해제
        // newPos.x < yongsaPos.x = 왼쪽 이동, Flip X 적용
        // 상하 이동은 방향 유지 (x가 같으면 건드리지 않음)
        SpriteRenderer sr = yongsaObject.GetComponent<SpriteRenderer>();
        if (newPos.x > yongsaPos.x) { sr.flipX = true; }
        else if (newPos.x < yongsaPos.x) { sr.flipX = false; }

        // 팬클럽 grid 이동
        for (int i = fanClubPositions.Count - 1; i > 0; i--)
        {
            fanClubPositions[i] = fanClubPositions[i - 1];
        }
        if (fanClubPositions.Count > 0)
        {
            fanClubPositions[0] = yongsaPos;
        }

        // 시작 팬클럽 생성
        if (remainingStartFanCount > 0)
        {
            fanClubPositions.Add(yongsaStartPos);
            float sfx = yongsaStartPos.x - (stageData.width - 1) / 2f;
            float sfy = yongsaStartPos.y - (stageData.height - 1) / 2f;
            GameObject fanObj = Instantiate(fanClubPrefab, new Vector3(sfx, sfy, -1), Quaternion.identity);
            fanClubObjects.Add(fanObj);
            remainingStartFanCount--;
            Debug.Log("시작 팬클럽 생성! 남은 수: " + remainingStartFanCount);
        }

        // 용사 위치 업데이트
        yongsaPos = newPos;
        visitedTiles.Add(yongsaPos);

        // GridManager 상태 업데이트
        GridManager.Instance.UpdateCharacterPositions(yongsaPos, fanClubPositions);

        Debug.Log("용사 이동: " + newPos);

        // 오브젝트 부드럽게 이동 시작
        StartCoroutine(SmoothMoveAll());
    }

    // 용사 + 팬클럽 전체 부드럽게 이동
    private IEnumerator SmoothMoveAll()
    {
        isMoving = true;

        StageData stageData = StageManager.Instance.stageData;

        // 용사 목표 위치
        Vector3 yongsaTarget = new Vector3(
            yongsaPos.x - (stageData.width - 1) / 2f,
            yongsaPos.y - (stageData.height - 1) / 2f,
            -1);

        // 팬클럽 목표 위치 목록
        List<Vector3> fanTargets = new List<Vector3>();
        for (int i = 0; i < fanClubPositions.Count; i++)
        {
            fanTargets.Add(new Vector3(
                fanClubPositions[i].x - (stageData.width - 1) / 2f,
                fanClubPositions[i].y - (stageData.height - 1) / 2f,
                -1));
        }

        // 시작 위치 기록
        Vector3 yongsaStart = yongsaObject.transform.position;
        List<Vector3> fanStarts = new List<Vector3>();
        for (int i = 0; i < fanClubObjects.Count; i++)
        {
            fanStarts.Add(fanClubObjects[i].transform.position);
        }

        // Lerp로 부드럽게 이동
        float elapsed = 0f;
        while (elapsed < moveSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveSpeed);

            yongsaObject.transform.position = Vector3.Lerp(yongsaStart, yongsaTarget, t);

            for (int i = 0; i < fanClubObjects.Count; i++)
            {
                fanClubObjects[i].transform.position = Vector3.Lerp(fanStarts[i], fanTargets[i], t);
            }

            yield return null;
        }

        // 정확한 위치로 고정
        yongsaObject.transform.position = yongsaTarget;
        for (int i = 0; i < fanClubObjects.Count; i++)
        {
            fanClubObjects[i].transform.position = fanTargets[i];
        }

        isMoving = false;
    }

    // 팬클럽 추가 함수 - 성물 먹을 때 호출
    public void AddFanClub()
    {
        StageData stageData = StageManager.Instance.stageData;

        Vector2Int newPos = fanClubPositions.Count > 0
            ? fanClubPositions[fanClubPositions.Count - 1]
            : yongsaPos;

        fanClubPositions.Add(newPos);

        float fx = newPos.x - (stageData.width - 1) / 2f;
        float fy = newPos.y - (stageData.height - 1) / 2f;
        GameObject fanObj = Instantiate(fanClubPrefab, new Vector3(fx, fy, -1), Quaternion.identity);
        fanClubObjects.Add(fanObj);

        Debug.Log("팬클럽 추가! 현재 팬클럽 수: " + fanClubPositions.Count);
    }

    public List<Vector2Int> GetFanClubPositions()
    {
        return new List<Vector2Int>(fanClubPositions);
    }

    public HashSet<Vector2Int> GetVisitedTiles()
    {
        return new HashSet<Vector2Int>(visitedTiles);
    }

    public int GetRemainingStartFanCount()
    {
        return remainingStartFanCount;
    }

    public Vector2Int GetYongsaStartPos()
    {
        return yongsaStartPos;
    }

    public void ResetVisitedTiles()
    {
        visitedTiles.Clear();
        visitedTiles.Add(yongsaPos);
    }

    // 현재 상태 저장 - 성물 배치 확정 직전에 호출
    public void SaveState()
    {
        prevYongsaPos = yongsaPos;
        prevFanClubPositions = new List<Vector2Int>(fanClubPositions);
        prevRemainingStartFanCount = remainingStartFanCount;
        prevVisitedTiles = new HashSet<Vector2Int>(visitedTiles);
    }

    // 저장된 상태로 복구 - 되돌리기 버튼 눌렀을 때 호출
    public void RestoreState()
    {
        // 기존 팬클럽 오브젝트 전부 제거
        foreach (GameObject obj in fanClubObjects)
        {
            Destroy(obj);
        }
        fanClubObjects.Clear();

        // 이전 상태 복구
        yongsaPos = prevYongsaPos;
        fanClubPositions = new List<Vector2Int>(prevFanClubPositions);
        remainingStartFanCount = prevRemainingStartFanCount;
        visitedTiles = new HashSet<Vector2Int>(prevVisitedTiles);

        // 팬클럽 오브젝트 다시 생성
        StageData stageData = StageManager.Instance.stageData;
        foreach (Vector2Int pos in fanClubPositions)
        {
            float fx = pos.x - (stageData.width - 1) / 2f;
            float fy = pos.y - (stageData.height - 1) / 2f;
            GameObject fanObj = Instantiate(fanClubPrefab, new Vector3(fx, fy, -1), Quaternion.identity);
            fanClubObjects.Add(fanObj);
        }

        // 용사 오브젝트 위치 이동
        float x = yongsaPos.x - (stageData.width - 1) / 2f;
        float y = yongsaPos.y - (stageData.height - 1) / 2f;
        yongsaObject.transform.position = new Vector3(x, y, -1);

        // GridManager 상태도 업데이트
        GridManager.Instance.UpdateCharacterPositions(yongsaPos, fanClubPositions);
    }
}