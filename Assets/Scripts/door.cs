using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : MonoBehaviour {
    private bool moving = false;
    private bool triggered = false;

    IEnumerator rotateLeft(Vector3 direction)
    {
        moving = true;
        Vector3 startRot = transform.localRotation.eulerAngles;
        float angle = 0;
        float deltaAngle = 0;
        float anglePrSec = 45;
        float desiredAngles = 90;
        Vector3 around = transform.TransformPoint(new Vector3(.5f, -.5f, 0));
        while (angle < desiredAngles)
        {
            deltaAngle = anglePrSec * Time.deltaTime;
            angle += deltaAngle;
            transform.RotateAround(around, direction, deltaAngle);
            yield return null;
        }
        moving = false;
    }

    public void trigger()
    {
        if (!triggered && !moving)
        {
            StartCoroutine(rotateLeft(Vector3.back));
            triggered = !triggered;
        }
        if (triggered && !moving)
        {
            StartCoroutine(rotateLeft(Vector3.forward));
            triggered = !triggered;
        }
    }
}
