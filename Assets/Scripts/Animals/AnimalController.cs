using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AnimalController : MonoBehaviour
{
    //State enum
    public enum MovementState{
        PlayerControl, Idle, Still, Chasing, Exploring, Running
    }

    //private variables
    [SerializeField] private MovementState startingState;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float idleWaitTime;
    [SerializeField] private float maxIdleWalkDist;

    private MovementState currentState;
    private NavMeshAgent agent;
    private float timeUntilMoveIdle;
    private Transform objectToChase;
    private Transform objectToRunFrom;
    
    #region [Start & Update]
    // Start is called before the first frame update
    void Start()
    {
        //Update navmesh agent
        agent = GetComponent<NavMeshAgent>();
		agent.updateRotation = false;
		agent.updateUpAxis = false; 
        currentState = startingState;
        ResetMovementSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        timeUntilMoveIdle -= Time.deltaTime;
    }

    //Handle movement depending on the current movement state
    private void Movement(){
        switch (currentState){
            case MovementState.PlayerControl: //Move to mouse click from player input
                if (Input.GetMouseButtonDown(0)){
                    MoveToMouse();
                }
                break;
            case MovementState.Idle: //Move around randomly
                //Find new random position once timer has ended and the agent doesn't have a destination
                if (!agent.hasPath && timeUntilMoveIdle <= 0f){ 
                    Vector2 randomPos = FindRandomPosition();
                    agent.SetDestination(GameManager.ClosestNavMeshPos(randomPos));
                    timeUntilMoveIdle = idleWaitTime;
                }
                break;
            case MovementState.Still: //Don't move
                agent.SetDestination(transform.position);
                break;
            case MovementState.Chasing: //Move towards a specified object
                agent.SetDestination(objectToChase.position);
                break;
            case MovementState.Exploring: //Search for a specified object
                break;
            case MovementState.Running: //Move away from a specified object
                Vector2 dir = transform.position - objectToRunFrom.position;
                Vector2 runPos = GameManager.ClosestNavMeshPos((Vector2)transform.position + dir);
                agent.SetDestination(runPos);
                break;
        }
    }
    #endregion

    #region [Check Movement & Speed]
    //Get current movement state
    public bool CheckMovementState(MovementState state){
        return currentState == state;
    }

    //Check if agent is moving
    public bool IsMoving(){
        return agent.velocity.magnitude > 0f;
    }

    //Sets movement speed of the navmesh agent
    public void SetMovementSpeed(float speed){
        agent.speed = speed;
    }
    public void ResetMovementSpeed(){
        agent.speed = movementSpeed;
    }
    #endregion

    #region [Movement States]
    //Changes the current movement state
    private void ChangeState(MovementState movementState){
        currentState = movementState;
    }

    //Stops navmesh agent's movement
    public void StopMovement(){
        StopChasing();
        StopRunning();
        ChangeState(MovementState.Still);
    }

    //Player takes control of object
    public void TakePlayerControl(){
        ChangeState(MovementState.PlayerControl);
    }

    //Sets navmesh agent's destination to the mouse position
    private void MoveToMouse(){
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        agent.SetDestination(mouseWorldPosition);
    }

    //Public chase object functions
    public void ChaseObject(Transform obj){
        objectToChase = obj;
        ChangeState(MovementState.Chasing);
    }
    public void StopChasing(){
        if (!CheckMovementState(MovementState.Chasing)) return;
        objectToChase = null;
        ChangeState(startingState);
    }

    //Public run from object functions
    public void RunFrom(Transform obj){
        objectToRunFrom = obj;
        ChangeState(MovementState.Running);
    }
    public void StopRunning(){
        if (!CheckMovementState(MovementState.Running)) return;
        objectToRunFrom = null;
        ChangeState(startingState);
    }
    #endregion

    #region [Other]
    //Finds a random position within the max idle walk distance
    private Vector2 FindRandomPosition(){
        Vector2 randomPoint = Random.insideUnitCircle * maxIdleWalkDist;
        return (Vector2)transform.position + randomPoint;
    }
    #endregion
}
