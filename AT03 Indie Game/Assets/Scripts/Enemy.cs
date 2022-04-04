using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : FiniteStateMachine
{
    public Bounds bounds;
    public float viewRadius = 5f;
    public Transform player;
    public EnemyIdleState idleState;
    public EnemyWanderState wanderState;
    public EnemyChaseState chaseState;

    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }
    public AudioSource AudioSource { get; private set; }


    protected override void Awake()
    {
        idleState = new EnemyIdleState(this, idleState);
        wanderState = new EnemyWanderState(this, wanderState);
        chaseState = new EnemyChaseState(this, chaseState);
        entryState = new EnemyIdleState(this);
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
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        //here we can weite custom code to be executed agyer the original Start definition is run
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
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
    public EnemyIdleState(Enemy instance) : base(instance)
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
            Instance.SetState(new EnemyChaseState(Instance));
        }    

        if(timer >= 0)
        {
            timer += Time.deltaTime;
            if(timer >= idleTime)
            {
                Debug.Log("exit idle after" + idleTime + "seconds.");
                Instance.SetState(new EnemyWanderState(Instance));

            }
        }
    }
}

public class EnemyWanderState : EnemyBehaviourState
{
    private Vector3 targetPostion;
    private float wanderSpeed = 3.5f;
    [SerializeField]
    private AudioClip audioClip
    public EnemyWanderState(Enemy instance) : base(instance)
    {
        wanderClip = wanderSpeed.wanderClip
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
        targetPostion = randomPosInBounds;
        Instance.Agent.SetDestination(targetPostion);
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
        Vector3 t = targetPostion;
        t.y = 0;
        if(Vector3.Distance(Instance.transform.position, targetPostion) <= Instance.Agent.stoppingDistance)
        {
            Instance.SetState(new EnemyIdleState(Instance));
        }

        if (Vector3.Distance(Instance.transform.position, targetPostion) <= Instance.viewRadius)
        {
            Instance.SetState(new EnemyChaseState(Instance));
        }
    }

    public override void DrawStateGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(targetPostion, 0.5f);
    }
}

public class EnemyChaseState : EnemyBehaviourState
{
    [SerializeField]
    private float chaseSpeed = 5f;
    [SerializeField]
    private AudioClip chaseClip;
    public EnemyChaseState(Enemy instance) : base(instance)
    {
    }
    public override void OnStateEnter()
    {
        Instance.Agent.isStopped = false;
        Instance.Agent.speed = chaseSpeed;
        Instance.Anim.SetBool("isMoving", true);
        Instance.Anim.SetBool("isChasing", false);

    }

    public override void OnStateExit()
    {
        Debug.Log("exit chase");
    }

    public override void OnStateUpdate()
    {
        if (Vector3.Distance(Instance.transform.position, Instance.player.position) > Instance.viewRadius)
        {
            Instance.SetState(new EnemyWanderState(Instance));
        }
    }
}
