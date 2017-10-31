using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDoor : MonoBehaviour {
    private bool moving = false;
    private bool triggered = false;
    private GameObject leftDoor;
    private GameObject rightDoor;
    void Start()
    {
        leftDoor = GetComponentsInChildren<BoxCollider2D>()[0].gameObject;
        rightDoor = GetComponentsInChildren<BoxCollider2D>()[1].gameObject;
    }
    public void trigger()
    {
        if (!triggered && !moving)
        {
            StartCoroutine(Tools.moveObject(leftDoor, Vector3.left, 1, 2));
            StartCoroutine(Tools.moveObject(rightDoor, Vector3.right, 1, 2));
            triggered = !triggered;
        }
    }
}
