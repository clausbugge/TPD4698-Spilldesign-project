using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubbleSpawner : MonoBehaviour {

    public GameObject speechBubblePrefab;
    private GameObject speechBubble;
    private float timeSinceTextSpawn = 0.0f;
    private float textDuration = 3.0f;

    bool visible = false;
	// Use this for initialization
	void Start () {
        speechBubble = Instantiate(speechBubblePrefab, transform.position + new Vector3(0.5f, 0.7f, 0.0f), Quaternion.identity) as GameObject;
        speechBubble.transform.parent = transform;
        speechBubble.SetActive(visible);
    }
	// Update is called once per frame
	void Update () {

        timeSinceTextSpawn += TimeManager.instance.gameDeltaTime;
        //removes text bubble
        if(timeSinceTextSpawn > textDuration)
        {
            DestroySpeechBubble();
        }
        //hack for spawning text bubble
		if(Input.GetKeyDown(KeyCode.T))
        {
            if(!visible)
            {
                SpawnSpeechBubble("hey text this is pretty long to please fit the image");
            }
            else
            {
                DestroySpeechBubble();
            }
        }
        
        FadeSpeechBubble();
    }

    private void FadeSpeechBubble()
    {
        float fadeOutStart = textDuration / 2;
        if(fadeOutStart < timeSinceTextSpawn)
        {
            var bubbleImage = speechBubble.transform.GetChild(0).GetComponent<Image>();
            if(bubbleImage != null)
            {
                var imageColor = bubbleImage.color;
                bubbleImage.color = new Color(imageColor.r, imageColor.g, imageColor.b, 2*(textDuration-timeSinceTextSpawn)/textDuration);
            }
            else
            {
                Debug.Log("failed to find image");
            }
        }
    }

    public void DestroySpeechBubble()
    {
        visible = false;
        speechBubble.SetActive(false);
    }

    public void SpawnSpeechBubble(string displayText, float duration = 5.0f)
    {
        textDuration = duration;
        var bubbleImage = speechBubble.transform.GetChild(0);
        if (bubbleImage != null)
        {
            var textGameObject = bubbleImage.GetChild(0);
            if (textGameObject != null)
            {
                bubbleImage.GetComponent<Image>().color = new Color(255, 255, 255, 255);
                visible = true;
                speechBubble.SetActive(visible);
                var text = textGameObject.GetComponent<Text>().text = displayText;
                timeSinceTextSpawn = 0.0f;
            }
            else
            {
                Debug.Log("Textarea not found");
            }
        }
        else
        {
            Debug.Log("Bubble image not found!");
        }
    }
}
