using UnityEngine;

[RequireComponent(typeof(CharacterController))]      // Unity adds one if it is missing
public class PlayerController : MonoBehaviour
{
    [SerializeField] MovementSettings settings;      // the data asset
    [SerializeField] PlayerInputReader input;

    public MovementSettings Settings => settings;    // read-only access for the states
    public PlayerInputReader InputReader => input;
    public CharacterController Body { get; private set; }
    public Vector3 Velocity;                         // current speed and direction, shared by all states
    public bool IsGrounded => Body.isGrounded;       // true if the last move touched the ground

    public GroundState Ground { get; private set; }  // one instance of each state, reused
    public AirState Air { get; private set; }

    PlayerState current;                             // the state running right now

    float lastJumpPressedTime = -999f;               // when jump was last pressed; -999 = "long ago"
    public float LastGroundedTime = -999f;           // when we last stood on the ground

    public bool JumpBuffered => Time.time - lastJumpPressedTime <= settings.jumpBufferTime; // pressed recently?
    public bool InCoyoteTime => Time.time - LastGroundedTime <= settings.coyoteTime;        // left ground recently?
    public string StateName => current.GetType().Name;   // "GroundState", "AirState"; for the debug display

    void Awake()
    {
        Body = GetComponent<CharacterController>();
        Ground = new GroundState(this);              // "this" hands the state a link back to us
        Air = new AirState(this);
    }

    void Start()
    {
        ChangeState(Air);                            // start falling; we land on the first frame
    }

    void Update()
    {
        if (input.JumpPressed) lastJumpPressedTime = Time.time;  // remember the press
        current.Tick();                              // the state edits Velocity
        Body.Move(Velocity * Time.deltaTime);        // then we move, with collisions
    }

    public void ChangeState(PlayerState next)
    {
        current?.Exit();                             // "?." skips this if there is no state yet
        current = next;
        current.Enter();
    }

    public Vector3 WishDirection()                   // where the player is trying to go
    {
        Vector2 move = input.Move;
        Vector3 wish = transform.right * move.x + transform.forward * move.y; // relative to facing
        return Vector3.ClampMagnitude(wish, 1f);     // stops diagonals being faster
    }

    public void Accelerate(Vector3 target, float acceleration)  // steer horizontal speed toward target
    {
        Vector3 horizontal = new Vector3(Velocity.x, 0f, Velocity.z);   // ignore up/down
        horizontal = Vector3.MoveTowards(horizontal, target, acceleration * Time.deltaTime);
        Velocity.x = horizontal.x;
        Velocity.z = horizontal.z;
    }

    public void ApplyGravity()
    {
        Velocity.y -= settings.gravity * Time.deltaTime;       // fall faster every frame
    }

    public void Jump()
    {
        Velocity.y = Mathf.Sqrt(2f * settings.gravity * settings.jumpHeight); // speed needed to reach jumpHeight
        lastJumpPressedTime = -999f;                 // use up the press so it can't fire twice
        LastGroundedTime = -999f;                    // and end coyote time, so no double jump
    }
}