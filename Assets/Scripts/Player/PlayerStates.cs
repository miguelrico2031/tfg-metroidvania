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
        m_Player.Input.JumpBuffer.Clear();
        m_Player.Input.OnJumpReleased += OnJumpReleased;
        m_Player.Stamina.RegisterActionPerformed(StaminaAction.Jump);
        m_Player.Animator.StartJumpAnimation();
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
        m_Player.Movement.ApplyDash();
        m_Player.AttackTarget.SetInvulnerable(true);
        m_Player.Input.DashBuffer.Clear();
        m_Player.Stamina.RegisterActionPerformed(StaminaAction.Dash);
        m_Player.Animator.StartDashAnimation();
    }
    public override void End()
    {
        m_Player.Movement.FinishDash();
        m_Player.AttackTarget.SetInvulnerable(false);
    }
}

public class PlayerKnockbackState : APlayerState
{
    public PlayerKnockbackState(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        AttackData attack = m_Player.AttackTarget.ResolvedAttackThisFrame.Attack;
        m_Player.Movement.ApplyAttackKnockback(attack);
        m_Player.AttackTarget.SetInvulnerable(true);
        bool isAirborne = attack.Knockback.Height > 0.1f || !m_Player.GroundCheck.IsGrounded;
        m_Player.Stamina.RegisterActionPerformed(StaminaAction.Knockback);
        m_Player.Animator.StartKnockbackAnimation(isAirborne);
    }
    public override void End()
    {
        m_Player.Movement.FinishKnockback();
        m_Player.AttackTarget.SetInvulnerable(false);
    }
}

public class PlayerDyingState : APlayerState
{
    public PlayerDyingState(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        m_Player.Movement.Stop();
        m_Player.Animator.StartDeathAnimation();
    }
}

public class PlayerAttacking1State : APlayerState
{
    private bool m_AttackFinished;
    public PlayerAttacking1State(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        m_AttackFinished = false;
        m_Player.Movement.Stop();
        m_Player.Attack.StartAttack(PlayerAttack.Attack1);
        m_Player.Input.Attack1Buffer.Clear();
        m_Player.Stamina.RegisterActionPerformed(StaminaAction.Attack);
        m_Player.Animator.StartAttackAnimation();
    }
    public override void Update()
    {
        if (!m_AttackFinished && m_Player.Animator.Attack1JustCompleted)
        {
            m_AttackFinished = true;
            m_Player.Attack.FinishAttack(PlayerAttack.Attack1);
            //If this state gets to see Animator.AttackComplete == true it means that the conditions for continuing 
            //to next attack state were not met, so it should resolve the animation to withdraw
            m_Player.Animator.ResolveAttackAnimation(continueAttack: false);
        }
    }
    public override void End()
    {
        if (!m_AttackFinished)
        {
            m_Player.Attack.FinishAttack(PlayerAttack.Attack1);
        }
    }
}

public class PlayerAttacking2State : APlayerState
{
    private bool m_AttackFinished;
    public PlayerAttacking2State(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        m_AttackFinished = false;
        m_Player.Attack.StartAttack(PlayerAttack.Attack2);
        m_Player.Input.Attack2Buffer.Clear();
        m_Player.Stamina.RegisterActionPerformed(StaminaAction.Attack);
        m_Player.Animator.ResolveAttackAnimation(continueAttack: true);
    }
    public override void Update()
    {
        if (!m_AttackFinished && m_Player.Animator.Attack2JustCompleted)
        {
            m_AttackFinished = true;
            m_Player.Attack.FinishAttack(PlayerAttack.Attack2);
        }
    }
    public override void End()
    {
        if (!m_AttackFinished)
        {
            m_Player.Attack.FinishAttack(PlayerAttack.Attack2);
        }
    }
}


#endregion