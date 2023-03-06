using FSMs;
using UnityEngine;
using Steerings;

[CreateAssetMenu(fileName = "FSM_Mouse", menuName = "Finite State Machines/FSM_Mouse", order = 1)]
public class FSM_Mouse : FiniteStateMachine
{
    PathFollowing pathFollowing;
    GoToTarget goToTarget;
    MOUSE_Blackboard blackboard;
    SteeringContext context;

    public override void OnEnter()
    {
        pathFollowing = GetComponent<PathFollowing>();
        goToTarget = GetComponent<GoToTarget>();
        blackboard = GetComponent<MOUSE_Blackboard>();
        context = GetComponent<SteeringContext>();
        goToTarget.enabled = true;
        base.OnEnter(); // do not remove
    }

    public override void OnExit()
    {
        goToTarget.enabled = false;
        DisableAllSteerings();
        base.OnExit();
    }

    public override void OnConstruction()
    {
        State needToPoo = new State("NeedToPoo",
            () =>
            {
                blackboard.target = blackboard.GenerateMarker();
                goToTarget.target = blackboard.target;
                goToTarget.enabled = true;
            },
            () => { },
            () =>
            {
                goToTarget.enabled = false;
                goToTarget.target = null;
                blackboard.DestroyMarker();
            });
        State goHome = new State("GoingHome",
            () =>
            {
                blackboard.target = blackboard.RandomExitPoint();
                goToTarget.target = blackboard.target;
                goToTarget.enabled = true;
            },
            () => { if (goToTarget.routeTerminated())
                    Destroy(gameObject); },
            () =>
            {
                goToTarget.enabled = false;
            });
        State runAway = new State("RunningAway",
           () =>
           {
               context.maxAcceleration *= 4;
               context.maxSpeed *= 2;
               blackboard.target = blackboard.NearestExitPoint();
               goToTarget.target = blackboard.target;
               goToTarget.enabled = true;
           },
           () => { if (goToTarget.routeTerminated())
                   Destroy(gameObject); },
           () =>
           {
               context.maxAcceleration /= 4;
               context.maxSpeed /= 2;
               goToTarget.enabled = false;
           });

        Transition poo = new Transition("poo",
            () =>{ return goToTarget.routeTerminated(); },
            () =>{ blackboard.GeneratePoo();});

        Transition flee = new Transition("flee",
            () => { return SensingUtils.FindInstanceWithinRadius(gameObject, "ROOMBA", blackboard.roombaDetectionRadius); },
            () => { });

        AddStates(needToPoo, goHome, runAway);
        AddTransition(needToPoo, flee, runAway);
        AddTransition(needToPoo, poo, goHome);
        AddTransition(goHome, flee, runAway);
        initialState = needToPoo;

    }
}
