using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedCarpet : MonoBehaviour
{
    public int carpetLength;
    public Material carpetMat;
    public GameObject carpetPiece;
    public float openDuration;
    private GameObject[] carpetSegments;
    [Range(15, 45)]
    public float anglePerPiece; //some strange behaviour with this variable... seem to encounter issues when anglePerPiece*carpetLength > ~450. probably imprecision with time and floats
    // Use this for initialization
    void Start()
    {
        carpetSegments = new GameObject[carpetLength];
        carpetSegments[0] = Instantiate(carpetPiece);
        carpetSegments[0].transform.parent = transform;
        carpetSegments[0].transform.localPosition = Vector3.zero;
        carpetSegments[0].transform.Rotate(270+0, 0, 0);

        for (int i = 1; i < carpetLength; i++)
        {
            carpetSegments[i] = Instantiate(carpetPiece);
            carpetSegments[i].transform.parent = transform;
            carpetSegments[i].transform.localEulerAngles = Vector3.zero;
            Transform t = carpetSegments[i - 1].transform;
            carpetSegments[i].transform.position = t.position - t.forward * 0.1f;
            carpetSegments[i].transform.rotation = t.rotation;
            carpetSegments[i].transform.RotateAround(t.position - t.forward * 0.1f * 0.5f, Vector3.right, anglePerPiece);
        }
    }

    public IEnumerator openCarpet()
    {
        Vector3 newPos = Vector3.zero;
        Vector3 deltaPos = Vector3.zero;
        Vector3 oldPos = Vector3.zero;
        
        //Camera.main.GetComponent<CameraScript>().target = gameObject;
        //Camera.main.GetComponent<CameraScript>().targetOffset = Vector2.zero;
        for (float t = 0; t < openDuration; t+= TimeManager.instance.gameDeltaTime)
        {
            deltaPos = Vector3.zero;
            for (int i = 1; i < carpetLength; i++)
            {
                Transform trans = carpetSegments[i - 1].transform;
                carpetSegments[i].transform.position = (trans.position- trans.forward * 0.1f * 0.5f) + (-carpetSegments[i].transform.forward*0.1f*0.5f);
                carpetSegments[i].transform.RotateAround(trans.position - trans.forward * 0.1f * 0.5f, Vector3.left, anglePerPiece * (i/openDuration)* TimeManager.instance.gameDeltaTime); 
            }
            yield return null;
        }

    }
}
