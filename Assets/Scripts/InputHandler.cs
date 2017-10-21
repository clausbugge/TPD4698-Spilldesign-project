using System.Collections;
using UnityEngine;

enum HERO_STATE
{
    IDLE,
    MOVING,
    DASHING,
    NO_OF_STATES
}

enum GHOST_STATE
{
    HUMAN,
    GHOST,
    NO_OF_STATES
}

enum MOVE_DIRECTION
{
    UP = 1,
    DOWN = 2,
    LEFT = 4,
    RIGHT = 8,
    UPLEFT = UP+LEFT,
    UPRIGHT = UP + RIGHT,
    DOWNLEFT = DOWN + LEFT,
    DOWNRIGHT = DOWN + RIGHT
}

struct Inputs
{
    public bool up;
    public bool down;
    public bool left;
    public bool right;
    public bool space;
}

public class InputHandler : MonoBehaviour {
    public int moveSpeed = 1;
    public Sprite humanIdleDashingSprite; //idle hero sprite when he is dashing
    public Sprite ghostIdleDashingSprite; //idle ghost sprite when he is dashing
    public Sprite humanDashingSprite; //Sprite for actual dash object
    public Sprite ghostDashingSprite; //Sprite for actual dash object
    public Sprite humanSprite;
    public Sprite ghostSprite;

    public float dashTimer; //dashtime in seconds
    public float dashRange; //dashrange in unity units
    private Vector2 dashOrigin;
    GameObject dasher; //dasher/placeholder for original
    public Material dasherMaterial;

    private Vector2 newVelocity;
    private HERO_STATE state;
    private GHOST_STATE ghostState;
    private float dt;
    private GameObject flashLightChild;
    private GameObject ghostHighlightChild;
    private GameObject rubberBandParticlesChild;
    private GameObject borderParticlesChild;
    private GameObject borderRubberBandParticlesChild;
    private Light ghostHighlightChildComponent;
    private float flickerPos;
    private float flickerSize;
    private Inputs inputs;
    private Vector2 breakingPoint; //for where human/ghost enters dark/light
    private Vector2 darknessBreakingPoint; //for where ghost leaves legal radius
    private float distanceAllowedOutside; //how far player can travelled from safe zone. better than time spent outside methinks.
    private float distanceAllowedInDarkness; //bad name. how far player can travell from 
    private float distanceAllowedOutsideDarkness; 
    private Vector2 lastDirection;
    private void Awake()
    {
        dashOrigin = Vector2.zero;
        flashLightChild = transform.GetChild(0).gameObject;
        ghostHighlightChild = transform.GetChild(1).gameObject;
        rubberBandParticlesChild = transform.GetChild(2).gameObject;
        borderParticlesChild = transform.GetChild(3).gameObject;
        borderRubberBandParticlesChild = transform.GetChild(4).gameObject;
        ghostHighlightChildComponent = ghostHighlightChild.GetComponent<Light>();
        ghostHighlightChildComponent.enabled = false;
        rubberBandParticlesChild.SetActive(false);
        borderRubberBandParticlesChild.SetActive(false);
        inputs.up = false;
        inputs.down = false;
        inputs.left = false;
        inputs.right = false;
        lastDirection = Vector2.right;
       // direction = (int)MOVE_DIRECTION.LEFT;
        ghostState = GHOST_STATE.HUMAN;
        breakingPoint = Vector2.zero; //TODO: can have repercussions once every blue moon
        distanceAllowedOutside = 1.5f;
        distanceAllowedInDarkness = 8.0f;
        ParticleSystem.ShapeModule bps = borderParticlesChild.GetComponent<ParticleSystem>().shape;
        bps.radius = distanceAllowedInDarkness;
        distanceAllowedOutsideDarkness = 4.0f;
    }

    // Use this for initialization
    void Start () {
        state = (int)HERO_STATE.IDLE;
	}

    int findMask(string[] layers, bool flipped = false)
    {
        int layerMask = 0;
        foreach (var layer in layers)
        {
            layerMask += 1 << LayerMask.NameToLayer(layer);
        }
        return flipped ? ~layerMask : layerMask;
    }

