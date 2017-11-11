using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;
    private static int currentLevel;
    bool gameLoadedOnce = false;
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void Start()
    {
        currentLevel = 0;
        //DEBUG: this whole check if 100% for debugging so you can start scene from different levels but still hear music and transition to next level
        //assumes all level start with L/l and 6th element is levelNumber
        if (SceneManager.GetActiveScene().name[0] != 'L' && SceneManager.GetActiveScene().name[0] != 'l')
        {
         //   StartCoroutine(nextLevel());
        }
        else
        {

            currentLevel = int.Parse(SceneManager.GetActiveScene().name[5].ToString());
           // print(currentLevel);
            //MusicManager.instance.playSong();
        }

    }

    public void initiateNextLevel()
    {
        //has to be like this because THIS object has to know when loading is complete, and coroutines have to go in order
        //TODO: probably don't HAVE to be.. but this is my solution for now
        if (instance == this) 
        {
            StartCoroutine(nextLevel());
        }
    }

    public void loadLevel(int levelToLoad)
    {
        currentLevel = levelToLoad;
        StartCoroutine(nextLevel());
    }

    public IEnumerator nextLevel()
    {
        if (gameLoadedOnce)
        {
            yield return StartCoroutine(Camera.main.GetComponent<CameraScript>().fade(false, 4));
        }
        else
        {
            gameLoadedOnce = true;
        }
        
        
        
        string nextLvlName = "Scenes/level" + currentLevel.ToString(); //important: all scenes have to be in Scenes folder
        currentLevel++;

        //if (!SceneManager.GetSceneByName(nextLvlName).IsValid()) //TODO: can't get this to work for now. cba to fix
        {

            //print(nextLvlName + " not found. end of available levels reached"); //TOOD: add victory screen or whatever
            //  return;
        }
        //else
        {
            print(nextLvlName);
            AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(nextLvlName);
            int loadingFrames = 0;
            while (!asyncLoadLevel.isDone)
            {
                //loading screen stuff. probably fine without loading screen x - D
                loadingFrames++;
                print("loading frames:" + loadingFrames);
                yield return null;                
            }
            MusicManager.instance.playSong(MusicManager.songEnums.MAIN_THEME);
        }
    }
}
