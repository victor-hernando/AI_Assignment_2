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
    private GameObject closerPoo;

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
        /* STAGE 1: create the states with their logic(s)
         *-----------------------------------------------*/

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
            }
        );

        State GoingToPoo = new State("GoingToPoo",
            () => {
                steeringContext.maxSpeed *= 1.3f;
                steeringContext.maxAcceleration *= 2.6f;
                goToTarget.enabled = true;
                goToTarget.target = poo;
            },
            () => { 
                GameObject dustDetected = SensingUtils.FindInstanceWithinRadius(gameObject, "DUST", blackboard.dustDetectionRadius);
                if (dustDetected != null) blackboard.AddToMemory(dustDetected);
            },
            () => {
                steeringContext.maxSpeed /= 1.3f;
                steeringContext.maxAcceleration /= 2.6f;
                goToTarget.enabled = false;                
            }
        );

        /* STAGE 2: create the transitions with their logic(s)
         * ---------------------------------------------------*/

        Transition WaypointReached = new Transition("WaypointReached",
            () => { return goToTarget.routeTerminated(); },
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
            () => { return goToTarget.routeTerminated(); },
            () => { Destroy(dust); }
        );

        Transition DustReachedWithMemory = new Transition("DustReached",
            () => { return goToTarget.routeTerminated() && blackboard.somethingInMemory(); },
            () => {
                Destroy(dust);
                dust = blackboard.RetrieveFromMemory();
            }
        );

        Transition PooDetected = new Transition("PooDetected",
            () => {
                poo = SensingUtils.FindInstanceWithinRadius(gameObject, "POO", blackboard.pooDetectionRadius);
                return poo != null;
            },
            () => { }
        );

        Transition CloserPooDetected = new Transition("CloserPooDetected",
            () => {
                closerPoo = SensingUtils.FindInstanceWithinRadius(gameObject, "POO", blackboard.pooDetectionRadius);
                return poo != closerPoo;
            },
            () => { poo = closerPoo; }
        );

        Transition PooReached = new Transition("PooReached",
            () => { return goToTarget.routeTerminated(); },
            () => { Destroy(poo); }
        );

        Transition PooReachedDustInMemory = new Transition("PooReachedDustInMemory",
            () => { return goToTarget.routeTerminated() && blackboard.somethingInMemory(); },
            () => { 
                Destroy(poo);
                dust = blackboard.RetrieveFromMemory();
            }
        );

        /* STAGE 3: add states and transitions to the FSM 
         * ----------------------------------------------*/

        AddStates(Patrolling, GoingToDust, GoingToPoo);

        AddTransition(Patrolling, WaypointReached, Patrolling);
        AddTransition(Patrolling, PooDetected, GoingToPoo);
        AddTransition(GoingToPoo, CloserPooDetected, GoingToPoo);
        AddTransition(GoingToPoo, PooReachedDustInMemory, GoingToDust);
        AddTransition(GoingToPoo, PooReached, Patrolling);
        AddTransition(Patrolling, DustDetected, GoingToDust);
        AddTransition(GoingToDust, DustReachedWithMemory, GoingToDust);
        AddTransition(GoingToDust, DustReached, Patrolling);
        AddTransition(GoingToDust, PooDetected, GoingToPoo);

        initialState = Patrolling;
    }

}
