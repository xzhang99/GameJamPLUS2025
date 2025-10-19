using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenu : MonoBehaviour
{
    [Header("Name of the main menu scene")]
    public string mainMenuSceneName = "Menu";

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
