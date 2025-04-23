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
    public virtual void OnUpdate() { 
        if(!cat.isFollowing ) // 如果貓咪沒有跟隨玩家，並且玩家在視野內，並且貓咪沒有睡覺，並且貓咪沒有坐下
        {
            if(!cat.sleeping && !cat.sitting && cat.IsUserVisible()) // 如果貓咪沒有睡覺，並且貓咪沒有坐下
            {
                if(cat.wantFollow()){
                    cat.ChangeState(new CatFollowState(cat)); // 轉換到跟隨狀態
                    cat.isFollowing = true; // Set the following flag to true
                }else if(cat.wantFlee()){
                    cat.ChangeState(new CatFleeState(cat)); // 轉換到逃跑狀態
                }else{
                    this.Update();
                }
                
            }else{
                this.Update();
            }
        }else{
            this.Update();
        }
    }
    public virtual void Exit() { }
}

public class CatIdleState : CatStateBase // 閒置狀態 
{
    public CatIdleState(CatStateManager cat) : base(cat) { }
    private float idleDuration = 0f; 

    private float[] stateProbabilities = {5,0,0,0,0}; // 機率 array

    public override void Enter()
    {
        cat.animator.Play("Idle");
        Debug.Log("Cat is now idle.");
        idleDuration = Random.Range(1f, 1.5f); 
    }

    public override void Update()
    {   
        if(cat.hungerSystem.GetHunger(cat.catName) < 30){
            GameObject food = cat.FindFood();
            if (food != null)
            {
                cat.ChangeState(new CatMoveToObjectState(cat, food, new CatEatState(cat, food)));
                return;
            }else{
                // 如果找不到食物，就要求食物地方
            }
        }
        if (Time.time - cat.stateEnterTime > idleDuration)
        {
            // 隨機到 CatWanderState, CatSleepState, CatRollState, CatGroomingState, CatMoveToPointState, 用 array 來表示個個機率
            // 0: CatWanderState, 1: CatSleepState, 2: CatRollState, 3: CatGroomingState, 4: CatMoveToPointState, 5: CatPlayWithItemState
            int randomState = cat.getRandomStateNumber(stateProbabilities);
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
                    cat.ChangeState(new CatMoveToObjectState(cat,  cat.FindBox(), new CatPlayWithItemState(cat)));
                    break;
                default:
                    cat.ChangeState(new CatIdleState(cat));
                    break;
            }
        }


    }
}

public class CatWanderState : CatStateBase // 閒晃狀態 -> completed
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

public class CatSleepState : CatStateBase // 睡覺狀態 -> completed? 缺少叫醒的條件
{
    // randomly sleep for 10-50 seconds, then change to CatIdleState
    private float sleepDuration = 0f; // Duration of sleep
    
    public CatSleepState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        sleepDuration = Random.Range(10f, 50f); // Random sleep duration between 10 and 50 seconds
        Debug.Log("Cat is now sleeping.");
        cat.animator.Play("Sleep");
        cat.sleeping = true; // Set the sleeping flag to true
    }
    public override void Update()
    {
        if (Time.time - cat.stateEnterTime > sleepDuration)
        {
            cat.sleeping = false;
            cat.ChangeState(new CatIdleState(cat));
        }
    }
}

public class CatRollState : CatStateBase // 打滾狀態 -> completed
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

public class CatGroomingState : CatStateBase // 理毛狀態 -> completed
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

public class CatMoveToObjectState : CatStateBase // 移動到物件狀態
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

public class CatEatState : CatStateBase // 吃東西狀態 -> completed
{
    private GameObject food; // The food object to eat
    public CatEatState(CatStateManager cat, GameObject food) : base(cat)
    {
        this.food = food;
    }

    public override void Enter()
    {
        Debug.Log("Cat is now wanna eating.");
        if(food){
            cat.animator.Play("Eat");
            cat.hungerSystem.AddHunger(cat.catName,50); // Add hunger points when eating
        }else{
            cat.ChangeState(new CatAskForFoodState(cat,food));
        }
    }

    public override void Update()
    {
        // Eating logic here, if needed
        // For now, just wait for the duration to end
        // if animation is finished, change to CatIdleState
        if (cat.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f )
        {
            cat.ChangeState(new CatIdleState(cat)); 
        }
    }
}

public class CatDrinkState : CatStateBase // 喝水狀態 -> 之後整合至eating狀態
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

public class CatPlayWithItemState : CatStateBase // 玩玩具狀態
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

public class CatFollowState : CatStateBase // 跟隨狀態 -> 等手勢偵測
{
    public CatFollowState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Walk");
    }

    public override void Update()
    {
        if(!cat.sitting){
            cat.FollowUser();
        }
        cat.LookAtUser();

    }
}

public class CatAttackUserState : CatStateBase // 攻擊玩家狀態 -> completed
{
    public CatAttackUserState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Attack");
        cat.AttackUser();
    }

    public override void Update()
    {
        // Attack logic here, if needed
        // For now, just wait for the attack animation to finish
        if (cat.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            cat.ChangeState(new CatIdleState(cat)); // Transition to idle state after attack
        }
    }
}

public class CatPlayWithCatTeaserState : CatStateBase // 玩逗貓棒狀態
{
    public CatPlayWithCatTeaserState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {}

    public override void Update()
    {
        cat.FollowToyPointer(); // Follow the toy pointer logic here
    }
}

public class CatPetState : CatStateBase // 撫摸狀態 -> completed
{
    public CatPetState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Petted");
    }

    public override void Update()
    {
        // Petting logic here, if needed
        // For now, just wait for the duration to end
        if (cat.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            cat.ChangeState(new CatFollowState(cat)); // Transition to idle state after petting
        }
    }
}

