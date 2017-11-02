using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{

    public static MusicManager instance;
    AudioSource audioSource;
    public AudioClip[] songs;
    [Range(0, 1)]
    public float volume;
    enum songEnums
    {
        MAIN_THEME
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        //audioSource.clip = songs[(int)songEnums.MAIN_THEME];
        audioSource.clip = songs[Random.Range(0, songs.Length)];// (int)songEnums.MAIN_THEME];
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.PlayDelayed(0.5f); //0.5 second delay
    }

    void Update()
    {
        audioSource.volume = volume;
    }
}
