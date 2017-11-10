using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : MonoBehaviour {
    private bool moving = false;
    private bool triggered = false;
    private SoundCaller sc;
    public AudioClip[] doorOpeningSounds;
    void Awake()
    {
        sc = GetComponent<SoundCaller>();
    }

    IEnumerator rotateLeft(Vector3 direction)
    {
        moving = true;
        Vector3 startRot = transform.localRotation.eulerAngles;
        float oldAngle = 0;
        float newAngle = 0;
        float deltaAngle = 0;
        float anglePrSec = 45;
        float desiredAngles = 90;
        float duration = desiredAngles / anglePrSec;
        Vector3 around = transform.TransformPoint(new Vector3(.5f, -.5f, 0));
        for (float i = 0; i < duration; i+=Time.deltaTime)
        {
            float v = i / duration;
            //v = v * v * (3 - 2 * v); //smoothify
            v = v * v * v;
            oldAngle = newAngle;
            newAngle = (desiredAngles * v) + (0 * (1 - v));
            deltaAngle = newAngle -oldAngle;
            //angle += deltaAngle;
            transform.RotateAround(around, direction, deltaAngle);
            
            yield return null;
        }

        transform.RotateAround(around, direction, desiredAngles - newAngle); //make sure we have correct angle (error margin based on fps)
        moving = false;
    }

    public void trigger()
    {
        if (!triggered && !moving)
        {
            StartCoroutine(rotateLeft(Vector3.back));
            sc.attemptSound(doorOpeningSounds[Random.Range(0, doorOpeningSounds.Length)],0.02f);
            triggered = !triggered;
        }
        if (triggered && !moving)
        {
            StartCoroutine(rotateLeft(Vector3.forward));
            triggered = !triggered;
        }
    }
}