public class CatFleeState : CatStateBase // 逃跑狀態 -> completed
{
    public CatFleeState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.RunAwayFromUser();
        cat.animator.Play("Run");
    }
}

public class CatAskForFoodState : CatStateBase // 要食物狀態 -> completed
{   
    private GameObject food; // The food object to eat
    public CatAskForFoodState(CatStateManager cat, GameObject food) : base(cat) {
        this.food = food;
    }

    public override void Enter()
    {
        cat.animator.Play("AskForFood");
    }

    public override void Update()
    {
        // Ask for food logic here, if needed
        // For now, just wait for the duration to end
        // if animation is finished, change to CatIdleState
        if (cat.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            if (food){
                cat.ChangeState(new CatEatState(cat,food)); // Transition to idle state after asking for food
            }else{
                cat.ChangeState(new CatAskForFoodState(cat,food)); // Transition to idle state after asking for food
            }
        }
    }
}

public class CatPlayDeadState : CatStateBase // 裝死狀態 -> completed
{
    public CatPlayDeadState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("PlayDead");
    }

    public override void Update()
    {
        // Play dead logic here, if needed
        // For now, just wait for the duration to end
        // if animation is finished, change to CatIdleState
        if (cat.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            cat.ChangeState(new CatFollowState(cat)); // Transition to idle state after play dead
        }
    }
}

public class CatBackflipState : CatStateBase // 後空翻狀態 -> completed
{
    public CatBackflipState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("Backflip");
    }

    public override void Update()
    {
        // Backflip logic here, if needed
        // For now, just wait for the duration to end
        // if animation is finished, change to CatIdleState
        if (cat.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            cat.ChangeState(new CatFollowState(cat)); // Transition to idle state after backflip
        }
    }
}

public class CatSitDownState : CatStateBase // 坐下狀態 -> completed
{
    public CatSitDownState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.sitting = true;
        cat.animator.Play("SitDown");
    }

    public override void Update()
    {
        // Check if the cat is already sitting
        if (cat.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            cat.ChangeState(new CatFollowState(cat)); // Transition to idle state after attack
        }
    }
}

public class CatStandUpState : CatStateBase // 起立狀態 -> completed
{
    public CatStandUpState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("StandUp");
    }

    public override void Update()
    {
        // Check if the cat is already sitting
        if (cat.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            cat.sitting = false; // Set the sitting flag to true
            cat.ChangeState(new CatFollowState(cat)); // Transition to idle state after attack
        }
    }

}

public class CatPlayWithOtherCatState : CatStateBase // 玩其他貓狀態
{
    public CatPlayWithOtherCatState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.animator.Play("PlayWithOtherCat");
    }
}

// cat Manager

public class CatStateManager : MonoBehaviour
{
    private CatStateBase currentState;
    public string catName; // Name of the cat
    public bool sitting = false; // Flag to check if the cat is sitting
    public bool sleeping = false; // Flag to check if the cat is sleeping
    public bool isFollowing = false; // Flag to check if the cat is following the user
    public Animator animator; // Reference to the Animator component
    public AudioSource audioSource; // Reference to the AudioSource component
    public AudioClip[] meowClips; // Array of meow sound clips
    public UnityEngine.AI.NavMeshAgent agent; // Reference to the NavMeshAgent for movement
    public GameObject catSkeleton; // Reference to the cat skeleton prefab
    public GameObject user; // Reference to the user object
    public GameObject othercat; // Reference to the user object
    public FavorSystem favorSystem;
    public HungerSystem hungerSystem; // Reference to the hunger system
    public float stateEnterTime;
    void Start()
    {   
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            agent.Warp(hit.position); 
            // change agent speed to 0.5f
            agent.speed = 0.5f;
        }
        
        ChangeState(new CatIdleState(this));
    }

    void Update()
    {
        currentState?.OnUpdate();
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
        randomDirection.y += 2f;
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

    public bool IsUserVisible()
    {
        // Check if the user is visible to the cat
        Vector3 directionToUser = user.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToUser);
        if (angle < 45f) // Adjust the angle as needed
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToUser.normalized, out hit, 10f)) // Adjust the distance as needed
            {
                if (hit.collider.gameObject == user)
                {
                    return true; // User is visible
                }
            }
        }
        return false; // User is not visible
    }
    public bool wantFollow() { 
        
        return false;
    }
    public bool wantFlee() { 
        
        return false;
    }
    public void LookAtUser() {

    }
    public void FollowUser() { 
        if (user != null)
        {
            Vector3 direction = (user.transform.position - transform.position).normalized;
            Vector3 targetPosition = user.transform.position - direction * 0.5f;
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
     }
    public void RunAwayFromUser() { 
        if (user != null)
        {
            Vector3 direction = (transform.position - user.transform.position).normalized;
            // random length between 1 and 3
            float randomLength = Random.Range(1f, 3f);
            Vector3 targetPosition = transform.position + direction * randomLength;
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
     
    public void FollowToyPointer() { 
        // Follow the toy pointer logic here
        // For example, you can set the destination to the toy pointer's position
        GameObject toyPointer = GameObject.FindGameObjectWithTag("ToyPointer");
        if (toyPointer != null)
        {
            Vector3 targetPosition = toyPointer.transform.position;
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
    public void AttackUser() { /* Follow laser/target logic */ }
    public GameObject FindFood() { 
        // find the food position in the scene, and return the position
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        int length = foods.Length;
        if (length > 0)
        {
            return foods[Random.Range(0, length)];
        }
        return null;
    }
    // find box position
    public GameObject FindBox() { 
        // find the box position in the scene, and return the position
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
        int length = boxes.Length;
        if (length > 0)
        {
            return boxes[Random.Range(0, length)];
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

    public int getRandomStateNumber( float[] stateProbabilities){
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
        return randomState;
    }
}
