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

    public enum PlayerState
    {
        Grounded,
        Airborne,
        WallRiding,
        WallJumping
    }
    private PlayerState state;

    [Header("Movement Settings")]
    [Tooltip("The max speed the player can move via input.")]
    [SerializeField] private float maxMoveVelocity = 3.5f;
    [SerializeField] private float moveSpeed = 1;

    [Header("Wall Ride Settings")]
    [SerializeField] [Range(0, 2)] private float wallRideTime = 1;
    [Tooltip("How long to make the player stick to walls before moving off, when attempting to move away " +
        "from that wall.")]
    [SerializeField] [Range(0, 0.5f)] private float stickyWallTime = 0.1f;
    [Tooltip("How long to deaden input towards a wall after jumping off of it.")]
    [SerializeField] [Range(0, 1)] private float wallJumpInputDeadenTime = 0.5f;
    private float wallRideTimer = 0;
    private float stickyWallTimer = 0;
    private float wallJumpInputDeadenTimer = 0;
    private Vector3? wallNormal = null;

    [Header("Jump Settings")]
    [SerializeField] private float jumpPower = 5;
    [SerializeField] [Range(1.001f, 4)] private float fallGravityMultiplier = 2f;
    [SerializeField] [Range(0, 0.5f)] private float jumpLeewayTime = 0.1f;
    [SerializeField] [Range(0, 0.5f)] private float jumpCooldownTime = 0.1f;
    [SerializeField] [Range(0, 0.5f)] private float jumpBufferTime = 0.1f;
    private bool jumpLeeway;
    private float leewayTimer;
    private PlayerState lastJumpState;
    private bool onJumpCooldown;
    private bool jumpBuffered;
    private Coroutine jumpBufferCoroutine;

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

        //Now that input direction has been collected, adjust it depending on circumstance.

        //Update sticky wall status, which allows some leeway for wall jumping.
        StickyWalls(ref inputDir);

        //Turn movement toward a wall while airborne into upward movement, allowing wall riding.
        TryWallRide(ref inputDir);

        //Deaden input for a bit after wall jumping. This prevents wall jumps from getting
        //stuffed out, and prevents funky behavior like wall climbing by mashing jump.
        DeadenInputTowardWall(ref inputDir);

        //If wall riding, don't use gravity, and vice versa.
        rb.useGravity = state != PlayerState.WallRiding;

        //Now that we've settled what direction to move in, move in that direction.
        if (inputDir != Vector3.zero) { MoveWithDirection(inputDir.normalized); }
    }

    private void Update()
    {
        //Inspired / adapted from http://answers.unity.com/answers/196395/view.html
        //Cast a sphere with the same radius as the player downward to see if there's something underneath.
        bool groundedCheck = Physics.SphereCast
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

        //If the player touched the "ground," but hit.normal is roughly on or below the XZ plane, the hit wasn't
        //actually with the ground.
        //Also, if on jump cooldown, don't set grounded status, to account for the groundedCheck's leeway.
        if (!onJumpCooldown && groundedCheck && Vector3.Dot(Vector3.up, groundHit.normal) >= 0.25f)
        {
            state = PlayerState.Grounded;
        }

        //If we're grounded, allow wall riding.
        if (state == PlayerState.Grounded)
        {
            wallRideTimer = 0;
            wallNormal = null;
        }

        //Set whether the player can jump or not depending on groundedness, accounting for leeway
        SetJumpLeeway();

        //If the user presses space, (re)start the buffer coroutine (if it's running)
        //This "saves" the button press so the player can press jump early and still have the jump happen
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpBufferCoroutine != null) { StopCoroutine(jumpBufferCoroutine); }
            jumpBufferCoroutine = StartCoroutine(CheckForBufferedJump(jumpBufferTime));
        }

        //Now that allowances and leeway and whatnot are set, actually do the jump if applicable.
        TryJump();
        AddJumpGravity();
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
        Vector3 newVel = velocityXZ + moveDir * moveSpeed;
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
    private void TryWallRide(ref Vector3 moveDir)
    {
        //If we aren't grounded, haven't exceeded the max wall ride time, and are moving, we could be wall riding.
        if (state != PlayerState.Grounded && wallRideTimer < wallRideTime && moveDir != Vector3.zero)
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
                //Check if we're on jump cooldown before setting our state, to account for the leeway on
                //the cast above.
                if (!onJumpCooldown) { state = PlayerState.WallRiding; }

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

                return;
            }
        }

        state = state != PlayerState.WallJumping ? PlayerState.Airborne : state;
    }

    private void TryJump()
    {
        //If the user didn't press jump just a bit before this, has pressed jump, and is within the jump leeway,
        //make them jump.
        if (!onJumpCooldown && jumpBuffered && jumpLeeway)
        {
            //Start a cooldown on jumping, so the player can't jump again immediately.
            //This mitigates problems with the leeway on IsGrounded(), which is otherwise necessary.
            StartCoroutine(SetJumpCooldown(jumpCooldownTime));

            //If the state we're jumping from is WallRiding and wallNormal isn't null, then we're jumping off
            //a wall, and we should jump away from that wall.
            if (lastJumpState == PlayerState.WallRiding && wallNormal is Vector3 normal)
            {
                //Jump up, away from the wall, and in whatever direction we were moving just before this.
                Vector3 jumpVect = Vector3.up + normal;
                jumpVect.Normalize();
                jumpVect *= jumpPower;
                rb.velocity += new Vector3(jumpVect.x, jumpVect.y, jumpVect.z);

                state = PlayerState.WallJumping;
            }
            //Otherwise, just jump up.
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);

                state = PlayerState.Airborne;
            }

            //Also expedite the rolling progress during jumps by adding some torque.
            //This cross product gets the axis of angular velocity (perpendicular to velocity)
            rb.AddTorque(Vector3.Cross(Vector3.up, rb.velocity));

            //Take note that the player just jumped.
            jumpLeeway = false;
            jumpBuffered = false;
        }
    }

    private void AddJumpGravity()
    {
        //If the player isn't holding space after they jump, and they haven't hit the peak of their jump yet,
        //increase gravity to allow for a short hop
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

    private void StickyWalls(ref Vector3 moveDir)
    {
        //If the player is wall riding, we have the normal of the wall, and the player is moving away 
        //from the wall,
        if (state == PlayerState.WallRiding && wallNormal is Vector3 normal && Vector3.Dot(normal, moveDir) > 0)
        {
            //Check if the sticky wall timer hasn't exceeded the sticky wall time, and if so,
            if (stickyWallTimer < stickyWallTime)
            {
                //Subtract the component away from the wall from inputDir and increment the timer.
                //This makes it so the player sticks to the wall for a bit, to give them leeway for a wall jump.
                moveDir -= Vector3.Project(moveDir, normal);
                stickyWallTimer += Time.deltaTime;
            }
        }
        //If the player is not wall riding or is not holding away from the wall, reset the sticky wall timer.
        else
        {
            stickyWallTimer = 0;
        }
    }

    private void DeadenInputTowardWall(ref Vector3 moveDir)
    {
        //If the player is wall jumping,
        if (state == PlayerState.WallJumping)
        {
            //Add to the deaden timer.
            wallJumpInputDeadenTimer += Time.deltaTime;

            //If the timer hasn't exceeded the max time and the player is trying to move toward
            //the wall,
            if (wallJumpInputDeadenTimer < wallJumpInputDeadenTime)
            {
                //Deaden all movement input.
                moveDir = Vector3.zero;
            }
        }
        else
        {
            wallJumpInputDeadenTimer = 0;
        }
    }

    //--- Helper Functions ---//

    /// <summary>
    /// Set whether the player can jump or not, allowing some leeway after leaving ground.
    /// </summary>
    /// /// <param name="grounded">Whether the player is in a state they can jump from or not.</param>
    private void SetJumpLeeway()
    {
        //If the player is in a state they can jump from,
        if (state == PlayerState.Grounded || state == PlayerState.WallRiding)
        {
            //Reset the coyote time counter, and make sure the player can jump.
            //They hit the ground, so they're not jumping anymore.
            leewayTimer = 0;
            jumpLeeway = true;
            lastJumpState = state;
        }
        //If the player is not in a state they can jump from,
        else
        {
            //Add to the number of seconds the player has been away from a jump state.
            leewayTimer += Time.deltaTime;

            //If the player has been away from a jump state for too long, make it so they can't jump anymore.
            //This allows players to jump even after they leave a jump state for a bit.
            if (leewayTimer > jumpLeewayTime)
            {
                jumpLeeway = false;
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
