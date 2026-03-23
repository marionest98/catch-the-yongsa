using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("클리어 팝업 연결")]
    public GameObject clearPopup;
    public TMP_Text starText;

    [Header("일시정지 팝업 연결")]
    public GameObject pausePopup;
    public TMP_Text pauseStarText; // 일시정지 팝업의 별 표시

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 게임 시작 시 팝업 전부 숨기기
        clearPopup.SetActive(false);
        pausePopup.SetActive(false);
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
        // 현재 스테이지 최고 별 수 읽어서 표시
        int currentStage = PlayerPrefs.GetInt("SelectedStage", 0);
        int bestStars = PlayerPrefs.GetInt("StarCount_" + currentStage, 0);
        pauseStarText.text = bestStars > 0 ? bestStars + "성" : "미클리어";

        pausePopup.SetActive(true);
        TurnManager.Instance.enabled = false; // 일시정지 중 턴 진행 중지
    }

    // 계속하기 버튼 - 일시정지 팝업 닫기
    public void OnResumeButton()
    {
        pausePopup.SetActive(false);
        TurnManager.Instance.enabled = true; // 턴 진행 재개
    }

    // 다시하기 버튼 - 현재 씬 재시작
    public void OnRetryButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 나가기 버튼 - 스테이지 선택 씬으로 전환
    public void OnExitButton()
    {
        SceneManager.LoadScene("StageSelectScene");
    }
}