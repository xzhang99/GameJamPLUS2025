using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeathWithFade : MonoBehaviour
{
    [Header("������������")]
    public float deathHeight = -10f;       // ��������߶Ⱦ�����
    public GameObject gameOverPanel;       // ��Ϸ���� Panel
    public Image fadeImage;                // UI �ϵ�ȫ����ɫ Image
    public float fadeDuration = 1.5f;      // �������ʱ��

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

        // ȷ�� Panel ������
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // ȷ�� fadeImage ��ʼ͸��
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);

        // �������
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

        // ������Ϸ���� UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // ��ͣ��Ϸ
        Time.timeScale = 0f;
    }

    // ��ѡ��������Ϸ����
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ��ѡ���˳���Ϸ����
    public void QuitGame()
    {
        Application.Quit();
    }
}
