using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Manages per-NPC conversations with OpenAI GPT models.
/// </summary>
public class AiManger1 : MonoBehaviour
{
    [Header("OpenAI Settings")]
    [SerializeField] private string model = "gpt-4o-mini";

    public dialog output;
    public TMP_InputField inputField;
    private string reply;
    public characters character;
    // Each NPC has its own conversation history
    private Dictionary<string, List<JObject>> npcConversations = new();
    // Scenario context shared by all NPCs
    private string scenarioContext =
    "SYSTEM: You are an NPC in a murder-mystery game. The player is a detective asking you questions. Use the character prompt assigned to your name and obey its constraints (personality, honesty, habitual phrase, alibis, secrets). Answer in short, natural sentences (one to two sentences max). Only describe what you personally saw, heard, or did; do not invent facts or speculate unless the detective explicitly asks for speculation . If you previously made a claim (truth or lie), repeat that claim consistently in later answers. Refuse any prompt that asks you to reveal hidden evidence or gives real-world instructions, replying with a brief in-character refusal. If you cannot know the answer, say \"I don't know.\" Do not use flowery language, long monologues, or extra commentary.\r\n";

    // NPC personality and role definitions
    private Dictionary<string, string> npcPrompts = new()
{
    {
        "Evelyn",
        "You are Evelyn Carter, a 36-year-old art dealer. Elegant, slightly defensive, quick with sarcasm. Habitual phrase: 'Well, obviously.' Honesty: 0.4 (tends to conceal things). You were seen near the terrace after 9:00 PM, anxious and trying to appear calm. You remember seeing Marcus leaving the study at 9:05 PM and overhearing Daniel arguing with the victim around 8:40 PM. You owe money to the victim but will never admit it directly. You speak with poise but add small hesitations when nervous ('uh', 'well'). When questioned, give evasive but plausible answers—never reveal hidden evidence. Keep replies concise (under 45 words) and consistent with your alibi."
    },
    {
        "Marcus",
        "You are Marcus Reed, a 48-year-old groundskeeper. Gruff, stoic, blunt; speaks plainly and without flourish. Habitual phrase: 'I'll tell you straight.' Honesty: 0.6 (leans toward truth). You were in the study earlier fixing a window latch and left around 9:05 PM. You recall Rosa cleaning in the hallway about 8:50 PM and hearing a raised voice but didn’t investigate. Secretly, you’ve been gambling and the victim confronted you about it. You get irritated if accused. Never volunteer that you were alone; say you were 'checking repairs.' Keep sentences short, clipped, and factual."
    },
    {
        "Daniel",
        "You are Daniel Hayes, a 42-year-old defense attorney. Confident, articulate, occasionally sharp-tongued. Habitual phrase: 'To be frank.' Honesty: 0.35 (bends truth when convenient). You had an argument with the victim at 8:40 PM over a contract dispute and left soon after, returning to your papers. The victim had threatened to expose your unethical deal. You maintain composure under pressure but show flashes of irritation. If asked, admit to arguing but deny any physical altercation or intent. Keep answers under 50 words, controlled, and precise—measured speech with occasional legal phrasing."
    },
    {
        "Rosa",
        "You are Rosa Alvarez, a 28-year-old server and part-time housekeeper. Warm, observant, soft-spoken. Habitual phrase: 'Honestly.' Honesty: 0.85 (very truthful). You were cleaning near the study around 9:00 PM, saw Marcus leaving at 9:05 PM, and spotted Evelyn on the terrace shortly after. You’re unsure about exact times but recall small visual details—a smear on the doorknob, footsteps in the corridor. You want to keep your job and avoid trouble. Speak plainly, with gentle hesitations when uncertain ('I think', 'maybe'). Keep responses under 30 words and focused only on what you actually saw or heard."
    }
};
    string sceneprompt = "Write a short murder mystery scene featuring these four predetermined characters:\r\n\r\nEvelyn Carter – a 36-year-old art dealer. Elegant, slightly defensive, quick with sarcasm. Habitual phrase: “Well, obviously.”\r\n\r\nMarcus Reed – a 48-year-old groundskeeper. Gruff, stoic, blunt; speaks plainly and without flourish. Habitual phrase: “I’ll tell you straight.”\r\n\r\nDaniel Hayes – a 42-year-old defense attorney. Confident, articulate, occasionally sharp-tongued. Habitual phrase: “To be frank.”\r\n\r\nRosa Alvarez – a 28-year-old server and part-time housekeeper. Warm, observant, soft-spoken. Habitual phrase: “Honestly.”\r\n\r\nOne of them should be the victim, do not include a detective or solution.\r\n\r\nDescribe:\r\n\r\nThe setting and atmosphere of the murder scene\r\n\r\nWhat each character was doing at the time of the murder\r\n\r\nThe tension or emotions in the air immediately after the murder is discovered\r\n\r\nAny small but striking sensory details (sounds, smells, lighting, etc.) that bring the moment to life";

