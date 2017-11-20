using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : MonoBehaviour {
    private bool moving = false;
    private bool triggered = false;
    private SoundCaller sc;
    public AudioClip[] doorOpeningSounds;
    [Range(-360,360)]
    public float arcAngle = 90.0f;
    public float openTimeInSeconds = 2.0f;
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
        float anglePrSec = arcAngle/openTimeInSeconds;
        Vector3 around = transform.TransformPoint(new Vector3(.5f, -.5f, 0));
        for (float i = 0; i < openTimeInSeconds; i+= TimeManager.instance.gameDeltaTime)
        {
            float v = i / openTimeInSeconds;
            //v = v * v * (3 - 2 * v); //smoothify
            v = v * v * v;
            oldAngle = newAngle;
            newAngle = (arcAngle * v) + (0 * (1 - v));
            deltaAngle = newAngle -oldAngle;
            //angle += deltaAngle;
            transform.RotateAround(around, direction, deltaAngle);
            
            yield return null;
        }

        transform.RotateAround(around, direction, arcAngle - newAngle); //make sure we have correct angle (error margin based on fps)
        moving = false;
    }

    public void trigger()
    {
        if (!triggered && !moving)
        {
            StartCoroutine(rotateLeft(Vector3.back));
            int test = Random.Range(0, doorOpeningSounds.Length);
            sc.attemptSound(doorOpeningSounds[test], 0.02f);
            triggered = !triggered;
        }
        if (triggered && !moving)
        {
            StartCoroutine(rotateLeft(Vector3.forward));
            triggered = !triggered;
        }
    }
}
