using UnityEngine;

public class BGMManager : MonoBehaviour
{
    void Awake()
    {
        // 이미 BGMManager가 있으면 중복 제거
        if (FindObjectsByType<BGMManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        // 씬이 바뀌어도 파괴되지 않게 설정
        DontDestroyOnLoad(gameObject);
    }
}