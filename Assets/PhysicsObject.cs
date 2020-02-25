using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.SocialPlatforms;
using UnityEngine.Tilemaps;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PhysicsObject : MonoBehaviour
{
    public float minGroundNormalY = .65f;
    public float gravityModifier = 1f;
    public float maxFallSpeed = 1f;
    public LayerMask collideWith;

    protected Vector2 targetVelocity;
    public bool isGrounded { get; protected set; }
    protected Vector2 groundNormal;
    protected GameObject groundObject;
    
    protected Rigidbody2D rb2d;
    protected Collider2D col2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

    protected const float minMoveDistance = 0.001f;
    protected const float shellRadius = 0.01f;
    
    // Properties
    public Vector2 Velocity
    {
        get { return velocity; }
    }
    
    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        col2d = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(collideWith);
        contactFilter.layerMask = collideWith;
        contactFilter.useLayerMask = true;
    }

    void Update()
    {
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {
        
    }

    protected virtual void OnTouchGround()
    {
        
    }

    protected virtual void OnCollision(RaycastHit2D hit)
    {

    }

    void FixedUpdate()
    {
        // set up gravity velocity
        velocity += Physics2D.gravity * (gravityModifier * Time.deltaTime);
        // clamp max fall speed
        if(velocity.y < 0)
            velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
        
        velocity.x = targetVelocity.x;

        isGrounded = false;
        
        // calculate deltaPos aka the step
        Vector2 deltaPosition = velocity * Time.deltaTime;

        // moveAlong allows to move along slopes and flats
        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);
        
        // set up X movement
        Vector2 movement = deltaPosition;
        if(groundNormal != Vector2.zero) movement = moveAlongGround * deltaPosition.x;
        Move(movement, false);
        
        // set up Y movement
        movement = Vector2.up * deltaPosition.y;
        Move(movement, true);
    }

    void Move(Vector2 movement, bool yMovement)
    {
        // distance is the size of the movement step vector
        float distance = movement.magnitude;
        
        // if the step is big enough, do movement calculations
        if (distance > minMoveDistance)
        {
            //count is how many objects are colliding with the box in the next step
            //contactFilter.layerMask = collideWith;
            int count = rb2d.Cast(movement, contactFilter, hitBuffer, distance + shellRadius);
            hitBufferList.Clear();
            
            // for each object detected by the raycast, add it to the list buffer
            PlatformEffector2D platform = null;
            for (int i = 0; i < count; i++)
            {
                platform = hitBuffer[i].collider.GetComponent<PlatformEffector2D>();
                bool collideFromUp = (Mathf.Abs(1 - Vector2.Dot(hitBuffer[i].normal, Vector2.up)) < 0.01);
                bool isPlatformColliding =
                    (platform && collideFromUp && velocity.y < 0 && yMovement/* &&
                     Physics2D.OverlapArea(col2d.bounds.min, col2d.bounds.min + Vector3.right * col2d.bounds.size.x)*/);

                if (!platform || (isPlatformColliding)){
 
                    hitBufferList.Add (hitBuffer[i]);
 
                }
            }

            // for each object in the list buffer, get its normal
            Vector2 currentNormal;
            for (int i = 0; i < hitBufferList.Count; i++)
            {
                currentNormal = hitBufferList[i].normal;
                groundObject = hitBufferList[i].transform.gameObject;
                // if the normal is big enough, then the object is grounded
                if (currentNormal.y > minGroundNormalY)
                {
                    isGrounded = true;
                    if (yMovement)
                    {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                        
                        OnTouchGround();
                    }
                }

                /* projection determines whether we need to subtract from velocity to prevent clipping
                 or to continue sliding X velocity when jumping under a ceiling without stopping Y velocity */
                float projection = Vector2.Dot(velocity, currentNormal);


                if (projection < 0)
                {
                    // resets velocity value, and changes it accordingly when colliding with slopes
                    velocity -= projection * currentNormal;
                }

                // prevents clipping
                float platformFix = 0;
                ColliderDistance2D colliderDistance = hitBufferList[i].collider.Distance(col2d);
                if (platform && isGrounded && yMovement && colliderDistance.isOverlapped)
                {
                    platformFix = (colliderDistance.pointA - colliderDistance.pointB).magnitude;
                }
                
                float modifiedDistance = hitBufferList[i].distance - shellRadius - platformFix;
                // sets distance depending if a collision has occurred
                if (modifiedDistance < distance) OnCollision(hitBufferList[i]);
                distance = modifiedDistance < distance ? modifiedDistance : distance;
                
            }
        }

        rb2d.position = rb2d.position + movement.normalized * distance;
    }
}
