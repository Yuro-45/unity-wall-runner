using UnityEngine;

[CreateAssetMenu(menuName = "WallRunner/Movement Settings")]   // adds it to the Create menu
public class MovementSettings : ScriptableObject                // a data asset, not a component
{
    [Header("Ground")]
    public float walkSpeed = 7f;            // metres per second
    public float sprintSpeed = 10f;
    public float groundAcceleration = 70f;  // how fast you reach that speed, and stop
    public float groundStick = 5f;          // downward push that keeps you on slopes

    [Header("Air")]
    public float airAcceleration = 25f;     // steering strength while airborne
    public float gravity = 25f;             // stronger than real life, feels snappier
    public float jumpHeight = 1.3f;         // metres
    public float coyoteTime = 0.12f;        // seconds you can still jump after leaving a ledge
    public float jumpBufferTime = 0.12f;     // seconds an early jump press is remembered
}