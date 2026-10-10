using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] PlayerInputReader input;        // where look input comes from
    [SerializeField] Transform cameraHolder;         // the object that tilts up and down
    [SerializeField] float sensitivity = 0.1f;       // degrees turned per pixel of mouse movement
    [SerializeField] float maxPitch = 85f;           // how far up or down you can look

    float pitch;                                     // current up/down angle

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;    // pin the cursor to the screen centre
        Cursor.visible = false;                      // and hide it
    }

    void Update()                                    // runs every frame
    {
        Vector2 look = input.Look * sensitivity;     // mouse movement converted to degrees

        transform.Rotate(0f, look.x, 0f);            // turn the whole player left/right

        pitch = Mathf.Clamp(pitch - look.y, -maxPitch, maxPitch); // add up/down, stop at the limits
        cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f); // tilt only the camera holder
    }
}