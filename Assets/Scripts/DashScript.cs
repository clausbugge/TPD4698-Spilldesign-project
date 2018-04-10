using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashScript : MonoBehaviour {

    private bool inNewFloor;

    public void OnTriggerEnter2D(Collider2D floor)
    {
        inNewFloor = true;
    }
	// Use this for initialization
	void Start () {
        inNewFloor = false;
	}

    public bool isInNewFloor()
    {
        return inNewFloor;
    }
    
}
