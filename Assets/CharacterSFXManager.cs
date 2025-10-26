using UnityEngine;
using System.Collections.Generic;

public class CharacterSFXManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterSFX
    {
        public string characterName;
        public AudioClip sfx;
    }

    public List<CharacterSFX> characterSounds = new List<CharacterSFX>();
    private AudioSource audioSource;
    private Dictionary<string, AudioClip> sfxLookup = new Dictionary<string, AudioClip>();

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Convert list to dictionary for fast lookup
        foreach (var c in characterSounds)
        {
            if (!sfxLookup.ContainsKey(c.characterName))
                sfxLookup.Add(c.characterName, c.sfx);
        }
    }

    public void PlayCharacterSFX(string characterName)
    {
        if (sfxLookup.TryGetValue(characterName, out AudioClip clip) && clip != null)
        {
            
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"No SFX found for character: {characterName}");
        }
    }
}
