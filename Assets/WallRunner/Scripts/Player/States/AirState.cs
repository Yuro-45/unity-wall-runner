using UnityEngine;

public class AirState : PlayerState
{
    public AirState(PlayerController player) : base(player) { }

    public override void Tick()
    {
        var settings = player.Settings;
        Vector3 wish = player.WishDirection();

        if (wish.sqrMagnitude > 0.01f)                  // only steer while a key is held,
        {                                               // so letting go keeps your momentum
            float speed = player.InputReader.SprintHeld ? settings.sprintSpeed : settings.walkSpeed;
            player.Accelerate(wish * speed, settings.airAcceleration);
        }

        if (player.JumpBuffered && player.InCoyoteTime)  // pressed jump just after leaving a ledge
            player.Jump();

        player.ApplyGravity();

        bool hitHead = (player.Body.collisionFlags & CollisionFlags.Above) != 0; // did the last move hit a ceiling?
        if (hitHead && player.Velocity.y > 0f)
            player.Velocity.y = 0f;                     // stop rising so you don't stick to it

        if (player.IsGrounded && player.Velocity.y <= 0f)  // touched ground while falling
            player.ChangeState(player.Ground);
        else if (!player.IsGrounded && player.Sensor.HasWall && player.InputReader.Move.y > 0f)
            player.ChangeState(player.WallRun);            // wall beside us + holding forward → wall run
    }
}