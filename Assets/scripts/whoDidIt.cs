using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class whoDidIt : MonoBehaviour
{
    public AiManger1 aiManager;
    public TextMeshProUGUI text;
    public GameObject panels;
    public GameObject restart;
    public string caught;
    public string gotaway;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panels.SetActive(false);
        StartCoroutine(aiManager.SendMessageToGPT("Using" + aiManager.sceneprompt + "create a responce from the killer after they got caught using their persolaity and explainign their motive and how they did it",  response =>
        {
            caught = response;
        }));
        StartCoroutine(aiManager.SendMessageToGPT("Using" + aiManager.sceneprompt + "create a responce from the killer after they got away with it using their persolaity and explainign their motive and how they did it", response =>
        {
            gotaway = response;
        }));




    }

    // Update is called once per frame
    void Update()
    {

    }
    public void knowIt() { 
        panels.SetActive(true);
}

    public void hide()
    {
        panels.SetActive(false);

    }
    public void Evelyn()
    {
          if(aiManager.killer == "Evelyn")
        {
            text.text = "Correct! Evelyn is the killer.";
            Debug.Log(caught);
        }
        else
        {
            text.text = "Wrong! Evelyn is not the killer, it was " + aiManager.killer + "!";
            Debug.Log(gotaway);

        }
        restart.SetActive(true);
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
        restart.SetActive(true);

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
        restart.SetActive(true);

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
        restart.SetActive(true);

    }
}
