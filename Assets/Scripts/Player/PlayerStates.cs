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

public class PlayerDashingState : APlayerState
{
    public PlayerDashingState(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        m_Player.Movement.Dash();
        m_Player.Input.ClearDashBuffer();
        m_Player.Animator.StartDashAnimation();
        m_Player.Stamina.RegisterActionPerformed(PlayerStaminaAction.Dash);
    }
    public override void End()
    {
        m_Player.Movement.FinishDash();
    }
}
#endregion

#region TRANSITIONS
public static class PlayerTransitions
{
    public static bool TransitionGroundedToJumping(PlayerStateComponent player) => IsJumpRequestedAndAllowed(player);
    public static bool TransitionGroundedToFalling(PlayerStateComponent player) => !player.GroundCheck.IsGrounded;
    public static bool TransitionGroundedToDashing(PlayerStateComponent player) => IsDashRequestedAndAllowed(player);
    
    public static bool TransitionJumpingToFalling(PlayerStateComponent player) => player.Movement.VerticalVelocity <= 0f;
    
    public static bool TransitionFallingToGrounded(PlayerStateComponent player) => player.GroundCheck.IsGrounded;
    public static bool TransitionFallingToJumping(PlayerStateComponent player) =>
        player.GroundCheck.CheckCoyoteTime() &&
        IsJumpRequestedAndAllowed(player);
   
    public static bool TransitionDashingToGrounded(PlayerStateComponent player) => player.GroundCheck.IsGrounded && IsDashFinished(player);
    public static bool TransitionDashingToFalling(PlayerStateComponent player) => !player.GroundCheck.IsGrounded && IsDashFinished(player);

    private static bool IsJumpRequestedAndAllowed(PlayerStateComponent player) =>
        player.Input.CheckJumpBuffer() && 
        player.Stamina.CanPerformAction(PlayerStaminaAction.Jump);
    
    private static bool IsDashRequestedAndAllowed(PlayerStateComponent player)
    {
        bool a = player.Input.CheckDashBuffer();
        bool b = player.Stamina.CanPerformAction(PlayerStaminaAction.Dash);
        bool c =  !player.ObstacleCheck.IsObstructed;
        return a && b && c;
    }

    private static bool IsDashFinished(PlayerStateComponent player) => !player.Movement.IsDashing || player.ObstacleCheck.IsObstructed;
}
#endregion