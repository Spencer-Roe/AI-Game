using UnityEngine;
using TMPro;
using System.Collections;

public class Test : MonoBehaviour
{
    public AiManger dialogueManager;
    public TMP_InputField messageInput;   // Reference to Input Field
   

    private bool isWaitingForReply = false;

    public void SendFromInput()
    {
        if (isWaitingForReply)
        {
            AppendChat("System", " Waiting for Evelyn’s reply...");
            return;
        }

        string playerMessage = messageInput.text.Trim();
        if (string.IsNullOrEmpty(playerMessage))
            return;

        messageInput.text = ""; // Clear input field
        StartCoroutine(SendMessageToBob(playerMessage));
    }

    private IEnumerator SendMessageToBob(string message)
    {
        isWaitingForReply = true;

        AppendChat("You", message);

        // Send message to GPT through AiManager
        yield return StartCoroutine(dialogueManager.SendNPCMessage("Evelyn", message, (reply) =>
        {
            AppendChat("Evelyn", reply);
        }));

        isWaitingForReply = false;
    }

    private void AppendChat(string speaker, string text)
    {
        
            Debug.Log($"{speaker}: {text}");
    }
}
