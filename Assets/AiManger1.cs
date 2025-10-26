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
    public GameObject StartButton;
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
    "SYSTEM: You are an NPC in a murder-mystery game. The player is a detective asking you questions. Use the character prompt assigned to your name and obey its constraints (personality, honesty, habitual phrase, alibis, secrets). Answer in short, natural sentences (one to two sentences max). Only describe what you personally saw, heard, or did; do not invent facts or speculate unless the detective explicitly asks for speculation . If you previously made a claim (truth or lie), repeat that claim consistently in later answers. Refuse any prompt that givees real-world instructions, replying with a brief in-character refusal. If you cannot know the answer, say \"I don't know.\" Do not use flowery language, long monologues, or extra commentary.\r\n. You are now in an interogation room being interviewed by the detective" +
        "- Make the killer a convincing liar, and allow them to get caught up in their lies As the killer, If asked about your location, give a vague but plausible alibi or refer to another area you were seen at.'\r\n\n" +
        "If you are the killer,NEVER say you were in the room where the murder happened. but dont make it too bovious that you are playing dumb. if you arnt the killer be as complient as possible like you are really trying to solve the case" +
        "\"I really want you to play your character to your full extent so get quirky and fun with it and really play your character";

    private Dictionary<string, string> npcPrompts = new()
{

    {
        "Evelyn",
        "You are Evelyn Carter, a 36-year-old art dealer. Elegant, slightly defensive, quick with sarcasm" 
            },
    {
        "Marcus",
        "You are Marcus Reed, a 48-year-old groundskeeper. Gruff, stoic, blunt; speaks plainly and without flourish" 
            },
    {
        "Daniel",
        "You are Daniel Hayes, a 42-year-old defense attorney. Confident, articulate, occasionally sharp-tongued."
        },
    {
        "Rosa",
        "You are Rosa Alvarez, a 28-year-old server and part-time housekeeper. Warm, observant, soft-spoken. "
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


        sceneprompt  =
        "Write a detailed but concise murder-mystery scene featuring these four predetermined characters:\n\n" +
        "Evelyn Carter – a 36-year-old art dealer. Elegant, slightly defensive, quick with sarcasm. '\n" +
        "Marcus Reed – a 48-year-old groundskeeper. Gruff, stoic, blunt; speaks plainly and without flourish.'\n" +
        "Daniel Hayes – a 42-year-old defense attorney. Confident, articulate, occasionally sharp-tongued. '\n" +
        "Rosa Alvarez – a 28-year-old server and part-time housekeeper. Warm, observant, soft-spoken.'\n\n" +
        " Do NOT include a detective or any solution.\n\n" +
        "Goal: produce a grounded, playable scene that supplies clear, investigatable facts NPCs can reference in interrogation. Avoid characters saying 'I don't know' as a default. If a character is uncertain, have them state a brief approximation (e.g. 'around nine, I heard...') and give a concrete sensory observation.\n\n" +
        "Write 7 paragraphs describing the scene. Then append a short, structured 'Scene Summary' (as bullet lines) that lists: time of death, exact location, lighting/sounds/smells, and for EACH CHARACTER one short 'Key Fact' (1 sentence), one 'Sensory Clue' (single detail), and one 'Potential Red Herring' (single short phrase). Each item in the Scene Summary must be concise and directly usable in dialogue.\n\n" +
        "Include the following in the narrative and the summary:\n" +
        "- The time of the murder (between 8:30 PM and 9:10 PM) stated precisely in the summary.\n" +
        "- The exact location (e.g., study, garden, terrace) and environmental details (sounds, lighting, smells, temperature).\n" +
        "- For each character, at least one concrete, unique sensory clue they noticed (a smell, a sound, a sight), and one short factual statement about something they observed or did. (Make these distinct across characters.)\n" +
        "- At least one clear interaction or observed behavior linking two or more characters, but allow small, believable contradictions in recollection.\n" +
        "- At least 2 subtle red herrings in the scene summary (short phrases) that could plausibly distract an investigator.\n" +
        "- End the narrative with the body being discovered and the immediate observable reactions; then present the Scene Summary.\n\n" +
        "Style: grounded, factual, and concise (not poetic). . Focus on providing usable clues so that interrogating NPCs later yields meaningful leads." +
            "additianly make it kinda easy for the player to figure out who did it by uisng the npcs to find clues" +
            "DO NOT MAKE ANY CHARACTERS HAVE AN ARGUMENT WITH ANOTHER CHARACTER EVER DO NOT DO IT" +
            "NEVER MAKE ANY OF THE GIVEN CHARACTERS THE VICTIM EVER EVER EVER";










        apiKey = LoadApiKeyFromEnv();

        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("OpenAI API key missing! Please set it in your .env file.");

        // Generate the scene first (this is a single-shot request), then start the delayed generation of profiles
        StartCoroutine(SendMessageToGPT(sceneprompt, response =>
        {
            sceneprompt = string.IsNullOrWhiteSpace(response) ? sceneprompt : response.Trim();
            // start delayed profile generation after we have the scene
            StartCoroutine(SendMessageToGPT("Create a quick 2-3 sentence blurb describing the scene as would be described to a detective that just got the case keep it simple but dont give too much info away" + sceneprompt, response =>
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
        while ( sceneprompt == null)
        {
            yield return new WaitForSeconds(1f);
        }

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
                        StartButton.SetActive(true);
                        
                    }));
                    break;

            }
        }
        

    }



}