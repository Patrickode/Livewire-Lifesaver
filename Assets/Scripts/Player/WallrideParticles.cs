using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrideParticles : MonoBehaviour
{
    [SerializeField] ParticleSystem pSystem = null;
    [SerializeField] PlayerMovement player = null;

    /// <summary>
    /// The original and maximum size that wallride particles should be.
    /// </summary>
    private float originalStartSize = 0;

    private void Start()
    {
        //Unparent this and take note of the particle system's current value. This will be the "max" size.
        transform.parent = null;
        originalStartSize = pSystem.main.startSize.constant;
    }

    private void Update()
    {
        //If the player is wall riding and we have a reference to their hit info,
        if (player.State == PlayerState.WallRiding && player.WallHit is RaycastHit hit)
        {
            //Position / align the particle system with the hit, and start playing it if it's not playing already.
            transform.position = hit.point;
            transform.forward = hit.normal;
            if (!pSystem.isPlaying) { pSystem.Play(); }

            //Now figure out how much time the player has left, and linearly make size / alpha go down as the 
            //player runs out of time to wall ride.
            float percentTimeLeft = Mathf.InverseLerp(player.WallRideTime, 0, player.WallRideTimer);
            ParticleSystem.MainModule psMain = pSystem.main;
            psMain.startSize = Mathf.Lerp(0, originalStartSize, percentTimeLeft);
        }
        else
        {
            //Stop the particle system if it's playing.
            if (pSystem.isPlaying) { pSystem.Stop(); }
        }
    }
}
