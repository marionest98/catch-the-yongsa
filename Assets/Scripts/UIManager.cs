using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("사이드패널 연결")]
    public TMP_Text stageText;        // StageText 오브젝트 연결
    public TMP_Text relicCountText;   // RelicCountText 오브젝트 연결
    public Button speedButton;        // SpeedButton 오브젝트 연결
    private TMP_Text speedButtonText; // 버튼 안 텍스트 (Start에서 자동으로 찾아옴)
    public TMP_Text dialogueText;     // 악마 대사 텍스트 연결

    [Header("클리어 팝업 연결")]
    public GameObject clearPopup;
    public Image star1Image;            // Star1 오브젝트 연결
    public Image star2Image;            // Star2 오브젝트 연결
    public Image star3Image;            // Star3 오브젝트 연결
    public Sprite starActiveSprite;     // Active@4x 스프라이트 연결
    public Sprite starUnactiveSprite;   // Unactive@4x 스프라이트 연결

    [Header("일시정지 팝업 연결")]
    public GameObject pausePopup;
    public Image pauseStar1Image;  // PausePopup Star1 연결
    public Image pauseStar2Image;  // PausePopup Star2 연결
    public Image pauseStar3Image;  // PausePopup Star3 연결

    // =====================
    // 초기화
    // =====================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 팝업 숨기기
        clearPopup.SetActive(false);
        pausePopup.SetActive(false);

        // 씬 안 모든 버튼에 클릭 후 선택 해제 자동 등록
        // FindObjectsByType: 씬 안의 모든 Button 컴포넌트를 찾아주는 함수
        foreach (Button btn in FindObjectsByType<Button>(FindObjectsSortMode.None))
        {
            btn.onClick.AddListener(() =>
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            });
        }

        // 스테이지 번호 표시 - PlayerPrefs에서 읽어서 한 번만 세팅
        int stageIndex = PlayerPrefs.GetInt("SelectedStage", 0);
        stageText.text = (stageIndex + 1) + "스테이지";

        // 성물 카운트 초기화
        relicCountText.text = "성물: 0";

        // SpeedButton 안의 TMP_Text 자동으로 찾기
        // GetComponentInChildren: 자식 오브젝트에서 해당 컴포넌트를 찾아주는 함수
        speedButtonText = speedButton.GetComponentInChildren<TMP_Text>();

        // 저장된 배속 상태 불러와서 버튼 텍스트 세팅
        bool isDoubleSpeed = PlayerPrefs.GetInt("IsDoubleSpeed", 0) == 1;
        speedButtonText.text = isDoubleSpeed ? "2배속" : "1배속";

        ShowStartDialogue();
    }

    // =====================
    // HUD 업데이트
    // =====================

    // 성물 카운트 업데이트 - GridManager.PlaceRelic()에서 호출
    public void UpdateRelicCount(int count)
    {
        relicCountText.text = "성물: " + count;
    }

    // 배속 버튼 텍스트 갱신 - TurnManager 스페이스바 입력 시 호출
    public void UpdateSpeedButtonText(bool isDoubleSpeed)
    {
        speedButtonText.text = isDoubleSpeed ? "2배속" : "1배속";
    }

    // =====================
    // 대사
    // =====================

    // 스테이지 시작 대사 표시 - Start()에서 호출
    public void ShowStartDialogue()
    {
        StageData stageData = StageManager.Instance.stageData;
        if (dialogueText != null)
        {
            dialogueText.text = stageData.startDialogue;
        }
    }

    // 배치 불가 대사 표시 - GridManager에서 호출
    public void ShowCantPlaceDialogue()
    {
        if (dialogueText != null)
        {
            dialogueText.text = "용사는 키가 작아서 그런 곳에 두면 못 본다고. 다른 곳에 놔봐.";
        }
    }

    // =====================
    // 버튼 동작
    // =====================

    // 배속 토글 버튼 - SpeedButton OnClick에 등록
    // TurnManager의 스페이스바 로직이랑 동일하게 처리
    public void OnSpeedButton()
    {
        // 플레이어 턴이 아닐 때는 배속 변경 불가
        if (TurnManager.Instance.currentTurn != TurnManager.TurnState.PlayerTurn) { return; }

        bool isDoubleSpeed = PlayerPrefs.GetInt("IsDoubleSpeed", 0) == 1;
        isDoubleSpeed = !isDoubleSpeed;

        CharacterManager.Instance.moveSpeed = isDoubleSpeed ? 0.25f : 0.5f;
        PlayerPrefs.SetInt("IsDoubleSpeed", isDoubleSpeed ? 1 : 0);
        PlayerPrefs.Save();

        speedButtonText.text = isDoubleSpeed ? "2배속" : "1배속";
        Debug.Log("배속 버튼: " + (isDoubleSpeed ? "2배속" : "1배속"));
    }

    // 계속하기 버튼
    public void OnResumeButton()
    {
        pausePopup.SetActive(false);
        TurnManager.Instance.enabled = true;
    }

    // 다시하기 버튼
    public void OnRetryButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 나가기 버튼
    public void OnExitButton()
    {
        SceneManager.LoadScene("StageSelectScene");
    }

    // 버튼 클릭 후 선택 상태 해제 - 호버 정상 작동을 위해 필요
    // 모든 버튼 OnClick에 이 함수 추가 등록
    public void DeselectButton()
    {
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    // =====================
    // 팝업
    // =====================

    // 승리 팝업 표시 - WinConditionChecker에서 호출
    public void ShowClearPopup(int stars)
    {
        clearPopup.SetActive(true);
        StartCoroutine(PlayStarAnimation(stars));
        Debug.Log("클리어 팝업 표시 - 별: " + stars + "개");
    }

    // 별 수에 따라 순서대로 애니메이션 실행
    private IEnumerator PlayStarAnimation(int stars)
    {
        // 전부 Unactive + scale 0으로 초기화
        star1Image.sprite = starUnactiveSprite;
        star2Image.sprite = starUnactiveSprite;
        star3Image.sprite = starUnactiveSprite;
        star1Image.transform.localScale = Vector3.zero;
        star2Image.transform.localScale = Vector3.zero;
        star3Image.transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(0.3f); // 팝업 뜨고 잠깐 대기

        // 별 수에 따라 Active/Unactive 스프라이트 전달
        // yield return StartCoroutine(): 이 코루틴이 끝날 때까지 기다렸다가 다음 줄 실행
        yield return StartCoroutine(AnimateStar(star1Image, 0f, stars >= 1 ? starActiveSprite : starUnactiveSprite));
        yield return StartCoroutine(AnimateStar(star2Image, 0.15f, stars >= 2 ? starActiveSprite : starUnactiveSprite));
        yield return StartCoroutine(AnimateStar(star3Image, 0.15f, stars >= 3 ? starActiveSprite : starUnactiveSprite));
    }

    // 별 하나를 0→1.3→1.0 scale로 튀어오르게 채우는 코루틴
    // image: 대상 별 / delay: 시작 전 대기 시간 / sprite: 사용할 스프라이트 (Active or Unactive)
    private IEnumerator AnimateStar(Image image, float delay, Sprite sprite)
    {
        yield return new WaitForSeconds(delay);

        // 전달받은 스프라이트로 교체 (Active or Unactive)
        image.sprite = sprite;

        float duration = 0.25f; // 0→1.3 구간 시간
        float bounce = 0.1f;    // 1.3→1.0 구간 시간

        // scale 0 → 1.3 (커지는 구간)
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float scale = Mathf.Lerp(0f, 1.3f, t);
            image.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // scale 1.3 → 1.0 (튕기는 구간)
        elapsed = 0f;
        while (elapsed < bounce)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bounce);
            float scale = Mathf.Lerp(1.3f, 1.0f, t);
            image.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        image.transform.localScale = Vector3.one; // 정확히 1.0으로 고정
    }

    // 일시정지 팝업 표시 - PauseButton에서 호출
    public void ShowPausePopup()
    {
        int currentStage = PlayerPrefs.GetInt("SelectedStage", 0);
        int bestStars = PlayerPrefs.GetInt("StarCount_" + currentStage, 0);

        // 별 수에 따라 Active/Unactive 스프라이트 교체
        pauseStar1Image.sprite = bestStars >= 1 ? starActiveSprite : starUnactiveSprite;
        pauseStar2Image.sprite = bestStars >= 2 ? starActiveSprite : starUnactiveSprite;
        pauseStar3Image.sprite = bestStars >= 3 ? starActiveSprite : starUnactiveSprite;

        // scale 초기화 (ClearPopup 애니메이션이 건드렸을 수 있음)
        pauseStar1Image.transform.localScale = Vector3.one;
        pauseStar2Image.transform.localScale = Vector3.one;
        pauseStar3Image.transform.localScale = Vector3.one;

        pausePopup.SetActive(true);
        TurnManager.Instance.enabled = false;
    }
}