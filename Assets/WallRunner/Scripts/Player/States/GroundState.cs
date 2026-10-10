public class GroundState : PlayerState
{
    public GroundState(PlayerController player) : base(player) { }  // pass the player up to PlayerState

    public override void Tick()
    {
        var settings = player.Settings;
        float speed = player.InputReader.SprintHeld ? settings.sprintSpeed : settings.walkSpeed; // pick a top speed

        player.Accelerate(player.WishDirection() * speed, settings.groundAcceleration); // no input = target zero = stop
        player.Velocity.y = -settings.groundStick;      // press into the ground so isGrounded stays true
        player.LastGroundedTime = UnityEngine.Time.time;   // stamp "on the ground right now"


        if (player.JumpBuffered)                        // a press from the last 0.12 s counts
        {
            player.Jump();
            player.ChangeState(player.Air);
        }
        else if (!player.IsGrounded)                    // walked off an edge
        {
            player.Velocity.y = 0f;                     // drop the ground push so the fall starts from zero
            player.ChangeState(player.Air);
        }
    }
}