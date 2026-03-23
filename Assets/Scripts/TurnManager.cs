using UnityEngine;
using UnityEngine.InputSystem;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public enum TurnState
    {
        PlayerTurn,
        YongsaTurn
    }

    public TurnState currentTurn;
    
    private bool isDoubleSpeed = false;// 2배속 여부

    private bool canUndo = false; // 되돌리기 가능 여부

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentTurn = TurnState.PlayerTurn;

        // 저장된 배속 상태 불러오기
        isDoubleSpeed = PlayerPrefs.GetInt("IsDoubleSpeed", 0) == 1;
        CharacterManager.Instance.moveSpeed = isDoubleSpeed ? 0.25f : 0.5f;

        Debug.Log("현재 턴: " + currentTurn);
    }

    public void EndPlayerTurn()
    {
        currentTurn = TurnState.YongsaTurn;
        Debug.Log("현재 턴: " + currentTurn);
        StartCoroutine(AIManager.Instance.MoveYongsaCoroutine()); // 코루틴으로 호출
    }

    public void EndYongsaTurn()
    {
        currentTurn = TurnState.PlayerTurn;
        canUndo = true; // 용사 턴 끝나면 되돌리기 가능
        Debug.Log("현재 턴: " + currentTurn);
    }

    // 되돌리기 함수 - UndoButton에서 호출
    public void OnUndoButton()
    {
        if (currentTurn != TurnState.PlayerTurn) { return; }
        if (!canUndo) { return; }

        StopAllCoroutines(); // 혹시 남아있는 코루틴 강제 종료
        GridManager.Instance.RestoreState();
        CharacterManager.Instance.RestoreState();

        canUndo = false;
        Debug.Log("되돌리기 실행!");
    }

    void Update()
    {
        // 스페이스바로 2배속 토글
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            isDoubleSpeed = !isDoubleSpeed;
            CharacterManager.Instance.moveSpeed = isDoubleSpeed ? 0.25f : 0.5f;
            PlayerPrefs.SetInt("IsDoubleSpeed", isDoubleSpeed ? 1 : 0); // 배속 상태 저장
            PlayerPrefs.Save();
            Debug.Log("이동 속도: " + (isDoubleSpeed ? "2배속" : "1배속"));
        }

        // 플레이어 턴일 때 마우스 클릭 감지
        if (currentTurn == TurnState.PlayerTurn && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            int x = Mathf.RoundToInt(worldPos.x + (StageManager.Instance.stageData.width - 1) / 2f);
            int y = Mathf.RoundToInt(worldPos.y + (StageManager.Instance.stageData.height - 1) / 2f);
            if (x >= 0 && x < StageManager.Instance.stageData.width &&
                y >= 0 && y < StageManager.Instance.stageData.height)
            {
                GridManager.Instance.PlaceRelic(x, y);
            }
        }
    }
}