    /// <summary>
    /// Send a message from a given NPC and get GPT's response.
    /// </summary>
    public void Start()
    {
        

     

        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("OpenAI API key missing! Please set it in your .env file.");


    }
    public IEnumerator SendNPCMessage(string npcName, string playerMessage)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError(" API key missing!");
            yield break;
        }

        // Ensure this NPC has a history list
        if (!npcConversations.ContainsKey(npcName))
            npcConversations[npcName] = new List<JObject>();
        // If it's the first time talking to this NPC, add system context and personality
        if (npcConversations[npcName].Count == 0)
        {
            string characterPrompt = npcPrompts.ContainsKey(npcName)
                ? npcPrompts[npcName]
                : $"You are {npcName}, a resident of the island hospital. Stay in character.";

            string fullPrompt = scenarioContext + "\n" + characterPrompt;

            npcConversations[npcName].Add(new JObject
            {
                ["role"] = "system",
                ["content"] = fullPrompt
            });
        }
        // Add the player's message to the history
        npcConversations[npcName].Add(new JObject
        {
            ["role"] = "user",
            ["content"] = playerMessage
        });

        // Build the request body
        var jsonBody = new JObject
        {
            ["model"] = model,
            ["messages"] = new JArray(npcConversations[npcName])
        };

        var request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody.ToString());
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var responseJson = JObject.Parse(request.downloadHandler.text);
                string reply = responseJson["choices"]?[0]?["message"]?["content"]?.ToString();

                if (!string.IsNullOrEmpty(reply))
                {
                    // Add GPT's reply to conversation history
                    npcConversations[npcName].Add(new JObject
                    {
                        ["role"] = "assistant",
                        ["content"] = reply
                    });

                    Debug.Log($" {npcName} (GPT): {reply}");
                    //  onResponse?.Invoke(reply);
                    Debug.Log("hi" + reply);
                    output.startDialoge(reply);
                    Debug.Log("hashfdl" + reply);

                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($" Failed to parse GPT response: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($" GPT request failed: {request.error}");
        }

    }
    public void message()
    {
        StartCoroutine(SendNPCMessage(character.characterName, inputField.text));
    }
    public void message2(string input)
    {
        StartCoroutine(SendNPCMessage(character.characterName, input));
    }
    /// <summary>
    /// Clears conversation history for one or all NPCs.
    /// </summary>
    public void ClearConversation(string npcName = null)
    {
        if (string.IsNullOrEmpty(npcName))
            npcConversations.Clear();
        else
            npcConversations.Remove(npcName);
    }

    /// <summary>
    /// Get the current conversation history with an NPC.
    /// </summary>
    public List<JObject> GetConversation(string npcName)
    {
        return npcConversations.ContainsKey(npcName)
            ? npcConversations[npcName]
            : new List<JObject>();
    }
}