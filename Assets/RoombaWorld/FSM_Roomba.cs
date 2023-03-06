using FSMs;
using UnityEngine;
using Steerings;

[CreateAssetMenu(fileName = "FSM_Roomba", menuName = "Finite State Machines/FSM_Roomba", order = 1)]
public class FSM_Roomba : FiniteStateMachine
{
    private ROOMBA_Blackboard blackboard;
    private GoToTarget goToTarget;
    private GameObject waypoint;
    private GameObject dust;
    private GameObject poo;

    public override void OnEnter()
    {
        blackboard = GetComponent<ROOMBA_Blackboard>();
        goToTarget = GetComponent<GoToTarget>();
        base.OnEnter(); 
    }

    public override void OnExit()
    {
        base.DisableAllSteerings();
        base.OnExit();
    }

    public override void OnConstruction()
    {
        //STATES
         
        State Patrolling = new State("Patrolling",
            () => { 
                waypoint = RandomLocationGenerator.RandomPatrolPoint();
                goToTarget.enabled = true;
                goToTarget.target = waypoint;
            }, 
            () => { }, 
            () => { goToTarget.enabled = false; }    
        );

        State GoingToDust = new State("GoingToDust",
            () => {
                goToTarget.enabled = true;
                goToTarget.target = dust;
            },
            () => { },
            () => { 
                goToTarget.enabled = false;
                Destroy(dust);
            }
        );

        State GoingToPoo = new State("GoingToPoo",
            () => {
                goToTarget.enabled = true;
                goToTarget.target = poo;
            },
            () => { },
            () => { 
                goToTarget.enabled = false;
                Destroy(poo);
            }
        );

        //TRANSITIONS
        Transition WaypointReached = new Transition("WaypointReached",
            () => {
                return SensingUtils.DistanceToTarget(gameObject, goToTarget.target) < blackboard.waypointReachedRadius;
            },
            () => { }
        );

        Transition DustDetected = new Transition("DustDetected",
            () => {
                dust = SensingUtils.FindInstanceWithinRadius(gameObject, "DUST", blackboard.dustDetectionRadius);
                return dust != null;
            }, 
            () => { }  
        );

        Transition DustReached = new Transition("DustDetected",
            () => {
                return SensingUtils.DistanceToTarget(gameObject, goToTarget.target) < blackboard.dustReachedRadius;
            },
            () => { }
        );

        Transition PooDetected = new Transition("PooDetected",
            () => {
                poo = SensingUtils.FindInstanceWithinRadius(gameObject, "POO", blackboard.pooDetectionRadius);
                return poo != null;
            },
            () => { }
        );

        Transition PooReached = new Transition("PooDetected",
            () => {
                return SensingUtils.DistanceToTarget(gameObject, goToTarget.target) < blackboard.pooReachedRadius;
            },
            () => { }
        );

        //ADD

        AddStates(Patrolling, GoingToDust, GoingToPoo);

        AddTransition(Patrolling, WaypointReached, Patrolling);
        AddTransition(Patrolling, PooDetected, GoingToPoo);
        AddTransition(GoingToPoo, PooReached, Patrolling);
        AddTransition(Patrolling, DustDetected, GoingToDust);
        AddTransition(GoingToDust, DustReached, Patrolling);
        AddTransition(GoingToDust, PooDetected, GoingToPoo);

        initialState = Patrolling;
    }

}
