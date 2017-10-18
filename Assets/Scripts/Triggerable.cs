using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triggerable : MonoBehaviour {

    public void startTrigger()//GameObject triggeringObject)
    {
        gameObject.SendMessage("trigger",SendMessageOptions.DontRequireReceiver);//, triggeringObject);
    }
}
