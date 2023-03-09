using FSMs;
using UnityEngine;
using Steerings;

[CreateAssetMenu(fileName = "FSM_AlertStateRoomba", menuName = "Finite State Machines/FSM_AlertStateRoomba", order = 1)]
public class FSM_AlertStateRoomba : FiniteStateMachine
{
    private ROOMBA_Blackboard blackboard;
    private GoToTarget goToTarget;

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

        FiniteStateMachine Clean = ScriptableObject.CreateInstance<FSM_Roomba>();
        Clean.Name = "CLEAN";

        State GoingToCharge = new State("GoingToCharge",
            () => { 
                goToTarget.enabled = true;
                goToTarget.target = blackboard.GetClosestCharger();
            }, 
            () => { },
            () => { goToTarget.enabled = false; }
        );

        State Charging = new State("Charging",
            () => { },
            () => { blackboard.Recharge(Time.deltaTime); },
            () => { }
        );

        /* STAGE 2: create the transitions with their logic(s)
         * ---------------------------------------------------*/

        Transition LowEnergy = new Transition("LowEnergy",
            () => { return blackboard.currentCharge < blackboard.minCharge; }, 
            () => { }  
        );

        Transition ChargeStationReached = new Transition("ChargeStationReached",
            () => { return goToTarget.routeTerminated(); },
            () => { }
        );

        Transition FullEnergy = new Transition("FullEnergy",
            () => { return blackboard.currentCharge >= blackboard.maxCharge; },
            () => { }
        );


        /* STAGE 3: add states and transitions to the FSM 
         * ----------------------------------------------*/

        AddStates(Clean, GoingToCharge, Charging);

        AddTransition(Clean, LowEnergy, GoingToCharge);
        AddTransition(GoingToCharge, ChargeStationReached, Charging);
        AddTransition(Charging, FullEnergy, Clean);

        initialState = Clean; 

    }
}