    bool isPointInDark(Vector3 point) //TODO: Will be converted to vec2 in function. maybe rewrite whole game to use 3d physics (some weakness with 2d)
    {
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if(light.name=="Ghost Highlight") //TODO: slightly hacky. would be better with proper handler for light(low-prio)
            {
                continue;
            }
            if (light.isActiveAndEnabled)
            {
                float distance = ((Vector2)light.transform.position - (Vector2)point).magnitude;
                Ray2D ray = new Ray2D();
                ray.origin = point;
                ray.direction = (light.transform.position - point).normalized;
                if (!Physics2D.Raycast(ray.origin, ray.direction, distance))//, soup))
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        dt = Time.deltaTime;
        checkInput();

        //if is in ghost state
        updateGhostHighlight();
        switch (state)
        {
            case HERO_STATE.IDLE:
                if (inputs.space) //turns out he can dash without moving after all!
                {
                    StartCoroutine(startDash());
                }
                break;
            case HERO_STATE.DASHING:
                break;
            case HERO_STATE.MOVING:
                updateFlashlight();
                updateCamera();
                if (inputs.space) //turns out he can dash without moving after all!
                {
                    StartCoroutine(startDash());
                }
                break;
            default:
                break;
        }
    }
    private void updateCamera()
    {
        Camera.main.GetComponent<CameraScript>().targetOffset = newVelocity.normalized*1.2f;
    }
    private void checkInput()
    {
        inputs.up = false;
        inputs.down = false;
        inputs.left = false;
        inputs.right = false;
        inputs.space = false;

        if (Input.GetKey(KeyCode.W) || Input.GetKeyDown(KeyCode.W))
        {
            inputs.up = true;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKeyDown(KeyCode.S))
        {
            inputs.down = true;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKeyDown(KeyCode.A))
        {
            inputs.left = true;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKeyDown(KeyCode.D))
        {
            inputs.right = true;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            inputs.space = true;
        }
    }
    private void updateGhostHighlight()
    {
        flickerPos+= Time.deltaTime*Random.Range(-1,2);
        ghostHighlightChild.transform.localPosition = new Vector3(Mathf.Sin(flickerPos * 3 * Mathf.PI) * 0.08f,
                                                                  Mathf.Cos(flickerPos * 3 * Mathf.PI) * 0.08f, -1.0f);
        flickerSize+= Time.deltaTime*Random.Range(-1, 2);
        ghostHighlightChild.GetComponent<Light>().intensity = (Mathf.Sin(flickerSize * 2) + 1) * 0.4f + 0.8f;
    }

    private void updateFlashlight()
    {
        int inputsHeld = 0;
        float newXRot = 0;
        if (inputs.up)
        {
            newXRot += 270.0f;
            inputsHeld++;
        }
        if (inputs.down)
        {
            newXRot += 90.0f;
            inputsHeld++;
        }
        if (inputs.left)
        {
            newXRot += 180.0f;
            inputsHeld++;
        }
        if (inputs.right)
        {
            newXRot += 360.0f;
            inputsHeld++;
        }
        if (inputsHeld != 0)
        {
            //newXRot %= 360;
            newXRot /= inputsHeld;
            if (!Mathf.Approximately(flashLightChild.transform.localRotation.x, newXRot)) //don't do it if flashlight already facing correct location
            {
                flashLightChild.transform.localRotation = Quaternion.Euler(newXRot, 90.0f, 0.0f);
            }
        }
    }

    enum INTERPOLATION_TYPE
    {
        LERP,
        SMOOTH
    }

    private float lerp(float pd) //pd = percentage done. just for testing.
    {
        return pd;
    }

    private float smoothInterpolation(float pd) //pd = percentage done
    {
        return pd * pd * (3 - 2 * pd);
    }

    IEnumerator moveObject(GameObject go, Vector3 direction, float duration, float distance, INTERPOLATION_TYPE type = INTERPOLATION_TYPE.SMOOTH, int power = 1) //direction normalized
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
                print("invalid interpolation type. default= smooth interpolation");
                break;
        }
        
