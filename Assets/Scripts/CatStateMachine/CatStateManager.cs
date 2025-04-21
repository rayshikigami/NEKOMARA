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


public class CatIdleState : CatStateBase // complete 
{
    public CatIdleState(CatStateManager cat) : base(cat) { }
    private float idleDuration = 0f; 

    private float[] stateProbabilities = {5,0,0,0,0,0}; // 機率 array

    public override void Enter()
    {
        cat.animator.Play("Idle");
        Debug.Log("Cat is now idle.");
        idleDuration = Random.Range(1f, 1.5f); 
    }

    public override void Update()
    {
        if (Time.time - cat.stateEnterTime > idleDuration)
        {
            // 隨機到 CatWanderState, CatSleepState, CatRollState, CatGroomingState, CatMoveToPointState, 用 array 來表示個個機率
            float totalProbability = 0f;
            foreach (float probability in stateProbabilities)
            {
                totalProbability += probability;
            }
            float randomValue = Random.Range(0f, totalProbability);
            float cumulativeProbability = 0f;
            int randomState = 0;
            for (int i = 0; i < stateProbabilities.Length; i++)
            {
                cumulativeProbability += stateProbabilities[i];
                if (randomValue <= cumulativeProbability)
                {
                    randomState = i;
                    break;
                }
            }
            switch (randomState)
            {
                case 0:
                    cat.ChangeState(new CatWanderState(cat));
                    break;
                case 1:
                    cat.ChangeState(new CatSleepState(cat));
                    break;
                case 2:
                    cat.ChangeState(new CatRollState(cat));
                    break;
                case 3:
                    cat.ChangeState(new CatGroomingState(cat));
                    break;
                case 4:
                    cat.ChangeState(new CatMoveToObjectState(cat,  cat.FindFood(), new CatEatState(cat)));
                    break;
                case 5:
                    cat.ChangeState(new CatMoveToObjectState(cat,  cat.FindBox(), new CatPlayWithItemState(cat)));
                    break;
                default:
                    cat.ChangeState(new CatIdleState(cat));
                    break;
            }
        }


    }
}

public class CatWanderState : CatStateBase // complete
{
    public CatWanderState(CatStateManager cat) : base(cat) { }

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

public class CatSleepState : CatStateBase // complete
{
    // randomly sleep for 10-50 seconds, then change to CatIdleState
    private float sleepDuration = 0f; // Duration of sleep
    
    public CatSleepState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        sleepDuration = Random.Range(1f, 5f); // Random sleep duration between 10 and 50 seconds
        Debug.Log("Cat is now sleeping.");
        cat.animator.Play("Sleep");
    }
    public override void Update()
    {
        if (Time.time - cat.stateEnterTime > sleepDuration)
        {
            cat.ChangeState(new CatIdleState(cat));
        }
    }
}

public class CatRollState : CatStateBase // complete
{
    // randomly roll for 2~5 seconds, then change to CatIdleState
    public CatRollState(CatStateManager cat) : base(cat) { }
    private float rollDuration = 0f; // Duration of rolling
    public override void Enter()
    {
        rollDuration = Random.Range(2f, 5f); // Random roll duration between 2 and 5 seconds
        Debug.Log("Cat is now rolling.");
        cat.animator.Play("Roll");
    }
    public override void Update()
    {
        // 打滾一段時間後停止
        if (Time.time - cat.stateEnterTime > rollDuration)
        {
            cat.ChangeState(new CatIdleState(cat));
        }
    }
}

public class CatGroomingState : CatStateBase // complete
{
    public CatGroomingState(CatStateManager cat) : base(cat) { }
    // randomly groom for 2~5 seconds, then change to CatIdleState
    private float groomingDuration = 0f; // Duration of grooming
    public override void Enter()
    {
        groomingDuration = Random.Range(2f, 5f); // Random roll duration between 2 and 5 seconds
        Debug.Log("Cat is now grooming.");
        cat.animator.Play("Grooming");
    }

    public override void Update()
    {
        // 理毛一段時間後停止
        if (Time.time - cat.stateEnterTime > groomingDuration)
        {
            cat.ChangeState(new CatIdleState(cat));
        }
    }
}

