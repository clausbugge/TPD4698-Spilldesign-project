using UnityEngine;

public class FlickeringLight : MonoBehaviour
{    
    private Vector3 startPos;
    private Vector2 flickerOffset;
    [Header("Position related")]
    public float flickerRadius;
    [Range(0, 1)]
    public float flickerDeltaRange;

    private float startIntensity;
    private float intensityOffset;
    [Header("Intensity related")]
    public float intensityRange;
    [Range(0,1)]
    public float intensityDeltaRange;

    private Light lightComponent;
    
    // Use this for initialization
    void Start ()
    {
        lightComponent = GetComponent<Light>();
        startPos = transform.localPosition;
        flickerOffset = Vector2.zero;

        startIntensity = lightComponent.intensity;
        intensityOffset = 0.0f;
    }
	
	// Update is called once per frame
	void Update ()
    {
        flickerOffset.x += Time.deltaTime * Random.Range(-flickerDeltaRange, flickerDeltaRange);
        flickerOffset.y += Time.deltaTime * Random.Range(-flickerDeltaRange, flickerDeltaRange);
        transform.localPosition = startPos + new Vector3(Mathf.Cos((flickerOffset.x*360 +90)*Mathf.Deg2Rad) * flickerRadius,
                                                         Mathf.Sin(flickerOffset.y *360*Mathf.Deg2Rad) * flickerRadius, 0);

        intensityOffset += Time.deltaTime * Random.Range(-intensityDeltaRange, intensityDeltaRange);
        lightComponent.intensity = startIntensity + Mathf.Sin(intensityOffset)*intensityRange;
    }
}
