using UnityEngine;

// PURPOSE: while airborne next to a runnable wall and holding forward,
// run along the wall: fixed speed along its surface, weak gravity, time-limited.
// Talks to: PlayerController (Velocity, Settings, ChangeState), WallSensor via player.Sensor.
// Entered from AirState; leaves back to AirState.
public class WallRunState : PlayerState
{
    float startTime;                                             // when this wall run began

    public WallRunState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        startTime = Time.time;
        player.Velocity.y = Mathf.Max(player.Velocity.y, 0f);    // kill falling speed, keep any upward speed
    }

    public override void Tick()
    {
        var settings = player.Settings;
        var sensor = player.Sensor;

        bool timeUp = Time.time - startTime > settings.wallRunMaxTime;
        if (!sensor.HasWall || player.InputReader.Move.y <= 0f || timeUp || player.IsGrounded)
        {
            player.ChangeState(player.Air);                      // drop off the wall
            return;                                              // skip the rest of this frame
        }

        Vector3 along = Vector3.Cross(sensor.WallNormal, Vector3.up);  // flat direction along the wall
        if (Vector3.Dot(along, player.transform.forward) < 0f)
            along = -along;                                      // make it point the way we're facing

        Vector3 move = along * settings.wallRunSpeed - sensor.WallNormal * 1f; // run along + press into wall
        player.Velocity.x = move.x;
        player.Velocity.z = move.z;

        player.Velocity.y -= settings.wallRunGravity * Time.deltaTime;  // weak gravity, slow sink
    }
}