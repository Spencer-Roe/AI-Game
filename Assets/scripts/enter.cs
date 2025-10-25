using UnityEngine;

public class enter : MonoBehaviour
{
    public AiManger1 aimanger1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            aimanger1.message();

        }
    }
}
