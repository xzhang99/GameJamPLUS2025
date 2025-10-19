using UnityEngine;

public class QuitButtonLogic : MonoBehaviour
{
    [SerializeField] MenuButtonController menuButtonController;
    [SerializeField] int thisIndex;

    void Update()
    {

        if (menuButtonController.index == thisIndex)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;  
#else
                Application.Quit();  
#endif
            }
        }
    }
}
