using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : FiniteStateMachine, IInteractable
{
    public Bounds bounds;
    public float viewRadius = 5f;
    public Transform player;
    public EnemyIdleState idleState;
    public EnemyWanderState wanderState;
    public EnemyChaseState chaseState;

    public NavMeshAgent Agent { get; private set; }
    public Transform Target { get; private set; }
    public Animator Anim { get; private set; }
    public AudioSource AudioSource { get; private set; }
    public bool ForceChaseTarget { get; private set; } = false;


    protected override void Awake()
    {
        idleState = new EnemyIdleState(this, idleState);
        wanderState = new EnemyWanderState(this, wanderState);
        chaseState = new EnemyChaseState(this, chaseState);
        entryState = idleState;

        if (TryGetComponent(out NavMeshAgent agent) == true)
        {
            Agent = agent;
        }
        if (TryGetComponent (out AudioSource aSrc) == true)
        {
            AudioSource = aSrc;
        }
        if(transform.GetChild(0).TryGetComponent(out Animator anim) == true ) 
        {
            Anim = anim; 
        }
        TargetItem.ObjectiveActivatedEvent += TriggerForceChasePlayer;
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (Vector3.Distance(transform.position, player.position) <= viewRadius)
        {
            if(CurrentState.GetType() != typeof(EnemyChaseState))
            {
                Debug.Log("Enter Chase State");
                SetState(new EnemyChaseState(this, chaseState));
            }
        }
        else
        {
            if (CurrentState.GetType() == typeof(EnemyChaseState))
            {
                Debug.Log("player out of range, enter wander state");
                SetState(new EnemyWanderState(this, wanderState));
            }
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
       base.Update();
        if (Vector3.Distance(transform.position, player.position) <= viewRadius)
        {
            if (CurrentState.GetType() != typeof(EnemyChaseState))
            {
                Debug.Log("player in range, enter chase");
                SetState(new EnemyChaseState(this, chaseState));
            }
        }
        else
        {
            if (CurrentState.GetType() == typeof(EnemyWanderState))
            {
                Debug.Log("player out of range, wander state");
                SetState(new EnemyWanderState(this, wanderState));

            }
        }
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }

    public void Activate()
    {
      //set state to stun state
    }

    private void TriggerForceChasePlayer()
    {
        if(ForceChaseTarget == false)
        {
            ForceChaseTarget = true;
            SetState(chaseState);
        }
    }
}


public abstract class EnemyBehaviourState : IState
    
{
    protected Enemy Instance { get; private set; }
    public EnemyBehaviourState(Enemy instance)
    {
        Instance = instance;
    }

    public abstract void OnStateEnter();

    public abstract void OnStateExit();

    public abstract void OnStateUpdate();

    public virtual void DrawStateGizmos()
    {

    }
    public virtual void DrawStateGizmo()
    {

    }
}

[System.Serializable]

public class EnemyIdleState : EnemyBehaviourState
{
    [SerializeField]
    private Vector2 idleTimeRange = new Vector2(3, 10);
    [SerializeField]
    private AudioClip idleClip;

    private float timer = -1;
    private float idleTime = 0;
    public EnemyIdleState(Enemy instance, EnemyIdleState idle) : base(instance)
    {
        idleTimeRange = idle.idleTimeRange;
        idleClip = idle.idleClip;
    }

    public override void OnStateEnter()
    {
        Instance.Agent.isStopped = true;
        idleTime = Random.Range(idleTimeRange.x, idleTimeRange.y);
        timer = 0;
        Instance.Anim.SetBool("isMoving", false);
        Instance.AudioSource.PlayOneShot(idleClip);
    }

    public override void OnStateExit()
    {
        timer = -1;
        idleTime = 0;
        Debug.Log("exit idle");
    }

    public override void OnStateUpdate()
    {
        if(Vector3.Distance(Instance.transform.position, Instance.player.position) <= Instance.viewRadius)
        {
            Instance.SetState(Instance.chaseState);
        }    

        if(timer >= 0)
        {
            timer += Time.deltaTime;
            if(timer >= idleTime)
            {
                Debug.Log("exit idle after" + idleTime + "seconds.");
                Instance.SetState(Instance.wanderState);

            }
        }
    }
}
[System.Serializable]
public class EnemyWanderState : EnemyBehaviourState
{
    private Vector3 targetPosition;

    [SerializeField]
    private float wanderSpeed = 3.5f;
    [SerializeField]
    private AudioClip wanderClip;

    public EnemyWanderState(Enemy instance, EnemyWanderState wander) : base(instance)
    {
        wanderSpeed = wander.wanderSpeed;
        wanderClip = wander.wanderClip;
    }
    public override void OnStateEnter()
    {
        Instance.Agent.speed = wanderSpeed;
        Instance.Agent.isStopped = false;
        Vector3 randomPosInBounds = new Vector3
            (
            Random.Range(-Instance.bounds.extents.x, Instance.bounds.extents.x),
            Instance.transform.position.y,
            Random.Range(-Instance.bounds.extents.z, Instance.bounds.extents.z)
            );
        targetPosition = randomPosInBounds + Instance.bounds.center;
        Instance.Agent.SetDestination(targetPosition); 
        Instance.Anim.SetBool("isMoving", true);
        Instance.Anim.SetBool("isChasing", false);
        Instance.AudioSource.PlayOneShot(wanderClip);
    }

    public override void OnStateExit()
    {
        Debug.Log("wander exit");
    }

    public override void OnStateUpdate()
    {
        Vector3 t = targetPosition;
        t.y = 0;
        if(Vector3.Distance(Instance.transform.position, targetPosition) <= Instance.Agent.stoppingDistance)
        {
            Instance.SetState(Instance.idleState);
        }

        if (Vector3.Distance(Instance.transform.position, Instance.player.position) > Instance.viewRadius)
        {
            Instance.SetState(Instance.wanderState);
        }
    }

    public override void DrawStateGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }
}

[System.Serializable]
public class EnemyChaseState : EnemyBehaviourState
{
    private Vector3 targetPosition;
    [SerializeField]
    private float chaseSpeed = 5f;
    [SerializeField]
    private AudioClip chaseClip;
    public EnemyChaseState(Enemy instance, EnemyChaseState chase) : base(instance)
    {
        chaseSpeed = chase.chaseSpeed;
        chaseClip = chase.chaseClip;
    }
    public override void OnStateEnter()
    {
        Instance.Agent.isStopped = false;
        Instance.Agent.speed = chaseSpeed;
        Instance.Anim.SetBool("isMoving", false);
        Instance.Anim.SetBool("isChasing", true);
        Instance.AudioSource.PlayOneShot(chaseClip);
        Instance.Agent.SetDestination(targetPosition);
    }

    public override void OnStateExit()
    {
        Debug.Log("exit chase state");
    }

    public override void OnStateUpdate()
    {
        if (Vector3.Distance(Instance.transform.position, Instance.player.position) > Instance.viewRadius)
            {
            if(Instance.ForceChaseTarget == true)
            {
                Instance.SetState(Instance.chaseState);
            }
            else
            {
                Instance.SetState(Instance.wanderState);
            }
        }
        else
        {
            Instance.Agent.SetDestination(Instance.player.position);
        }
    }
}
