using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class whoDidIt : MonoBehaviour
{
    public AiManger1 aiManager;
    public TextMeshProUGUI text;
    public GameObject panels;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panels.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void knowIt() { 
        panels.SetActive(true);
}


    public void Evelyn()
    {
          if(aiManager.killer == "Evelyn")
        {
            text.text = "Correct! Evelyn is the killer.";
        }
        else
        {
            text.text = "Wrong! Evelyn is not the killer, it was " + aiManager.killer + "!";

        }

    }

    public void Marcus()
    {
        if(aiManager.killer == "Marcus")
        {
            text.text = "Correct! Marcus is the killer.";
        }
        else
        {
            text.text = "Wrong! Marcus is not the killer, it was " + aiManager.killer + "!";
        }

    }
    public void Daniel()
    {
       if(aiManager.killer == "Daniel")
        {
            text.text = "Correct! Daniel is the killer.";
        }
        else
        {
            text.text = "Wrong! Daniel is not the killer, it was " + aiManager.killer +"!";
        }

    }
    public void Rosa()
    {
        if(aiManager.killer == "Rosa")
        {
            text.text = "Correct! Rosa is the killer.";
        }
        else
        {
            text.text = "Wrong! Rosa is not the killer, it was " + aiManager.killer + "!";
        }

    }
}
