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

    [Tooltip("The max speed the player can move via input.")]
    [SerializeField] private float maxMoveVelocity = 3.5f;
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private float jumpPower = 5;
    [SerializeField] [Range(1, 4)] private float fallGravityMultiplier = 2f;

    private void Start()
    {
        //We subtract jumpGravityMultiplier by one because Physics.gravity.y is already being added
        //once every frame; by doing this, we ensure the "multiplier" is accurate in the jump code
        //We don't want to multiply by zero, though, so only subtract if greater than 1
        if (fallGravityMultiplier > 1)
        {
            fallGravityMultiplier -= 1;
        }
    }

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

        //Now that we've gotten all the directions of the user's input, normalize it and move with it.
        MoveLaterally(inputDir.normalized);
    }

    private void Update()
    {
        //If the user presses space and is touching ground, make the player jump.
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpPower, rb.velocity.z);
        }

        //If the player isn't holding space after they jump, and they haven't hit the peak of their jump yet,
        //Increase gravity to allow for a short hop
        if (!Input.GetKey(KeyCode.Space) && rb.velocity.y > 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * fallGravityMultiplier * Time.deltaTime;
        }
        //Otherwise, if the player has reached the peak of their jump, increase gravity to make the jump feel
        //weightier
        else if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * fallGravityMultiplier * Time.deltaTime;
        }
    }

    /// <summary>
    /// Applies velocity in the given direction, diregarding the y component of velocity.
    /// </summary>
    /// <param name="moveDir">The direction to move in.</param>
    private void MoveLaterally(Vector3 moveDir)
    {
        //First, get the current velocity and discard the y component.
        Vector3 velocityXZ = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        //Now add movespeed to it, and then clamp it to make sure it doesn't exceed maxMoveVelocity.
        Vector3 newVel = velocityXZ + moveDir * moveSpeed * Time.deltaTime;
        newVel = Vector3.ClampMagnitude(newVel, maxMoveVelocity);

        //Finally, now that we've set and clamped the new velocity, apply it.
        rb.velocity = new Vector3(newVel.x, rb.velocity.y, newVel.z);
    }

    private bool IsGrounded()
    {
        //Inspired / adapted from http://answers.unity.com/answers/196395/view.html
        //Cast a sphere, very slightly smaller than the player, downward a by a very small amount. This checks if
        //there is a collider right below the player.
        return Physics.SphereCast(transform.position, coll.bounds.extents.y - 0.0001f, Vector3.down, out _, 0.002f);
    }
}
