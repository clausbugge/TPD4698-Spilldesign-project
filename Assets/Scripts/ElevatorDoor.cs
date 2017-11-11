using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDoor : MonoBehaviour {
    private bool moving = false;
    private bool triggered = false;
    private GameObject leftDoor;
    private GameObject rightDoor;
    private GameObject redCarpet;
    public GameObject heroObject;
    void Start()
    {
        leftDoor = transform.GetChild(0).gameObject;
        rightDoor = transform.GetChild(1).gameObject;
        redCarpet = transform.GetChild(2).gameObject;
        if(name =="StartElevator")
        {
            //so it's easier to test. can just have hero in scene for testing
            if (GameObject.Find("Hero") == null)
            {
                StartCoroutine(beginLevel());
            }
        }
    }
    public void trigger()
    {
        if (!triggered && !moving)
        {
            StartCoroutine(finishLevel());
        }
    }

    IEnumerator beginLevel()
    {
        GameObject newHero = Instantiate(heroObject);
        Camera.main.GetComponent<CameraScript>().target = null;
        Vector3 spawnPos = transform.position;
        spawnPos.z = -0.5f;
        newHero.transform.position = spawnPos;

        newHero.GetComponent<InputHandler>().changeHeroState(InputHandler.HERO_STATE.DISABLED);

        Camera.main.transform.rotation = Quaternion.Euler(Vector3.left * 90);
        Vector3 camSpawnPos = transform.position;
        camSpawnPos.z = -1.0f;
        camSpawnPos.y -= 3.0f;
        Camera.main.transform.position = camSpawnPos;

        StartCoroutine(Tools.moveObject(leftDoor, Vector3.left, 2, 1f));
        yield return StartCoroutine(Tools.moveObject(rightDoor, Vector3.right, 2, 1f));
        StartCoroutine(Camera.main.GetComponent<CameraScript>().levelTransitionZoom2(gameObject, 4,Camera.main.GetComponent<CameraScript>().getStartDistance()));
        yield return StartCoroutine(redCarpet.GetComponent<RedCarpet>().openCarpet());

        Vector3 moveDir = Vector3.down;
        moveDir.z = 0.0f;
        float moveTime = 3.5f;
        StartCoroutine(Tools.moveObject(newHero, moveDir, moveTime, 3, Tools.INTERPOLATION_TYPE.LERP));
        Camera.main.GetComponent<CameraScript>().target = newHero;
        newHero.GetComponent<InputHandler>().changeHeroState(InputHandler.HERO_STATE.IDLE);
    }

    IEnumerator finishLevel()
    {
        triggered = !triggered;
        MusicManager.instance.playFanfare();
        GameObject hero = GameObject.Find("Hero") != null ? GameObject.Find("Hero") : GameObject.Find("Hero(Clone)");
        hero.GetComponent<InputHandler>().changeHeroState(InputHandler.HERO_STATE.DISABLED);
        StartCoroutine(Tools.moveObject(leftDoor, Vector3.left, 2, 1f));
        StartCoroutine(Camera.main.GetComponent<CameraScript>().levelTransitionZoom(gameObject,4,-1));
        yield return StartCoroutine(Tools.moveObject(rightDoor, Vector3.right, 2, 1f));
        yield return StartCoroutine(redCarpet.GetComponent<RedCarpet>().openCarpet());

        
        Vector3 moveDir = (-hero.transform.position + transform.position).normalized;
        moveDir.z = 0.0f;
        float moveTime = 2.0f;
        StartCoroutine(Tools.moveObject(hero, moveDir, moveTime, 3, Tools.INTERPOLATION_TYPE.LERP));
        LevelManager.instance.initiateNextLevel();
    }
}
