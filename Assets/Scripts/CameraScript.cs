﻿using System.Collections;
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

    public IEnumerator levelTransitionZoom(GameObject elevator, float duration, float newZ)
    {
        Vector3 camOrigin = transform.position;
        camOrigin.z = -1;// target != null ? target.transform.position.z : ;
        target = null;
        float xRot = transform.rotation.x;
        float angle = xRot;
        float startZ = transform.localPosition.z;
        float deltaZ = startZ;
        Vector3 endPos = elevator.transform.position;
        endPos.y -= 3;
        endPos.z -= 1;
        Vector3 lookatPos = camOrigin;
        Vector3 directionVector = (endPos- transform.position).normalized;
        float distance = (endPos - transform.position).magnitude; //TODO: maybe add overload for moveobject which takes just one vector
        StartCoroutine(Tools.moveObject(gameObject, directionVector, duration, distance));
        for (float i = 0; i < duration; i += Time.deltaTime)
        {

            float pd = i / duration;
            lookatPos = camOrigin + (elevator.transform.position - camOrigin) * pd;
            transform.LookAt(lookatPos);
            yield return null;
        }
    }
    public IEnumerator levelTransitionZoom2(GameObject elevator, float duration, float newZ) //TODO: "needs" to be rewritten
    {
        Vector3 camOrigin;
        camOrigin.z = -1;
        float xRot = transform.rotation.x;
        float angle = xRot;
        float startZ = transform.localPosition.z;
        float deltaZ = startZ;
        Vector3 endPos = elevator.transform.position;
        endPos.y -= 3;
        endPos.z = -5;
        camOrigin = transform.position;
        Vector3 endView = camOrigin;
        endView.y -= 3;
        endView.z = -1;
        
        //camOrigin.z = -1;
        Vector3 lookatPos = camOrigin;
        Vector3 directionVector = (endPos - transform.position).normalized;
        float distance = (endPos - transform.position).magnitude; //TODO: maybe add overload for moveobject which takes just one vector
        StartCoroutine(Tools.moveObject(gameObject, directionVector, duration, distance));
        for (float i = 0; i < duration; i += Time.deltaTime)
        {
            float pd = i / duration;
            lookatPos = elevator.transform.position + (Vector3.down*3/*endView - camOrigin*/) * pd;
            transform.LookAt(lookatPos);
            yield return null;
        }
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        if (target != null)
        {
            float followSpeed = 1.0f;
            Vector3 distanceVector = target.transform.position + (new Vector3(targetOffset.x, targetOffset.y, 0)) - transform.position;
            float distanceFromTarget = distanceVector.magnitude;
            transform.Translate(new Vector3(distanceVector.x, distanceVector.y, 0) * distanceFromTarget * followSpeed * Time.fixedDeltaTime);
        }
        
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
