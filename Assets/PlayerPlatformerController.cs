using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformerController : PhysicsObject
{
    public float moveSpeed = 2;
    public float jumpTakeOffSpeed = 3;
    public float fallspeedIncrease = 1.5f;
    public float throwFreezeTime = 0.1f;

    private Vector2 moveInput; // moveInput is literal player's input
    private Vector2 externalForce; // used to give character's impulse by bomb
    public Vector2 MoveInput
    {
        get { return moveInput; }
    }

    private Vector3 _spawnPoint;
    private float _timeCount;

    void Start()
    {
        _spawnPoint = this.transform.position;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("ThrowObject"), LayerMask.NameToLayer("Default"));
    }

    protected override void ComputeVelocity()
    {
        _timeCount += Time.deltaTime;
        //player goes to spawn point if it's below the screen
        if (transform.position.y < -50)
            this.transform.position = _spawnPoint;
        
        // set up move variable;
        
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

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
        if(Mathf.Abs(externalForce.x) > 0.1f)
            this.targetVelocity.x = Mathf.Lerp(externalForce.x, moveInput.x * moveSpeed, _timeCount * 3);
        else
            this.targetVelocity.x = moveInput.x * moveSpeed;
    }

    IEnumerator TossFreeze()
    {
        Vector2 tossInput = moveInput;
        Vector2 previousVelocity = velocity;
        Vector2 previousTargetVelocity = targetVelocity;
        rb2d.simulated = false;
        yield return new WaitForSeconds(throwFreezeTime);
        yield return null;
        rb2d.simulated = true;
        velocity = Vector2.zero;
        _timeCount = 0;
        externalForce = -tossInput * jumpTakeOffSpeed / 1.5f;
        this.velocity.y = externalForce.y;
        previousTargetVelocity = targetVelocity;
    }
}
