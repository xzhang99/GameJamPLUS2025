using UnityEngine;

public class ControlsButtonLogic : MonoBehaviour
{
    [SerializeField] MenuButtonController menuButtonController;
    [SerializeField] int thisIndex;
    [SerializeField] GameObject controlsPanel; 

    void Start()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false); 
    }

    void Update()
    {
 
        if (menuButtonController.index == thisIndex)
        {
   
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (controlsPanel != null)
                    controlsPanel.SetActive(true);
            }
        }

  
        if (controlsPanel != null && controlsPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                controlsPanel.SetActive(false);
            }
        }
    }
}
