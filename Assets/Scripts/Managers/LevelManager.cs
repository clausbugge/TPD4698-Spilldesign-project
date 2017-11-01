using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;
    private int currentLevel;
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
        currentLevel = 0;
    }

    void Start()
    {
        nextLevel();
    }
    public void nextLevel()
    {
        currentLevel++;
        SceneManager.LoadScene("level" + currentLevel.ToString());
    }
}
