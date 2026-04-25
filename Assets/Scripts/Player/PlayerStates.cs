using System;
using UnityEngine;

#region STATES
public abstract class APlayerState : FSM.IState
{
    protected readonly PlayerStateComponent m_Player;
    public APlayerState(PlayerStateComponent player)
    {
        m_Player = player;
    }
    public virtual void Start(Type lastState = null) => Start();
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
        m_Player.GroundCheck.CoyoteTimeBuffer.Clear();
        m_Player.Input.JumpBuffer.Clear();
        m_Player.Animator.StartJumpAnimation();
    }
    public override void Update()
    {
        if(m_Player.Input.ReleaseJumpBuffer.Consume())
        {
            m_Player.Movement.CutJump();
        }
    }
    public override void FixedUpdate()
    {
        m_Player.Movement.MoveAirborne(m_Player.Input.Movement);
    }
}

public class PlayerFallingState : APlayerState
{
    public PlayerFallingState(PlayerStateComponent player) : base(player) { }
    public override void Start(Type lastState)
    {
        m_Player.Movement.SetFallingGravity();
        m_Player.Animator.StartFallAnimation(transitionFromJump: lastState == typeof(PlayerJumpingState));
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
        int forwardDirection = Mathf.CeilToInt(m_Player.transform.right.x);
        int dashDirection = m_Player.Input.Movement != 0 ? m_Player.Input.Movement : forwardDirection;
        m_Player.Movement.ApplyDash(dashDirection);
        m_Player.AttackTarget.SetInvulnerable(true);
        m_Player.Input.DashBuffer.Clear();
        m_Player.Animator.StartDashAnimation();
    }
    public override void End()
    {
        m_Player.Movement.FinishDash();
        m_Player.AttackTarget.SetInvulnerable(false);
    }
}

public class PlayerStandingState : APlayerState
{
    public PlayerStandingState(PlayerStateComponent player) : base(player) { }
    public override void Start(Type lastState)
    {
        m_Player.Movement.Stop();
        m_Player.AttackTarget.SetInvulnerable(true);
        m_Player.Animator.StartStandAnimation(transitionFromKnockback: lastState == typeof(PlayerKnockbackState));
    }
    public override void End()
    {
        m_Player.AttackTarget.SetInvulnerable(false);
    }

}

public class PlayerKnockbackState : APlayerState
{
    private bool m_IsAirborne;
    public PlayerKnockbackState(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        var attack = m_Player.AttackTarget.ResolvedAttackThisFrame;
        m_Player.Movement.ApplyAttackKnockback(attack.Knockback, attack.Source.Position);
        m_Player.AttackTarget.SetInvulnerable(true);
        m_IsAirborne = attack.Knockback.Height > 0.1f || !m_Player.GroundCheck.IsGrounded;
        m_Player.Animator.StartKnockbackAnimation(m_IsAirborne);
    }
    public override void Update()
    {
        if(!m_IsAirborne && !m_Player.GroundCheck.IsGrounded)
        {
            m_IsAirborne = true;
            m_Player.Animator.StartKnockbackAnimation(isAirborne: true);
        }
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
        m_Player.DeathHandler.HandleDeadPlayer(m_Player.transform.root.gameObject);
    }
}

public class PlayerAttackingState : APlayerState
{
    private bool m_AttackFinished;
    public PlayerAttackingState(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        m_AttackFinished = false;
        m_Player.Movement.Stop();
        m_Player.AttackCombo.StartAttack();
        m_Player.Input.AttackBuffer.Clear();
        m_Player.Animator.StartAttackAnimation(isFirstAttack: m_Player.AttackCombo.ActiveAttack == 1);
    }
    public override void Update()
    {
        if (!m_AttackFinished && m_Player.Animator.AttackAnimationPhaseCompletedThisFrame is AttackAnimationPhase.Striking)
        {
            m_AttackFinished = true;
            m_Player.AttackCombo.FinishAttack();
        }
    }
    public override void End()
    {
        if (!m_AttackFinished)
        {
            m_Player.AttackCombo.FinishAttack();
        }
    }
}

public class PlayerBlockingState : APlayerState
{
    private bool m_BlockingSet;
    public PlayerBlockingState(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        m_BlockingSet = false;
        m_Player.Movement.Stop();
        m_Player.Animator.StartBlockAnimation();
    }
    public override void Update()
    {
        if(!m_BlockingSet && m_Player.Animator.BlockAnimationPhase is BlockAnimationPhase.Blocking)
        {
            m_BlockingSet = true;
            m_Player.AttackTarget.StartBlocking(right: m_Player.transform.right.x > 0f);
        }
    }
}

public class PlayerStopBlockingState : APlayerState
{
    public PlayerStopBlockingState(PlayerStateComponent player) : base(player) { }
    public override void Start()
    {
        m_Player.AttackTarget.StopBlocking();
        m_Player.Animator.StartStopBlockAnimation();
    }
}

#endregion