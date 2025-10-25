using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class dialog : MonoBehaviour
{
    public TextMeshProUGUI textcomp;
    public string lines;
    public float speed; 
    private int index;
    private void Start()
    {
        textcomp.text = string.Empty;
        //startDialoge();
    }
    public void startDialoge(string line)
    {
        textcomp.text = "";
        lines = line;
        StartCoroutine(TypeLine());
    }
    IEnumerator TypeLine()
    {
        foreach (char c in lines.ToCharArray())
        {
            textcomp.text += c;
            yield return new WaitForSeconds(speed);

        }
    }

  
}
