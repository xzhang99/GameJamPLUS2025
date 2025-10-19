using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class StartButtonLogic : MonoBehaviour
{
    public string sceneToLoad = "Temp_testLighting";
    [SerializeField] MenuButtonController menuButtonController;
    [SerializeField] int thisIndex;

    void Update()
    {
        if (menuButtonController.index == thisIndex)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}
