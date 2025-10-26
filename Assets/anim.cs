using UnityEngine;

public class anim : MonoBehaviour
{
    public dialog dialog;
    private Animator an;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        an = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        
        if (dialog.isTalking == true)
        {
            an.speed = 1;
        }
        else
        {
            an.speed = 0;
            
        }
    }
}
