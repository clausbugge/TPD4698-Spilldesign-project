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
        string nextLvlName = "Scenes/level" + currentLevel.ToString(); //important: all scenes have to be in Scenes folder
        //if (!SceneManager.GetSceneByName(nextLvlName).IsValid()) //TODO: can't get this to work for now. cba to fix
        {

            //print(nextLvlName + " not found. end of available levels reached"); //TOOD: add victory screen or whatever
          //  return;
        }
        //else
        {
            SceneManager.LoadScene(nextLvlName);
        }
        
    }
}
