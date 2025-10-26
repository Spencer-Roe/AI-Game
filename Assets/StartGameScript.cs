using TMPro;
using UnityEngine;

public class StartGameScript : MonoBehaviour
{
    public GameObject GameCanvas;
    public GameObject BlurbCanvas;
    public GameObject character;
    public TextMeshProUGUI textMeshPro;
    public void StartGame() 
    {
        GameCanvas.SetActive(true);
        BlurbCanvas.SetActive(false);
        character.SetActive(true);
    }
    public void scenerio()
    {
        BlurbCanvas.SetActive(true);
        GameCanvas.SetActive(false);
        character.SetActive(false);
        textMeshPro.text = "back";

    }
    public void back()
    {
        BlurbCanvas.SetActive(false);
        GameCanvas.SetActive(true);
        character.SetActive(true);

    }
}
