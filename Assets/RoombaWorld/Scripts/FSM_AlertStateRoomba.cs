using FSMs;
using UnityEngine;
using Steerings;

[CreateAssetMenu(fileName = "FSM_AlertStateRoomba", menuName = "Finite State Machines/FSM_AlertStateRoomba", order = 1)]
public class FSM_AlertStateRoomba : FiniteStateMachine
{
    private ROOMBA_Blackboard blackboard;

    public override void OnEnter()
    {
        blackboard = GetComponent<ROOMBA_Blackboard>();
        base.OnEnter();
    }

    public override void OnExit()
    {

        base.OnExit();
    }

    public override void OnConstruction()
    {
        /* STAGE 1: create the states with their logic(s)
         *-----------------------------------------------*/

        FiniteStateMachine CLEAN = ScriptableObject.CreateInstance<FSM_Roomba>();
        CLEAN.Name = "CLEAN";

        State GoingToChargingStation = new State("GoingToChargingStation",
            () => { }, 
            () => { },
            () => { }  
        );



        /* STAGE 2: create the transitions with their logic(s)
         * ---------------------------------------------------

        Transition varName = new Transition("TransitionName",
            () => {  }, // write the condition checkeing code in {}
            () => { }  // write the on trigger code in {} if any. Remove line if no on trigger action needed
        );


        /* STAGE 3: add states and transitions to the FSM 
         * ----------------------------------------------
            
        AddStates(...);

        AddTransition(sourceState, transition, destinationState);

        initialState = ... 

        */
    }
}
