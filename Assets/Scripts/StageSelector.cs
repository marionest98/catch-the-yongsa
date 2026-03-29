using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class StageSelector : MonoBehaviour
{
    [Header("버튼 프리팹 연결")]
    public GameObject stageButtonPrefab; // Inspector에서 버튼 프리팹 연결

    [Header("버튼 배치할 부모 오브젝트 연결")]
    public Transform buttonParent; // Inspector에서 Content 오브젝트 연결

    // 스테이지 수 - 나중에 스테이지 추가하면 여기만 수정
    private const int stageCount = 10;

    void Start()
    {
        CreateStageButtons();
    }

    // 스테이지 버튼 생성 - stageCount 수만큼 자동 생성
    private void CreateStageButtons()
    {
        for (int i = 0; i < stageCount; i++)
        {
            GameObject btn = Instantiate(stageButtonPrefab, buttonParent);

            TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
            Button button = btn.GetComponent<Button>();

            // 1스테이지(index 0)는 항상 해금
            // 나머지는 이전 스테이지 클리어 여부 확인
            bool isUnlocked = (i == 0) || (PlayerPrefs.GetInt("StageClear_" + (i - 1), 0) == 1);

            if (isUnlocked)
            {
                // 해금된 스테이지
                if (label != null) { label.text = (i + 1) + "스테이지"; }
                button.interactable = true;

                // 별 수 표시 - PlayerPrefs에서 읽어옴 (클리어 안 했으면 0)
                TMP_Text starText = btn.transform.Find("StarText")?.GetComponent<TMP_Text>();
                if (starText != null)
                {
                    int stars = PlayerPrefs.GetInt("StarCount_" + i, 0);
                    starText.text = stars > 0 ? stars + "성" : "";
                }
            }
            else
            {
                // 잠긴 스테이지
                if (label != null) { label.text = "잠금"; }
                button.interactable = false;

                // 잠긴 스테이지는 별 표시 숨기기
                TMP_Text starText = btn.transform.Find("StarText")?.GetComponent<TMP_Text>();
                if (starText != null) { starText.text = ""; }
            }

            int stageIndex = i;
            button.onClick.AddListener(() => OnStageSelected(stageIndex));
        }
    }

    // 스테이지 선택 시 호출 - PlayerPrefs에 번호 저장 후 GameScene으로 전환
    private void OnStageSelected(int stageIndex)
    {
        PlayerPrefs.SetInt("SelectedStage", stageIndex);
        PlayerPrefs.Save(); // 저장 즉시 반영
        SceneManager.LoadScene("GameScene");
        Debug.Log("선택된 스테이지: " + (stageIndex + 1));
    }
}