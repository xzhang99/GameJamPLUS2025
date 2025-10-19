using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("倒计时时间（秒）")]
    public float startTime = 60f;

    [Header("UI 文本显示")]
    public TextMeshProUGUI timerText;

    private float currentTime;
    private bool isGameOver = false;

    void Start()
    {
        currentTime = startTime;
    }

    void Update()
    {
        // 如果游戏暂停（Time.timeScale == 0）或者游戏结束，不更新计时
        if (Time.timeScale == 0f || isGameOver)
            return;

        // 倒计时递减
        currentTime -= Time.deltaTime;

        // 更新UI显示（向上取整，避免负数）
        timerText.text = Mathf.CeilToInt(currentTime).ToString();

        // 倒计时到0，触发结束逻辑
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isGameOver = true;
            OnTimeUp();
        }
    }

    void OnTimeUp()
    {
        Debug.Log("⏰ 倒计时结束，游戏结束！");
        Time.timeScale = 0f; // 暂停游戏
    }
}
