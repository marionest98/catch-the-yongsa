using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("사이드패널 연결")]
    public TMP_Text stageText;        // StageText 오브젝트 연결
    public TMP_Text relicCountText;   // RelicCountText 오브젝트 연결
    public Button speedButton;        // SpeedButton 오브젝트 연결
    private TMP_Text speedButtonText; // 버튼 안 텍스트 (Start에서 자동으로 찾아옴)

    [Header("클리어 팝업 연결")]
    public GameObject clearPopup;
    public TMP_Text starText;

    [Header("일시정지 팝업 연결")]
    public GameObject pausePopup;
    public TMP_Text pauseStarText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 팝업 숨기기
        clearPopup.SetActive(false);
        pausePopup.SetActive(false);

        // 스테이지 번호 표시 - PlayerPrefs에서 읽어서 한 번만 세팅
        int stageIndex = PlayerPrefs.GetInt("SelectedStage", 0);
        stageText.text = (stageIndex + 1) + "스테이지";

        // 성물 카운트 초기화
        relicCountText.text = "성물: 0";

        // SpeedButton 안의 TMP_Text 자동으로 찾기
        // GetComponentInChildren: 자식 오브젝트에서 해당 컴포넌트를 찾아주는 함수
        // 버튼 오브젝트 안에 텍스트가 자식으로 있어서 이렇게 찾음
        speedButtonText = speedButton.GetComponentInChildren<TMP_Text>();

        // 저장된 배속 상태 불러와서 버튼 텍스트 세팅
        bool isDoubleSpeed = PlayerPrefs.GetInt("IsDoubleSpeed", 0) == 1;
        speedButtonText.text = isDoubleSpeed ? "2배속" : "1배속";
    }

    // 성물 카운트 업데이트 - GridManager.PlaceRelic()에서 호출
    public void UpdateRelicCount(int count)
    {
        relicCountText.text = "성물: " + count;
    }

    // 배속 토글 버튼 - SpeedButton OnClick에 등록
    // TurnManager의 스페이스바 로직이랑 동일하게 처리
    public void OnSpeedButton()
    {
        bool isDoubleSpeed = PlayerPrefs.GetInt("IsDoubleSpeed", 0) == 1;
        isDoubleSpeed = !isDoubleSpeed;

        CharacterManager.Instance.moveSpeed = isDoubleSpeed ? 0.25f : 0.5f;
        PlayerPrefs.SetInt("IsDoubleSpeed", isDoubleSpeed ? 1 : 0);
        PlayerPrefs.Save();

        speedButtonText.text = isDoubleSpeed ? "2배속" : "1배속";
        Debug.Log("배속 버튼: " + (isDoubleSpeed ? "2배속" : "1배속"));
    }

    // 승리 팝업 표시 - WinConditionChecker에서 호출
    public void ShowClearPopup(int stars)
    {
        clearPopup.SetActive(true);
        starText.text = stars + "성";
        Debug.Log("클리어 팝업 표시 - 별: " + stars + "개");
    }

    // 일시정지 팝업 표시 - PauseButton에서 호출
    public void ShowPausePopup()
    {
        int currentStage = PlayerPrefs.GetInt("SelectedStage", 0);
        int bestStars = PlayerPrefs.GetInt("StarCount_" + currentStage, 0);
        pauseStarText.text = bestStars > 0 ? bestStars + "성" : "미클리어";

        pausePopup.SetActive(true);
        TurnManager.Instance.enabled = false;
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
}