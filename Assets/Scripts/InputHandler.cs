using System.Collections;
using UnityEngine;

public class InputHandler : MonoBehaviour {
    public enum HERO_STATE
    {
        IDLE,
        MOVING,
        DASHING,
        DISABLED,
        NO_OF_STATES
    }

    public enum GHOST_STATE
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
        UPLEFT = UP + LEFT,
        UPRIGHT = UP + RIGHT,
        DOWNLEFT = DOWN + LEFT,
        DOWNRIGHT = DOWN + RIGHT
    }
    public int moveSpeed = 1;
    public Sprite humanIdleDashingSprite; //idle hero sprite when he is dashing
    public Sprite ghostIdleDashingSprite; //idle ghost sprite when he is dashing
    public Sprite humanDashingSprite; //Sprite for actual dash object
    public Sprite ghostDashingSprite; //Sprite for actual dash object
    public Sprite humanSprite;
    public Sprite ghostSprite;

    public GameObject dasherPrefab;
    public GameObject playerPosIndicatorParticleSystem;
    public GameObject swapSplashParticleSystem;
    public AudioClip[] dashSounds;
    public AudioClip[] dashErrorSounds;
    public AudioClip[] barrierBreakSounds;
    public float dashTimer; //dashtime in seconds
    public float dashRange; //dashrange in unity units
    private Vector3 dashOrigin;
    GameObject dasher; //dasher/placeholder for original
    public Material dasherMaterial;
    private bool isRubberbanding;
    private Vector2 newVelocity;
    public HERO_STATE state;
    public GHOST_STATE ghostState;
    private float dt;
    private float fixedDt;
    private GameObject ghostHighlightChild;
    private GameObject rubberBandParticlesChild;
    private GameObject borderParticlesChild;
    private GameObject borderRubberBandParticlesChild;
    
    private Light ghostHighlightChildComponent;
    private SoundCaller sc;

    private Vector2 breakingPoint; //for where human/ghost enters dark/light
    private Vector2 darknessBreakingPoint; //for where ghost leaves legal radius
    [Tooltip("Distance player can move in darkness/light(in humanState/ghostState).")]
    public float distanceAllowedOutside; //how far player can travel from safe zone. better than time spent outside methinks.
    [Tooltip("Distance ghost state can move from human state.")]
    public float distanceAllowedInDarkness; 
    [Tooltip("Distance ghost can move outside alloted distance in distanceAllowedInDarkness.")]
    public float distanceAllowedOutsideDarkness; 
    private Vector2 lastDirection;
    private void Awake()
    {
        sc = GetComponent<SoundCaller>();
        dashOrigin = Vector2.zero;
        ghostHighlightChild = transform.GetChild(1).gameObject;
        rubberBandParticlesChild = transform.GetChild(2).gameObject;
        borderParticlesChild = transform.GetChild(3).gameObject;
        borderRubberBandParticlesChild = transform.GetChild(4).gameObject;
        ghostHighlightChildComponent = ghostHighlightChild.GetComponent<Light>();
        ghostHighlightChildComponent.enabled = false;
        rubberBandParticlesChild.SetActive(false);
        borderRubberBandParticlesChild.SetActive(false);
        lastDirection = Vector2.right;
        ghostState = GHOST_STATE.HUMAN;
        breakingPoint = Vector2.zero; //TODO: can have repercussions once every blue moon
        ParticleSystem.ShapeModule bps = borderParticlesChild.GetComponent<ParticleSystem>().shape;
        bps.radius = distanceAllowedInDarkness;
        isRubberbanding = false;
    }

    // Use this for initialization
    void Start () {
        playerPosIndicatorParticleSystem = Instantiate(playerPosIndicatorParticleSystem); //not sure if this is considered a good idea
        playerPosIndicatorParticleSystem.SetActive(false);
        playerPosIndicatorParticleSystem.transform.parent = this.transform;
        playerPosIndicatorParticleSystem.transform.localPosition = Vector3.zero;
    //    state = (int)HERO_STATE.IDLE;
    }

    bool isPointInDark(Vector3 point) //TODO: Will be converted to vec2 in function. maybe rewrite whole game to use 3d physics (some weakness with 2d)
    {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.name == "Ghost Highlight") //TODO: slightly hacky. would be better with proper handler for light(low-prio)
            {
                continue;
            }
            if (light.isActiveAndEnabled)
            {
                point.z = -0.1f; //TODO:hacky mc duck. fix for a future rework of everything. (hack is needed because of billboarding and 2d-physics
                float distance = (light.transform.position - point).magnitude;
                if (light.type == LightType.Point && distance < light.range)
                {
                    float distance2d = ((Vector2)light.transform.position - (Vector2)point).magnitude;
                    Ray2D ray = new Ray2D();
                    ray.origin = point;
                    ray.direction = ((Vector2)light.transform.position - (Vector2)point).normalized;
                    if (!Physics2D.Raycast(ray.origin, ray.direction, distance2d))//, soup))
                    {
                        return false;
                    }
                }
                if (light.type == LightType.Spot)
                { }
            }
        }
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        dt = TimeManager.instance.gameDeltaTime;

        switch (state)
        {
            case HERO_STATE.DISABLED:
                break;
            case HERO_STATE.IDLE:
                if (Input.GetAxis("Dash") == 1) //turns out he can dash without moving after all!
                {
                    StartCoroutine(startDash());
                }
                break;
            case HERO_STATE.DASHING:
                break;
            case HERO_STATE.MOVING:
                updateCamera();
                if (Input.GetAxis("Dash") == 1) //turns out he can dash without moving after all!
                {
                    StartCoroutine(startDash());
                }
                break;
            default:
                break;
        }
        if (ghostState == GHOST_STATE.GHOST)
        {
            Vector3 newIndicatorPSRot = playerPosIndicatorParticleSystem.transform.rotation.eulerAngles;
            float angle = Vector2.Angle(dasher.transform.position - transform.position, new Vector3(1, 0, 0));
            newIndicatorPSRot.z = (dasher.transform.position.y - transform.position.y) > 0 ?angle : -angle ;
            playerPosIndicatorParticleSystem.transform.rotation = Quaternion.Euler(newIndicatorPSRot);
            ParticleSystem ps = playerPosIndicatorParticleSystem.GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule pss = ps.shape;
            //pss.radius = (dasher.transform.position - transform.position).magnitude / 2.0f;
            //Vector3 newPssPos = pss.position;
            //newPssPos.x = pss.radius; //(dasher.transform.position - transform.position).magnitude;
           // pss.position = newPssPos;
            
            //ParticleSystem.EmissionModule pse = ps.emission;
            //pse.rateOverTime = (transform.position - dasher.transform.position).magnitude*500.0f;
        }
    }

    private void updateCamera()
    {
        Camera.main.GetComponent<CameraScript>().targetOffset = newVelocity.normalized*1.2f;
    }

    private GameObject createDasher()
    {
        GameObject dasher = Instantiate(dasherPrefab);// new GameObject("dasher", typeof(SpriteRenderer));
        dasher.transform.position = transform.position;
        Camera.main.GetComponent<CameraScript>().target = dasher;
        //dasher.GetComponent<SpriteRenderer>().sprite = humanDashingSprite;
        //dasher.GetComponent<SpriteRenderer>().sortingOrder = 1;
        return dasher;
    }
    IEnumerator dashToOrigin()
    {
        playerPosIndicatorParticleSystem.SetActive(false);
        float distance = (dasher.transform.position - transform.position).magnitude;
        Vector2 direction = (dasher.transform.position - transform.position).normalized;
        GetComponent<BoxCollider2D>().enabled = false;
        Camera.main.GetComponent<CameraScript>().target = dasher;
        yield return StartCoroutine(Tools.moveObject(gameObject, direction, dashTimer, distance)); //dash away
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
                //gameObject.GetComponent<SpriteRenderer>().sprite = ghostSprite;
                ghostHighlightChildComponent.enabled = true;
                playerPosIndicatorParticleSystem.SetActive(true);
                Vector3 newIndicatorPSRot = playerPosIndicatorParticleSystem.transform.rotation.eulerAngles;
                float angle= Vector2.Angle(dasher.transform.position - transform.position, new Vector3(1,0,0));
                newIndicatorPSRot.z = angle;
                playerPosIndicatorParticleSystem.transform.rotation = Quaternion.Euler(newIndicatorPSRot);
                break;
            case GHOST_STATE.HUMAN:
                gameObject.GetComponent<SpriteRenderer>().sprite = humanSprite;
                ghostHighlightChildComponent.enabled = false;
                playerPosIndicatorParticleSystem.SetActive(false);
                borderRubberBandParticlesChild.SetActive(false);
                break;
            default:
                break;
        }
    }
    public void changeHeroState(HERO_STATE newState)
    {
        state = newState;
        switch (newState)
        {
            case HERO_STATE.MOVING:
                break;
            case HERO_STATE.DISABLED:
                gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                break;
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
        Vector3 newVelNormal = lastDirection.normalized;
        newVelNormal.z = 0.0f;
        sc.attemptSound(dashSounds[Random.Range(0, dashSounds.Length)]);
        yield return StartCoroutine(Tools.moveObject(dasher, newVelNormal, dashTimer, dashRange)); //dash away
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
            Destroy(Instantiate(swapSplashParticleSystem, transform.position, Quaternion.Euler(Vector3.zero)), 0.9f); //create and destroy splash effect. hardcoded duration. cba to fix
        }
        if (!isInDark) //swap failed
        {
            GetComponent<SpeechBubbleSpawner>().SpawnSpeechBubble("Ghostie is afraid of the light!");
            yield return StartCoroutine(Tools.shakeObject(dasher, Vector3.back, 20, 0.25f,2));
            yield return StartCoroutine(Tools.moveObject(dasher, -newVelNormal, dashTimer, dashRange));
            Destroy(dasher);
        }
    }

    IEnumerator startDash()
    {
        changeHeroState(HERO_STATE.DASHING);
        switch (ghostState)
        {
            case GHOST_STATE.HUMAN:
                if  (!isRubberbanding)
                {
                    yield return StartCoroutine(dashAway());
                }
                else
                {
                    changeHeroState(HERO_STATE.DISABLED);
                    yield return StartCoroutine(Tools.shakeObject(gameObject, Vector3.back, 10, 0.15f,3));
                    changeHeroState(HERO_STATE.IDLE);
                    sc.attemptSound(dashErrorSounds[Random.Range(0,dashErrorSounds.Length)], 5);
                }
                break;
            case GHOST_STATE.GHOST:
                gameObject.GetComponent<SpriteRenderer>().sprite = ghostIdleDashingSprite;
                yield return StartCoroutine(dashToOrigin());
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
        fixedDt = TimeManager.instance.fixedGameDeltaTime;
        if (state == HERO_STATE.DASHING)
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
        }
        if (state != HERO_STATE.DASHING && state != HERO_STATE.DISABLED)
        {
            newVelocity = Vector2.zero;
            float speed = 4 * TimeManager.instance.gameTimeMultiplier;
            newVelocity.y = Input.GetAxis("Vertical");
            newVelocity.x = Input.GetAxis("Horizontal");
            changeHeroState((newVelocity.x == 0 && newVelocity.y == 0) ?
                HERO_STATE.IDLE :
                HERO_STATE.MOVING); //hurray for good code
            if (!(Mathf.Approximately(newVelocity.x,0) && Mathf.Approximately(newVelocity.y, 0)))
            {
                lastDirection = newVelocity;
            }           
            newVelocity = newVelocity.normalized * speed;
            bool nextPosInDark = isPointInDark(transform.position + new Vector3(newVelocity.x, newVelocity.y, 0) * fixedDt);
            
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
        if (distanceFromHero > distanceAllowedInDarkness)
        {
            isRubberbanding = true;
            if (darknessBreakingPoint == Vector2.zero)
            {
                darknessBreakingPoint = transform.position;
                sc.attemptSound(barrierBreakSounds[Random.Range(0, barrierBreakSounds.Length)]);

            }
            else if (distanceFromHero > distanceAllowedOutsideDarkness+distanceAllowedInDarkness)
            {
                isRubberbanding = false;
                borderRubberBandParticlesChild.SetActive(false);
                StartCoroutine(dashToDarknessPoint(distanceFromBreakingPoint));
            }
            else
            {
                ParticleSystem.MainModule psm = borderRubberBandParticlesChild.GetComponent<ParticleSystem>().main;
                psm.startSpeed = distanceFromBreakingPoint;// *0.4f;
                gameObject.GetComponent<Rigidbody2D>().velocity *= Mathf.Pow((((distanceAllowedOutsideDarkness * 1.5f) - distanceFromBreakingPoint) / (distanceAllowedOutsideDarkness * 1.5f)), 3);
                Vector2 distanceNormalized = new Vector2(transform.position.x - darknessBreakingPoint.x, transform.position.y - darknessBreakingPoint.y).normalized;
                Vector3 newRot = borderRubberBandParticlesChild.transform.rotation.eulerAngles;
                if (transform.position.y - darknessBreakingPoint.y > 0)
                {
                    newRot.z = 90.0f + Mathf.Acos(distanceNormalized.x) * Mathf.Rad2Deg;
                }
                if (transform.position.y - darknessBreakingPoint.y <= 0)
                {
                    newRot.z = 90.0f - Mathf.Acos(distanceNormalized.x) * Mathf.Rad2Deg;
                }
                borderRubberBandParticlesChild.transform.rotation = Quaternion.Euler(newRot);
                borderRubberBandParticlesChild.SetActive(true);

                GetComponent<SpeechBubbleSpawner>().SpawnSpeechBubble("I cannot go further!");
            }
        }
        else
        {
            borderRubberBandParticlesChild.SetActive(false);
            darknessBreakingPoint = Vector2.zero;
            isRubberbanding = false;
        }

    }

    private void handleVelocity(bool inLegalZoneNextFrame) //should only be called from fixedUpdate
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = newVelocity;
        float distanceFromBreakingPoint = (transform.position - new Vector3(breakingPoint.x, breakingPoint.y, 0)).magnitude;
        if (inLegalZoneNextFrame)
        {
            breakingPoint = Vector2.zero;
            isRubberbanding = false;
            rubberBandParticlesChild.SetActive(false);
        }
        else if (breakingPoint == Vector2.zero)
        {
            breakingPoint = transform.position;
            isRubberbanding = true;
            sc.attemptSound(barrierBreakSounds[Random.Range(0, barrierBreakSounds.Length)]);

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

            switch (ghostState)
            {
                case GHOST_STATE.GHOST: //TODO: refactor
                    GetComponent<SpeechBubbleSpawner>().SpawnSpeechBubble("The light, it burns!", 3);
                    break;
                case GHOST_STATE.HUMAN:
                    GetComponent<SpeechBubbleSpawner>().SpawnSpeechBubble("Darkness is scary!", 3);
                    break;
                default:
                    break;
            }


            
        }
    }

    IEnumerator dashToBreakingPoint(float distanceFromBreakingPoint)
    {
        changeHeroState(HERO_STATE.DASHING);
        Vector2 direction = new Vector2(transform.position.x - breakingPoint.x, transform.position.y - breakingPoint.y).normalized;
        yield return StartCoroutine(Tools.moveObject(gameObject, -direction, 0.2f, distanceFromBreakingPoint*1.3f));
        changeHeroState(HERO_STATE.IDLE);

        if (ghostState == GHOST_STATE.HUMAN  && isPointInDark(breakingPoint))
        {
            changeHeroState(HERO_STATE.DISABLED);
            yield return StartCoroutine(deathAnimation());
            LevelManager.instance.triggerGameOver();
        }
    }

    IEnumerator dashToDarknessPoint(float distanceFromdarknessPoint)
    {
        changeHeroState(HERO_STATE.DASHING);
        Vector2 direction = new Vector2(transform.position.x - dasher.transform.position.x, transform.position.y - dasher.transform.position.y).normalized;
        float distance = (transform.position - dasher.transform.position).magnitude;
        yield return StartCoroutine(Tools.moveObject(gameObject, -direction, 0.3f, distance, Tools.INTERPOLATION_TYPE.LERP,4));
        borderParticlesChild.transform.SetParent(gameObject.transform);
        Destroy(dasher);
        changeGhostState(GHOST_STATE.HUMAN);
        changeHeroState(HERO_STATE.IDLE);     
    }

    IEnumerator deathAnimation()
    {
        rubberBandParticlesChild.SetActive(false);
        float deathTime = 3.0f;
        StartCoroutine(Camera.main.GetComponent<CameraScript>().gameOverZoom(deathTime));
        Vector3 newRot = transform.rotation.eulerAngles;
        Vector3 startScale = transform.localScale;
        Vector3 newScale = startScale;
        float pd;
        for (float i = 0; i < deathTime; i+=TimeManager.instance.gameDeltaTime)
        {
            pd = i / deathTime;
            newScale = new Vector2(startScale.x * (1.0f - pd) , startScale.y * (1.0f - pd));
            //newScale.x = startScale.x * (0.85f - pd)+0.15f;
            //newScale.x = startScale.y * (0.85f - pd) + 0.15f;
            newRot.z = i*720.0f;
            transform.rotation = Quaternion.Euler(newRot);
            transform.localScale = newScale;
            yield return null;
        }

        yield return null;
    }
}