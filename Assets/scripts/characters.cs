using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class characters : MonoBehaviour
{
    
    public string characterName;
    public AiManger1 aiManager;
    public TextMeshProUGUI nameText;
    bool EFirst = true;
    bool MFirst = true;
    bool DFirst = true;
    bool RFirst = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterName = "Evelyn";

        nameText.text = characterName;

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Evelyn()
    {

        characterName = "Evelyn";
        nameText.text = characterName;
        if (EFirst)
        {
            EFirst = false;
            aiManager.message2("Hello, who are you?");
        }
        else 
        {
            aiManager.message2("Hello again");
        }


    }

    public void Marcus()
    {
        characterName = "Marcus";
        nameText.text = characterName;
        if (MFirst)
        {
            MFirst = false;
            aiManager.message2("Hello, who are you?");
        }
        else
        {
            aiManager.message2("Hello again");
        }

    }
    public void Daniel()
    {
        characterName = "Daniel";
        nameText.text = characterName;
        if (DFirst)
        {
            DFirst = false;
            aiManager.message2("Hello, who are you?");
        }
        else
        {
            aiManager.message2("Hello again");
        }

    }
    public void Rosa()
    {
        characterName = "Rosa";
        nameText.text = characterName;
        if (RFirst)
        {
            RFirst = false;
            aiManager.message2("Hello, who are you?");
        }
        else
        {
            aiManager.message2("Hello again");
        }

    }
}
