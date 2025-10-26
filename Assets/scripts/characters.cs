using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class characters : MonoBehaviour
{
    
    public string characterName;
    public AiManger1 aiManager;
    public TextMeshProUGUI nameText;
    public dialog dialog;
    bool EFirst = true;
    bool MFirst = true;
    bool DFirst = true;
    bool RFirst = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterName = "Evelyn";
        hideChildren();

        nameText.text = characterName;

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void hideChildren()
    {
        for (int i = 0; i < 4; i++)
        {
            this.transform.GetChild(i).gameObject.SetActive(false);
        }
        Transform child = transform.Find(characterName);
        child.gameObject.SetActive(true);
     }
    public void Evelyn()
    {

        characterName = "Evelyn";
        nameText.text = characterName;
        hideChildren();
        dialog.loading = true;
        aiManager.message2("Hello, who are you?");
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
        hideChildren();
        dialog.loading = true;
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
        hideChildren();
        dialog.loading = true;
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
        hideChildren();
        dialog.loading = true;
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
