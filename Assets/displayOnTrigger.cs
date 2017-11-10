using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class displayOnTrigger : MonoBehaviour {

    public bool startVisible = false;
    public bool canBeDisabled = false;
    private bool isVisible;

	// Use this for initialization
	void Start () {
        isVisible = startVisible ? true : false;
        if (!isVisible)
        {
            Color newColor = GetComponent<SpriteRenderer>().color;
            newColor.a = 0;
            GetComponent<SpriteRenderer>().color = newColor;
        }
	}
	
    public void trigger()
    {
        if (!GetComponent<Renderer>().enabled)
        {
            StopCoroutine(changeVisibility()); //doesn't seem to do jack shit
            StartCoroutine(changeVisibility());
        }
        else if (canBeDisabled)
        {
            StopCoroutine(changeVisibility()); //doesn't seem to do jack shit
            StartCoroutine(changeVisibility());
        }
    }

    IEnumerator changeVisibility()
    {
        float start = isVisible ? 1 : 0;
        float goal = isVisible ? 0 : 1;
        float fadeTime = 1.5f;
        Color newColor = GetComponent<SpriteRenderer>().color;
        for (float i = 0; i < fadeTime; i+=Time.deltaTime)
        {
            newColor.a = (goal * (i / fadeTime)) + start*(1-(i/fadeTime));// curAlpha;
            GetComponent<SpriteRenderer>().color = newColor;
            yield return null;
        }
        isVisible = !isVisible;
    }

}
