using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAnimation : MonoBehaviour
{
    public int amountOfSpritesPerDirection = 2;
    public int leftAnimationIndexStart = 4;
    public int rightAnimationIndexStart = 6;
    public int upAnimationIndexStart = 2;
    public int downAnimationIndexStart = 0;

    private SpriteRenderer m_renderer;
    private Rigidbody2D m_body;
    private InputHandler m_inputhandler;
    public List<Sprite> sprites;
    private int spriteDirectionIndex = 0;
    private int spriteAnimationIndex = 0;
    private float animationUpdateTimer = 0.0f;
    // Use this for initialization
    void Start()
    {
        m_renderer = gameObject.GetComponent<SpriteRenderer>();
        m_body = gameObject.GetComponent<Rigidbody2D>();
        m_inputhandler = gameObject.GetComponent<InputHandler>();

    }

    // Update is called once per frame
    void Update()
    {
        if (m_inputhandler.ghostState == InputHandler.GHOST_STATE.GHOST)
        {

            Vector2 velocity = m_body.velocity;
            if (velocity.x > 0)
            {
                spriteDirectionIndex = rightAnimationIndexStart;
            }
            else if (velocity.x < 0)
            {
                spriteDirectionIndex = leftAnimationIndexStart;
            }
            else if (velocity.y > 0)
            {
                spriteDirectionIndex = upAnimationIndexStart;
            }
            else if (velocity.y < 0)
            {
                spriteDirectionIndex = downAnimationIndexStart;
            }
            animationUpdateTimer += Time.deltaTime;
            if (velocity.SqrMagnitude() > 0 && animationUpdateTimer > 0.1f)
            {
                animationUpdateTimer = 0.0f;
                spriteAnimationIndex++;
                spriteAnimationIndex = spriteAnimationIndex % amountOfSpritesPerDirection;
                m_renderer.sprite = sprites[spriteDirectionIndex + spriteAnimationIndex];
            }

        }
    }

}
