using UnityEngine;

public class Test : MonoBehaviour
{
    public AiManger dialogueManager;

    void Start()
    {
        // Example: Send message to NPC "Bob"
        StartCoroutine(dialogueManager.SendNPCMessage("Bob", "Hey Bob, how’s the smuggling operation going?", (reply) =>
        {
            Debug.Log("NPC replied: " + reply);
        }));

        // Example: Send another message to the same NPC (keeps conversation memory)
        StartCoroutine(dialogueManager.SendNPCMessage("Bob", "Tell me more about your boss.", (reply) =>
        {
            Debug.Log("NPC replied: " + reply);
        }));

        // Example: Talk to a different NPC
        StartCoroutine(dialogueManager.SendNPCMessage("Clara", "Hey Clara, what do you know about Bob?", (reply) =>
        {
            Debug.Log("Clara says: " + reply);
        }));
    }
    private void Update()
    {
        
    }
}
