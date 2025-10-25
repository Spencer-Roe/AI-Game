using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class characters : MonoBehaviour
{
    
    public string characterName;
    public AiManger1 aiManager;
    public TextMeshProUGUI nameText;
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
        aiManager.message2("Hello, who are you?");


    }

    public void Marcus()
    {
        characterName = "Marcus";
        nameText.text = characterName;
        aiManager.message2("Hello, who are you?");

    }
    public void Daniel()
    {
        characterName = "Daniel";
        nameText.text = characterName;
        aiManager.message2("Hello, who are you?");

    }
    public void Rosa()
    {
        characterName = "Rosa";
        nameText.text = characterName;
        aiManager.message2("Hello, who are you?");

    }
}
