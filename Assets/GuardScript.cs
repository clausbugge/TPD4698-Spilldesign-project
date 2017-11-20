using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardScript : MonoBehaviour {

    private enum PATROL_DIRECTION
    {
        UP,
        DOWN,
        LEFT,
        RIGHT
    }
    public Vector2 patrolDirection;
    public float patrolSpeed;
    private Rigidbody2D rb2d;
    public Sprite[] sprites;
    private Sprite currentSprite;
    private int curSpriteID;
    private GameObject flashLightRotator;
    // Use this for initialization
    void Start () {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.velocity = patrolDirection * patrolSpeed;
        patrolDirection.Normalize();

        curSpriteID = rb2d.velocity.x < 0 ? (int)PATROL_DIRECTION.LEFT : (int)PATROL_DIRECTION.RIGHT;
        if (rb2d.velocity.y != 0)
        {
            curSpriteID = rb2d.velocity.y < 0 ? (int)PATROL_DIRECTION.UP : (int)PATROL_DIRECTION.DOWN;
        }
        flashLightRotator = transform.GetChild(0).gameObject;

        flashLightRotator.transform.rotation = Quaternion.Euler(0, 0, getAngle(patrolDirection));
        currentSprite = sprites[curSpriteID];
        GetComponent<SpriteRenderer>().sprite = currentSprite;

    }
	
    float getAngle(Vector2 direction)
    {
        if (direction.y >= 0)
        {
            return Mathf.Acos(direction.x) * Mathf.Rad2Deg;
        }
        else
        {
            if (direction.x >= 0)
            {
                return Mathf.Asin(direction.y) * Mathf.Rad2Deg;
            }
            else
            {
                return Mathf.Acos(direction.x) * Mathf.Rad2Deg + 90.0f;
            }
        }
    }
	// Update is called once per frame
	void FixedUpdate () {
		if (justBeforeCollision()) //need changes to support real time modifications in inspector, but not needed atm
        {
            rb2d.velocity *= -1;
            curSpriteID = curSpriteID + 1 - 2 * ((curSpriteID) % 2); //yep
            currentSprite = sprites[curSpriteID];
            Vector2 newDir = rb2d.velocity.normalized;
            flashLightRotator.transform.rotation = Quaternion.Euler(0, 0, getAngle(newDir));
            GetComponent<SpriteRenderer>().sprite = currentSprite;
        }
	}

    bool justBeforeCollision()
    {
        RaycastHit2D[] boxCasts = Physics2D.BoxCastAll(transform.position, 
            transform.localScale,
            0,
            rb2d.velocity,
            1);
        for (int i = 0; i < boxCasts.Length; i++) 
        {
            if (boxCasts[i].collider.tag != "Player")
            {
                return true;
            }
        }
        return false;
    }

}
