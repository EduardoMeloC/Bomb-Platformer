using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerAnimations : MonoBehaviour
{
    public Animator animator;
    public PlayerPlatformerController player;
    private SpriteRenderer sprend;

    void Awake()
    {
        sprend = player.transform.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        animator.SetBool("isGrounded", player.isGrounded);
        animator.SetFloat("velocityX", Mathf.Abs(player.MoveInput.x));
        animator.SetFloat("velocityY", player.Velocity.y);

        // Set player flipDirection
        bool flipSprite = (sprend.flipX ? (player.MoveInput.x > 0.01f) : (player.MoveInput.x < -0.01f));
        if (flipSprite)
        {
            sprend.flipX = !sprend.flipX;
        }
    }
}
