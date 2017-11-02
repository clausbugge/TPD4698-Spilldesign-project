using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;
    private static int currentLevel;
    AsyncOperation asyncLoadLevel;
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
        StartCoroutine(nextLevel());
    }

    public void initiateNextLevel()
    {
        //has to be like this because THIS object has to know when loading is complete, and coroutines have to go in order
        //TODO: probably don't HAVE to be.. but this is my solution for now
        StartCoroutine(nextLevel()); 
    }

    public IEnumerator nextLevel()
    {
        if(currentLevel !=0)
        {
            yield return StartCoroutine(Camera.main.GetComponent<CameraScript>().fade(false, 4));
        }
        
        currentLevel++;
        string nextLvlName = "Scenes/level" + currentLevel.ToString(); //important: all scenes have to be in Scenes folder
        //if (!SceneManager.GetSceneByName(nextLvlName).IsValid()) //TODO: can't get this to work for now. cba to fix
        {

            //print(nextLvlName + " not found. end of available levels reached"); //TOOD: add victory screen or whatever
          //  return;
        }
        //else
        {
            AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(nextLvlName);
            int loadingFrames = 0;
            while (!asyncLoadLevel.isDone)
            {
                //loading screen stuff. probably fine without loading screen x - D
                loadingFrames++;
                print("loading frames:" + loadingFrames);
                yield return null;                
            }
            MusicManager.instance.playSong();
        }
    }
}
