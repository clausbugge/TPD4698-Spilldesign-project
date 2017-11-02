using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{

    public static MusicManager instance;
    AudioSource audioSource;
    public AudioClip[] songs;
    public AudioClip[] fanfares;
    [Range(0, 1)]
    public float volume;
    public enum songEnums
    {
        MAIN_THEME,
        FANFARE
    };

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            DestroyObject(gameObject);
            //Destroy(this);
        }
        DontDestroyOnLoad(this);
        audioSource = GetComponent<AudioSource>();
    }


    public void playSong()
    {
        //audioSource.clip = songs[(int)songEnums.MAIN_THEME];
        audioSource.clip = songs[Random.Range(0, songs.Length)];// (int)songEnums.MAIN_THEME];
        audioSource.loop = true;
        audioSource.volume = volume;
        audioSource.PlayDelayed(0.5f); //0.5 second delay
    }

    public void playFanfare()
    {
        audioSource.clip = fanfares[Random.Range(0, fanfares.Length)];// (int)songEnums.MAIN_THEME];
        audioSource.volume = volume;
        audioSource.loop = false;
        audioSource.Play();
    }

    void Update()
    {
        audioSource.volume = volume;
    }
}
