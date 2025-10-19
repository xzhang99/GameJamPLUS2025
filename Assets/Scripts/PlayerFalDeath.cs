using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeathWithFade : MonoBehaviour
{
    [Header("掉落死亡设置")]
    public float deathHeight = -10f;       // 低于这个高度就死亡
    public GameObject gameOverPanel;       // 游戏结束 Panel
    public Image fadeImage;                // UI 上的全屏黑色 Image
    public float fadeDuration = 1.5f;      // 渐变持续时间

    private bool isDead = false;

    void Update()
    {
        if (!isDead && transform.position.y < deathHeight)
        {
            StartCoroutine(DieWithFade());
        }
    }

    private IEnumerator DieWithFade()
    {
        isDead = true;

        // 确保 Panel 先隐藏
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // 确保 fadeImage 初始透明
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);

        // 渐变黑屏
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (fadeImage != null)
            {
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = new Color(0, 0, 0, alpha);
            }
            yield return null;
        }

        // 激活游戏结束 UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // 暂停游戏
        Time.timeScale = 0f;
    }

    // 可选：重启游戏方法
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 可选：退出游戏方法
    public void QuitGame()
    {
        Application.Quit();
    }
}
