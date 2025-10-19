using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuLogic : MonoBehaviour
{
    public GameObject pausedPanel; 
    public string mainMenuSceneName = "Menu"; 

    private bool isPaused = false;

    void Start()
    {
        if (pausedPanel != null)
            pausedPanel.SetActive(false); 
    }

    void Update()
    {
  
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausedPanel != null)
            {
                isPaused = !isPaused;
                pausedPanel.SetActive(isPaused);
                Time.timeScale = isPaused ? 0f : 1f; 
            }
        }

     
        if (isPaused && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f; 
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
