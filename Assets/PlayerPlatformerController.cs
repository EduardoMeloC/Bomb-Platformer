using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformerController : PhysicsObject
{
    public float maxSpeed = 2;
    public float jumpTakeOffSpeed = 3;
    public float fallspeedIncrease = 1.5f;
    public float throwFreezeTime = 0.1f;

    private Vector2 moveInput;
    public Vector2 MoveInput
    {
        get { return moveInput; }
    }

    private Vector3 _spawnPoint;

    void Start()
    {
        _spawnPoint = this.transform.position;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ThrowObject"), LayerMask.NameToLayer("Default"));
    }

    protected override void ComputeVelocity()
    {
        //player goes to spawn point if it's below the screen
        if (transform.position.y < -50)
            this.transform.position = _spawnPoint;
        
        // set up move variable;
        
        moveInput.x = Input.GetAxisRaw("Horizontal");

        // make player jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = jumpTakeOffSpeed;
        }
        // player jumps lower if jump key isn't held
        else if (Input.GetButtonUp("Jump"))
        {
            if (velocity.y > 0)
                velocity.y = velocity.y * .5f;
        }

        // make player freeze for a while when tossing bomb in the air
        if(!isGrounded && Input.GetButtonDown("Fire"))
        {
            StartCoroutine(TossFreeze());
        }
        
        // make player fall faster
        if (velocity.y < 0)
            velocity.y -= fallspeedIncrease;
        
        // set up player's movement velocity
        this.targetVelocity = moveInput * maxSpeed;
    }

    IEnumerator TossFreeze()
    {
        Vector2 previousVelocity = velocity;
        Vector2 previousTargetVelocity = targetVelocity;
        rb2d.simulated = false;
        yield return new WaitForSeconds(throwFreezeTime);
        yield return null;
        rb2d.simulated = true;
        velocity = previousVelocity.x * Vector2.right + jumpTakeOffSpeed/2 * Vector2.up;
        previousTargetVelocity = targetVelocity;
    }
}
