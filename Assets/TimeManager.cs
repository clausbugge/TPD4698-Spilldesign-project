using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour {

    public static TimeManager instance;
    public float gameTimeMultiplier = 1.0f;
    public float gameDeltaTime;
    public float fixedGameDeltaTime;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            DestroyObject(gameObject);
        }
        DontDestroyOnLoad(this);
    }

    public void pauseGameTime()
    {
        gameTimeMultiplier = 0.0f;
    }
    public void resumeGameTime()
    {
        gameTimeMultiplier = 1.0f;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        gameDeltaTime = gameTimeMultiplier * Time.deltaTime;
    }
    void FixedUpdate()
    {
        fixedGameDeltaTime = gameTimeMultiplier * Time.fixedDeltaTime;
    }
}
