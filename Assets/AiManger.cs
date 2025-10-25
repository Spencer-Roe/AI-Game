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


string apiKey = null;

    public void Start()
    {
        foreach (var line in File.ReadAllLines(".env"))
        {
            if (line.StartsWith("OPENAI_API_KEY="))
            {
                apiKey = line.Split('=')[1].Trim();
                break;
            }
        }

        if (string.IsNullOrEmpty(apiKey))
            throw new Exception("OpenAI API key missing! Please set it in your .env file.");
    }


[Header("OpenAI Settings")]

    [SerializeField] private string model = "gpt-4o-mini";

    // Each NPC has its own conversation history
    private Dictionary<string, List<JObject>> npcConversations = new();

    /// <summary>
    /// Send a message from a given NPC and get GPT's response.
    /// </summary>
    public IEnumerator SendNPCMessage(string npcName, string playerMessage, System.Action<string> onResponse)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError(" API key missing!");
            yield break;
        }

        // Ensure this NPC has a history list
        if (!npcConversations.ContainsKey(npcName))
            npcConversations[npcName] = new List<JObject>();

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
                    onResponse?.Invoke(reply);
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
