using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public GameObject target; //target to follow
    public Vector2 targetOffset;
	// Use this for initialization
	void Start () {
        targetOffset = new Vector3(0, 0, 0);
	}
	// Update is called once per frame
	void LateUpdate () {
        float followSpeed = 1.0f;
        Vector3 distanceVector = target.transform.position+(new Vector3(targetOffset.x, targetOffset.y,0)) - transform.position;
        float distanceFromTarget = distanceVector.magnitude;
        transform.Translate(new Vector3(distanceVector.x, distanceVector.y, 0)*distanceFromTarget*followSpeed*Time.deltaTime);
        targetOffset = new Vector3(0, 0, 0);
	}
}
