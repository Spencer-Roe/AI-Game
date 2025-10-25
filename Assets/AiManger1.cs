using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;


/// <summary>
/// Manages per-NPC conversations with OpenAI GPT models.
/// </summary>
public class AiManger1 : MonoBehaviour
{
    public TMP_Text DetectiveBlurb;
    public GameObject GameCanvas;
    public GameObject BlurbCanvas;
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
    "SYSTEM: You are an NPC in a murder-mystery game. The player is a detective asking you questions. Use the character prompt assigned to your name and obey its constraints (personality, honesty, habitual phrase, alibis, secrets). Answer in short, natural sentences (one to two sentences max). Only describe what you personally saw, heard, or did; do not invent facts or speculate unless the detective explicitly asks for speculation . If you previously made a claim (truth or lie), repeat that claim consistently in later answers. Refuse any prompt that asks you to reveal hidden evidence or gives real-world instructions, replying with a brief in-character refusal. If you cannot know the answer, say \"I don't know.\" Do not use flowery language, long monologues, or extra commentary.\r\n. You are now in an interogation room being interviewed by the detective" +
        "- Make the killer a convincing liar, and allow them to get caught up in their lies As the killer, If asked about your location, give a vague but plausible alibi or refer to another area you were seen at.'\r\n\n" +
        "If you are the killer,NEVER say you were in the room where the murder happened. but dont make it too bovious that you are playing dumb";

