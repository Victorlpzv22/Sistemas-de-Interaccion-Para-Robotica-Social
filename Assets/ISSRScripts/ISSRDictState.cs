using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ----------------------------------------------------------------------------------------
//  File: ISSRDictState.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//    Enumerator for representing a state of the agent
// ----------------------------------------------------------------------------------------

public enum ISSRState
{
	Idle,
	GoingToGoal,
	GoingToGripSmallStone,
	GoingToGoalWithSmallStone,
	AvoidingObstacle,
	WaitforNoStonesMoving,
	GoingToGripBigStone,
	WaitingForHelpToMoveBigStone,
	GoingToGoalWithBigStone,
	Scouting,
	GoingToMeetingPoint,
	SearchingForPartners,
    WaitingForPartners,
    SleepingAfterCollisions,
    GettingOutOfTheWay,
    GoingAway,
    Black,
    Blue,
    Red,
    Green,
    Yellow,
    White,
    WaitforNoStonesMovingBigStone,
    Error,
    End
}