using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightScript : MonoBehaviour {
    public bool startOn; //more readable
    private bool triggered;
    Light lightComponent;
    // Use this for initialization
    void Start()
    {
        triggered = startOn;
        lightComponent = GetComponent<Light>();
        lightComponent.enabled = startOn;
    }
    public void trigger()
    {
        triggered = !triggered;
        lightComponent.enabled = triggered;
    }
}
