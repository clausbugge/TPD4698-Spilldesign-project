using UnityEngine;

public class FlickeringLight : MonoBehaviour
{    
    private Vector3 startPos;
    private Vector2 flickerOffset;
    [Header("Position related")]
    public Vector2 flickerRange;
    //[Range(0, 1)]
    public Vector2 flickerDeltaRange;
    public bool randomFlickerDirection;

    private float startIntensity;
    private float intensityOffset;
    [Header("Intensity related")]
    public float intensityRange;
    [Range(0,1)]
    public float intensityDeltaRange;

    float curIntensity = 0.0f;
    Vector2 curFlickerPos = Vector2.zero;

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
        curFlickerPos += TimeManager.instance.gameDeltaTime * (randomFlickerDirection ? new Vector2(Random.Range(-flickerDeltaRange.x, flickerDeltaRange.x), 
                                                                                                    Random.Range(-flickerDeltaRange.y, flickerDeltaRange.y)) : 
                                                                                                    flickerDeltaRange);
        Vector3 newPos = transform.localPosition;
        newPos = startPos + new Vector3(Mathf.Cos(1 * 2 * Mathf.PI * curFlickerPos.x) * flickerRange.x, 
                                        Mathf.Sin(1 * 2 * Mathf.PI * curFlickerPos.y) * flickerRange.y, 0);
        transform.localPosition = newPos;
        curIntensity += TimeManager.instance.gameDeltaTime * intensityDeltaRange;
        lightComponent.intensity = startIntensity + Mathf.Sin(curIntensity*2*Mathf.PI)*intensityRange;
    }
}