public class CatMoveToObjectState : CatStateBase // complete
{
    private GameObject target;
    private CatStateBase nextState; // The next state to transition to after reaching the target
    public CatMoveToObjectState(CatStateManager cat, GameObject target, CatStateBase nextState) : base(cat)
    {
        this.target = target;
        this.nextState = nextState;
    }
    public override void Enter()
    {
        cat.animator.Play("Walk");
        Debug.Log("Cat is now moving to point.");
        cat.agent.SetDestination(target.transform.position); // Set the destination to the target's position
    }

    public override void Update()
    {
        cat.setFrontPosition(target); // Set the destination to the target's position
        if (cat.HasReachedDestination())
        {
            cat.ChangeState(nextState); // Transition to the next state
        }
    }
}

public class CatEatState : CatStateBase // complete
{
    // randomly eat for 2~5 seconds, then change to CatIdleState
    private float eatDuration = 0f; // Duration of eating
    public CatEatState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        eatDuration = Random.Range(2f, 5f); // Random eat duration between 2 and 5 seconds
        Debug.Log("Cat is now eating.");
        cat.animator.Play("Eat");
    }

    public override void Update()
    {
        // Eating logic here, if needed
        // For now, just wait for the duration to end
        if (Time.time - cat.stateEnterTime > eatDuration)
        {
            cat.ChangeState(new CatIdleState(cat)); // Transition to idle state after eating
        }
    }
}

public class CatDrinkState : CatStateBase // complete
{
    // randomly eat for 2~5 seconds, then change to CatIdleState
    private float drinkDuration = 0f; // Duration of eating
    public CatDrinkState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        drinkDuration = Random.Range(2f, 5f); // Random eat duration between 2 and 5 seconds
        Debug.Log("Cat is now drinking.");
        cat.animator.Play("Drink");
    }

    public override void Update()
    {
        // Eating logic here, if needed
        // For now, just wait for the duration to end
        if (Time.time - cat.stateEnterTime > drinkDuration)
        {
            cat.ChangeState(new CatIdleState(cat)); // Transition to idle state after eating
        }
    }
}
public class CatPlayWithItemState : CatStateBase // complete
{
    // randomly play with item for 2~5 seconds, then change to CatIdleState
    private float playDuration = 0f; // Duration of playing with item
    public CatPlayWithItemState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        playDuration = Random.Range(2f, 5f); // Random play duration between 2 and 5 seconds
        Debug.Log("Cat is now playing with item.");
        cat.animator.Play("Play");
    }

    public override void Update()
    {
        
        if (Time.time - cat.stateEnterTime > playDuration)
        {
            cat.ChangeState(new CatIdleState(cat)); // Transition to idle state after playing
        }
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
        cat.LookAtUser();
    }
}

public class CatAttackState : CatStateBase
{
    public CatAttackState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Attack");
    }
}


public class CatStateManager : MonoBehaviour
{
    private CatStateBase currentState;
    public Animator animator;
    public UnityEngine.AI.NavMeshAgent agent; // Reference to the NavMeshAgent for movement
    public GameObject catSkeleton; // Reference to the cat skeleton prefab
    public GameObject user; // Reference to the user object
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
            pos.y = hit.position.y ; // Adjust the height to match the ground level
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
        // randomDirection.z += 3f;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
        {
            Debug.Log("Moving to random point: " + hit.position);
            agent.SetDestination(hit.position);
        }
    }
    public bool HasReachedDestination()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }
    public void LookAtUser() { /* LookAt logic */ }
    public void FollowUser() { 
        if (user != null)
        {
            Vector3 direction = (user.transform.position - transform.position).normalized;
            Vector3 targetPosition = user.transform.position - direction * 1.0f;
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
     }
    public void RunAwayFromUser() { /* Flee logic */ }
    public void FollowToyPointer() { /* Follow laser/target logic */ }
    public void AttackUser() { /* Follow laser/target logic */ }
    public GameObject FindFood() { 
        // find the food position in the scene, and return the position
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        if (foods.Length > 0)
        {
            return foods[0];
        }
        return null;
    }
    // find box position
    public GameObject FindBox() { 
        // find the box position in the scene, and return the position
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
        if (boxes.Length > 0)
        {
            return boxes[0];
        }
        return null;
    }
    public void setFrontPosition(GameObject target) { 
        if (target != null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            Vector3 targetPosition = target.transform.position - direction * 0.1f;
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
}
