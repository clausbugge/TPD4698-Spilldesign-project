using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SubMenu : MonoBehaviour {

    public enum BUTTONS
    {
        NEW_GAME,
        LEVEL_SELECT,
        EXIT,
        BACK,
        SELECT_LEVEL
    }

    private GameObject[] menuObjects; //are identified by being texts. all need TextPulse script
    private int highlightedButton;
    private bool directionDown = false;
    // Use this for initialization
    public BUTTONS[] buttonIDs; //IMPORTANT! need to be as long as number of buttons
	void Start () {
        int i = 0;
        menuObjects = new GameObject[gameObject.GetComponentsInChildren<Text>().Length];
		foreach (Text menuObject in gameObject.GetComponentsInChildren<Text>())
        {
            menuObjects[i] = menuObject.gameObject;
            menuObjects[i].GetComponent<TextPulse>().setButtonInfo(i, (int)buttonIDs[i]);
            i++;
        }
        highlightedButton = 0;
        menuObjects[highlightedButton].GetComponent<TextPulse>().highlight();
	}

    public void changeHighlightedButton(int newButton)
    {
        menuObjects[highlightedButton].GetComponent<TextPulse>().highlight(false);
        highlightedButton = newButton;
        menuObjects[highlightedButton].GetComponent<TextPulse>().highlight();
    }

	// Update is called once per frame
	void Update ()
    {
        if (Input.GetAxis("Vertical") != 0 && directionDown == false)
        {
            directionDown = true;            
            int newButton = (highlightedButton + Mathf.RoundToInt(Input.GetAxis("Vertical")*-1)) % menuObjects.Length; //*-1 because we add buttons from top to buttom, while axis go from bottom to top
            if (newButton < 0)
            {
                newButton = menuObjects.Length -1;
            }
            changeHighlightedButton(newButton);
        }
        if (Input.GetAxis("Vertical") == 0)
        {
            directionDown = false;
        }
    }
}
