using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    // 이번 프레임에 마우스 또는 터치 입력이 있었는지 확인
    public bool WasTapped()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame) { return true; }
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) { return true; }
        return false;
    }

    // 탭/클릭 위치를 월드 좌표로 변환해서 반환
    // Camera.main.ScreenToWorldPoint: 화면 픽셀 좌표 → 게임 세계 좌표 변환 함수
    public Vector3 GetTapWorldPosition()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    // 스페이스바가 이번 프레임에 눌렸는지 확인
    public bool WasSpacePressed()
    {
        return Keyboard.current.spaceKey.wasPressedThisFrame;
    }
}