using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashScript : MonoBehaviour {

    private bool inNewFloor;

    void init(Vector3 velocity)
    {

    }
	// Use this for initialization
	void Start () {
        inNewFloor = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public bool isInNewFloor()
    {
        return inNewFloor;
    }
    public void OnTriggerEnter2D(Collider2D floor)
    {
        inNewFloor = true;
    }
}
