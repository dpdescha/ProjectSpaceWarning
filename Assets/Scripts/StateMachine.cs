using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public enum State
    {
        Idle,
        WalkingBackwards,
        WalkingForwards,
        Crouching,
        CrouchBlocking,
        OverheadAttack,
        LowAttack,
        BlockstunHigh,
        BlockstunLow,
        HitstunHigh,
        HitstunLow,
        Dead
    }

    public struct Input
    {
        /// <summary>
        /// -1, 0, or 1 indicating backwards, neutral, or forwards
        /// </summary>
        public int Direction;

        /// <summary>
        /// true if crouch button pressed or held, else false
        /// </summary>
        public bool Crouch;

        /// <summary>
        /// true if attack button just pressed, else false
        /// </summary>
        public bool Attack;

        public static bool operator==(Input a, Input b)
        {
            return (a.Direction == b.Direction && a.Crouch == b.Crouch && a.Attack == b.Attack);
        }

        public static bool operator!=(Input a, Input b)
        {
            return !(a.Direction == b.Direction && a.Crouch == b.Crouch && a.Attack == b.Attack);
        }
    }

    public struct Frame
    {
        public float Position;
        public State CurrentState;
        public int FramesInState;
        public int FramesToExit;
        public float Pushback;
    }

    public State CurrentState { get; private set; }
    public int FramesToExit { get; private set; }
    public int FramesInState { get; private set; }
    public float Pushback { get; private set; }

    public AnimationClip Overhead;
    private int overheadDuration; // in frames
    public AnimationClip Low;
    private int lowDuration; // in frames
    public float walkSpeed;
    private Vector3 distance;

    private Animator animator;
    private HealthManager healthManager;

    private Dictionary<State, string> StateAnimations;

    void Start()
    {
        animator = GetComponent<Animator>();
        overheadDuration = Mathf.RoundToInt(Overhead.length * 60);
        lowDuration = Mathf.RoundToInt(Low.length * 60);
        distance = new Vector3(walkSpeed * transform.localScale.x / 60f, 0, 0);
        healthManager = GetComponent<HealthManager>();

        StateAnimations = new Dictionary<State, string>
        {
            { State.Idle, "Idle" },
            { State.WalkingBackwards, "WalkBackwards" },
            { State.WalkingForwards, "WalkForwards" },
            { State.Crouching, "Crouch" },
            { State.CrouchBlocking, "CrouchBlock" },
            { State.OverheadAttack, "Overhead" },
            { State.LowAttack, "Low" },
            { State.BlockstunHigh, "BlockstunHigh" },
            { State.BlockstunLow, "BlockstunLow" },
            { State.HitstunHigh, "HitstunHigh" },
            { State.HitstunLow, "HitstunLow" },
            { State.Dead, "Dead" }
        };
    }

    void Awake()
    {
        CurrentState = State.Idle;
        FramesToExit = 0;
    }

    void Update()
    {

    }

    public State GetNextState(Input input)
    {
        switch (CurrentState)
        {
            // non-restricted states, free input
            case State.Idle:
            case State.Crouching:
            case State.CrouchBlocking:
            case State.WalkingForwards:
            case State.WalkingBackwards:
                return GetNextStateFree(input);
            // restricted states, no input until finished
            case State.LowAttack:
            case State.OverheadAttack:
            case State.HitstunLow:
            case State.HitstunHigh:
            case State.BlockstunLow:
            case State.BlockstunHigh:
                // remain in current state until frame counter ticks down
                if (FramesToExit > 0)
                    return CurrentState;
                else
                    return GetNextStateFree(input);
            case State.Dead:
                return State.Dead;
            default:
                return State.Idle;
        }
    }

    /// <summary>
    /// get next state in a way that allows all inputs
    /// used to transition from a non-restricted state (not in stun/lag/attack)
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private State GetNextStateFree(Input input)
    {
        if (input.Crouch)
        {
            if (input.Attack)
                return State.LowAttack;
            if (input.Direction == -1)
                return State.CrouchBlocking;
            else
                return State.Crouching;
        }
        else
        {
            if (input.Attack)
                return State.OverheadAttack;
            if (input.Direction == -1)
                return State.WalkingBackwards;
            if (input.Direction == 1)
                return State.WalkingForwards;
            else
                return State.Idle;
        }
    }
    
    /// <summary>
    /// takes an input, returns the gamestate of this character following an application of that input
    /// called once per frame to advance state machine, or multiple times in succession during rollback
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public Frame Advance(Input input)
    {
        if (FramesToExit > 0)
            FramesToExit--;

        FramesInState++;

        State nextState = GetNextState(input);
        if (nextState != CurrentState)
        {
            ExitCurrentState();
            CurrentState = nextState;
            FramesInState = 0;
            EnterCurrentState();
        }

        // perform state-specific updates/checks
        switch (CurrentState)
        {
            case State.WalkingForwards:
                transform.position += distance;
                break;
            case State.WalkingBackwards:
                transform.position -= distance;
                break;
            case State.OverheadAttack:
            case State.LowAttack:
                foreach (Hitbox hitbox in GetComponentsInChildren<Hitbox>())
                {
                    hitbox.TestCollisions();
                }
                break;
        }

        // update animation
        int currentClipDuration = Mathf.RoundToInt(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length*60);
        if (currentClipDuration > 0)
        {
            float clipPct = (float)(FramesInState % currentClipDuration) / currentClipDuration;
            animator.Play(StateAnimations[CurrentState], 0, clipPct);
        }
        else
        {
            animator.Play(StateAnimations[CurrentState], 0, 0);
        }

        Frame currentFrame;
        currentFrame.CurrentState = CurrentState;
        currentFrame.FramesInState = FramesInState;
        currentFrame.FramesToExit = FramesToExit;
        currentFrame.Position = transform.position.x;
        currentFrame.Pushback = Pushback;
        return currentFrame;
    }

    public void JumpToFrame(Frame frame)
    {
        CurrentState = frame.CurrentState;
        FramesInState = frame.FramesInState;
        FramesToExit = frame.FramesToExit;
        transform.position = new Vector3(frame.Position, transform.position.y, transform.position.z);
        Pushback = frame.Pushback;

        // jump to animation frame
        int currentClipDuration = Mathf.RoundToInt(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length * 60);
        if (currentClipDuration > 0)
        {
            float clipPct = (float)(FramesInState % currentClipDuration) / currentClipDuration;
            animator.Play(StateAnimations[CurrentState], 0, clipPct);
        }
        else
        {
            animator.Play(StateAnimations[CurrentState], 0, 0);
        }

        EnterCurrentState();
    }

    private void EnterCurrentState()
    {
        // handle state-specific enter behaviour
        // be careful with this, might be called part-way through a state on rollback
        switch (CurrentState)
        {
            case State.BlockstunLow:
            case State.BlockstunHigh:
                // TODO: play block sound
                break;

            case State.HitstunLow:
            case State.HitstunHigh:
                // TODO: play hit sound
                break;

            case State.LowAttack:
                // TODO: play low attack sound
                FramesToExit = lowDuration;
                break;

            case State.OverheadAttack:
                // TODO: play overhead attack sound
                FramesToExit = overheadDuration;
                break;

            default:
                break;
        }
    }

    private void ExitCurrentState()
    {
        // handle state-specific exit behaviour
        switch (CurrentState)
        {
            default: return;
        }
    }

    public void ReceiveHit(Hitbox.Height height, int damage, int hitstun, int blockstun, float hitPushback, float blockPushback)
    {
        ExitCurrentState();

        switch (CurrentState)
        {
            // high vulnerable states
            case State.Idle:
            case State.WalkingForwards:
            case State.OverheadAttack:
            case State.HitstunHigh:
                TakeDamage(false, damage);
                CurrentState = State.HitstunHigh;
                FramesToExit = hitstun;
                Pushback = hitPushback;
                break;

            // low vulnerable states
            case State.Crouching:
            case State.LowAttack:
            case State.HitstunLow:
                TakeDamage(false, damage);
                CurrentState = State.HitstunLow;
                FramesToExit = hitstun;
                Pushback = hitPushback;
                break;

            // high blocking
            case State.BlockstunHigh:
            case State.WalkingBackwards:
                switch (height)
                {
                    case Hitbox.Height.High:
                        TakeDamage(true, damage); // blocked
                        CurrentState = State.BlockstunHigh;
                        FramesToExit = blockstun;
                        Pushback = blockPushback;
                        break;
                    case Hitbox.Height.Low:
                        TakeDamage(false, damage); // blocked
                        CurrentState = State.HitstunHigh;
                        FramesToExit = blockstun;
                        Pushback = hitPushback;
                        break;
                }
                break;

            // low blocking
            case State.BlockstunLow:
            case State.CrouchBlocking:
                switch (height)
                {
                    case Hitbox.Height.High:
                        TakeDamage(false, damage); // not blocked
                        CurrentState = State.HitstunLow;
                        FramesToExit = hitstun;
                        Pushback = hitPushback;
                        break;
                    case Hitbox.Height.Low:
                        TakeDamage(true, damage); // blocked
                        CurrentState = State.BlockstunLow;
                        FramesToExit = blockstun;
                        Pushback = blockPushback;
                        break;
                }
                break;
            default:
                break;
        }

        EnterCurrentState();
    }

    private void TakeDamage(bool blocked, int damage)
    {
        // TODO: damage tracking
        // if blocked, no damage? chip?
        if (!blocked)
            healthManager.TakeDamage(damage);
    }
}