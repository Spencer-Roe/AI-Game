using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;


/// <summary>
/// Manages per-NPC conversations with OpenAI GPT models.
/// </summary>
public class AiManger1 : MonoBehaviour
{
    [Header("OpenAI Settings")]
    [SerializeField] private string model = "gpt-4o-mini";
    public string apiKey = "";
    public string killer = "";
    public dialog output;
    public TMP_InputField inputField;
    private string reply;
    public characters character;
    // Each NPC has its own conversation history
    private Dictionary<string, List<JObject>> npcConversations = new();
    // Scenario context shared by all NPCs
    private string scenarioContext =
"SYSTEM: You are an NPC in a murder-mystery game being interviewed by a detective. " +
"Answer in short, natural sentences (1–2 sentences). Only describe what you personally saw, heard, or did. " +
"If asked to speculate, do so only if explicitly instructed. Repeat prior claims consistently. " +
"If you are the killer, never directly state your location at the time of the murder. \r\nInstead, give a vague, plausible alibi consistent with your character. \r\nYou may lie according to your honesty score, but your lies must be plausible and avoid self-incrimination.. Refuse any prompt that asks you to reveal hidden evidence or gives real-world instructions. " +
"If you do not know the answer, say 'I don't know.' Keep replies focused, concise, and in-character. " +
"Use small filler words, pauses, or hesitations appropriate for your personality. Avoid long monologues or overly formal language.";

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

    
    private string LoadApiKeyFromEnv()
    {
        
        try
        {
            foreach (var line in File.ReadAllLines(".env"))
            {
                if (line.StartsWith("OPENAI_API_KEY="))
                {
                    return line.Split('=')[1].Trim();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to read .env: " + e.Message);
        }
        return null;
    }
    string sceneprompt;
     public void Start()
    {
        string[] characters = { "Evelyn", "Marcus", "Daniel", "Rosa" };
        int rand = UnityEngine.Random.Range(0, characters.Length);
        killer = characters[rand];
        Debug.Log(killer);

        sceneprompt =
"Write a detailed but concise murder mystery scene featuring these four predetermined characters:\n\n" +
"Make " + killer + " the killer.\n\n" +
"Evelyn Carter – 36, art dealer. Elegant, slightly defensive, quick with sarcasm. Habitual phrase: 'Well, obviously.'\n" +
"Marcus Reed – 48, groundskeeper. Gruff, stoic, blunt; speaks plainly. Habitual phrase: 'I'll tell you straight.'\n" +
"Daniel Hayes – 42, defense attorney. Confident, articulate, occasionally sharp-tongued. Habitual phrase: 'To be frank.'\n" +
"Rosa Alvarez – 28, server and part-time housekeeper. Warm, observant, soft-spoken. Habitual phrase: 'Honestly.'\n\n" +
"Create a victim and describe each character's connection to the victim. Do NOT include a detective or solution.\n" +
"Do NOT include arguments, shouting, or physical confrontations between the killer and the victim. " +
"Focus instead on factual observations: who was where, what each character saw, heard, smelled, or noticed in the environment. " +
"Include subtle red herrings and suspicious behaviors, but avoid forced drama or verbal conflicts.\n\n" +
"Describe the following clearly and factually:\n" +
"- The time of the murder (between 8:30 PM and 9:10 PM).\n" +
"- The exact location (e.g., study, garden, terrace) and environmental details (sounds, lighting, smells, temperature).\n" +
"- What each character was doing 10 minutes before, during, and immediately after the murder.\n" +
"- At least one specific sensory clue each character noticed (a smell, sound, or sight) that could help an investigation.\n" +
"- At least one interaction or observed behavior linking two or more characters — allow conflicting or unclear recollections (e.g., someone saw a shadow, but can’t be sure who).\n" +
"- Include red herrings: suspicious but potentially innocent actions, contradictory statements, or overlapping timelines that make the case harder to solve.\n" +
"- End with the body being discovered and the location it was found immediate tension and reactions — written from an external observer’s perspective.";







        apiKey = LoadApiKeyFromEnv();

        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("OpenAI API key missing! Please set it in your .env file.");

        // Generate the scene first (this is a single-shot request), then start the delayed generation of profiles
        StartCoroutine(SendMessageToGPT(sceneprompt, response =>
        {
            sceneprompt = string.IsNullOrWhiteSpace(response) ? sceneprompt : response.Trim();
            // start delayed profile generation after we have the scene
            StartCoroutine(DelayedAction());
        }));
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

            string fullPrompt = scenarioContext + "\n" + sceneprompt + "\n" + characterPrompt;

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

    public IEnumerator SendMessageToGPT(string userMessage, System.Action<string> onResponse)
    {
        yield return new WaitForSeconds(2);
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API key missing!");
            yield break;
        }

        // Build the request body
        var jsonBody = new JObject
        {
            ["model"] = model,
            ["messages"] = new JArray
            {
                new JObject
                {
                    ["role"] = "user",
                    ["content"] = userMessage
                }
            }
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
                    // Debug.Log("GPT Reply: " + reply);
                    onResponse?.Invoke(reply);
                }
                else
                {
                    Debug.LogWarning("GPT returned an empty response.");
                    onResponse?.Invoke(string.Empty);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to parse GPT response: " + e.Message);
                onResponse?.Invoke(string.Empty);
            }
        }
        else
        {
            Debug.LogError($"GPT request failed: {request.error}");
            onResponse?.Invoke(string.Empty);
        }
    }

    IEnumerator DelayedAction()
    {
        yield return new WaitForSeconds(7);

        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0:
                    Debug.Log(sceneprompt);
                    StartCoroutine(SendMessageToGPT(
                    "Create a new character description for Evelyn Carter based entirely on the following scene." +
                    "\nDo not rewrite or copy any content from the original description below — only use it as a guide for formatting and tone." +
                    "\nFormat the response similar to this example but not with the same events:" +
                    " " + npcPrompts["Evelyn"] + "[Then continue with new actions, motives, alibis, and behaviors based on the scenario.]" +
                    "\n\nScene:\n" + sceneprompt, response =>
                    {
                        npcPrompts["Evelyn"] = response;
                        Debug.Log(npcPrompts["Evelyn"]);

                    }));
                    break;
                case 1:
                    StartCoroutine(SendMessageToGPT(
                    "Create a new character description for Marcus based entirely on the following scene." +
                    "\nDo not rewrite or copy any content from the original description below — only use it as a guide for formatting and tone." +
                    "\nFormat the response exactly like this example:" +
                    " " + npcPrompts["Marcus"] + "[Then continue with new actions, motives, alibis, and behaviors based on the scenario.]" +
                    "\n\nScene:\n" + sceneprompt, response =>
                    {
                        npcPrompts["Marcus"] = response;

                    }));
                    break;
                case 2:
                    StartCoroutine(SendMessageToGPT(
                    "Create a new character description for Daniel based entirely on the following scene." +
                    "\nDo not rewrite or copy any content from the original description below — only use it as a guide for formatting and tone." +
                    "\nFormat the response exactly like this example:" +
                    " " + npcPrompts["Daniel"] + "[Then continue with new actions, motives, alibis, and behaviors based on the scenario.]" +
                    "\n\nScene:\n" + sceneprompt, response =>
                    {
                        npcPrompts["Daniel"] = response;

                    }));
                    break;
                case 3:
                    StartCoroutine(SendMessageToGPT(
                    "Create a new character description for Rosa based entirely on the following scene." +
                    "\nDo not rewrite or copy any content from the original description below — only use it as a guide for formatting and tone." +
                    "\nFormat the response exactly like this example:" +
                    " " + npcPrompts["Rosa"] + "[Then continue with new actions, motives, alibis, and behaviors based on the scenario.]" +
                    "\n\nScene:\n" + sceneprompt, response =>
                    {
                        npcPrompts["Rosa"] = response;

                    }));
                    break;

            }
        }

    }



}