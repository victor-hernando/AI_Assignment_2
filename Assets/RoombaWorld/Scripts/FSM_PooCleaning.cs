using FSMs;
using UnityEngine;
using Steerings;

[CreateAssetMenu(fileName = "FSM_PooCleaning", menuName = "Finite State Machines/FSM_PooCleaning", order = 1)]
public class FSM_PooCleaning : FiniteStateMachine
{
    private ROOMBA_Blackboard blackboard;
    private SteeringContext steeringContext;
    private GoToTarget goToTarget;
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

        FSM_Roomba CleanDust = ScriptableObject.CreateInstance<FSM_Roomba>();
        CleanDust.Name = "CLEANDUST";

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

        Transition PooDetected = new Transition("PooDetected",
            () => {
                poo = SensingUtils.FindInstanceWithinRadius(gameObject, "POO", blackboard.pooDetectionRadius);
                return poo != null;
            },
            () => { if (CleanDust.dust != null) blackboard.AddToMemory(CleanDust.dust); }
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

        /* STAGE 3: add states and transitions to the FSM 
         * ----------------------------------------------*/

        AddStates(CleanDust, GoingToPoo);

        AddTransition(CleanDust, PooDetected, GoingToPoo);
        AddTransition(GoingToPoo, CloserPooDetected, GoingToPoo);
        AddTransition(GoingToPoo, PooReached, CleanDust);

        initialState = CleanDust;

    }
}
