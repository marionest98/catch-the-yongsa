using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [Header("UI 연결")]
    public Image demonImage; // Inspector에서 DemonImage 연결

    void Update()
    {
        // 탭/클릭 시 스테이지 선택 화면으로 전환
        // 입력 감지를 InputManager에 위임
        if (InputManager.Instance.WasTapped())
        {
            SceneManager.LoadScene("StageSelectScene");
        }
    }
}