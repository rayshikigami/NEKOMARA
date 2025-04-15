using UnityEngine;

public abstract class CatStateBase
{
    protected CatStateManager cat;

    public CatStateBase(CatStateManager catManager)
    {
        this.cat = catManager;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}

public class CatIdleState : CatStateBase
{
    public CatIdleState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Idle");
        Debug.Log("Cat is now idle.");
    }

    public override void Update()
    {
        if (Time.time - cat.stateEnterTime > 5f)
        {
            cat.ChangeState(new CatWalkState(cat));
        }
    }
}

public class CatWalkState : CatStateBase
{
    public CatWalkState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Walk");
        Debug.Log("Cat is now walking.");
        cat.MoveToRandomPoint();
    }

    public override void Update()
    {
        if (cat.HasReachedDestination())
        {
            cat.ChangeState(new CatIdleState(cat));
        }
    }
}

public class CatSleepState : CatStateBase
{
    public CatSleepState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Sleep");
    }
}

public class CatInteractState : CatStateBase
{
    public CatInteractState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Interact");
        cat.LookAtUser();
    }
}

public class CatFollowState : CatStateBase
{
    public CatFollowState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Walk");
    }

    public override void Update()
    {
        cat.FollowUser();
    }
}

public class CatScaredState : CatStateBase
{
    public CatScaredState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Scared");
        cat.RunAwayFromUser();
    }
}

public class CatPlayState : CatStateBase
{
    public CatPlayState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Play");
    }

    public override void Update()
    {
        cat.FollowToyPointer();
    }
}

public class CatStateManager : MonoBehaviour
{
    private CatStateBase currentState;
    public Animator animator;
    public UnityEngine.AI.NavMeshAgent agent; // Reference to the NavMeshAgent for movement
    public float stateEnterTime;
    void Start()
    {   
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            agent.Warp(hit.position); 

        }
        
        ChangeState(new CatIdleState(this));
    }

    void Update()
    {
        currentState?.Update();
    }

    void LateUpdate()
    {
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
        {
            Vector3 pos = transform.position;
            pos.y = hit.position.y; 
            transform.position = pos;
        }
    }


    public void ChangeState(CatStateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        stateEnterTime = Time.time;
        currentState.Enter();
    }

    public void MoveToRandomPoint() { 
        Vector3 randomDirection = Random.insideUnitSphere * 5f;
        randomDirection += transform.position;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    public bool HasReachedDestination()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }
    public void LookAtUser() { /* LookAt logic */ }
    public void FollowUser() { /* Follow user logic */ }
    public void RunAwayFromUser() { /* Flee logic */ }
    public void FollowToyPointer() { /* Follow laser/target logic */ }
}
