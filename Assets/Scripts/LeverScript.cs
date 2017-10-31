using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverScript : MonoBehaviour
{
    public enum LEVER_STATE
    {
        ON,
        OFF
    }

    bool triggerButtonAlreadyDown = false;

    public List<GameObject> triggerList;
    private bool playerOnTrigger = false;
    public LEVER_STATE state;
    public Sprite onSprite;
    public Sprite offSprite;
    // Use this for initialization
    void Start()
    {
        switch (state) //just checking what is set in inspector
        {
            case LEVER_STATE.ON:
                GetComponent<SpriteRenderer>().sprite = onSprite;
                break;
            case LEVER_STATE.OFF:
                GetComponent<SpriteRenderer>().sprite = offSprite;
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerOnTrigger && Input.GetAxis("Interact") == 1 && !triggerButtonAlreadyDown)
        {
            triggerButtonAlreadyDown = true;
            switch(state)
            {
                case LEVER_STATE.ON:
                    
                    GetComponent<SpriteRenderer>().sprite = offSprite;
                    state = LEVER_STATE.OFF;
                    break;
                case LEVER_STATE.OFF:
                    GetComponent<SpriteRenderer>().sprite = onSprite;
                    state = LEVER_STATE.ON;
                    break;
                default:
                    break;
            }
            onTrigger();
        }
        if (Input.GetAxis("Interact") == 0)
        {
            triggerButtonAlreadyDown = false;
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
