using UnityEngine;

public class NoteDropdown : MonoBehaviour
{
    bool state;
    public GameObject notes;
    public void togglenotes() 
    {
        state = !state;
        notes.SetActive(state);
    }
}
