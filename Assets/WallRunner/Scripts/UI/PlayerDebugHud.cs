using UnityEngine;

// PURPOSE: on-screen debug text while testing: current state, speed, and what the wall sensor sees.
public class PlayerDebugHud : MonoBehaviour
{
    [SerializeField] PlayerController player;        // reads State + Velocity from here
    [SerializeField] WallSensor sensor;              // reads HasWall + WallOnRight from here

    void OnGUI()                                     // Unity's quick built-in UI, drawn every frame
    {
        Vector3 v = player.Velocity;
        float speed = new Vector3(v.x, 0f, v.z).magnitude;        // horizontal speed only
        GUI.Label(new Rect(10, 10, 300, 25), "State: " + player.StateName);
        GUI.Label(new Rect(10, 30, 300, 25), "Speed: " + speed.ToString("0.0") + " m/s");

        string wall = !sensor.HasWall ? "None"                    // no wall → None
                    : sensor.WallOnRight ? "Right" : "Left";      // otherwise pick the side
        GUI.Label(new Rect(10, 50, 300, 25), "Wall: " + wall);
    }
}