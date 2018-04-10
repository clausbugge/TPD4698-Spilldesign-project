using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashLight : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        updateFlashlight();
	}

    private void updateFlashlight()
    {
        //int inputsHeld = 0;
        //float newXRot = 0;
        //if (inputs.up)
        //{
        //    newXRot += 270.0f;
        //    inputsHeld++;
        //}
        //if (inputs.down)
        //{
        //    newXRot += 90.0f;
        //    inputsHeld++;
        //}
        //if (inputs.left)
        //{
        //    newXRot += 180.0f;
        //    inputsHeld++;
        //}
        //if (inputs.right)
        //{
        //    newXRot += 360.0f;
        //    inputsHeld++;
        //}
        //if (inputsHeld != 0)
        //{
        //    newXRot %= 360;
        //    newXRot /= inputsHeld;
        //    if (!Mathf.Approximately(flashLightChild.transform.localRotation.x, newXRot)) //don't do it if flashlight already facing correct location
        //    {
        //        flashLightChild.transform.localRotation = Quaternion.Euler(newXRot, 90.0f, 0.0f);
        //    }
        //}
    }
}
