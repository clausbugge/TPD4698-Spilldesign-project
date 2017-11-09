using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class TextPulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Text txt;
    bool highlighted = false;
    Vector3 startScale;
    int buttonID;
    int buttonType;
    //keep comments in case they fall out. (don't think they can, but not sure)
    //public Color inactiveColor = new Color(0 / 255.0f, 150 / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f); //this is beautiful and you know it
    //public Color highlightedColor = new Color(0 / 255.0f, 0 / 255.0f, 255.0f / 255.0f, 255.0f);
    public Color inactiveColor;
    public Color highlightedColor;
    bool mouseOver = false;
    //SubMenu parentSubMenu;
    void Awake()
    {
      //  parentSubMenu = transform.parent.gameObject.GetComponent<SubMenu>();
        txt = GetComponent<Text>();
        txt.color = inactiveColor;
        startScale = transform.localScale;
    }
    public void setButtonInfo(int id,int type)
    {
        buttonID= id;
        buttonType = type;
    }

    public void highlight(bool toHighlightButton = true)
    {
        highlighted = toHighlightButton;
        if (!highlighted)
        {
            txt.color = inactiveColor;
            transform.localScale = startScale;
            //mouseOver = false;
        }
        if (highlighted)
        {
            txt.color = highlightedColor;
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        mouseOver = true;
        transform.parent.GetComponent<SubMenu>().changeHighlightedButton(buttonID);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        mouseOver = false;
        transform.parent.GetComponent<SubMenu>().changeHighlightedButton(buttonID);
    }
    // Update is called once per frame
    void Update()
    {
        float pulseSpeed = 5.0f;
        if (highlighted)
        {
            transform.localScale = Vector3.one + (Vector3.one + Vector3.one * Mathf.Sin(Time.time * pulseSpeed)) * 0.04f;
            if (Input.GetKeyUp(KeyCode.Space) ||
            Input.GetKeyUp(KeyCode.Return) ||
            (Input.GetMouseButtonUp(0) && mouseOver))
            {
                trigger();
            }
        }
        
        if (mouseOver && !highlighted && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
        {
            mouseOver = false;
            transform.parent.GetComponent<SubMenu>().changeHighlightedButton(buttonID);
        }
    }

    public void trigger()
    {
        
        switch(buttonType)
        {
            case (int)SubMenu.BUTTONS.NEW_GAME:
                LevelManager.instance.loadLevel(0);
                break;
            case (int)SubMenu.BUTTONS.LEVEL_SELECT:
                transform.parent.parent.GetChild(1).gameObject.SetActive(true); //hacky-ish but works
                transform.parent.parent.GetChild(0).gameObject.SetActive(false);                
                break;
            case (int)SubMenu.BUTTONS.EXIT:
                Application.Quit();
                break;
            case (int)SubMenu.BUTTONS.SELECT_LEVEL:
                int level = (int)char.GetNumericValue(name[6]); //this it not just hacky-ish, this is hacky as fuck. CBA to add custom system for level select..
                LevelManager.instance.loadLevel(level-1);
                break;
            case (int)SubMenu.BUTTONS.BACK:
                transform.parent.parent.GetChild(0).gameObject.SetActive(true);
                transform.parent.parent.GetChild(1).gameObject.SetActive(false);
                break;
            default:
                break;
        }
    }
}
