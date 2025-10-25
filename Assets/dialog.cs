using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class dialog : MonoBehaviour
{
    public TextMeshProUGUI textcomp;
    public string[] lines;
    public float speed; 
    private int index;
    private void Start()
    {
        textcomp.text = string.Empty;
        startDialoge();
    }
    void startDialoge()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }
    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textcomp.text += c;
            yield return new WaitForSeconds(speed);

        }
    }

  
}
