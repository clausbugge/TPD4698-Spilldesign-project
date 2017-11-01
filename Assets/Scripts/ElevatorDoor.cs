using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDoor : MonoBehaviour {
    private bool moving = false;
    private bool triggered = false;
    private GameObject leftDoor;
    private GameObject rightDoor;
    private GameObject redCarpet;
    void Start()
    {
        leftDoor = transform.GetChild(0).gameObject;
        rightDoor = transform.GetChild(1).gameObject;
        redCarpet = transform.GetChild(2).gameObject;

    }
    public void trigger()
    {
        if (!triggered && !moving)
        {
            StartCoroutine(finishLevel());
        }
    }

    IEnumerator finishLevel()
    {
        triggered = !triggered;
        StartCoroutine(Tools.moveObject(leftDoor, Vector3.left, 2, 1.027125f));
        yield return StartCoroutine(Tools.moveObject(rightDoor, Vector3.right, 2, 1.027125f));
        
        redCarpet.GetComponent<Triggerable>().startTrigger();
    }
}
