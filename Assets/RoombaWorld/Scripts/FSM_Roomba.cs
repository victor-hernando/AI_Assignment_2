using FSMs;
using UnityEngine;
using Steerings;

[CreateAssetMenu(fileName = "FSM_Roomba", menuName = "Finite State Machines/FSM_Roomba", order = 1)]
public class FSM_Roomba : FiniteStateMachine
{
    private ROOMBA_Blackboard blackboard;
    private SteeringContext steeringContext;
    private GoToTarget goToTarget;
    private GameObject waypoint;
    private GameObject dust;
    private GameObject poo;

    public override void OnEnter()
    {
        blackboard = GetComponent<ROOMBA_Blackboard>();
        goToTarget = GetComponent<GoToTarget>();
        steeringContext = GetComponent<SteeringContext>();
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
                steeringContext.maxSpeed *= 1.3f;
                steeringContext.maxAcceleration *= 2.6f;
                goToTarget.enabled = true;
                goToTarget.target = poo;
            },
            () => { },
            () => {
                steeringContext.maxSpeed /= 1.3f;
                steeringContext.maxAcceleration /= 2.6f;
                goToTarget.enabled = false;
                Destroy(poo);
            }
        );

        //TRANSITIONS
        Transition WaypointReached = new Transition("WaypointReached",
            () => {
                return goToTarget.routeTerminated();
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

        Transition DustReached = new Transition("DustReached",
            () => {
                return goToTarget.routeTerminated();
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

        Transition PooReached = new Transition("PooReached",
            () => {
                return goToTarget.routeTerminated();
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
