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
        // 마우스 클릭 또는 터치 시 스테이지 선택 화면으로 전환
        if (Mouse.current.leftButton.wasPressedThisFrame ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
        {
            SceneManager.LoadScene("StageSelectScene");
        }
    }
}