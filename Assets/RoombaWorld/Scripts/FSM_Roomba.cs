using FSMs;
using UnityEngine;
using Steerings;

[CreateAssetMenu(fileName = "FSM_Roomba", menuName = "Finite State Machines/FSM_Roomba", order = 1)]
public class FSM_Roomba : FiniteStateMachine
{
    private ROOMBA_Blackboard blackboard;
    private GoToTarget goToTarget;
    private GameObject waypoint;
    public GameObject dust;

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
                dust = null;
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

        Transition DustInMemory = new Transition("DustReached",
            () => { return blackboard.somethingInMemory(); },
            () => { dust = blackboard.RetrieveFromMemory(); }
        );

        /* STAGE 3: add states and transitions to the FSM 
         * ----------------------------------------------*/

        AddStates(Patrolling, GoingToDust);

        AddTransition(Patrolling, DustInMemory, GoingToDust);
        AddTransition(Patrolling, DustDetected, GoingToDust);
        AddTransition(Patrolling, WaypointReached, Patrolling);
        AddTransition(GoingToDust, DustReached, Patrolling);

        initialState = Patrolling;
    }

}