        for (float i = 0; i < duration; i += dt)
        {
            float pd = 0.25f + (i * 0.75f / duration);
            newPos = interpolationFunc(pd);
            newPos = Mathf.Pow(newPos, power); 
            delta = newPos - oldPos;
            oldPos = newPos;
            go.transform.Translate(direction * delta * distance);
            yield return 0;
        }
        float so = 1.0f;
        go.transform.position = startPos + (direction * distance);
    }

    private GameObject createDasher()//Sprite dashSprite)//, int dashLayer)
    {
        GameObject dasher = new GameObject("dasher", typeof(SpriteRenderer));//, typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(DashScript));
        dasher.transform.position = transform.position;
        Camera.main.GetComponent<CameraScript>().target = dasher;
        dasher.GetComponent<SpriteRenderer>().sprite = humanDashingSprite;
        dasher.GetComponent<SpriteRenderer>().sortingOrder = 1;
        return dasher;
    }
    IEnumerator dashToOrigin()
    {
        float distance = (dasher.transform.position - transform.position).magnitude;
        Vector2 direction = (dasher.transform.position - transform.position).normalized;
        GetComponent<BoxCollider2D>().enabled = false;
        Camera.main.GetComponent<CameraScript>().target = dasher;
        yield return StartCoroutine(moveObject(gameObject, direction, dashTimer, distance)); //dash away
        Camera.main.GetComponent<CameraScript>().target = this.gameObject;
        GetComponent<BoxCollider2D>().enabled = true;
        changeGhostState(GHOST_STATE.HUMAN);
        borderParticlesChild.transform.SetParent(transform);
        borderParticlesChild.transform.localPosition = Vector3.zero;
        Destroy(dasher);
        yield return null;
    }

    private void changeGhostState(GHOST_STATE newState)
    {
        ghostState = newState;
        switch (newState)
        {
            case GHOST_STATE.GHOST:
                gameObject.GetComponent<SpriteRenderer>().sprite = ghostSprite;
                ghostHighlightChildComponent.enabled = true;
                break;
            case GHOST_STATE.HUMAN:
                gameObject.GetComponent<SpriteRenderer>().sprite = humanSprite;
                ghostHighlightChildComponent.enabled = false;
                break;
            default:
                break;
        }
    }
    private void changeHeroState(HERO_STATE newState)
    {
        state = newState;
        switch (newState)
        {
            case HERO_STATE.MOVING:
            case HERO_STATE.IDLE:
                break;
            case HERO_STATE.DASHING:
                switch(ghostState)
                {
                    case GHOST_STATE.GHOST:
                        gameObject.GetComponent<SpriteRenderer>().sprite = ghostIdleDashingSprite;
                        break;
                    case GHOST_STATE.HUMAN:
                        gameObject.GetComponent<SpriteRenderer>().sprite = ghostIdleDashingSprite;
                        break;
                }
                gameObject.GetComponent<SpriteRenderer>().sprite = humanIdleDashingSprite;
                gameObject.GetComponent<SpriteRenderer>().sprite = humanSprite;
                break;
            default:
                break;
        }
    }

    IEnumerator dashAway()
    {
        dasher = createDasher();
        Vector3 newVelNormal = lastDirection;
        newVelNormal.z = 0.0f;
        yield return StartCoroutine(moveObject(dasher, newVelNormal, dashTimer, dashRange)); //dash away
        bool isInDark = isPointInDark(dasher.transform.position);
        if (isInDark) //Swap successful
        {
            dashOrigin = transform.position;
            transform.position = dasher.transform.position;
            dasher.transform.position = dashOrigin;//easy swap solution
            breakingPoint = Vector2.zero;
            changeGhostState(GHOST_STATE.GHOST);
            borderParticlesChild.transform.SetParent(dasher.transform);
            borderParticlesChild.transform.localPosition = Vector3.zero;
        }
        if (!isInDark) //swap failed
        {
            float failTimer = 0.07f; //never question this variable
            float rotateAngle = 10.0f;
            float randomNumber = 6.0f; //while you're at it, don't question this one either

            for (float i = 0; i < failTimer * randomNumber; i += dt)
            {
                float phase = Mathf.Sin(i / failTimer);
                dasher.transform.rotation = Quaternion.Euler(new Vector3(0, 0, phase * rotateAngle));
                yield return 0;
            }
            dasher.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));//make sure we are straight!
            yield return StartCoroutine(moveObject(dasher, -newVelNormal, dashTimer, dashRange));
            Destroy(dasher);
        }
    }

    IEnumerator startDash()
    {
        changeHeroState(HERO_STATE.DASHING);
        switch (ghostState)
        {
            case GHOST_STATE.HUMAN:
                {
                    yield return StartCoroutine(dashAway());
                }
                break;
            case GHOST_STATE.GHOST:
                {
                    gameObject.GetComponent<SpriteRenderer>().sprite = ghostIdleDashingSprite;
                    yield return StartCoroutine(dashToOrigin());
                }
                break;
            default:
                print("SOMETHING BROKE! invalid ghost state(and game will probably break because of it..."); 
                break;
        }
        changeHeroState(HERO_STATE.IDLE);
        Camera.main.GetComponent<CameraScript>().target = gameObject;
    }

    void FixedUpdate ()
    {
        if (state == HERO_STATE.DASHING)
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
        }
        if (state != HERO_STATE.DASHING)
        {
            newVelocity = new Vector2(0, 0);
            float speed = 4;
            if (inputs.up)
            {
                newVelocity.y += 1;
            }
            if (inputs.down)
            {
                newVelocity.y -= 1;
            }
            if (inputs.left)
            {
                newVelocity.x -= 1;
            }
            if (inputs.right)
            {
                newVelocity.x += 1;
            }
            changeHeroState((newVelocity.x == 0 && newVelocity.y == 0) ?
                HERO_STATE.IDLE :
                HERO_STATE.MOVING); //hurray for good code
            if (!(Mathf.Approximately(newVelocity.x,0) && Mathf.Approximately(newVelocity.y, 0)))
            {
                lastDirection = newVelocity;
            }           
            newVelocity = newVelocity.normalized * speed;
            
            bool nextPosInDark = isPointInDark(transform.position+new Vector3(newVelocity.x, newVelocity.y,0)*Time.fixedDeltaTime);
            
            switch(ghostState)
            {
                case GHOST_STATE.GHOST: //TODO: refactor
                    handleVelocity(nextPosInDark);
                    if (nextPosInDark)
                    {
                        handleDarknessBounce();
                    }
                    break;
                case GHOST_STATE.HUMAN:
                    handleVelocity(!nextPosInDark);
                    break;
                default:
                    break;
            }
        }
    }
    
    private void handleDarknessBounce()
    {
        float distanceFromHero = (transform.position - dasher.transform.position).magnitude;
        float distanceFromBreakingPoint = distanceFromHero - distanceAllowedInDarkness;
        print(distanceFromBreakingPoint);
        if (distanceFromHero > distanceAllowedInDarkness)
        {
            if (darknessBreakingPoint == Vector2.zero)
            {
                darknessBreakingPoint = transform.position;
            }
            else if (distanceFromHero > distanceAllowedOutsideDarkness+distanceAllowedInDarkness)
            {
                borderRubberBandParticlesChild.SetActive(false);
                StartCoroutine(dashToDarknessPoint(distanceFromBreakingPoint));
                
            }
            else
            {
                ParticleSystem.MainModule psm = borderRubberBandParticlesChild.GetComponent<ParticleSystem>().main;
                psm.startSpeed = distanceFromBreakingPoint;// *0.4f;
                gameObject.GetComponent<Rigidbody2D>().velocity *= Mathf.Pow((((distanceAllowedOutsideDarkness * 1.5f) - distanceFromBreakingPoint) / (distanceAllowedOutsideDarkness * 1.5f)), 3);
                Vector2 distanceNormalized = new Vector2(transform.position.x - breakingPoint.x, transform.position.y - breakingPoint.y).normalized;
                Vector3 newRot = borderRubberBandParticlesChild.transform.rotation.eulerAngles;
                if (transform.position.y - breakingPoint.y > 0)
                {
                    newRot.z = 90.0f + Mathf.Acos(distanceNormalized.x) * Mathf.Rad2Deg;
                }
                if (transform.position.y - breakingPoint.y <= 0)
                {
                    newRot.z = 90.0f - Mathf.Acos(distanceNormalized.x) * Mathf.Rad2Deg;
                }
                borderRubberBandParticlesChild.transform.rotation = Quaternion.Euler(newRot);
                borderRubberBandParticlesChild.SetActive(true);
            }
        }
        else
        {
           borderRubberBandParticlesChild.SetActive(false);
        }

    }

    private void handleVelocity(bool inLegalZoneNextFrame) //should only be called from fixedUpdate
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = newVelocity;
        float distanceFromBreakingPoint = (transform.position - new Vector3(breakingPoint.x, breakingPoint.y, 0)).magnitude;
        if (inLegalZoneNextFrame)
        {
            breakingPoint = Vector2.zero;
            rubberBandParticlesChild.SetActive(false);
        }
        else if (breakingPoint == Vector2.zero)
        {
            breakingPoint = transform.position;
        }
        else if (distanceFromBreakingPoint > distanceAllowedOutside)
        {
            StartCoroutine(dashToBreakingPoint(distanceFromBreakingPoint));
        }
        else
        {
            ParticleSystem.MainModule psm= rubberBandParticlesChild.GetComponent<ParticleSystem>().main;
            psm.startSpeed = distanceFromBreakingPoint;// *0.4f;
            gameObject.GetComponent<Rigidbody2D>().velocity *= Mathf.Pow((((distanceAllowedOutside* 1.5f) - distanceFromBreakingPoint) / (distanceAllowedOutside * 1.5f)),3);
            Vector2 distanceNormalized = new Vector2(transform.position.x - breakingPoint.x, transform.position.y - breakingPoint.y).normalized;
            Vector3 newRot = rubberBandParticlesChild.transform.rotation.eulerAngles;
            if (transform.position.y - breakingPoint.y > 0)
            {
                newRot.z = 90.0f + Mathf.Acos(distanceNormalized.x) * Mathf.Rad2Deg;
            }
            if (transform.position.y - breakingPoint.y <= 0)
            {
                newRot.z = 90.0f - Mathf.Acos(distanceNormalized.x) * Mathf.Rad2Deg;
            }
            rubberBandParticlesChild.transform.rotation = Quaternion.Euler(newRot);
            rubberBandParticlesChild.SetActive(true); 
        }
    }

    IEnumerator dashToBreakingPoint(float distanceFromBreakingPoint)
    {
        changeHeroState(HERO_STATE.DASHING);
        Vector2 direction = new Vector2(transform.position.x - breakingPoint.x, transform.position.y - breakingPoint.y).normalized;
        yield return StartCoroutine(moveObject(gameObject, -direction, 0.2f, distanceFromBreakingPoint*1.3f));
        transform.position = breakingPoint-direction*0.3f;
        changeHeroState(HERO_STATE.IDLE);
    }

    IEnumerator dashToDarknessPoint(float distanceFromdarknessPoint)
    {
        changeHeroState(HERO_STATE.DASHING);
        Vector2 direction = new Vector2(transform.position.x - dasher.transform.position.x, transform.position.y - dasher.transform.position.y).normalized;
        float distance = (transform.position - dasher.transform.position).magnitude;
        yield return StartCoroutine(moveObject(gameObject, -direction, 0.3f, distance,INTERPOLATION_TYPE.LERP,4));
        borderParticlesChild.transform.SetParent(gameObject.transform);
        Destroy(dasher);
        changeGhostState(GHOST_STATE.HUMAN);
        changeHeroState(HERO_STATE.IDLE);     
    }
}