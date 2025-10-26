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
    [SerializeField] private string model = "gpt-4.1";
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
@"SYSTEM: You are an NPC in a murder-mystery game. Follow these rules exactly:
- Answer in **at most 2 short sentences**. Each sentence should be concise (<= 20 words).
- Only report what you personally observed (sight, sound, smell, touch). No inner thoughts or speculation.
- Do NOT add extra commentary, scene descriptions, or instructions to the player.
- Be consistent with earlier factual claims (truths or lies).
- If you are instructed to lie (killer), lie concisely and consistently.
Now await the detective's question and answer only as the character.";


    private Dictionary<string, string> npcPrompts = new()
{

    {
        "Evelyn",
        "You are Evelyn Carter, a 36-year-old art dealer. Elegant, very defensive, quick with sarcasm, rude almost karen like, doesnt like being called a liar, very very rude" 
            },
    {
        "Marcus",
        "You are Marcus Reed, a 86-year-old retired war verterin. Gruff, stoic, blunt; speaks plainly and without flourish, often compares things to the war, very kind and is there to help in any way he can" 
            },
    {
        "Daniel",
        "You are Daniel Hayes, a 42-year-old defense attorney. Confident, articulate, occasionally sharp-tongued, likes to alwasy assume he is in a court case, brings up random court rules and procedure all the time"
        },
    {
        "Rosa",
        "You are Rosa Alvarez, a 18-year-old server intern housekeeper. nervious, observant, soft-spoken. is constantly scared and shaken up by the whole thing, cries a lot and is very scared all the time "
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


        string[] locations = {
    "Victorian manor with multiple wings and a grand staircase",
    "seaside estate overlooking the cliffs",
    "country farmhouse with adjoining barn and cellar",
    "mountain lodge surrounded by pine forest",
    "suburban two-story home with basement and attic",
    "city townhouse with connected study and rooftop terrace",
    "abandoned mansion with dusty corridors and broken chandeliers",
    "modern lake house with large glass windows and private dock",
    "old monastery converted into a residence",
    "remote hunting cabin with nearby shed and tools",
    "luxury penthouse apartment spanning two floors",
    "vineyard estate with a wine cellar and outdoor patio",
    "riverside villa with garden and pool house",
    "country inn with guest rooms and dining hall",
    "old boarding school repurposed as a private residence",
    "mountain chalet with ski storage and large fireplace hall"
};


        string chosenLocation = locations[UnityEngine.Random.Range(0, locations.Length)];

        string[] KillingMethods = { "Poison", "stabbing", "bluntforce trauma", "pushing off roof", "gunshot", };

        string MurderType = locations[UnityEngine.Random.Range(0, locations.Length)];

        int randomSeed = UnityEngine.Random.Range(1000, 9999);

        sceneprompt =
 $@"Write a grounded murder-mystery scene set in a {chosenLocation}. 
Do NOT set the story in an art gallery or exhibition. Make the scene include " + killer + " as the killer and tie them into the story as the murderer, but do NOT reveal that fact except in the final 'Author's Solution' section (see format below). " +

"Characters:  " + "Make the murder method" + MurderType +
"- Evelyn Carter – 36, art dealer.  Elegant, very defensive, quick with sarcasm, rude almost karen like, doesnt like being called a liar, very very rude" +
"- Marcus Reed – 86-year-old retired war verterin. Gruff, stoic, blunt; speaks plainly and without flourish, often compares things to the war, very kind and is there to help in any way he can" +
"- Daniel Hayes – 42-year-old defense attorney. Confident, articulate,  sharp-tongued, likes to alwasy assume he is in a court case, brings up random court rules and procedure all the time" +
"- Rosa Alvarez – 18-year-old server intern housekeeper. nervious, observant, soft-spoken. is constantly scared and shaken up by the whole thing, cries a lot and is very scared all the time " +

"Requirements:" +
"- A murder occurs between 8:30 PM and 9:10 PM. The victim is NOT one of the above characters." +
"- The crime scene is the {chosenLocation}. " +
"- Each character must have one concrete sensory detail (sound, smell, sight, or touch)." +
"- Include at least one believable contradiction between two characters' outward accounts (no arguments or shouting)." +
"- Avoid inner thoughts, speculation, or explicit corroboration statements like 'this can be confirmed by X'." +
"- Make it moderately challenging to deduce the killer from the scene; include at least two plausible red herrings." +

"Format: " +
"1. Write 6–7 short paragraphs describing the scene and what each character was doing (concise, factual)." +
"2. End with the body being discovered and immediate outward reactions." +
"3. Append a 'Scene Summary' with the time of death (exact minute between 8:30–9:10), location, lighting/sounds/smells, and for each character: Key Fact; Sensory Clue; Potential Red Herring. " +
"4. Finally, append an 'Author's Solution' section (see exact template below). The 'Author's Solution' is the only place where the killer is named and the logical solution explained." +

"Style: factual, concise, playable. Random seed: {randomSeed}." + 

"Author's Solution:\r\n- Killer: <Character Name>\r\n- Method: <one short sentence>\r\n- Motive (brief): <one short sentence>\r\n- Key evidence linking killer to crime (3 bullets, concise, observable facts only — no inner thoughts):\r\n  * ...\r\n  * ...\r\n  * ...\r\n- How to deduce (3 clear logical steps a detective can take, each 1 short sentence):\r\n  1. ...\r\n  2. ...\r\n  3. ...\r\n" +
"Make the characters respond more clearly and with more information when going along with this deduction path so the case is easier to solve and have the non killers kinda help the player out down this path";










        apiKey = LoadApiKeyFromEnv();

        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("OpenAI API key missing! Please set it in your .env file.");

        // Generate the scene first (this is a single-shot request), then start the delayed generation of profiles
        StartCoroutine(SendMessageToGPT(sceneprompt, response =>
        {
            sceneprompt = string.IsNullOrWhiteSpace(response) ? sceneprompt : response.Trim();
            // start delayed profile generation after we have the scene
            StartCoroutine(SendMessageToGPT("Create a quick 2-3 sentence blurb describing the scene as would be described to a detective that just got the case keep it simple but dont give too much info away just include the victim how they died and the location" + sceneprompt, response =>
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
            Debug.Log(characterPrompt + " are you real?");
            string fullPrompt = "here is the rules you should follow "+ scenarioContext + "                      Here is the scene prompt you should use this as a guide for questions asked by the user:" + sceneprompt + "               This is your character use the persolanity and quirks of this character when making respnces use the teaits and exagerate them greatly and make it fun:" + characterPrompt + "  DONT ramble on for too long  make your responces a max of 3 sentences";

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


        foreach (var npc in GetConversation(npcName)) 
        {
            Debug.Log(npc);
        }

    }
    public void message()
    {

        StartCoroutine(SendNPCMessage(character.characterName, inputField.text));
        inputField.text = string.Empty;
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