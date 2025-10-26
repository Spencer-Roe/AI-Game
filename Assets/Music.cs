using UnityEngine;

public class Music : MonoBehaviour
{
    public AudioClip backgroundMusic;
    private AudioSource audioSource;

    void Awake()
    {
        // Prevent duplicates when changing scenes
        if (FindObjectsOfType<Music>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = true;
        audioSource.volume = 0.5f;
        audioSource.Play();
    }
}
