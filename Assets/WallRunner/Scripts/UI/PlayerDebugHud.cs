using UnityEngine;

public class PlayerDebugHud : MonoBehaviour
{
    [SerializeField] PlayerController player;

    void OnGUI()                                     // Unity's quick built-in UI, drawn every frame
    {
        Vector3 v = player.Velocity;
        float speed = new Vector3(v.x, 0f, v.z).magnitude;        // horizontal speed only
        GUI.Label(new Rect(10, 10, 300, 25), "State: " + player.StateName);
        GUI.Label(new Rect(10, 30, 300, 25), "Speed: " + speed.ToString("0.0") + " m/s");
    }
}