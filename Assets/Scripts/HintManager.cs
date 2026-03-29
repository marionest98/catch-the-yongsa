using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance { get; private set; }

    [Header("프리팹 연결")]
    public GameObject hintPrefab; // Inspector에서 HintPrefab 연결

    [Header("UI 연결")]
    public TMP_Text hintButtonText;  // 힌트 버튼 안 텍스트 - 남은 무료 힌트 수
    public TMP_Text hintCountText;   // 버튼 위 텍스트 - 사용횟수/전체힌트수

    private int currentHintIndex = 0;  // 현재까지 공개된 힌트 인덱스
    private GameObject currentHintObj; // 현재 표시 중인 힌트 오브젝트
    private Coroutine blinkCoroutine;  // 점멸 코루틴 참조 (중복 실행 방지용)

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 시작 시 힌트 버튼 텍스트 초기화
        UpdateHintButtonText();
    }

    // 힌트 버튼 눌렀을 때 호출
    public void ShowNextHint()
    {
        StageData stageData = StageManager.Instance.stageData;

        // 무료 힌트 1개 소진 확인
        if (currentHintIndex >= 1)
        {
            Debug.Log("힌트 소진! 광고 or 재화로 추가 가능");
            // 나중에 광고 버튼 표시로 교체
            return;
        }

        // 힌트 좌표 범위 초과 확인
        if (currentHintIndex >= stageData.hintPositions.Length)
        {
            Debug.Log("힌트 소진!");
            // 나중에 광고 버튼 표시로 교체
            return;
        }

        // 기존 힌트 오브젝트 및 점멸 코루틴 제거
        ClearHint();

        // 다음 힌트 위치에 하이라이트 표시
        Vector2Int hintPos = stageData.hintPositions[currentHintIndex];
        float x = hintPos.x - (stageData.width - 1) / 2f;
        float y = hintPos.y - (stageData.height - 1) / 2f;
        currentHintObj = Instantiate(hintPrefab, new Vector3(x, y, -2), Quaternion.identity);

        // 점멸 코루틴 시작 - 2초 주기로 깜빡임
        blinkCoroutine = StartCoroutine(BlinkHint());

        currentHintIndex++;
        UpdateHintButtonText();
        Debug.Log("힌트 " + currentHintIndex + "번 표시: " + hintPos);
    }

    // 힌트 오브젝트 점멸 코루틴
    // Mathf.PingPong: 0→1→0→1 반복하는 함수, 부드러운 점멸에 사용
    private IEnumerator BlinkHint()
    {
        SpriteRenderer sr = currentHintObj.GetComponent<SpriteRenderer>();
        float maxAlpha = sr.color.a; // 원래 설정한 투명도를 최대값으로 사용
        float blinkSpeed = 1f / 2f;

        while (true)
        {
            // 0~1 사이를 2초 주기로 왕복 후 maxAlpha 비율로 스케일
            // 이렇게 하면 주기는 2초 유지, 최대값만 maxAlpha로 제한
            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            Color c = sr.color;
            c.a = t * maxAlpha;
            sr.color = c;
            yield return null;
        }
    }

    // 힌트 버튼 텍스트 업데이트
    private void UpdateHintButtonText()
    {
        StageData stageData = StageManager.Instance.stageData;

        // 버튼 위 텍스트: 사용횟수/전체힌트수
        if (hintCountText != null)
        {
            hintCountText.text = currentHintIndex + "/" + stageData.hintPositions.Length;
        }

        // 버튼 텍스트: 남은 무료 힌트 수 (무료는 1개 고정)
        if (hintButtonText != null)
        {
            int remaining = Mathf.Max(0, 1 - currentHintIndex);
            hintButtonText.text = "힌트 " + remaining;
        }
    }

    // 성물 배치 성공 시 힌트 제거 - GridManager에서 호출
    public void ClearHint()
    {
        // 점멸 코루틴 중지
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // 힌트 오브젝트 제거
        if (currentHintObj != null)
        {
            Destroy(currentHintObj);
            currentHintObj = null;
        }
    }
}