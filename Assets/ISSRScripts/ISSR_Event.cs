using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
//  File: ISSR_Event.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//    Definition of on Event in the game, they are received by the agents internally,
//     later an event handler is called according to the type of event.
//     It has a type of event and some optional information 
// ----------------------------------------------------------------------------------------


public enum ISSREventType
{
	// Special events
	Undefined,
	onMsgArrived,   // This event type does not use the ISSR_Event class, arrives in the list of incomming messages
	onTickElapsed,  // This event type does not use the ISSR_Event class, can be used in the corroutine for taking decisions 

	// Events generated internally and enqueued in event list.
	onEnterSensingArea,  // An ISSR_Object Enters the sensing area of the agent, becomes 'sensable'
	onExitSensingArea,   // An ISSR_Object Exits the sensing area of the agent or disappears, it is no more 'sensable'

	onGripSuccess,  // This agent has just gripped an object
	onGripFailure,  // This agent was trying to grip an object and failed  (object out of sensing area or already gripped)
	onObjectLost,   // This agent was trying to reach an object and failed,either after acGotoObject() or acGripObject()
	onDestArrived,  // This agent was moving to a destination and it has just arrived
	onCollision,	// This agent collided with another object 
	onGObjectCollision, // This agent is gripping an object and it has collided
    onManyCollisions,  // The agent is continuously colliding, listen to avoid deadlock
	onStop,			// This agent has stopped   (whenever the agent stops)
	onStartsMoving, // This agent has just started moving  (whenever the agent starts moving)
	onGObjectScored,  // This agent is gripping an object and it scored (Goal to Stone collision)
	onUngrip,       // This object was gripping an object and it is not anymore
	onAnotherAgentGripped,	// This agent is gripping a big stone, another agent has just GRIPPED IT
	onAnotherAgentUngripped,	// This agent is gripping a big stone, another agent has just UNGRIPPED IT
	onPushTimeOut,  // This agent is gripping a big stone and requested to move (push) 
	// but ISSR_BStoneBehaviour.MaxWaitTime elapsed and no matching pushing agents appeared, so it timeout
	//  this is an alternative to onStartsMoving that will be sent if success
    onTimerOut   // The agent set the timer and the period has elapsed
}


// Some situations .................................
// Agent moving  collides:    onCollision() + onStop()
// Agent stopped gripping an object but something collides with it onCollision() + onUngrip()
// Agent performed acGotoLocation() and arrived successfully to destination:   onDestArrived() + onStop()
// Agent performed acGotoObject() and object disappeared from sensing area: onObjectLost() + onStop()
// Agent performed acGripObject() and object disappeared from sensing area: onObjectLost() + onGripFailure() + onStop()
// Agent performed acGripObject() arrived at object but grip failed for example because now another agent is gripping it:
//   onGripFailure() + onStop()
// Agent pushing a stone makes it arrive to goal: onUngrip() + onGObjectScored() + onStop()
// Agent pushing a goal makes it reach a stone: onGObjectScored()   (no stop no ungrip)
// Agent grips another agent and begins to move: the gripped agent gets onStartMoving() without having called anything
// If trying to move either by acGotoLocation(), acGotoObject() or acGripObject() there is success if onStartMoving() raises

[System.Serializable]
public class ISSR_Event {

	public ISSREventType Type; 	// Event type
	// TODO add addition field for refining event information Cause or similar

	public ISSR_Object      Obj;		// Optional Object
    public float            f;          // Optional floating point number               


	/// <summary>
	/// Initializes a new instance of the <see cref="ISSR_Event"/> class.
	///   with the given parameters
	/// </summary>
	/// <param name="type">Type.</param>
	/// <param name="obj">Object.</param>
	public ISSR_Event(ISSREventType type, ISSR_Object obj)
	{
		this.Type = type;
		if (obj != null)
		{
			this.Obj = new ISSR_Object (obj);
		} else
		{
			this.Obj = null;
		}
			
	}


	/// <summary>
	/// Initializes a new empty instance of the <see cref="ISSR_Event"/> class.
	/// </summary>
	public ISSR_Event()
	{
		this.Obj = new ISSR_Object ();
	}
}