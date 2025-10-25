using UnityEngine;

public class characters : MonoBehaviour
{
    
    public string characterName;
    public AiManger1 aiManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterName = "Rosa";
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Evelyn()
    {

        characterName = "Evelyn";
        aiManager.message2("Hello, who are you?");


    }

    public void Marcus()
    {
        characterName = "Marcus";
        aiManager.message2("Hello, who are you?");

    }
    public void Daniel()
    {
        characterName = "Daniel";
        aiManager.message2("Hello, who are you?");

    }
    public void Rosa()
    {
        characterName = "Rosa";
        aiManager.message2("Hello, who are you?");

    }
}
