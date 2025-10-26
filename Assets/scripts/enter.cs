using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
public class enter : MonoBehaviour
{
    public AiManger1 aimanger1;
    public dialog dialog;
    private bool disabler;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(dialog.loading);
        if (dialog.loading == true)
        {
            DisableAllButtons();
        }
        else
        {
            EnableAllButtons();
            if (Input.GetKeyDown(KeyCode.Return) && dialog.loading == false && disabler == false && dialog.isTalking == false)
            {
                aimanger1.message();

            }
        }
        
        
    }
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void DisableAllButtons()
    {

        // Option 1: Find all buttons in the entire scene
        UnityEngine.UI.Button[] allButtons = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
        foreach (UnityEngine.UI.Button button in allButtons)
        {
            button.interactable = false;
        }
        disabler = true;

    }

    public void EnableAllButtons()
    {
        UnityEngine.UI.Button[] allButtons = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
        foreach (UnityEngine.UI.Button button in allButtons)
        {
            button.interactable = true;
        }
        disabler = false;
    }
}
