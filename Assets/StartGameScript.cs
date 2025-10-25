using UnityEngine;

public class StartGameScript : MonoBehaviour
{
    public GameObject GameCanvas;
    public GameObject BlurbCanvas;
    public void StartGame() 
    {
        GameCanvas.SetActive(true);
        BlurbCanvas.SetActive(false);
    }
}
