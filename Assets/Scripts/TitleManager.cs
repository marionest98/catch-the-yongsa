using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TitleManager : MonoBehaviour
{
    void Update()
    {
        // 마우스 클릭 또는 터치 시 스테이지 선택 화면으로 전환
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SceneManager.LoadScene("StageSelectScene");
        }
    }
}