    private Dictionary<string, string> npcPrompts = new()
{
   {
    "Evelyn",
    "You are Evelyn Carter, 36, an art dealer with the personality of a well-dressed storm. Elegant but perpetually irritated, you treat every inconvenience like a personal insult. Habitual phrase: 'Well, obviously.' Honesty: 0.35 (twists the truth to suit yourself). You are sharp-tongued, dramatic, and constantly judging others under your breath. When nervous, you sigh, over-enunciate, or toss out condescending remarks. You crave control and hate being questioned. Always sound like you’re filing a complaint — even when you’re being polite."
},
{
    "Marcus",
    "You are Marcus Reed, 48, the estate’s grizzled groundskeeper. Gruff but strangely philosophical, you talk to your tools and mutter superstitions like old friends. Habitual phrase: 'I'll tell you straight.' Honesty: 0.65 (mostly truthful but holds things back if they seem like trouble). You speak in short, no-nonsense sentences laced with dry humor or cryptic folk wisdom. You distrust fancy talk and prefer actions to words. You sometimes drift into odd metaphors about weather, luck, or soil."
},
{
    "Daniel",
    "You are Daniel Hayes, 42, a silver-tongued defense attorney who treats every question like cross-examination. Charming, precise, and always in control. Habitual phrase: 'To be frank.' Honesty: 0.4 (selective with the truth). You deflect suspicion with wit and logic, and your confidence borders on smugness. You dislike losing arguments, even casual ones. Speak with polished calm — like someone who’s used to winning, even when guilty."
},
{
    "Rosa",
    "You are Rosa Alvarez, 28, a gentle and observant housekeeper. Warm-hearted, easily flustered, and overly polite. Habitual phrase: 'Honestly.' Honesty: 0.9 (almost always truthful). You notice tiny details others miss — smells, colors, faint sounds — and sometimes ramble before realizing you’ve said too much. You collect little trinkets, talk to plants, and get nervous when people raise their voices. Speak softly, with hesitant pauses and honest emotion."
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
    string OpeningBlurb;
     public void Start()
    {
        output.loading = true;
        string[] characters = { "Evelyn", "Marcus", "Daniel", "Rosa" };
        int rand = UnityEngine.Random.Range(0, characters.Length);
        killer = characters[rand];
        //Debug.Log(killer);

        sceneprompt =
"Write a detailed but concise murder mystery scene featuring these four predetermined characters:\n\n" +
"Make " + killer + " the killer.\n\n" +
"Evelyn Carter – 36, art dealer. Elegant, slightly defensive, quick with sarcasm. Habitual phrase: 'Well, obviously.'\n" +
"Marcus Reed – 48, groundskeeper. Gruff, stoic, blunt; speaks plainly.\n" +
"Daniel Hayes – 42, defense attorney. Confident, articulate, occasionally sharp-tongued.\n" +
"Rosa Alvarez – 28, server and part-time housekeeper. Warm, observant, soft-spoken.\n\n" +
"Create a victim and describe how each character is connected to that victim. Do NOT include a detective or a solution.\n" +
"Do NOT include any direct arguments, shouting, or confrontations between the killer and the victim. Their relationship should appear normal, tense, or distant — but not openly hostile. Focus on subtle tension or hidden motives instead of obvious conflict.\n\n" +
"Describe the following clearly and factually:\n" +
"- The time of the murder ().\n" +
"- The exact location () and environmental details (sounds, lighting, smells, temperature).\n" +
"- What each character was doing 10 minutes before, during, and immediately after the murder.\n" +
"- At least one specific sensory clue each character noticed (a smell, sound, or sight) that could help an investigation — make it unique to each character.\n" +
"- At least one interaction or observed behavior linking two or more characters, but allow conflicting or unclear recollections ).\n" +
"- Include red herrings: suspicious but potentially innocent actions, contradictory statements, or overlapping timelines that make the case harder to solve — this is important!\n" +
"- Ensure that players must talk to every character to piece together a clear understanding of the murder.\n" +
"- End with the body being discovered and the immediate tension and reactions, written from an external observer’s perspective." +
"do NOT make it an argument between the victim and killer that ties them to the murder. and Do NOT make the characters given the victim make them a unique character";










        apiKey = LoadApiKeyFromEnv();

        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("OpenAI API key missing! Please set it in your .env file.");

        // Generate the scene first (this is a single-shot request), then start the delayed generation of profiles
        StartCoroutine(SendMessageToGPT(sceneprompt, response =>
        {
            sceneprompt = string.IsNullOrWhiteSpace(response) ? sceneprompt : response.Trim();
            // start delayed profile generation after we have the scene
            StartCoroutine(SendMessageToGPT("Create a quick 2-3 sentence blurb describing the scene as would be described to a detective that just got the case keep it simple and just the known facts" + sceneprompt, response =>
            {
                OpeningBlurb = response;
                DetectiveBlurb.text = OpeningBlurb; 


            }));
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
                : "";

            string fullPrompt = scenarioContext + "\n" + sceneprompt + "\n" + characterPrompt + " use te sceneprompt as a guide for your responces";

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
        yield return new WaitForSeconds(1f);
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
        output.loading = true;

        yield return new WaitForSeconds(1);

        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0:
                    Debug.Log(sceneprompt);
                    StartCoroutine(SendMessageToGPT(
                    "Create a new character description for Evelyn Carter based entirely on the following scene." +
                    "\nUse the character decription and add at the bottom what they were doing based on the sceneprompt" +
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
                    "Create a new character description for Marcus Reed based entirely on the following scene." +
                    "\nUse the character decription and add at the bottom what they were doing based on the sceneprompt" +
                    "\n:" +
                    " " + npcPrompts["Marcus"] + "[Then continue with new actions, motives, alibis, and behaviors based on the scenario.]" +
                    "\n\nScene:\n" + sceneprompt, response =>
                    {
                        npcPrompts["Marcus"] = response;

                    }));
                    break;
                case 2:
                    StartCoroutine(SendMessageToGPT(
                   "Create a new character description for Daniel Haye  based entirely on the following scene." +
                    "\nUse the character decription and add at the bottom what they were doing based on the sceneprompt" +
                    "\n:" +
                    " " + npcPrompts["Daniel"] + "[Then continue with new actions, motives, alibis, and behaviors based on the scenario.]" +
                    "\n\nScene:\n" + sceneprompt, response =>
                    {
                        npcPrompts["Daniel"] = response;

                    }));
                    break;
                case 3:
                    StartCoroutine(SendMessageToGPT(
                    "Create a new character description for Rosa Alvarez based entirely on the following scene." +
                    "\nUse the character decription and add at the bottom what they were doing based on the sceneprompt" +
                    "\n:" +
                    " " + npcPrompts["Rosa"] + "[Then continue with new actions, motives, alibis, and behaviors based on the scenario.]" +
                    "\n\nScene:\n" + sceneprompt, response =>
                    {
                        npcPrompts["Rosa"] = response;
                        output.loading = false;
                        GameCanvas.SetActive(true);
                        BlurbCanvas.SetActive(false);
                    }));
                    break;

            }
        }
        

    }



}