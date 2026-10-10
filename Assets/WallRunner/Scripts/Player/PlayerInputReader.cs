using UnityEngine;
using UnityEngine.InputSystem;                       // gives us InputAction and InputSystem

public class PlayerInputReader : MonoBehaviour
{
    InputAction moveAction;                   // WASD 
    InputAction lookAction;                   // mouse 
    InputAction jumpAction;                  // Space
    InputAction sprintAction;                 // Left Shift
    InputAction crouchAction;                // left ctrl

    public Vector2 Move => moveAction.ReadValue<Vector2>();     // x = left/right, y = forward/back
    public Vector2 Look => lookAction.ReadValue<Vector2>();     // how far the mouse moved this frame
    public bool JumpPressed => jumpAction.WasPressedThisFrame(); // true for one frame only
    public bool JumpHeld => jumpAction.IsPressed();              // true while the key is down
    public bool SprintHeld => sprintAction.IsPressed();
    public bool CrouchHeld => crouchAction.IsPressed();

    void Awake()                                     // runs once, before any Start
    {
        InputActionMap player = InputSystem.actions.FindActionMap("Player"); // the project's "Player" action group
        moveAction = player.FindAction("Move");      // look each action up by name, once
        lookAction = player.FindAction("Look");
        jumpAction = player.FindAction("Jump");
        sprintAction = player.FindAction("Sprint");
        crouchAction = player.FindAction("Crouch");
    }
}