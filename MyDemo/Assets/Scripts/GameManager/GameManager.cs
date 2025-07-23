using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static bool isGameOn;

    [Header("UI")]
    public GameObject victoryUI; // 胜利界面

    void Start()
    {
        victoryUI.SetActive(false);
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        isGameOn = true;
    }

    public void GameOver(bool isVictory)
    {
        if (isVictory && isGameOn)
        {
            Debug.Log("HHHHHHHHHH");
            victoryUI.SetActive(true);
            Time.timeScale = 0; // 暂停游戏
            Cursor.lockState = CursorLockMode.None; // 解锁鼠标
            Debug.Log("胜利");
            isGameOn = false;
        }
        // else 可扩展失败逻辑
    }
}