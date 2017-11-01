using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    public GameObject target; //target to follow
    public Vector2 targetOffset;
    public Color fadeColor;
    private Texture2D fadeTexture;
    public float defaultFadeTime;

	// Use this for initialization
	void Start () {
        targetOffset = new Vector3(0, 0, 0);
        fadeTexture = new Texture2D(1, 1,TextureFormat.ARGB32,false);
        fadeTexture.SetPixel(0, 0, fadeColor);
        fadeTexture.Apply();
        StartCoroutine(fade(true, defaultFadeTime));
    }

    public IEnumerator rotateWhileLookAt(Vector3 eulerAngles, GameObject lookAtObject, float duration)
    {
        StartCoroutine(Tools.rotateObject(gameObject, eulerAngles, duration));
        Vector3 startPos = transform.localPosition;
        Vector3 newPos = startPos;
        for (float i = 0; i < duration; i+=Time.deltaTime)
        {
            newPos = transform.localPosition;
            newPos.z = -5+(i/duration)*4;
            transform.localPosition = newPos;
            //Camera.main.transform.LookAt(lookAtObject.transform);
            yield return null;
        }
    } 

    // Update is called once per frame
    void FixedUpdate ()
    {
        float followSpeed = 1.0f;
        Vector3 distanceVector = target.transform.position+(new Vector3(targetOffset.x, targetOffset.y,0)) - transform.position;
        float distanceFromTarget = distanceVector.magnitude;
        transform.Translate(new Vector3(distanceVector.x, distanceVector.y, 0)*distanceFromTarget*followSpeed*Time.fixedDeltaTime);
	}

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.width), fadeTexture);
    }

    public IEnumerator fade(bool fadeIn, float fadeTime = 1.0f)
    {
        float multiplier = fadeIn ? -1.0f : 1.0f;
        float start = fadeIn ? 1.0f : 0.0f;
        float a = 0;
        for (float i = 0; i < fadeTime; i+=Time.deltaTime)
        {
            a = (i / fadeTime) * multiplier;
            fadeTexture.SetPixel(0, 0, new Color(fadeColor.r, fadeColor.g, fadeColor.b, start+a));
            fadeTexture.Apply();
            yield return null;
        }
        fadeTexture.SetPixel(0, 0, new Color(fadeColor.r, fadeColor.g, fadeColor.b, start + multiplier));
        fadeTexture.Apply();
    }
}
