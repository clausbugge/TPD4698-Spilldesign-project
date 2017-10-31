using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSounds : MonoBehaviour
{
    public AudioClip[] sounds;
    private Rigidbody2D r2d;
    public float moveSoundDelay;
    public float pitchRange;
    private SoundCaller sc;
    private Timer soundTimer;
    // Use this for initialization
    void Start ()
    {
        soundTimer = new Timer(moveSoundDelay);
        r2d = GetComponent<Rigidbody2D>();
        sc = GetComponent<SoundCaller>();
    }
	
	//Have no good idea on how to make this dynamic atm
	void Update ()
    { 
        if (soundTimer.hasEnded())
        {            
            if (r2d != null && (!Mathf.Approximately(r2d.velocity.x, 0) || !Mathf.Approximately(r2d.velocity.y, 0)))
            {
                AudioSource mySource = sc.findFreeAudioSource();
                if (mySource != default(AudioSource))
                {
                    soundTimer.restart();
                    mySource.pitch = Random.Range(1 - pitchRange, 1 + 1 + pitchRange);
                    mySource.clip = sounds[Random.Range(0, sounds.Length)];
                    mySource.Play();
                    
                }
            }
        }
		
	}
}
