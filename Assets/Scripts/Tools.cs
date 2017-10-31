using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools{
    public enum INTERPOLATION_TYPE
    {
        LERP,
        SMOOTH
    }

    public static float lerp(float pd) //pd = percentage done. just for testing.
    {
        return pd;
    }

    public static float smoothInterpolation(float pd) //pd = percentage done
    {
        return pd * pd * (3 - 2 * pd);
    }

    public static IEnumerator moveObject(GameObject go, Vector3 direction, float duration, float distance, INTERPOLATION_TYPE type = INTERPOLATION_TYPE.SMOOTH, int power = 1) //direction normalized
    {
        Vector3 startPos = go.transform.position;
        System.Func<float, float> interpolationFunc = smoothInterpolation;
        float delta = 0.0f;
        float oldPos = 0.0f;
        float newPos = 0.0f;

        switch (type)
        {
            case INTERPOLATION_TYPE.LERP:
                interpolationFunc = lerp;
                break;
            case INTERPOLATION_TYPE.SMOOTH:
                interpolationFunc = smoothInterpolation;
                break;
            default:
                Debug.Log("invalid interpolation type. default= smooth interpolation");
                break;
        }

        for (float i = 0; i < duration; i += Time.deltaTime)
        {
            float pd = 0.25f + (i * 0.75f / duration);
            newPos = interpolationFunc(pd);
            newPos = Mathf.Pow(newPos, power);
            delta = newPos - oldPos;
            oldPos = newPos;
            go.transform.Translate(direction * delta * distance);
            yield return 0;
        }
        go.transform.position = startPos + (direction * distance);
    }

    public static int findMask(string[] layers, bool flipped = false)
    {
        int layerMask = 0;
        foreach (var layer in layers)
        {
            layerMask += 1 << LayerMask.NameToLayer(layer);
        }
        return flipped ? ~layerMask : layerMask;
    }
}
