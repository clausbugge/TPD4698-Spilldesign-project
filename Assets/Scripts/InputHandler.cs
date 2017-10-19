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
    GHOST
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
   // public LayerMask soup;
    public int moveSpeed = 1;
    public Sprite humanIdleDashingSprite; //idle hero sprite when he is dashing
    public Sprite ghostIdleDashingSprite; //idle ghost sprite when he is dashing
    public Sprite humanDashingSprite; //Sprite for actual dash object
    public Sprite ghostDashingSprite; //Sprite for actual dash object
    public Sprite humanSprite;
    public Sprite ghostSprite;
    public float dashTimer; //dashtime in seconds
    public float dashRange; //dashrange in unity units
    public Material dasherMaterial;
    private Vector3 newVelocity;
    private int state;
    private int ghostState;
    private float dt;
    private GameObject flashLightChild;
    private GameObject ghostHighlightChild;
    private Light ghostHighlightChildComponent;
    private Inputs inputs;
   // private int direction;
    private void Awake()
    {
        flashLightChild = transform.GetChild(0).gameObject;
        ghostHighlightChild = transform.GetChild(1).gameObject;
        ghostHighlightChildComponent = ghostHighlightChild.GetComponent<Light>();
        ghostHighlightChildComponent.enabled = false;
        inputs.up = false;
        inputs.down = false;
        inputs.left = false;
        inputs.right = false;
       // direction = (int)MOVE_DIRECTION.LEFT;
        ghostState = (int)GHOST_STATE.HUMAN;
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
     //   LayerMask lightStopperMask = findMask(new string[] { "default" });
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
        if (isPointInDark(transform.position))
        {
            //move away from light/dark, depending on ghost state
        }
        //if is in ghost state
        updateGhostHighlight();
        switch (state)
        {
            case (int)HERO_STATE.IDLE:
                break;
            case (int)HERO_STATE.DASHING:
                break;
            case (int)HERO_STATE.MOVING:
                updateFlashlight();
                
                updateCamera();
                if (inputs.space) //can't dash unless you're moving(aka:holding a direction, can change l8r if we want OBVIOUSLY!)
                {
                    StartCoroutine(ghostDash());
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
    bool increase = true;
    float yo = 0;
    float yoey = 0;
    private void updateGhostHighlight()
    {
        //ghostHighlightChild.GetComponent<Light>().intensity += increase ? 0.35f*dt : -0.35f*dt;
        //Random.Range(0.005f, 0.04f) : 1
        //-Random.Range(0.005f, 0.04f);
        float posRange = 0.04f;
        float flickerSpeed = 0.4f * Time.deltaTime;
        Vector3 curPos = ghostHighlightChild.transform.localPosition;
        float peace = (curPos.x + posRange) / (posRange * 2);
        //ghostHighlightChild.transform.localPosition = new Vector3(Random.Range(-0.04f, 0.04f), Random.Range(-0.04f, 0.04f), -1.0f); //experimental..
        //Vector2 flickerDirection = new Vector2(Random.Range(((curPos.x + posRange) / (posRange * 2.0f)), 1.0f-((curPos.x + posRange) / (posRange * 2.0f))), 0);
        //print(flickerDirection.x);
        //Random.Range(-flickerSpeed * (curPos.y / posRange), flickerSpeed * (curPos.y / posRange)), 0);
        //ghostHighlightChild.transform.localPosition += new Vector3(flickerSpeed * flickerDirection.x, flickerSpeed * flickerDirection.x), 0, 0);
        yo+= Time.deltaTime*Random.Range(-1,2);
        ghostHighlightChild.transform.localPosition = new Vector3(Mathf.Sin(yo *3*Mathf.PI) * 0.08f, 
                                                                  Mathf.Cos(yo * 3 * Mathf.PI) * 0.08f, -1.0f);
        yoey+= Time.deltaTime*Random.Range(-1, 2);
        ghostHighlightChild.GetComponent<Light>().intensity = (Mathf.Sin(yoey*2)+1)*0.4f+0.8f;
        //if (ghostHighlightChild.GetComponent<Light>().intensity >= 1.2f)
        //{
        //    ghostHighlightChild.GetComponent<Light>().intensity = 1.19f;
        //    increase = !increase;
        //}
        //if (ghostHighlightChild.GetComponent<Light>().intensity <= 0.8f)
        //{
        //    ghostHighlightChild.GetComponent<Light>().intensity = 0.81f;
        //    increase = !increase;
        //}
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

    IEnumerator dashAway(GameObject dasher)
    {
        float smoothDelta = 0.0f;

        float oldSmoothPos = 0.0f;
        float smoothPos = 0.0f;
        int power = 4;
        for (float i = 0; i < dashTimer; i += dt)
        {
            float pd = 0.25f + (i * 0.75f / dashTimer); //pd =percent done
                                                        //smoothPos = pd * pd * (3 - 2 * pd);
                                                        //smoothPos = 1-(1 - (1 - pd) * (1 - pd));
            smoothPos = pd * pd * (3 - 2 * pd);
            smoothPos = Mathf.Pow(smoothPos, power); //hype function
            smoothDelta = smoothPos - oldSmoothPos;
            oldSmoothPos = smoothPos;
            dasher.transform.Translate(newVelocity.normalized * smoothDelta * dashRange);
            yield return 0;
        }
    }

    IEnumerator dashToStart(GameObject dasher)
    {
        float smoothDelta = 0.0f;
        float oldSmoothPos = 0.0f;
        float smoothPos = 0.0f;
        int power = 4;
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
        for (float i = 0; i < dashTimer; i += dt)
        {
            float pd = i / dashTimer; //pd =percent done
                                      //smoothPos = pd * pd * (3 - 2 * pd);
                                      //smoothPos = 1 - (1 - pd) * (1 - pd);
            smoothPos = pd * pd * (3 - 2 * pd);
            smoothPos = Mathf.Pow(smoothPos, power); //hype function
            smoothDelta = smoothPos - oldSmoothPos;
            oldSmoothPos = smoothPos;
            dasher.transform.Translate(-newVelocity.normalized * smoothDelta * dashRange);
            yield return 0;
        }
    }
    IEnumerator dashToDasher()
    {
       // float smoothDelta = 0.0f;
        float oldSmoothPos = 0.0f;
        float smoothPos = 0.0f;
        int power = 4;
        for (float i = 0; i < dashTimer; i += dt)
        {
            float pd = i / dashTimer; //pd =percent done
                                      //smoothPos = pd * pd * (3 - 2 * pd);
                                      //smoothPos = 1 - (1 - pd) * (a 1 - pd);
            smoothPos = pd * pd * (3 - 2 * pd);
            smoothPos = Mathf.Pow(smoothPos, power); //hype function
            oldSmoothPos = smoothPos;
            yield return 0;
        }
        transform.Translate(newVelocity.normalized * dashRange);
    }

    private GameObject createDasher(Sprite dashSprite, int dashLayer)
    {
        GameObject dasher = new GameObject("dasher", typeof(SpriteRenderer));//, typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(DashScript));
        dasher.transform.parent = this.transform;
        Camera.main.GetComponent<CameraScript>().target = dasher;
        dasher.transform.Translate(new Vector3(0, 0, 0.2f));
        dasher.transform.transform.localPosition = new Vector3(0, 0, 0);
        dasher.GetComponent<SpriteRenderer>().sprite = dashSprite;
        dasher.GetComponent<SpriteRenderer>().sortingOrder = 1;
        return dasher;
    }

    IEnumerator ghostDash()
    {
        state = (int)HERO_STATE.DASHING;
        Sprite dashSprite = null;
        int dashLayer = 0;
        bool swapOnDarkRoom = true;
        switch (ghostState)
        {
            case (int)GHOST_STATE.HUMAN:
                gameObject.GetComponent<SpriteRenderer>().sprite = humanIdleDashingSprite;
                dashSprite = humanDashingSprite;
                dashLayer = LayerMask.NameToLayer("Human Dasher");
                swapOnDarkRoom = true;
                break;
            case (int)GHOST_STATE.GHOST:
                gameObject.GetComponent<SpriteRenderer>().sprite = ghostIdleDashingSprite;
                dashSprite = ghostDashingSprite;
                dashLayer = LayerMask.NameToLayer("Ghost Dasher");
                swapOnDarkRoom = false;
                break;
            default:
                print("SOMETHING BROKE! invalid ghost state(and game will probably break because of it..."); 
                break;
        }
        GameObject dasher = createDasher(dashSprite, dashLayer);
        yield return StartCoroutine(dashAway(dasher));
        if (isPointInDark(dasher.transform.position) == swapOnDarkRoom)
        {
            
            yield return StartCoroutine(dashToDasher());
            switch (ghostState)
            {
                case (int)GHOST_STATE.HUMAN:
                    gameObject.GetComponent<SpriteRenderer>().sprite = ghostSprite;
                    ghostState = (int)GHOST_STATE.GHOST;
                    ghostHighlightChildComponent.enabled = true;
                    break;
                case (int)GHOST_STATE.GHOST:
                    gameObject.GetComponent<SpriteRenderer>().sprite = humanSprite;
                    ghostState = (int)GHOST_STATE.HUMAN;
                    ghostHighlightChildComponent.enabled = false;
                    break;
                default:
                    print("SOMETHING BROKE! invalid ghost state(and game will probably break because of it...");
                    break;
            }
        }
        if (isPointInDark(dasher.transform.position) != swapOnDarkRoom)
        {
            yield return StartCoroutine(dashToStart(dasher));
            switch (ghostState)
            {
                case (int)GHOST_STATE.HUMAN:
                    gameObject.GetComponent<SpriteRenderer>().sprite = humanSprite;
                    break;
                case (int)GHOST_STATE.GHOST:
                    gameObject.GetComponent<SpriteRenderer>().sprite = ghostSprite;
                    break;
                default:
                    print("SOMETHING BROKE! invalid ghost state(and game will probably break because of it...");
                    break;
            }
        }
        state = (int)HERO_STATE.IDLE;
        
        Camera.main.GetComponent<CameraScript>().target = this.gameObject;
        GameObject.Destroy(dasher); //we erase our tracks
    }

    void FixedUpdate ()
    {
        if (state == (int)HERO_STATE.DASHING)
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
        }
        if (state != (int)HERO_STATE.DASHING)
        {
            newVelocity = new Vector3(0, 0, 0);
            float speed = 200 * Time.fixedDeltaTime;
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
            state = newVelocity.x == 0 && newVelocity.y == 0 ?
                (int)HERO_STATE.IDLE :
                (int)HERO_STATE.MOVING;
            newVelocity = newVelocity.normalized * speed;
            gameObject.GetComponent<Rigidbody2D>().velocity = newVelocity;
            //gameObject.transform.Translate(newVelocity);

            // float maxSpeed = 4;
            //gameObject.GetComponent<Rigidbody2D>().AddForce(newVelocity * 8);
            //Vector2 oldVec = gameObject.GetComponent<Rigidbody2D>().velocity;
            //if (oldVec.x >= maxSpeed)
            //{
            //    gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(maxSpeed, oldVec.y);
            //}
            //if (oldVec.x <= -maxSpeed)
            //{
            //    gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(-maxSpeed, oldVec.y);
            //}

            //if (oldVec.y >= maxSpeed)
            //{
            //    gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(oldVec.x, maxSpeed);
            //}

            //if (oldVec.y <= -maxSpeed)
            //{
            //    gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(oldVec.x, -maxSpeed);
            //}
        }
    }
}
