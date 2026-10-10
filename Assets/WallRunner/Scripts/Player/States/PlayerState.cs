public abstract class PlayerState                    // a template; never used directly
{
    protected readonly PlayerController player;      // the player this state controls

    protected PlayerState(PlayerController player)   // constructor: stores the player
    {
        this.player = player;
    }

    public virtual void Enter() { }                  // runs once when the state starts
    public virtual void Exit() { }                   // runs once when it ends
    public abstract void Tick();                     // runs every frame; each state must write it
}