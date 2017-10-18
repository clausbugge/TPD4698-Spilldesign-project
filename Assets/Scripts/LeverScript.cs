using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverScript : MonoBehaviour
{


    //private bool Activated = true;
    public List<GameObject> triggerList;
    private bool playerOnTrigger = false;
    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (playerOnTrigger && Input.GetKeyDown(KeyCode.F))
        {
            onTrigger();
        }
    }

    void onTrigger()
    {
        foreach (GameObject obj in triggerList)
        {
            if (obj != null)
            {
                obj.GetComponent<Triggerable>().startTrigger();
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnTrigger = true;
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnTrigger = false;
        }
    }
}
