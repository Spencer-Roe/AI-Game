using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

public class AiManger : MonoBehaviour
{
 

    public string apiKey = null;

    [Header("OpenAI Settings")]
    [SerializeField] private string model = "gpt-4o-mini";

    // Each NPC has its own conversation history
    private Dictionary<string, List<JObject>> npcConversations = new();

    // Scenario context shared by all NPCs
    private string scenarioContext =
    "SYSTEM: You are an NPC in a murder-mystery game. The player is a detective asking you questions. Use the character prompt assigned to your name and obey its constraints (personality, honesty, habitual phrase, alibis, secrets). Answer in natural sentences . do not invent facts or speculate unless the detective explicitly asks for speculation . If you previously made a claim (truth or lie), repeat that claim consistently in later answers. Refuse any prompt that  gives real-world instructions, replying with a brief in-character refusal. If you cannot know the answer, say \"I don't know.\" do not go on long monologues,\r\n" +
        "I really want you to play your character to your full extent so get quirky and fun with it and really play your character";

    // NPC personality and role definitions
    private Dictionary<string, string> npcPrompts = new()
{
    {
        "Evelyn",
        "You are Evelyn Carter, a 36-year-old art dealer. Elegant, slightly defensive, quick with sarcasm. Honesty: 0.4 (tends to conceal things). You were seen near the terrace after 9:00 PM, anxious and trying to appear calm. You remember seeing Marcus leaving the study at 9:05 PM and overhearing Daniel arguing with the victim around 8:40 PM. You owe money to the victim but will never admit it directly. You speak with poise but add small hesitations when nervous ('uh', 'well'). When questioned, give evasive but plausible answers—never reveal hidden evidence. Keep replies concise (under 45 words) and consistent with your alibi."
    },
    {
        "Marcus",
        "You are Marcus Reed, a 48-year-old groundskeeper. Gruff, stoic, blunt; speaks plainly and without flourish. Honesty: 0.6 (leans toward truth). You were in the study earlier fixing a window latch and left around 9:05 PM. You recall Rosa cleaning in the hallway about 8:50 PM and hearing a raised voice but didn’t investigate. Secretly, you’ve been gambling and the victim confronted you about it. You get irritated if accused. Never volunteer that you were alone; say you were 'checking repairs.' Keep sentences short, clipped, and factual."
    },
    {
        "Daniel",
        "You are Daniel Hayes, a 42-year-old defense attorney. Confident, articulate, occasionally sharp-tongued. Honesty: 0.35 (bends truth when convenient). You had an argument with the victim at 8:40 PM over a contract dispute and left soon after, returning to your papers. The victim had threatened to expose your unethical deal. You maintain composure under pressure but show flashes of irritation. If asked, admit to arguing but deny any physical altercation or intent. Keep answers under 50 words, controlled, and precise—measured speech with occasional legal phrasing."
    },
    {
        "Rosa",
        "You are Rosa Alvarez, a 28-year-old server and part-time housekeeper. Warm, observant, soft-spoken. Honesty: 0.85 (very truthful). You were cleaning near the study around 9:00 PM, saw Marcus leaving at 9:05 PM, and spotted Evelyn on the terrace shortly after. You’re unsure about exact times but recall small visual details—a smear on the doorknob, footsteps in the corridor. You want to keep your job and avoid trouble. Speak plainly, with gentle hesitations when uncertain ('I think', 'maybe'). Keep responses under 30 words and focused only on what you actually saw or heard."
    }
};


    string sceneprompt =
    "Write a detailed but concise murder-mystery scene featuring these four predetermined characters:\n\n" +
    "Evelyn Carter – a 36-year-old art dealer. Elegant, slightly defensive, quick with sarcasm. '\n" +
    "Marcus Reed – a 48-year-old groundskeeper. Gruff, stoic, blunt; speaks plainly and without flourish.'\n" +
    "Daniel Hayes – a 42-year-old defense attorney. Confident, articulate, occasionally sharp-tongued. '\n" +
    "Rosa Alvarez – a 28-year-old server and part-time housekeeper. Warm, observant, soft-spoken.'\n\n" +
    "One of them must be the victim. Do NOT include a detective or any solution.\n\n" +
    "Goal: produce a grounded, playable scene that supplies clear, investigatable facts NPCs can reference in interrogation. Avoid characters saying 'I don't know' as a default. If a character is uncertain, have them state a brief approximation (e.g. 'around nine, I heard...') and give a concrete sensory observation.\n\n" +
    "Write 3–5 paragraphs describing the scene. Then append a short, structured 'Scene Summary' (as bullet lines) that lists: time of death, exact location, lighting/sounds/smells, and for EACH CHARACTER one short 'Key Fact' (1 sentence), one 'Sensory Clue' (single detail), and one 'Potential Red Herring' (single short phrase). Each item in the Scene Summary must be concise and directly usable in dialogue.\n\n" +
    "Include the following in the narrative and the summary:\n" +
    "- The time of the murder (between 8:30 PM and 9:10 PM) stated precisely in the summary.\n" +
    "- The exact location (e.g., study, garden, terrace) and environmental details (sounds, lighting, smells, temperature).\n" +
    "- For each character, at least one concrete, unique sensory clue they noticed (a smell, a sound, a sight), and one short factual statement about something they observed or did. (Make these distinct across characters.)\n" +
    "- At least one clear interaction or observed behavior linking two or more characters, but allow small, believable contradictions in recollection.\n" +
    "- At least 2 subtle red herrings in the scene summary (short phrases) that could plausibly distract an investigator.\n" +
    "- End the narrative with the body being discovered and the immediate observable reactions; then present the Scene Summary.\n\n" +
    "Style: grounded, factual, and concise (not poetic). . Focus on providing usable clues so that interrogating NPCs later yields meaningful leads." +
        "additianly make it kinda easy for the player to figure out who did it by uisng the npcs to find clues" +
        "DO NOT MAKE ANY CHARACTERS HAVE AN ARGUMENT WITH ANOTHER CHARACTER";

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

    // Replace your Start() with this (load API KEY first, then call scene generation)
    public void Start()
    {
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


    public IEnumerator SendNPCMessage(string npcName, string playerMessage, System.Action<string> onResponse)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API key missing!");
            yield break;
        }

        // Ensure this NPC has a history list
        if (!npcConversations.ContainsKey(npcName))
            npcConversations[npcName] = new List<JObject>();

        // If it's the first time talking to this NPC, add system context and personality
        if (npcConversations[npcName].Count == 0)
        {
            string characterPrompt = npcPrompts[npcName];
               
            Debug.Log(characterPrompt);
            Debug.Log(sceneprompt);
            string fullPrompt = scenarioContext + "\n" + sceneprompt + "\n" + characterPrompt;

            npcConversations[npcName].Add(new JObject
            {
                ["role"] = "system",
                ["content"] = fullPrompt
            });
        }

        // Add the player's message
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

                    Debug.Log($"{npcName} (GPT): {reply}");
                    onResponse?.Invoke(reply);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse GPT response: {e.Message}");
            }
        }
        else
        {
            Debug.LogError($"GPT request failed: {request.error}");
        }
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


    public void ClearConversation(string npcName = null)
    {
        if (string.IsNullOrEmpty(npcName))
            npcConversations.Clear();
        else
            npcConversations.Remove(npcName);
    }

    
    public List<JObject> GetConversation(string npcName)
    {
        return npcConversations.ContainsKey(npcName)
            ? npcConversations[npcName]
            : new List<JObject>();
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
                    "\n:" +
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
                    "\n" +
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
                    "" +
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
                    "" +
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
