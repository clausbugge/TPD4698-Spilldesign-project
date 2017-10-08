using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverScript : MonoBehaviour
{


    private bool Activated = false;
    public List<GameObject> triggerList;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void onTrigger()
    {
        foreach (GameObject obj in triggerList)
        {
            if (obj != null)
            {
                obj.GetComponent<Triggerable>().setTriggered(Activated);
            }
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Activated = !Activated;
                onTrigger();
            }
        }
    }

}
