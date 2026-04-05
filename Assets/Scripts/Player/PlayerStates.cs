using UnityEngine;

#region STATES
public abstract class APlayerState : IState
{
    protected readonly PlayerStateComponent m_Player;
    public APlayerState(PlayerStateComponent player)
    {
        m_Player = player;
    }
    public virtual void Start() { }
    public virtual void End() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}

public class PlayerGroundedState : APlayerState
{
    public PlayerGroundedState(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        m_Player.Movement.SetRisingGravity();
        m_Player.Animator.StartGroundedAnimation();
    }
    public override void FixedUpdate()
    {
        m_Player.Movement.MoveGrounded(m_Player.Input.Movement);
    }
}

public class PlayerJumpingState : APlayerState
{
    public PlayerJumpingState(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        m_Player.Movement.Jump();
        m_Player.Movement.SetRisingGravity();
        m_Player.GroundCheck.ClearCoyoteTime();
        m_Player.Input.ClearJumpBuffer();
        m_Player.Input.OnJumpReleased += OnJumpReleased;
        m_Player.Animator.StartJumpAnimation();
        m_Player.Stamina.RegisterActionPerformed(PlayerStaminaAction.Jump);
    }
    public override void End() => m_Player.Input.OnJumpReleased -= OnJumpReleased;
    public override void FixedUpdate()
    {
        m_Player.Movement.MoveAirborne(m_Player.Input.Movement);
    }
    private void OnJumpReleased()
    {
        m_Player.Movement.CutJump();
        m_Player.Input.OnJumpReleased -= OnJumpReleased; //Avoid cutting jump more than once
    }
}

public class PlayerFallingState : APlayerState
{
    public PlayerFallingState(PlayerStateComponent player) : base(player) { }

    public override void Start()
    {
        m_Player.Movement.SetFallingGravity();
        m_Player.Animator.StartFallAnimation();
    }
    public override void FixedUpdate()
    {
        m_Player.Movement.MoveAirborne(m_Player.Input.Movement);
    }
}
#endregion

#region TRANSITIONS
public static class PlayerTransitions
{
    public static bool TransitionGroundedToJumping(PlayerStateComponent player) => IsJumpRequestedAndAllowed(player);
    public static bool TransitionJumpingToFalling(PlayerStateComponent player) => player.Movement.VerticalVelocity <= 0f;
    public static bool TransitionFallingToGrounded(PlayerStateComponent player) => player.GroundCheck.IsGrounded;
    public static bool TransitionGroundedToFalling(PlayerStateComponent player) => !player.GroundCheck.IsGrounded;
    public static bool TransitionFallingToJumping(PlayerStateComponent player) => player.GroundCheck.CheckCoyoteTime() 
        && IsJumpRequestedAndAllowed(player);

    private static bool IsJumpRequestedAndAllowed(PlayerStateComponent player)
    {
        return player.Input.CheckJumpBuffer() && player.Stamina.CanPerformAction(PlayerStaminaAction.Jump);
    }
}
#endregion