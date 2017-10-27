using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLight : MonoBehaviour {

    private float flickerPos;
    private Vector3 startPos;
    private float flickerSize;

    // Use this for initialization
    void Start () {
        startPos = transform.localPosition;
        flickerPos = 0.0f;
        flickerSize = 0.0f;
    }
	
	// Update is called once per frame
	void Update () {
    {
        flickerPos += Time.deltaTime * Random.Range(-1, 2);
        transform.localPosition = startPos + new Vector3(Mathf.Sin(flickerPos * 3 * Mathf.PI) * 0.08f,
                                              Mathf.Cos(flickerPos * 3 * Mathf.PI) * 0.08f, 0);
        flickerSize += Time.deltaTime * Random.Range(-1, 2);
        GetComponent<Light>().intensity = (Mathf.Sin(flickerSize * 2) + 1) * 0.4f + 0.8f;
    }
}
}
