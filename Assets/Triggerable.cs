using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triggerable : MonoBehaviour {

    public bool triggered = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setTriggered(bool triggered)
    {
        gameObject.GetComponentInChildren<Light>().enabled = triggered;
        if(!triggered)
        {
            gameObject.layer = LayerMask.NameToLayer("Unlit Floor");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Floor");
        }
    }
}
