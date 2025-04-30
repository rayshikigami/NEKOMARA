using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using System.IO;

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
        if(!cat.isFollowing && cat.othercat == null) 
        {
            if(!cat.sleeping && !cat.sitting ) // 如果貓咪沒有睡覺，並且貓咪沒有坐下
            {
                if( cat.IsUserVisible()){
                    if(Time.time - cat.getLastSeeUserTime() > 0f){
                        cat.setLastSeeUserTime();
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
                    
                }else if(cat.othercat != null || (Time.time - cat.getLastSeeUserTime() > 30f && cat.IsOtherCatVisible())){
                    cat.setLastSeeUserTime();
                    cat.ChangeState(new CatPlayWithOtherCatState(cat)); // 轉換到跟隨狀態
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

    private float[] stateProbabilities = { 1f, 0f, 0f, 0f, 1f, 1f}; // 機率 array

    public override void Enter()
    {
        
        cat.animator.CrossFade("idle",0.5f);
        Debug.Log("Cat is now idle.");
        idleDuration = Random.Range(3f, 5f); 
    }

    public override void Update()
    {   
        if(cat.hungerSystem.GetHunger(cat.catName) < 30 ){
            GameObject food = cat.FindFavorFood();
            if (food != null)
            {
                cat.ChangeState(new CatMoveToObjectState(cat, food, new CatEatState(cat, food), 0.2f));
                return;
            }else{
                cat.ChangeState(new CatAskForFoodState(cat, food));
                return ;
            }
        }
        if (Time.time - cat.stateEnterTime > idleDuration)
        {
            // 隨機到 CatWanderState, CatSleepState, CatRollState, CatGroomingState, CatMoveToPointState, 用 array 來表示個個機率
            // 0: CatWanderState, 1: CatSleepState, 2: CatRollState, 3: CatGroomingState, 4: CatMoveToPointState and CatPlayWithItemState
            // 5: CatPlayWithCatTowerState
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
                    GameObject box = cat.FindBox();
                    if (box == null){
                        cat.ChangeState(new CatIdleState(cat));
                    }else{
                        cat.ChangeState(new CatMoveToObjectState(cat,  box, new CatPlayWithItemState(cat, box),0.6f));
                    }
                    break;
                case 5:
                    GameObject ct = cat.FindCatTower();
                    if (ct == null){
                        cat.ChangeState(new CatIdleState(cat));
                    }else{
                        cat.ChangeState(new CatMoveToObjectState(cat,  ct, new CatPlayWithCatTowerState(cat, ct), 0.8f));
                    }
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
        cat.animator.Play("walk");
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
        sleepDuration = Random.Range(30f, 50f); // Random sleep duration between 10 and 50 seconds
        Debug.Log("Cat is now sleeping.");
        cat.animator.Play("sleep");
        cat.sleeping = true; // Set the sleeping flag to true
        cat.audioSource.PlayOneShot(cat.sleepClips[Random.Range(0, cat.sleepClips.Length)]);
    }
    public override void Update()
    {
        if (Time.time - cat.stateEnterTime > sleepDuration)
        {
            cat.sleeping = false;
            cat.ChangeState(new CatIdleState(cat));
            return;
        }
        // if animation is done, replay
        AnimatorStateInfo stateInfo = cat.animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1.0f)
        {
            cat.animator.Play("sleep", 0, 0.8f);
        }
        // Check if the cat audio is finished playing
        if (cat.audioSource.isPlaying == false)
        {
            // Play a random sleep sound
            cat.audioSource.PlayOneShot(cat.sleepClips[Random.Range(0, cat.sleepClips.Length)]);
        }
    }
}

public class CatRollState : CatStateBase // 打滾狀態 -> complete?
{
    // randomly roll for 2~5 seconds, then change to CatIdleState
    public CatRollState(CatStateManager cat) : base(cat) { }
    private float rollDuration = 0f; // Duration of rolling
    public override void Enter()
    {
        rollDuration = Random.Range(2f, 5f); // Random roll duration between 2 and 5 seconds
        Debug.Log("Cat is now rolling.");
        cat.animator.CrossFade("roll",0.5f);
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
        cat.animator.CrossFade("grooming", 0.5f);
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
    private float ds;
    public CatMoveToObjectState(CatStateManager cat, GameObject target, CatStateBase nextState, float ds) : base(cat)
    {
        this.target = target;
        this.nextState = nextState;
        this.ds = ds;
    }
    public override void Enter()
    {
        cat.animator.Play("walk");
        Debug.Log("Cat is now moving to point.");
        Vector3 targetPosition = target.transform.position;
        Vector3 offset = targetPosition - cat.transform.position;
        offset.y = 0; // Set y to 0 to ignore height difference
        offset.Normalize(); // Normalize the direction vector
        targetPosition = targetPosition - offset * ds; // Set the target position to be in front of the target
        cat.agent.SetDestination(targetPosition); // Set the destination to the target's position
    }

    public override void Update()
    {
        if (cat.HasReachedDestination())
        {
            
            // check the cat is face to the target or not, if not , rotate to the target, if it is, then change to the next state
            Vector3 targetPosition = target.transform.position;
            Vector3 offset = targetPosition - cat.transform.position;
            offset.y = 0; // Set y to 0 to ignore height difference
           
            // if offset > 1f, then set the target position to be in front of the target
            if (offset.magnitude > ds * 1.3f)
            {
                offset.y = 0; // Set y to 0 to ignore height difference
                offset.Normalize(); // Normalize the direction vector
                targetPosition = targetPosition - offset * ds; // Set the target position to be in front of the target
                cat.agent.SetDestination(targetPosition);
                return;
            }
            offset.Normalize(); // Normalize the direction vector
            float angle = Vector3.SignedAngle(cat.transform.forward, offset, Vector3.up);
            if (Mathf.Abs(angle) > 5f)
            {
                // Rotate the cat to face the target
                Quaternion targetRotation = Quaternion.LookRotation(offset);
                cat.transform.rotation = Quaternion.RotateTowards(cat.transform.rotation, targetRotation, 360 * Time.deltaTime);
            }
            else
            {
                cat.ChangeState(nextState); // Transition to the next state
            }

           
        }
    }
}

public class CatEatState : CatStateBase // 吃東西狀態 -> completed
{
    // randomly eat for 2~5 seconds, then change to CatIdleState
    // 這裡的食物是指貓咪要吃的食物，可能是食物的 prefab 或者是食物的 GameObject
    private float eatDuration = 10f; // Duration of eating
    private GameObject food; // The food object to eat
    private CatBowl catBowlScript;
    private int foodType; // The food type to eat

    public CatEatState(CatStateManager cat, GameObject food) : base(cat)
    {
        // this.eatDuration = Random.Range(f, 5f); // Random eat duration between 2 and 5 seconds
        this.food = food;
        if (food != null){
            foodType = food.GetComponent<CatFood>().foodType;
            if(foodType == 0){
                catBowlScript = food.GetComponent<CatBowl>();
            }
        }
    }

    public override void Enter()
    {
        if( food == null){
            Debug.Log("Cat is now eating nothing.");
            cat.ChangeState(new CatAskForFoodState(cat,food)); // Transition to idle state after eating nothing
            return;
        }
        Debug.Log("Cat is now wanna eating.");
        if(foodType != 0 || catBowlScript.isFull ){
            cat.nextMeowTime = Time.time + Random.Range(20f, 30f);
            Debug.Log("Cat is now eating.");
            cat.animator.CrossFade("eating", 0.25f);
            cat.hungerSystem.AddHunger(cat.catName,50); // Add hunger points when eating
            // plat random eat sound
            cat.audioSource.PlayOneShot(cat.eatClips[Random.Range(0, cat.eatClips.Length)]);
            cat.love.SetActive(true);
            cat.loveTime = Time.time;
            if (cat.favorateFood == foodType){
                cat.favorSystem.AddFavor(cat.catName, 10); // Increase favor points when eating favorite food
                cat.achieveSystem.UpdateProgress("favor_up", 10);
            }else{
                cat.favorSystem.AddFavor(cat.catName, 5); // Increase favor points when eating food
                cat.achieveSystem.UpdateProgress("favor_up", 5);
            }
        }else{
            cat.ChangeState(new CatAskForFoodState(cat,food));
        }
    }

    public override void Update()
    {
        // Eating logic here, if needed
        // For now, just wait for the duration to end
        if (Time.time - cat.stateEnterTime > eatDuration)
        {
            cat.ChangeState(new CatIdleState(cat)); // Transition to idle state after eating
            // stop audio 
            cat.audioSource.Stop();
            if(foodType == 0)
            {
                catBowlScript.isFull = false; // Set the food bowl to not full after eating
            }
            else
            {
                food.GetComponent<CatFood>().BeEaten();
            }
            return;
        }
        AnimatorStateInfo stateInfo = cat.animator.GetCurrentAnimatorStateInfo(0);
    
        if ( stateInfo.normalizedTime >= 1.0f)
        {
            Debug.Log(stateInfo.normalizedTime);
            cat.animator.Play("eating",  0, 0.25f );
        }
        if (cat.audioSource.isPlaying == false)
        {
            // Play a random eat sound
            cat.audioSource.PlayOneShot(cat.eatClips[Random.Range(0, cat.eatClips.Length)]);
        }
    }
}

public class CatPlayWithItemState : CatStateBase // almost completed
{
    private GameObject item;
    private Transform bottomTransform;
    private Vector3 offset;

    private Vector3 originalPosition;

    public CatPlayWithItemState(CatStateManager cat, GameObject item) : base(cat)
    {
        this.item = item;
    }

    public override void Enter()
    {
        Debug.Log("Cat is now playing with item.");
        bottomTransform = item.transform.Find("Cube (4)");

        if (bottomTransform != null)
        {
            cat.SetJumpable(true);
            Vector3 pos = item.transform.position;
            Vector3 bottomPosition = bottomTransform.position;
            offset = pos - bottomPosition;

            originalPosition = cat.transform.position;

            if (offset.y > 0.1f)
            {
                Debug.Log("開口朝上");
                // 開口朝上
                cat.StartCoroutine(JumpIntoBoxRoutine());
            }
            else if (offset.y < -0.1f)
            {
                Debug.Log("開口朝下");
                // 開口朝下
                cat.SetJumpable(false);
                
                cat.StartCoroutine(ScratchRoutine());
            }
            else
            {
                Debug.Log("開口朝側面");
                // 水平開口，計算入口點
                // Vector3 targetPosition = pos + 1.2f * offset;

                // if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
                // {
                //     cat.StartCoroutine(EnterFromSideRoutine(hit.position));
                // }
                // else
                // {
                //     Debug.LogWarning("Failed to find NavMesh position near side entry.");
                //     cat.SetJumpable(false);
                //     cat.ChangeState(new CatIdleState(cat));
                // }
                cat.SetJumpable(false);
                
                cat.StartCoroutine(ScratchRoutine());
            }
        }
        else
        {
            Debug.LogWarning("Could not find Cube (4) in item.");
            cat.ChangeState(new CatIdleState(cat));
        }
    }

    private IEnumerator JumpIntoBoxRoutine()
    {
        cat.animator.Play("jump");

        yield return JumpToPosition(item.transform.position + Vector3.down * 0.2f, 0.5f); // into box
        yield return RotateBy180(0.5f);
        cat.animator.CrossFade("sitting",0.25f);
        yield return new WaitForSeconds(Random.Range(2f, 5f)); // stay inside

        cat.animator.CrossFade("jump",0.25f);
        yield return new WaitForSeconds(0.3f);
        yield return JumpToPosition(originalPosition, 0.5f); // jump back
        cat.SetJumpable(false);
        cat.ChangeState(new CatIdleState(cat));
    }

    private IEnumerator ScratchRoutine()
    {
        float moveDistance = 0.18f;
        float moveDuration = 0.2f;
        Vector3 startPos = cat.transform.position;
        Vector3 targetPos = startPos + cat.transform.forward * moveDistance;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            cat.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cat.transform.position = targetPos;
        cat.animator.Play("punch"); 

        yield return new WaitForSeconds(2f);
        cat.ChangeState(new CatIdleState(cat));
    }

    private IEnumerator EnterFromSideRoutine(Vector3 entryPoint)
    {
        cat.SetJumpable(false);
        cat.agent.SetDestination(entryPoint);

        // 等待抵達入口點
        while (Vector3.Distance(cat.transform.position, entryPoint) > 0.2f)
        {
            yield return null;
        }
        cat.SetJumpable(true);
        // 進入箱子（稍微往內走）
        Vector3 insidePosition = item.transform.position - offset.normalized * 0.3f;
        if (UnityEngine.AI.NavMesh.SamplePosition(insidePosition, out UnityEngine.AI.NavMeshHit innerHit, 1f, UnityEngine.AI.NavMesh.AllAreas))
        {
            cat.agent.SetDestination(innerHit.position);
            while (Vector3.Distance(cat.transform.position, innerHit.position) > 0.2f)
            {
                yield return null;
            }

            yield return new WaitForSeconds(Random.Range(2f, 5f));

            // 退回原地
            cat.agent.SetDestination(originalPosition);
            while (Vector3.Distance(cat.transform.position, originalPosition) > 0.2f)
            {
                yield return null;
            }
        }
        cat.SetJumpable(false);
        cat.ChangeState(new CatIdleState(cat));
    }
    private IEnumerator RotateBy180(float duration)
    {
        Quaternion startRotation = cat.transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 180, 0);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            cat.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cat.transform.rotation = targetRotation;
    }
    private IEnumerator JumpToPosition(Vector3 target, float duration)
    {
        Vector3 start = cat.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 position = Vector3.Lerp(start, target, t);
            position.y += Mathf.Sin(t * Mathf.PI) * 0.4f; // jump arc
            cat.transform.position = position;
            elapsed += Time.deltaTime;
            yield return null;
        }

        cat.transform.position = target;
    }

    public override void Update() { }
}


public class CatFollowState : CatStateBase // 跟隨狀態 -> 等手勢偵測 !!!
{
    public CatFollowState(CatStateManager cat) : base(cat) { }
    private float lastTimeSeeUser = 0f; // 上次看到玩家的時間
    
    public override void Enter()
    {
        Debug.Log("now is following");
        cat.animator.CrossFade("walk", 0.4f);
        cat.currentAnimation = CatStateManager.currentCatAnimation.walk;
        lastTimeSeeUser = Time.time;
    }
    
    public override void Update()
    {
        if(!cat.sitting){
            cat.FollowUser();
            // 
            if(cat.HasReachedDestination()){
                if (cat.currentAnimation != CatStateManager.currentCatAnimation.idle)
                {
                    cat.animator.CrossFade("idle", 0.2f);
                    cat.currentAnimation = CatStateManager.currentCatAnimation.idle;
                }
            }else{
                if (cat.currentAnimation != CatStateManager.currentCatAnimation.walk)
                {
                    cat.currentAnimation = CatStateManager.currentCatAnimation.walk;
                    cat.animator.CrossFade("walk", 0.2f);
                }
            }
            
        }else{
            // if cat's animation is not sitting, play sitting animation
            if (cat.currentAnimation != CatStateManager.currentCatAnimation.sitting)
            {
                cat.animator.CrossFade("sitting", 0.2f);
                cat.currentAnimation = CatStateManager.currentCatAnimation.sitting;
            }
            
        }
        CatTeaser catTeaser = Object.FindObjectOfType<CatTeaser>();
        //Debug.Log(cat.lefthand.gestureType);
        if (catTeaser != null && catTeaser.teasing)
        {
            cat.sitting = false;
            cat.ChangeState(new CatPlayWithCatTeaserState(cat)); // Transition to play with cat teaser state
            return;
        }
        if( cat.IsUserVisible() ){
            lastTimeSeeUser = Time.time;
        }
        if (cat.lefthand.gestureType == Hands.catGestureType.Leave || cat.righthand.gestureType == Hands.catGestureType.Leave)
        {
            cat.lefthand.gestureType = Hands.catGestureType.Normal;
            cat.righthand.gestureType = Hands.catGestureType.Normal;
            cat.sitting = false;
            cat.isFollowing = false;
            cat.ChangeState(new CatIdleState(cat)); // Transition to flee state 
            return;
        }
        if(cat.favorSystem.GetAdopt(cat.catName) == 1)
        {
            if (Time.time - cat.stateEnterTime > 3.5f)
            {
                if (cat.lefthand.gestureType == Hands.catGestureType.PlayDead || cat.righthand.gestureType == Hands.catGestureType.PlayDead)
                {
                    Debug.Log("GET PLAYDEAD GESTURE");
                    cat.lefthand.gestureType = Hands.catGestureType.Normal;
                    cat.righthand.gestureType = Hands.catGestureType.Normal;
                    cat.ChangeState(new CatPlayDeadState(cat));
                    return;
                }
                else if (cat.lefthand.gestureType == Hands.catGestureType.BackFlip || cat.righthand.gestureType == Hands.catGestureType.BackFlip)
                {
                    cat.lefthand.gestureType = Hands.catGestureType.Normal;
                    cat.righthand.gestureType = Hands.catGestureType.Normal;
                    cat.ChangeState(new CatBackflipState(cat)); // Transition to pet state
                    return;
                }

            }
            if (cat.lefthand.gestureType == Hands.catGestureType.Stand || cat.righthand.gestureType == Hands.catGestureType.Stand)
            {
                cat.lefthand.gestureType = Hands.catGestureType.Normal;
                cat.righthand.gestureType = Hands.catGestureType.Normal;
                cat.ChangeState(new CatStandUpState(cat)); // Transition to flee state
                return;
            }
            else if (cat.lefthand.gestureType == Hands.catGestureType.Sit || cat.righthand.gestureType == Hands.catGestureType.Sit)
            {
                cat.lefthand.gestureType = Hands.catGestureType.Normal;
                cat.righthand.gestureType = Hands.catGestureType.Normal;
                cat.ChangeState(new CatSitDownState(cat)); // Transition to sit state
                return;
            }
        }
        
        if ( Time.time - cat.stateEnterTime  > 10f ){
            cat.sitting = false;
            cat.isFollowing = false;
            cat.ChangeState(new CatAttackUserState(cat));
            return;
        }
        if( Time.time - lastTimeSeeUser > 10f ){
            cat.isFollowing = false;
            cat.sitting = false;
            cat.ChangeState(new CatIdleState(cat)); // Transition to idle state after not seeing user for a while
            return;
        }
    }
}

public class CatAttackUserState : CatStateBase // 攻擊玩家狀態 -> completed !!! 特效
{
    public CatAttackUserState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        Debug.Log("cat is now attack user");
        cat.animator.Play("punch");

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
    {
        Debug.Log("Cat is now playing with cat teaser.");
    }

    public override void Update()
    {
        cat.FollowToyPointer(); 
        CatTeaser catTeaser = Object.FindObjectOfType<CatTeaser>();
        if (catTeaser == null || !catTeaser.teasing)
        {
            cat.ChangeState(new CatFollowState(cat)); // Transition to play with cat teaser state
        }
    }
}

public class CatPetState : CatStateBase // 撫摸狀態 ->  !!!
{
    public CatPetState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        cat.achieveSystem.UpdateProgress("touch", 1);
        cat.love.SetActive(true);
        cat.loveTime = Time.time;
        cat.favorSystem.AddFavor(cat.catName, 5); // Decrease favor points when fleeing
        cat.achieveSystem.UpdateProgress("favor_up",5);
        cat.animator.CrossFade("petted", 0.25f);
        cat.audioSource.PlayOneShot(cat.sleepClips[Random.Range(0, cat.sleepClips.Length)]);
    }

    public override void Update()
    {
        // Petting logic here, if needed
        // For now, just wait for the duration to end
        if (cat.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            cat.ChangeState(new CatFollowState(cat)); // Transition to idle state after petting
            // stop audio
            cat.audioSource.Stop();
        }
    }
}

public class CatFleeState : CatStateBase // 逃跑狀態 -> completed
{
    public CatFleeState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {

        Debug.Log("cat is now flee");
        cat.favorSystem.AddFavor(cat.catName, -1); // Decrease favor points when fleeing
        cat.achieveSystem.UpdateProgress("favor_down",1);
        cat.RunAwayFromUser();
        cat.animator.Play("run");
        cat.agent.speed = 0.65f; // Increase speed when fleeing
    }

    public override void Update()
    {
        // Fleeing logic here, if needed
        // For now, just wait for the duration to end
        if (cat.HasReachedDestination())
        {
            cat.ChangeState(new CatIdleState(cat)); // Transition to idle state after fleeing
            cat.agent.speed = 0.35f; // Reset speed after fleeing
        }
    }
    public override void Exit()
    {
        cat.agent.speed = 0.35f; // Reset speed after fleeing
    }
}

public class CatAskForFoodState : CatStateBase // 要食物狀態 -> completed 之後加個抬頭?
{   
    private GameObject food; // The food object to eat
    private CatBowl catBowlScript;
    private int foodType; // The food type to eat
    private float askDuration = 10f; // Duration of asking for food
    private int askCount = 0; // Ask for food count
    public CatAskForFoodState(CatStateManager cat, GameObject food) : base(cat) {
        this.food = food;
        if (food != null){
            this.catBowlScript = food.GetComponent<CatBowl>();
            this.foodType = food.GetComponent<CatFood>().foodType;
        }
    }
    public CatAskForFoodState(CatStateManager cat, GameObject food, int ac) : base(cat) {
        this.food = food;
        if (food != null){
            this.catBowlScript = food.GetComponent<CatBowl>();
            this.foodType = food.GetComponent<CatFood>().foodType;
        }
        this.askCount = ac;
    }

    public override void Enter()
    {
        cat.animator.CrossFade("sitting", 0.5f);
        Debug.Log("Cat is now asking for food.");
        cat.audioSource.pitch = 1.5f;
        cat.audioSource.PlayOneShot(cat.meowClips[0]); // Play meow sound when asking for food
        cat.nextMeowTime = Time.time + + Random.Range(20f, 30f); 
    }
    public override void Update()
    {
        // Ask for food logic here, if needed
        // For now, just wait for the duration to end
        // if animation is finished, change to CatIdleState

        if(this.food != null){
            if(this.foodType == 0){
                if(this.catBowlScript.isFull){
                    cat.ChangeState(new CatEatState(cat, food)); // Transition to eat state after asking for food
                    cat.audioSource.pitch = 1.0f;
                }else{
                    if (Time.time - cat.stateEnterTime > askDuration)
                    {
                        cat.favorSystem.AddFavor(cat.catName, -3 * (askCount+1)); // Decrease favor points when fleeing
                        cat.achieveSystem.UpdateProgress("favor_down", 3 * (askCount+1));
                        if(askCount < 3){
                            cat.ChangeState(new CatAskForFoodState(cat,food, this.askCount + 1)); // Transition to idle state after asking for food
                        }else{
                            GameObject newfood = cat.FindFood();
                            if(newfood == this.food){
                                cat.ChangeState(new CatAskForFoodState(cat,food, this.askCount + 1));
                            }
                            cat.ChangeState(new CatMoveToObjectState(cat, newfood, new CatEatState(cat, newfood), 0.2f));
                            cat.audioSource.pitch = 1.0f;
                        }
                    }
                }
            }else if(this.foodType == 1){
                cat.ChangeState(new CatMoveToObjectState(cat, food, new CatEatState(cat, food), 0.2f));
                 cat.audioSource.pitch = 1.0f;
            }else if(this.foodType == 2){
                cat.ChangeState(new CatMoveToObjectState(cat, food, new CatEatState(cat, food), 0.2f));
                 cat.audioSource.pitch = 1.0f;
            }else if(this.foodType == 3){
                cat.ChangeState(new CatMoveToObjectState(cat, food, new CatEatState(cat, food), 0.2f));
                 cat.audioSource.pitch = 1.0f;
            }
        }
        if(Time.time - cat.stateEnterTime > 3f){
            cat.audioSource.pitch = 1.0f;
        }
        if (Time.time - cat.stateEnterTime > askDuration)
        {
            cat.favorSystem.AddFavor(cat.catName, -3 * (askCount+1)); // Decrease favor points when fleeing
            cat.achieveSystem.UpdateProgress("favor_down", 3 * (askCount+1));
            if(askCount < 3){
                GameObject newfood = cat.FindFavorFood();
                cat.ChangeState(new CatAskForFoodState(cat,newfood, this.askCount + 1)); // Transition to idle state after asking for food
            }else{
                GameObject newfood = cat.FindFood();
                if(newfood != null){
                    cat.ChangeState(new CatMoveToObjectState(cat, newfood, new CatEatState(cat, newfood),0.2f));
                    cat.audioSource.pitch = 1.0f;
                }
                cat.ChangeState(new CatAskForFoodState(cat,newfood, this.askCount + 1));
            }
            
        }
    }
}

public class CatPlayDeadState : CatStateBase // 裝死狀態 -> completed !!!
{
    public CatPlayDeadState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {

        Debug.Log("cat is now playdead");
        cat.animator.Play("playdead");
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

public class CatBackflipState : CatStateBase // 後空翻狀態 -> completed !!!
{
    public CatBackflipState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {

        Debug.Log("cat is now Backflip");
        cat.animator.Play("backflip");
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

public class CatSitDownState : CatStateBase // 坐下狀態 -> completed !!!
{
    public CatSitDownState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        if(cat.sitting == true){
            cat.ChangeState(new CatFollowState(cat)); // Transition to idle state after attack
            return;
        }
        cat.sitting = true;
        cat.animator.CrossFade("sitting", 0.3f);
        Debug.Log("cat is now sitting");
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

public class CatStandUpState : CatStateBase // 起立狀態 -> completed !!!
{
    public CatStandUpState(CatStateManager cat) : base(cat) { }

    public override void Enter()
    {
        if( cat.sitting == false){
            cat.ChangeState(new CatFollowState(cat)); // Transition to idle state after attack
            return;
        }
        cat.animator.CrossFade("standing", 0.3f);
        Debug.Log("cat is now standing");
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

public class CatPlayWithCatTowerState : CatStateBase // 玩玩具狀態 > > completed 
{
    // randomly play with item for 2~5 seconds, then change to CatIdleState
     private GameObject item;
    private Transform top;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    public CatPlayWithCatTowerState(CatStateManager cat, GameObject item) : base(cat) { 
        this.item = item;
        top = item.transform.Find("jump target");
    }

    public override void Enter()
    {
        
        Debug.Log("Cat is now playing with cat tower.");
        originalPosition = cat.transform.position;
        originalRotation = cat.transform.rotation;
        cat.SetJumpable(true); // Set the cat to be jumpable
        cat.StartCoroutine(JumpRoutine());
    }
    private IEnumerator JumpRoutine()
    {
        // 播放跳躍動畫
        cat.animator.Play("jump");

        // 等待動畫播放起跳的瞬間 (假設0.3秒後起跳)
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Jumped to cat tower top position.");
        // 跳到貓塔 top 的位置
        yield return JumpToPosition(top.position, 0.6f); // 0.6 秒完成跳躍
        // cat.transform.Rotate(0, 180, 0); 漸轉
        yield return RotateBy180(1.2f);
        
        cat.sitting = true; // Set the sitting flag to true
        float stayDuration = Random.Range(3f, 30f);
        Debug.Log("Staying on cat tower top for " + stayDuration + " seconds.");
        cat.animator.CrossFade("sitting", 0.5f);
        // 停留 2~5 秒
        
        yield return new WaitForSeconds(stayDuration);
        // rotate 180 degree
        
        cat.sitting = false;
        // 再次播放跳躍動畫
        
        cat.animator.CrossFade("idle", 0.25f);
        yield return new WaitForSeconds(0.2f);
        cat.animator.CrossFade("jump",0.25f);
        Debug.Log("Jumped back to original position.");
        // 跳回原地
        yield return JumpToPosition(originalPosition, 0.6f);
        
        // 切換狀態
        
        cat.SetJumpable(false); // Set the cat to be jumpable
        cat.ChangeState(new CatIdleState(cat));
    }

    private IEnumerator JumpToPosition(Vector3 target, float duration)
    {
        Vector3 start = cat.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // 模擬跳躍弧線
            Vector3 position = Vector3.Lerp(start, target, t);
            position.y += Mathf.Sin(t * Mathf.PI) * 0.5f; // 調整跳躍高度

            cat.transform.position = position;
            elapsed += Time.deltaTime;
            yield return null;
        }

        cat.transform.position = target;
    }
    private IEnumerator RotateBy180(float duration)
    {
        Quaternion startRotation = cat.transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 180, 0);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            cat.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cat.transform.rotation = targetRotation;
    }

    public override void Update()
    {
        
    }
}


public class CatPlayWithOtherCatState : CatStateBase // 玩其他貓狀態
{
    public CatPlayWithOtherCatState(CatStateManager cat) : base(cat) { }
    private float playDuraition = 10f; // Duration of playing with other cat
    public override void Enter()
    {
        cat.animator.Play("punch");
        Debug.Log("Cat is now playing with other cat.");
    }
    public override void Update()
    {
        // Play with other cat logic here, if needed
        // For now, just wait for the duration to end
        if (Time.time - cat.stateEnterTime > playDuraition)
        {
            cat.ChangeState(new CatIdleState(cat)); // Transition to idle state after playing with other cat
            cat.othercat = null; // Reset the other cat reference
        }
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
    public bool isJumpable = false;
    private bool isJumping = false;
    public Animator animator; // Reference to the Animator component
    public AudioSource audioSource; // Reference to the AudioSource component
    public AudioClip[] meowClips; // Array of meow sound clips
    public AudioClip[] eatClips; // Array of purr sound clips
    public AudioClip[] scratchClips; // Array of scratch sound clips
    public AudioClip[] sleepClips; // Array of scratch sound clips
    public UnityEngine.AI.NavMeshAgent agent; // Reference to the NavMeshAgent for movement
    public GameObject catSkeleton; // Reference to the cat skeleton prefab
    public GameObject user; // Reference to the user object
    public GameObject othercat; // Reference to the user object
    public AchieveSystem achieveSystem;
    public FavorSystem favorSystem;
    public HungerSystem hungerSystem; // Reference to the hunger system
    public float stateEnterTime;
    public float nextMeowTime; // Duration of the current state
    public Transform headBone;
    public float maxHeadTurnAngle = 20f;
    public float headTurnSpeed = 5f;
    public int favorateFood = 0; // Favorite food type (0: find bowl, 1: can, 2: fish, 3: 抹茶巴菲?)
    // personality with random number 0~1f
    public float personality;
    private float lastSeeUserTime ; // Duration of the current state
    private float lastSeeOtherCatTime ; // Duration of the current state
    public Hands lefthand; // Reference to the hands object
    public Hands righthand;
    public GameObject love;
    public float loveTime;
    public enum currentCatAnimation
    {
        idle,
        walk,
        sitting
    }
    public currentCatAnimation currentAnimation = currentCatAnimation.idle;
    void Start()
    {
        love.SetActive(false);
        agent.speed = 0.35f;
        transform.position = user.transform.position ; // Set the initial position of the cat to the user's position
        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
        {
            agent.Warp(hit.position);  
            // change agent speed to 0.5f
            
        }
        // catch catSkeleton
        
        catSkeleton = this.gameObject;
        if (headBone == null && catSkeleton != null)
        {
            Transform foundHead = FindChildByName(catSkeleton.transform, "head"); // 換成你骨架中的實際路徑
            if (foundHead != null)
            {
                headBone = foundHead;
            }
            else
            {
                Debug.LogWarning("Head bone not found in catSkeleton.");
            }
        }

        // LoadAudioClips("Audio/catSound/catNormalMeow", ref meowClips);
        nextMeowTime = Time.time + Random.Range(35f, 60f); // Set the initial meow time
        setLastSeeUserTime();
        ChangeState(new CatIdleState(this));
    }
    public float getLastSeeUserTime(){
        return this.lastSeeUserTime;
    }
    public void setLastSeeUserTime(){
        this.lastSeeUserTime = Time.time;
    }
    public float getLastSeeOtherCatTime(){
        return this.lastSeeOtherCatTime;
    }
    public void setLastSeeOtherCatTime(){
        this.lastSeeOtherCatTime = Time.time;
    }
    void LoadAudioClips(string folderPath, ref AudioClip[] clipArray)
    {
        // 確保路徑格式正確，並獲取所有 .mp3 檔案的路徑
        string[] filePaths = Directory.GetFiles("Assets/" + folderPath, "*.mp3");

        clipArray = new AudioClip[filePaths.Length];
        
        // 用協程加載每一個音效檔案
        for (int i = 0; i < filePaths.Length; i++)
        {
            string filePath = filePaths[i];
            StartCoroutine(LoadAudioClip(filePath, i, clipArray));
        }
    }

    System.Collections.IEnumerator LoadAudioClip(string filePath, int index, AudioClip[] clipArray)
    {
        // 使用 UnityWebRequestMultimedia 來加載音頻
        string url = "file://" + filePath;

        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load audio clip: " + www.error);
        }
        else
        {
            clipArray[index] = DownloadHandlerAudioClip.GetContent(www);
            Debug.Log("Loaded audio clip: " + clipArray[index].name);
        }
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindChildByName(child, name);
            if (result != null) return result;
        }
        return null;
    }


    void Update()
    {
        if (Time.time - loveTime > 1f)
        {
            love.SetActive(false);
        }
        currentState?.OnUpdate();
        // 每隔一段時間隨機叫一次
        if (Time.time > nextMeowTime )
        {
            int randomIndex = Random.Range(0, meowClips.Length);
            audioSource.PlayOneShot(meowClips[randomIndex]);
            nextMeowTime = Time.time + Random.Range(20f, 30f); // Reset the timer after meowing
            hungerSystem.AddHunger(catName, -Random.Range(5, 10)); // Decrease hunger points when meowing
        }
    }

    void LateUpdate()
    {
        if(!isJumpable){
            if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out UnityEngine.AI.NavMeshHit hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
            {
                Vector3 pos = transform.position;
                pos.y = hit.position.y ; // Adjust the height to match the ground level
                transform.position = pos;
            }
        }
        
        if (this.isFollowing){
            this.LookAtUser();
        }
    }


    public void ChangeState(CatStateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        stateEnterTime = Time.time;
        currentState.Enter();
    }

    public void WakeUp()
    {
        if (sleeping)
        {
            sleeping = false; // Set the sleeping flag to false
            ChangeState(new CatIdleState(this)); // Transition to idle state after waking up
        }
    }

    public void SetJumpable(bool jm){
        this.isJumpable = jm;
        if(jm){
            agent.enabled = false; // Disable the NavMeshAgent when jumping
            // agent.isStopped = true; // Stop the agent
        }else{  
            agent.enabled = true; // Enable the NavMeshAgent when not jumping
            // agent.isStopped = false; // Resume the agent
        }
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
        if (user == null)  return false; // User is not set
        // Check if the user is visible to the cat
        Vector3 directionToUser = user.transform.position - transform.position;
        directionToUser.y = 0; // Ignore vertical component
        float angle = Vector3.Angle(transform.forward, directionToUser);
        if (angle < 30f) // Adjust the angle as needed
        {
            if (directionToUser.magnitude < 2f) {
                return true;
            }
        }
        return false; // User is not visible
    }

    public bool IsOtherCatVisible()
    {
        // find all tag with cat object
        GameObject[] cats = GameObject.FindGameObjectsWithTag("cat");
        foreach (GameObject cat in cats)
        {
            if (cat != this.gameObject) // Ignore itself
            {
                // Check if the other cat is visible to the cat
                Vector3 directionToOtherCat = cat.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, directionToOtherCat);
                if (angle < 30f) // Adjust the angle as needed
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, directionToOtherCat.normalized, out hit, 2f)) // Adjust the distance as needed
                    {
                        if (hit.collider.gameObject == cat)
                        {
                            if (cat != null )
                            {
                                this.othercat = cat; // Set the other cat to the current cat\
                                CatStateManager othercatStateManager = cat.GetComponent<CatStateManager>();
                                if (othercatStateManager != null)
                                {
                                    othercatStateManager.othercat = this.catSkeleton; // Set the other cat to the current cat
                                }
                                return true; // Other cat is visible
                            }
                        }
                    }
                }
            }
        }
        return false; // No other cat is visible
    }


    public bool wantFollow() { 
        float x = Random.Range(0f, 1f); // 隨機生成一個 0~1 的數字
        float y = this.favorSystem.GetFavor(this.catName)/10f; // 將 favor 轉換為 sigmoid 函數的值
        if (x * y > personality) // 如果大於 personality 的話，就跟隨
        {
            return true; // Follow the user
        }
        return false;
    }
    public bool wantFlee() {
        return false;
        float x = Random.Range(0f, 1f); // 隨機生成一個 0~1 的數字
        float y = this.favorSystem.GetFavor(this.catName)/10f;
        if (x * y < personality * personality * 0.5)
        {
            return true; // flee 
        }
        return false;
    }
    public void LookAtUser()
    {
        Vector3 targetPosition = user.transform.position;
        Vector3 offset = targetPosition - this.transform.position;
        offset.y = 0; // Set y to 0 to ignore height difference
        offset.Normalize(); // Normalize the direction vector
        float angle = Vector3.SignedAngle(this.transform.forward, offset, Vector3.up);
        //Debug.Log("angle: "+Mathf.Abs(angle));
        if (HasReachedDestination() && Mathf.Abs(angle) > 30f)
        {
            // Rotate the cat to face the target
            this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(offset), 360 * Time.deltaTime);
        }
        
    }

    public void FollowUser() { 
        if (user != null)
        {
            Vector3 direction = user.transform.position - transform.position; 
            direction.y = 0f;
            if (Mathf.Abs(direction.magnitude - 0.5f) < 0.25f) return;
            direction.Normalize();
            Vector3 targetPosition = user.transform.position - direction * 0.5f;
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
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
     
    public void FollowToyPointer()
    {
        if (isJumping){
            return;
        }
        GameObject toyPointer = GameObject.FindGameObjectWithTag("ToyPointer");
        if (toyPointer != null)
        {
            Vector3 targetPosition = toyPointer.transform.position;
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out UnityEngine.AI.NavMeshHit hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                float distance = (hit.position - transform.position).magnitude;
                if (distance < 0.075f && (targetPosition - transform.position).y > 0.6f)
                {

                    // 停止 NavMeshAgent
                    this.SetJumpable(true);

                    isJumping = true;
                    // 執行跳躍協程
                    StartCoroutine(JumpToTarget(targetPosition));
                }
                else
                {
                    // if animation is not walk, play walk
                    AnimatorStateInfo stateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
                    if (!stateInfo.IsName("walk"))
                    {
                        this.animator.Play("walk");
                    }
                    
                    this.SetJumpable(false);
                    this.agent.SetDestination(hit.position);
                }
            }
        }else{
            Debug.Log("Toy pointer not found.");
        }
    }

    private IEnumerator JumpToTarget(Vector3 targetPos)
    {
        Debug.Log("Jumping to target position");
        float duration = 1.45f; // 總跳躍時間
        Vector3 start = transform.position;
        targetPos = targetPos - this.transform.forward.normalized * 0.25f; // 向後移動 0.15f
        float jumpHeight = (targetPos - start).y - 0.3f; // 跳躍高度
        targetPos.y = start.y; // 目標位置的 y 值設置為起始位置的 y 值
        
        // 在地上停0.5f
        this.animator.Play("idle");
        yield return new WaitForSeconds(1.5f);
        // 播放跳躍動畫
        this.animator.Play("playTeaser"); // 確保你有一個叫 "jump" 的動畫

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            if (t > 0.5f) {
                this.favorSystem.AddFavor(this.catName, 3);
                this.achieveSystem.UpdateProgress("favor_up", 3);
                love.SetActive(true);
                loveTime = Time.time;
            }
            
            // 使用拋物線軌跡
            Vector3 position = Vector3.Lerp(start, targetPos, t);
            position.y += jumpHeight * Mathf.Sin(Mathf.PI * t); // 拋物線

            transform.position = position;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        Debug.Log("Jumping to target position finished.");
        isJumping = false;
        this.SetJumpable(false);
    }

    public void AttackUser() {
        if (user != null)
        {
            Vector3 targetPosition = user.transform.position;
            Vector3 direction = (targetPosition - transform.position);
            // check distance between cat and user is less than 0.2f
            if (direction.magnitude < 0.2f)
            {
                this.favorSystem.AddFavor(this.catName, -2); 
                this.achieveSystem.UpdateProgress("favor_down", 2);
            }
        }
    }
    public GameObject FindFood() { 
        // find the food position in the scene, and return the position
        var allFoods = Resources.FindObjectsOfTypeAll<CatFood>();
        // find food's with the same type as the cat's favorite food, if two or more, return random one

        int length = allFoods.Length;
        if (length > 0)
        {
            List<CatFood> Foods = new List<CatFood>();
            foreach (var food in allFoods)
            {
                if (food.foodType != 0 &&
                    food.gameObject.scene.IsValid() &&
                    food.gameObject.activeInHierarchy)
                {
                    Foods.Add(food);
                }
            }
            Debug.Log("foods length: " + Foods.Count);
            if (Foods.Count > 0)
            {
                return Foods[Random.Range(0, Foods.Count)].gameObject;
            }
        }
        return null;
    }
    public GameObject FindFavorFood() { 
        // find the food position in the scene, and return the position
        var allFoods = Resources.FindObjectsOfTypeAll<CatFood>();
        // find food's with the same type as the cat's favorite food, if two or more, return random one

        int length = allFoods.Length;
        if (length > 0)
        {
            List<CatFood> favorFoods = new List<CatFood>();
            foreach (var food in allFoods)
            {
                if (food.foodType == this.favorateFood &&
                    food.gameObject.scene.IsValid() &&
                    food.gameObject.activeInHierarchy)
                {
                    favorFoods.Add(food);
                }
            }
            Debug.Log("favor foods length: " + favorFoods.Count);
            if (favorFoods.Count > 0)
            {
                return favorFoods[Random.Range(0, favorFoods.Count)].gameObject;
            }
        }
        return null;
    }
    // find box position
    public GameObject FindBox()
    {
        var boxes = Resources.FindObjectsOfTypeAll<Box>();

        List<Box> validBoxes = new List<Box>();

        foreach (var box in boxes)
        {
            if (box.gameObject.scene.IsValid() && box.gameObject.activeInHierarchy)
            {
                validBoxes.Add(box);
            }
        }

        int count = validBoxes.Count;

        if (count > 0)
        {
            return validBoxes[Random.Range(0, count)].gameObject;
        }

        Debug.Log("No active box found in the scene.");
        return null;
    }

    public GameObject FindCatTower()
    {
        var allCTs = Resources.FindObjectsOfTypeAll<CatTower>();
        List<CatTower> activeCTs = new List<CatTower>();

        foreach (var ct in allCTs)
        {
            if (ct.gameObject.scene.IsValid() && ct.gameObject.activeInHierarchy)
            {
                activeCTs.Add(ct);
            }
        }

        int count = activeCTs.Count;
        if (count > 0)
        {
            return activeCTs[Random.Range(0, count)].gameObject;
        }

        Debug.Log("No active CatTower found in the scene.");
        return null;
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
