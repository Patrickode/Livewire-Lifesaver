using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb = null;
    [SerializeField]
    private Collider coll = null;
    /// <summary>
    /// The object to reference when moving "forward," "left," "backward," or "right."
    /// </summary>
    [SerializeField]
    private GameObject orienter = null;

    [Header("Movement Settings")]
    [Tooltip("The max speed the player can move via input.")]
    [SerializeField] private float maxMoveVelocity = 3.5f;
    [SerializeField] private float moveSpeed = 1;

    [Header("Wall Ride Settings")]
    [SerializeField] [Range(0, 2)] private float wallRideTime = 1;
    private float wallRideTimer = 0;
    private bool wallRiding = false;
    private Vector3? wallNormal = null;

    [Header("Jump Settings")]
    [SerializeField] private float jumpPower = 5;
    [SerializeField] [Range(1.001f, 4)] private float fallGravityMultiplier = 2f;
    [SerializeField] [Range(0, 0.5f)] private float coyoteTime = 0.1f;
    [SerializeField] [Range(0, 0.5f)] private float jumpCooldownTime = 0.1f;
    [SerializeField] [Range(0, 0.5f)] private float jumpBufferTime = 0.1f;
    private bool grounded;
    private bool jumpLeeway;
    private bool onJumpCooldown;
    private float secondsOffGround;
    private bool jumpBuffered;

    //--- Update Functions --- //

    void FixedUpdate()
    {
        //Set up a movement vector.
        Vector3 inputDir = Vector3.zero;

        //If the WASD keys are held, add the corresponding direction to the move direction.
        if (Input.GetKey(KeyCode.W))
        {
            inputDir += orienter.transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputDir += -orienter.transform.right;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputDir += -orienter.transform.forward;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputDir += orienter.transform.right;
        }

        //Now that input direction has been collected, adjust it for wall riding if applicable.
        //Set gravity accordingly.
        wallRiding = TryWallRide(ref inputDir);
        rb.useGravity = !wallRiding;

        //Now that we've settled what direction to move in, move in that direction.
        if (inputDir != Vector3.zero) { MoveWithDirection(inputDir.normalized); }
    }

    private void Update()
    {
        //Inspired / adapted from http://answers.unity.com/answers/196395/view.html
        //Cast a sphere with the same radius as the player downward to see if there's something underneath.
        grounded = Physics.SphereCast
        (
            //Start just a little bit above the player, so if the player is very slightly in the ground
            //as rigidbodies sometimes are, then the ground can still be detected.
            //This ignores ceilings too, because SphereCast ignores colliders it starts inside of!
            transform.position + Vector3.up * 0.025f,
            coll.bounds.extents.y,
            Vector3.down,
            out RaycastHit groundHit,
            0.05f
        );
        Debug.DrawRay(transform.position, groundHit.normal, Color.cyan);

        //If the player touched the "ground," but hit.normal is roughly on the XZ plane, the hit wasn't
        //actually with the ground, so we're not grounded.
        if (grounded && Mathf.Abs(Vector3.Dot(Vector3.up, groundHit.normal)) <= 0.25f)
        {
            grounded = false;
        }

        //If we're grounded, we're not wall riding, and wall riding is allowed again.
        if (grounded)
        {
            wallRiding = false;
            wallRideTimer = 0;
            wallNormal = null;
        }

        //Set whether the player can jump or not depending on groundedness, accounting for leeway
        SetJumpLeeway(grounded || wallRiding);
        DoJumpLogic();
    }

    //--- Main Movement Functions ---//

    /// <summary>
    /// Applies velocity in the given direction, diregarding the y component of velocity.
    /// </summary>
    /// <param name="moveDir">The direction to move in.</param>
    private void MoveWithDirection(Vector3 moveDir)
    {
        //First, get the current velocity and discard the y component.
        Vector3 velocityXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        //Now add movespeed to it, and then clamp it to make sure it doesn't exceed maxMoveVelocity.
        Vector3 newVel = velocityXZ + moveDir * moveSpeed * Time.deltaTime;
        newVel = Vector3.ClampMagnitude(newVel, maxMoveVelocity);

        //Finally, now that we've set and clamped the new velocity, apply it.
        //If newVel has a y component, we're wall riding, so gravity doesn't matter and we can apply directly.
        if (newVel.y > 0)
        {
            rb.velocity = new Vector3(newVel.x, newVel.y, newVel.z);
        }
        else
        {
            rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
        }
    }

    /// <summary>
    /// Checks to see if the player is against a wall, and if so, returns an altered moveDir.
    /// </summary>
    /// <param name="moveDir">The direction to adjust if wall riding. Unchanged if not wall riding.</param>
    /// <returns>Whether the player is wall riding or not.</returns>
    private bool TryWallRide(ref Vector3 moveDir)
    {
        //If we haven't exceeded the max wall ride time and are moving, we could be wall riding.
        if (wallRideTimer < wallRideTime && moveDir != Vector3.zero)
        {
            //Check if there is something in the direction of movement.
            bool castSuccess = Physics.SphereCast
            (
                //same as grounded check in update, with different directions
                transform.position + -moveDir * 0.025f,
                coll.bounds.extents.y,
                moveDir,
                out RaycastHit hit,
                0.05f
            );

            //If the cast hit something and the normal of that something is roughly along the XZ plane,
            //it's time to wall ride; we're moving against a wall.
            if (castSuccess && Math.Abs(Vector3.Dot(Vector3.up, hit.normal)) <= 0.25f)
            {
                //Add to the wall ride timer.
                wallRideTimer += Time.deltaTime;

                //Get the component of moveDir that is toward the hit and subtract it from moveDir. We're about
                //to change that component to go up the hit surface instead of into it.
                Vector3 towardHit = Vector3.Project(moveDir, hit.normal);
                moveDir -= towardHit;

                //Redirect the component toward the hit upward, then add it back to velocity.
                towardHit = Vector3.up * towardHit.magnitude;
                moveDir += towardHit;

                //Save the normal of this wall to a variable in case of wall jumping.
                wallNormal = hit.normal;

                return true;
            }
        }

        //If we got this far, one of the checks above failed, and thus, we aren't wall riding.
        return false;
    }

    private void DoJumpLogic()
    {
        //If the user presses space and is allowed to jump, make the player jump.
        StartCoroutine(CheckForBufferedJump(jumpBufferTime));
        if (!onJumpCooldown && jumpBuffered && jumpLeeway)
        {
            wallRiding = false;

            //There is no longer a jump buffered because we're about to do that jump.
            jumpBuffered = false;

            //Start a cooldown on jumping, so the player can't jump again immediately.
            //This mitigates problems with the leeway on IsGrounded(), which is otherwise necessary.
            StartCoroutine(SetJumpCooldown(jumpCooldownTime));

            //If wallNormal isn't null, then we're jumping off a wall, and we should jump away from that wall.
            if (wallNormal is Vector3 normal)
            {
                //Jump up, away from the wall, and in whatever direction we were moving just before this.
                Vector3 jumpVect = Vector3.up + normal + rb.velocity.normalized;
                jumpVect.Normalize();
                jumpVect *= jumpPower;
                rb.velocity = new Vector3(jumpVect.x, jumpVect.y, jumpVect.z);
            }
            //Otherwise, just jump up.
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
            }

            //Also expedite the rolling progress during jumps by adding some torque.
            //This cross product gets the axis of angular velocity (perpendicular to velocity)
            rb.AddTorque(Vector3.Cross(Vector3.up, rb.velocity));

            //Take note that the player just jumped. This is reset upon being grounded, and ensures the player
            //can't jump twice.
            jumpLeeway = false;
        }

        //If the player isn't holding space after they jump, and they haven't hit the peak of their jump yet,
        //Increase gravity to allow for a short hop
        if (!Input.GetKey(KeyCode.Space) && rb.velocity.y > 0)
        {
            //We subtract fallGrav by 1 because gravity is already added once per frame; to make fallGrav
            //accurate, we need to subtract it by 1. fallGrav can't be 1 due to its range property, so no zeroes
            rb.velocity += Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1) * Time.deltaTime;
        }
        //Otherwise, if the player has reached the peak of their jump, increase gravity to make the jump feel
        //weightier
        else if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1) * Time.deltaTime;
        }
    }

    //--- Helper Functions ---//

    /// <summary>
    /// Set whether the player can jump or not, allowing some leeway after leaving ground.
    /// </summary>
    /// /// <param name="grounded">Whether the player is in a state they can jump from or not.</param>
    private void SetJumpLeeway(bool jumpState)
    {
        //If the player is in a state they can jump from,
        if (jumpState)
        {
            //Reset the coyote time counter, and make sure the player can jump.
            //They hit the ground, so they're not jumping anymore.
            secondsOffGround = 0;
            jumpLeeway = true;
        }
        //If the player is not in a state they can jump from,
        else
        {
            //Add to the number of seconds the player has been away from a jump state.
            secondsOffGround += Time.deltaTime;

            //If the player has been away from a jump state for too long, make it so they can't jump anymore.
            //This allows players to jump even after they leave a jump state for a bit.
            if (secondsOffGround > coyoteTime)
            {
                jumpLeeway = false;
                wallNormal = null;
            }
        }
    }

    /// <summary>
    /// Enables a jump cooldown for length seconds, to ensure the player can't jump twice.
    /// </summary>
    /// <param name="length">How long to wait before allowing the player to jump again.</param>
    private IEnumerator SetJumpCooldown(float length)
    {
        onJumpCooldown = true;
        yield return new WaitForSeconds(length);
        onJumpCooldown = false;
    }

    /// <summary>
    /// Check if the player has pressed the jump button, and if so, reflect that in a bool.
    /// This bool is reset after bufferTime has passed.
    /// </summary>
    /// <param name="bufferTime">How long to "hold on to" the player's input.</param>
    private IEnumerator CheckForBufferedJump(float bufferTime)
    {
        if (!jumpBuffered && Input.GetKeyDown(KeyCode.Space))
        {
            jumpBuffered = true;
            yield return new WaitForSeconds(bufferTime);
            jumpBuffered = false;
        }
    }
}
