using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ----------------------------------------------------------------------------------------
//  File: ISSR_Agent.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//     Services for any agent:   Base class from which their behaviour is derived
// ----------------------------------------------------------------------------------------

[System.Serializable]
public class ISSR_Agent
{
	

	#region Public Fields 
	// --------------PUBLIC FIELDS-------------------------------------------
	[Header("Public READ ONLY Fields")]
	[Tooltip("The object that represents this agent")]
	public ISSR_Object 			Myself;

	[Tooltip("Updated by the API with Grip and Ungrip")]
	public ISSR_Object 			GrippedObject;

	[Tooltip("Updated by the API independently of onEnterSensingArea() and onExitSensingArea()")]
	public List<ISSR_Object> SensableObjects;

	[Tooltip("The API fills this with the last event happened   enum ISSREventType")]
	public ISSREventType 	current_event;  // The API fills this with the last event happened   enum ISSREventType

    [Tooltip("The API fills this with the location the agent wants to move to")]
    public Vector3 dest_location; 

    [Tooltip("The API fills this with the last arrived message user code")]
    public int              user_msg_code;
    [Tooltip("The API fills this with object contained in the last arrived message")]
    public ISSR_Object      msg_obj;
    [Tooltip("The API fills this with the location contained in the last arrived message")]
    public Vector3          msg_location;


    [Header("Convenience READ/WRITE Public Fields ")]

	public ISSRState 		current_state;  // Free to use
	public ISSRState		last_state;
	public Vector3 			 focus_location;  
	public ISSR_Object 		 focus_object;
	public ISSR_Object 		 focus_agent;
    public ISSR_Object       colliding_object;
    public ISSR_Object       object_just_seen;
    public float             focus_time;
	public List<ISSR_Object> Stones;
	public List<ISSR_Object> Agents;
	public List<ISSR_Object> Objects;
	public List<Vector3> 	 Locations;

	public List<ISSR_Object> Valid_Small_Stones;
	public List<ISSR_Object> Invalid_Small_Stones;
	public List<ISSR_Object> Valid_Big_Stones;
	public List<ISSR_Object> Invalid_Big_Stones;
	public List<Vector3> 	 Valid_Locations;
	public List<Vector3>     Invalid_Locations;


    public ISSR_Object debug_object;




    //public ISSR_Message msg;
    // --------------END of PUBLIC FIELDS------------------------------------
    #endregion


    #region Mandatory Methods

    // Student's code has to implement Start() an Update() methods and as much event handlers as his/her code needs
    //  Here all these functions are present with the virtual keyword. 
    //  Student's code has to write his/her versions  with the same interface  but  using override instead of virtual.

    /// <summary>
    /// This method is called ONCE in the agent by the API before anyother function. 
    ///   It can be used for initializations.
    /// </summary>
    public virtual void Start ()
	{
		
		Debug.LogWarning ("A Start() method is not defined for this Agent Behaviour");
	}


	/// <summary>
	///  This method is a COROUTINE:  it should perform some PERIODIC TASK and return control to the system by
	///     yield return null (inmediately)  or yield return new WaitForSeconds(float time); (waiting time seconds)
	/// </summary>
	public virtual IEnumerator Update ()
	{
		Debug.LogError ("An Update() coroutine is not defined for this Agent Behaviour");
		yield return null;
	}


	#endregion 


	// EVENT HANDLERS-------------------------------------------
	// -------------all virtual functions-------------------------
	#region EVENT HANDLERS
	 
	// Student's code should provide its own implementations of these functions
	//   the are event handlers, this is : they are called by the API when something happens

	/// <summary>
	///  This method is called by the API when an object enters the agent's sensing area, 
	///   the agent BEGINS to 'SEE' the object
	///   it is closer in distance than iSensingRange() to the agent.
	///  The API also ADDs the object to the List SensableObjects
	/// </summary>
	/// <param name="obj">Object that is now 'sensable'.</param>
	public virtual void onEnterSensingArea( ISSR_Object obj)
	{
		
	}

	/// <summary>
	///  This method is called by the API when an object exits the agent's sensing area, 
	///   the agent DOES NOT'SEE' the object anymore
	///   it gets further than iSensingRange() in distance from the agent or has just disappeared.
	///  The API also REMOVEs the object from the List SensableObjects
	/// </summary>
	/// <param name="obj">Object that is no longer 'sensable'.</param>
	public virtual void onExitSensingArea( ISSR_Object obj)
	{
		
	}

	/// <summary>
	///  This method is called by the API when a MESSAGE is RECEIVED by this agent. 
	///   It must has been sent by another agent closer than iCommsRange(). 
	/// </summary>
	/// <param name="msg">Message received.</param>
	public virtual void onMsgArrived( ISSR_Message msg)
	{
		
	}


	/// <summary>
	///  This method is called by the API when the agent SUCCESSFULLY GRIPS
	///    the object previously requested  by acGripObject()
	///    If the object cannot be gripped   onGripFailure() is called instead
	///    If the agent is gripping an object and looses it onUngrip()  is called instead
	/// </summary>
	/// <param name="obj_gripped">Object successfully gripped.</param>
	public virtual void onGripSuccess(ISSR_Object obj_gripped)
	{
		
	}

	/// <summary>
	/// This method is called by the API when the agent CANNOT GRIP
	///    the object previously requested  by acGripObject()
	///  Possible reasons:  the object is now out of sensing area, or has disappeared
	///   the object has already been  gripped by a different agent
	/// </summary>
	/// <param name="obj_I_wanted_to_grip">Object i wanted to grip and could not.</param>
	public virtual void onGripFailure(ISSR_Object obj_I_wanted_to_grip)
	{
		
	}


	/// <summary>
	/// This method is called by the API when the agent 
	///  CANNOT 'SEE' the OBJECT she is TRYING to REACH
	///  Happens after  acGripObject() or acGotoObject() when the target object
	///   is no longer visible, it may be out of sensing range or has disappeared.
	///  If acGripObject() was the request, the agent will also receive onGripFailure() event.
	/// </summary>
	/// <param name="obj_i_was_looking_for">Object i was trying to get to.</param>
	public virtual void onObjectLost(ISSR_Object obj_i_was_looking_for)
	{
		
	}

	/// <summary>
	/// This method is called by the API when the agent COLLIDES with another object.
	///   Usually if the agent was moving it also stops and receives onStop() event
	///   If the agent was not moving but she was gripping an object, it will also ungrip the object
	///     and receive the onUngrip() event. 
	/// </summary>
	/// <param name="obj_that_collided_with_me">Object with which the agent collided.</param>
	public virtual void onCollision(ISSR_Object obj_that_collided_with_me)
	{
		
	}


	/// <summary>
	/// This method is called by the API when the agent has made a STONE SCORE in a GOAL,
	///   is the agent is carrying that stone, she will also receive onUngrip() and onStop()
	/// </summary>
	/// <param name="stone_that_scored">Stone that scored in the goal.</param>
	public virtual void onGObjectScored(ISSR_Object stone_that_scored)
	{
		
	}


	/// <summary>
	/// This method is called by the API when the agent is gripping an object and that 
	///  GRIPPED OBJECT COLLIDES with something
	///  if the agent was moving it will also stop and receive the onStop() event
	/// </summary>
	/// <param name="obj_that_collided_with_gripped_obj">Object with which collided the object I am carrying</param>
	public virtual void onGObjectCollision(ISSR_Object obj_that_collided_with_gripped_obj)
	{
		
	}

    /// <summary>
    ///  This method is called by the API when the agent is frecuently colliding with other objects
    ///   it can be usefull to avoid deadlocks
    /// </summary>
    public virtual void onManyCollisions() 
    {

    }

    /// <summary>
    /// This method is called by the API whenever the agent WAS MOVING AND HAS STOPPED 
    ///  This can be due to a number of reasons:
    ///   -A call to acStop()
    ///   -A collision of the agent or the object she is carrying 
    ///   -The agent gets to an object and grips it
    ///   -The agent scores with a stone in a goal
    ///   -Some other agents stop will carrying together a bit stone
    ///   -Some other agents are pushing the same big stone as me but the resulting push force
    ///     is very small, probably because those pushes cancel each other. 
    /// </summary>
    public virtual void onStop()
	{
		
	}


	/// <summary>
	/// This method is called by the API whenever the agent WAS STOPPED AND STARTS MOVING
	///  This usually happens after the agent behavior has called acGotoLocation()
	///   acGotoObject() or acGripObject()  and the conditions for moving hold.
	///  Reason why an agent calling one of those actions does not move:
	///    -The agent is gripping a small stone and there is already some stone(s) moving 
	///       there is a maximum of stones moving per team: ISSRManager.MaxStonesMovingPerTeam
	///    -She is gripping a big stone and not enough agents are also trying to move it
	///       in this case she will receive onPushTimeOut after ISSR_BStoneBehaviour.MaxWaitTime seconds 
	///    -For a big stone to move  3*number of pushing agents >= 3 + number of gripping agents
	/// </summary>
	public virtual void onStartsMoving()
	{
		
	}


	/// <summary>
	/// This method is called by the API when the AGENT LOOSES her GRIP of an object,
	///   Posible reasons are:
	///  -Something has collided with the agent when it was not moving
	///  -The agent has called acUngrip()
	///  -The agent has just scored with the stone.
	/// </summary>
	/// <param name="ungripped_object">Ungripped object.</param>
	public virtual void onUngrip(ISSR_Object ungripped_object)
	{
		
	}

	/// <summary>
	/// This method is called by the API when the agent GETS to the LOCATION requested
	///  with acGotoLocation() it will receive also onStop()
	/// </summary>
	public virtual void onDestArrived()
	{
		
	}

	/// <summary>
	/// This method is called by the API when the agent is gripping a BIG STONE and 
	///   SOME OTHER AGENT GRIPS the same big stone
	/// </summary>
	/// <param name="agent">Agent that has just gripped the same big stone I am gripping.</param>
	public virtual void onAnotherAgentGripped(ISSR_Object agent)
	{
		
	}

	/// <summary>
	/// This method is called by the API when the agent is gripping a BIG STONE and 
	///   SOME OTHER AGENT UNGRIPS the same big stone (she was gripping it previously)
	/// </summary>
	/// <param name="agent">Agent that has just ungripped the same big stone I am gripping.</param>
	public virtual void onAnotherAgentUngripped(ISSR_Object agent)
	{
		
	}


	/// <summary>
	/// This method is called by the API when an agent is gripping a big stone and wants to move but
	///  after ISSR_BStoneBehaviour.MaxWaitTime no other agents gripping it have tried to move also.
	///  If before that time another gripping agents try to move (push) onStartsMoving() is received instead.
	/// </summary>
	/// <param name="gripped_big_stone">Gripped big stone.</param>
	public virtual void onPushTimeOut(ISSR_Object gripped_big_stone)
	{
		
	}

    /// <summary>
    /// This method is called by the API after the timer set by the agent has elapsed
    ///   the agent sets the timer with acSetTimer(float delay). Only one timer can be active
    ///   at the same time.
    /// </summary>
    /// <param name="delay">time waited in seconds</param>
    public virtual void onTimerOut(float delay)
    {

    }
	#endregion
	//-------------------------------------------------------------
	// End of EVENT HANDLERS-------------------------------------------








	// SERVICES------------------------------------------------
	//---------------------------------------------------------



	// SPECIAL SERVICES ------------------------------------------
	#region SPECIAL SERVICES

	/// <summary>
	/// Enables or disables the ability for the agent to sense around her. By default it is enabled.
	/// </summary>
	/// <param name="enable">If set to <c>true</c> enables sensing, otherwise sensing is disabled.</param>
	public void EnableSensing(bool enable)
	{
		this.AgentBehaviour.sensing_enabled = enable;
	}

	/// <summary>
	/// Enables or disables the ability for the agent to receive messages from another agents. By default it is enabled.
	/// </summary>
	/// <param name="enable">If set to <c>true</c> enables receiving, otherwise receiving is disabled.</param>
	public void EnableReceiving(bool enable)
	{
		this.AgentBehaviour.receiving_enabled = enable;
	}
	#endregion
	// END of SPECIAL SERVICES -----------------------------------

	// INFO SERVICES------------------------------------------
	#region INFO SERVICES


	/// <summary>
	/// Returns the maximum sensing distance: an agent can "see" objects closer than this.
	/// </summary>
	/// <returns>The sensing range.</returns>
	public float iSensingRange()
	{
		return ISSRManager.SensingRange;
	}

	/// <summary>
	/// Returns the maximum communications distance: an agent can send an receive messages to/from agents closer than this.
	/// </summary>
	/// <returns>The comms range.</returns>
	public float iCommsRange()
	{
		return ISSRManager.CommsRange;
	}


	/// <summary>
	/// Return the number of agents that a team contains in the current game scenario.
	/// </summary>
	/// <returns>The agents per team.</returns>
	public int iAgentsPerTeam()
	{
		return this.ISSRManager.AgentsPerTeam;
	}


	/// <summary>
	/// Returns the type of the game scenario, enum type <see cref="ISSR_GameScenario"/> 
	///  Either ISSR_GameScenario.Cooperative or  ISSR_GameScenario.Competitive
	/// </summary>
	/// <returns>The game scenario type.</returns>
	public ISSR_GameScenario iGameScenarioType()
	{
		return this.ISSRManager.TypeOfGame;
	}

	/// <summary>
	/// Returns the location of the GOAL for my team. Use acGotoLocation() to get to it.
	/// </summary>
	/// <returns>The location of the goal where my team should score stones.</returns>
	public Vector3 iMyGoalLocation()
	{
		return ISSRManager.GoalLocation (Myself.type);
	}

	//// <summary>
	/// Returns the location of the GOAL for the other team. Use acGotoLocation() to get to it.
	/// </summary>
	/// <returns>The location of the goal where my team should score stones.</returns>
	public Vector3 iOtherGoalLocation()
	{
		//TODO check if cooperative scenario and give error.

		if (iGameScenarioType ()== ISSR_GameScenario.Cooperative)
		{
			Debug.LogWarningFormat("INFO: Trying to get the other team's location in scenario with only one team");
			return Vector3.zero;
		}
		switch(Myself.type)
		{
		case ISSR_Type.AgentA:
			return ISSRManager.GoalLocation (ISSR_Type.AgentB);
		case ISSR_Type.AgentB:
			return ISSRManager.GoalLocation (ISSR_Type.AgentA);
		default:
			return Vector3.zero;
		}
	}


	/// <summary>
	///  Return the remaining time to play or -1 if no time restriction
	/// </summary>
	/// <returns>The remaining time.</returns>
	public  float iRemainingTime()
	{
		if (ISSRManager.TimeBudget != 0)
		{
			return (float)ISSRManager.TimeLeft;
		}
		else
		{
			return -1;
		}
	}


	/// <summary>
	/// Returns the maximum number of stones that each team can be moving 
	///  simultaneously, usually only one.  You can check how many stones
	///   is your team moving with iMovingStonesInMyTeam() and in the
	///   other team with iMovingStonesInOtherTeam() in a competitive game.
	/// </summary>
	/// <returns>The max moving stones allowed.</returns>
	public int iMaxMovingStonesAllowed()
	{
		return ISSRManager.MaxStonesMovingPerTeam;
	}

	/// <summary>
	///  Returns the current number of stones that are moving by my team, compare with
	///   iMaxMovingStonesAllowed() to know if a new stone can be moved by my team
	/// </summary>
	/// <returns>The number moving stones in my team.</returns>
	public int iMovingStonesInMyTeam()
	{
		int team;

		if (Myself.type == ISSR_Type.AgentA)
		{
			team = 0;
		}
		else 
		{
			team = 1;
		}

		return ISSRManager.Teams [team].stones_moving;
	}


	/// <summary>
	/// Returns the current number of stones that are moving by the other team, 
	///   only en competitive scenario, compare with iMaxMovingStonesAllowed() 
	///   to know if a new stone can be moved by the other team
	/// </summary>
	/// <returns>The moving stones in other team.</returns>
	public int iMovingStonesInOtherTeam()
	{
		if (iGameScenarioType ()== ISSR_GameScenario.Cooperative)
		{
			Debug.LogWarningFormat("INFO: Trying to get the other team's moving stones in scenario with only one team");
			return 0;
		}
		switch(Myself.type)
		{
		case ISSR_Type.AgentA:
			return ISSRManager.Teams [1].stones_moving;
		case ISSR_Type.AgentB:
			return ISSRManager.Teams [0].stones_moving;
		default:
			return 0;
		}
	}


	/// <summary>
	///   Returns the current score of my team
	/// </summary>
	/// <returns>Current score of my team.</returns>
	public int iMyScore()
	{
		int team;
		if (Myself.type == ISSR_Type.AgentA)
		{
			team = 0;
		}
		else 
		{
			team = 1;
		}

		return ISSRManager.Teams [team].Score;
	}


	/// <summary>
	///  Returns the current score of the other team  in a competitive scenario.
	/// </summary>
	/// <returns>The current score of the other team.</returns>
	public int iOtherScore()
	{
		if (iGameScenarioType ()== ISSR_GameScenario.Cooperative)
		{
			Debug.LogWarningFormat("INFO: Trying to get the other team's score in scenario with only one team");
			return 0;
		}
		switch(Myself.type)
		{
		case ISSR_Type.AgentA:
			return ISSRManager.Teams [1].Score;
		case ISSR_Type.AgentB:
			return ISSRManager.Teams [0].Score;
		default:
			return 0;
		}
	}

	/// <summary>
	/// Returns the gameyard X dimension  West to East.
	/// </summary>
	/// <returns>The gameyard X dimension.</returns>
	public float iGameyardXDim()
	{
		return ISSRManager.GameyardXDim;
	}

	/// <summary>
	/// Returns the gameyard Z dimension  South to North.
	/// </summary>
	/// <returns>The gameyard Z dimension.</returns>
	public float iGameyardZDim()
	{
		return ISSRManager.GameyardZDim;
	}


    /// <summary>
    /// Returns true if the timer has been set and has not finished, false other wise
    /// </summary>
    /// <returns>true if timer running</returns>
    public bool iTimerRunning()
    {
        return AgentBehaviour.timer_running;
    }
        
	#endregion
	// End of INFO SERVICES-----------------------------------


	// OBJECT INFO SERVICES ###################################
	#region OBJECT INFO SERVICES

	/// <summary>
	/// True if error accessing objects
	/// Template for consulting error:
	/// 
	/// if (oiError) Debug.Log(oiError_msg(oiError_code)) or just call
	///  public void oiCheckError()
	/// </summary>
	[HideInInspector]
	public bool oiError;  // True if error accessing objects

	/// <summary>
	/// In case of oiError error code of enum type oiError_codes
	/// 
	/// if (oiError) Debug.Log(oiError_msg(oiError_code))  or call
	///  public void oiCheckError()  after each oiService
	/// </summary>
	[HideInInspector]
	public oiError_codes  oiError_code;  // In case of oiError error code of enum type oiError_codes

	public enum oiError_codes {
		Undefined,
		OutOFSenses,
		OnlyAgentsNumber,
		ObjectLocationSystem,
		OnlyAgentsGrip,
		NoObjectGripped,
		OnlyAgentsDirection,
	}

	private string[] oiError_msgs = {
		"The object is undefined",
		"The object is out of sensing range",
		"Only agents have numbers",
		"System: Undefined type of ISSR object when determining location",
		"Only agents can grip another objects",
		"No object is gripped by Agent, empty hands!",
		"Only agents have a forward direction"
	};

	private string oiActionName = "";

	/// <summary>
	///  Provides a string with a description of the error accessing an Object. 
	///  The error code is a public variable oiError_code of enum type oiError_codes, only assigned when there is an error 
	///   accessing an Object, in that case the public variable bool oiError is true
	/// </summary>
	/// <returns>string with description of error.</returns>
	/// <param name="code">Code.</param>
	public string oiError_msg( oiError_codes code)
	{
		int index;

		index = (int)code;

		if (index < oiError_msgs.Length)
		{
			return oiError_msgs [index];
		}
		else
		{
			return "Unknown error code. Should use oiError_code as parameter";
		}
	}

	/// <summary>
	///  Checks if there has been an error after calling an Object Information service and 
	///   calls Debug.LogError() with its related message
	/// </summary>
	public bool oiCheckError()
	{
		if (oiError) 
		{
			Debug.LogErrorFormat ("{0}@{1}: {2}", Myself.Name, oiActionName, oiError_msg(oiError_code));
			return true;
		}

		return false;
	}

	/// <summary>
	/// Returns true if the given object is sensable "visible" to the agent. It must be within sensing distance. 
	///   Sensable: Capable of being sensed; perceptible, tangible.
	/// </summary>
	/// <returns><c>true</c>, if the object is sensable, <c>false</c> otherwise.</returns>
	/// <param name="Obj">Object.</param>
	public bool oiSensable(ISSR_Object Obj)
	{
		bool sensable;
		//TODO very important Non Active Stones cannot be searched for

		sensable = ISSRManager.ObjectExists (Obj);

		if (sensable)
		{
			sensable = ISSRManager.DistanceFromAgentCenter (this.AgentGameObject, Obj, out oiError)
                <= (this.iSensingRange () + 0.7f); // TODO Revise 

			if ((sensable) && (!oiError))
			{
				Obj.LastLocation = ISSRManager.ISSR_ObjectLocation (this.AgentGameObject, Obj, out oiError);
				Obj.LastLocation.y = 0;
			}
		}

		return sensable;
	}



	/// <summary>
	/// Returns the distance from an object to the agent from her point of view 
	///    the "point of view" only affects to the walls: it returns the distance to the point on the wall 
	///    which is closer to the agent. The wall is a long slender structure.
	/// </summary>
	/// <returns>The distance to this agent.</returns>
	/// <param name="Obj">Object to measure distance to</param>
	public float oiDistanceToMe(ISSR_Object Obj)
	{
		float distance;
		oiActionName = "oiDistanceToMe";
	

		if ((Obj != null) && (Obj.type!= ISSR_Type.Undefined))
		{
			if (oiSensable (Obj))
			{
				distance = ISSRManager.DistanceFromAgentCenter (this.AgentGameObject, Obj, out oiError);

				Obj.LastLocation = ISSRManager.ISSR_ObjectLocation (this.AgentGameObject, Obj, out oiError);

				if (oiError)
				{
					oiError_code = oiError_codes.ObjectLocationSystem;
					return -1;
				}
				return distance; 
			}
			else
			{ // Object not in sensing range
				oiError = true;
				oiError_code = oiError_codes.OutOFSenses;
                debug_object = Obj;
				return -1;
			}
		}
		else
		{// Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return -1;
		}


	}


	/// <summary>
	/// Returns the type of object, enum ISSR_Type. ISSR_Type. 
	/// Can be read even if the object is out of sensing range, 
	/// it is like a "memory" of something the agent saw.
	///   The same as Obj.type but checking if Obj is null
	/// </summary>
	/// <returns>The type of the object.</returns>
	/// <param name="Obj">Object.</param> 
	public ISSR_Type oiType(ISSR_Object Obj)
	{
		oiActionName = "oiType";
		if (Obj != null)
		{
			oiError = false;
			return Obj.type;
		}
		else
		{ // Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return ISSR_Type.Undefined;
		}
	}

	/// <summary>
	///   Returns the location of the center of an object form the point of view of the agent
	///    the "point of view" only affects to the walls: it returns the location of the point on the wall 
	///    which is closer to the agent. The wall is a long slender structure.
	///  if some error occurs Vector3.zero is returned, and the error is signaled in oiError == true and oiError_code.
	///  An object must be in sensing range for the agent to know its location
	/// </summary>
	/// <returns>The current location of the object or Vector3.zero if error .</returns>
	/// <param name="Obj">Object.</param>
	public Vector3 oiLocation(ISSR_Object Obj)
	{
		Vector3 location;
		oiActionName = "oiLocation";

		if ((Obj != null)&&(Obj.type!= ISSR_Type.Undefined))
		{
			if (oiSensable (Obj))
			{
				location=  ISSRManager.ISSR_ObjectLocation (this.AgentGameObject, Obj, out oiError);
				location.y = 0;
				Obj.LastLocation = location;

				if (oiError)
				{
					oiError_code = oiError_codes.ObjectLocationSystem;
					return Vector3.zero;
				}
				return location; 
			}
			else
			{ // Object not in sensing range
				oiError = true;
				oiError_code = oiError_codes.OutOFSenses;
                debug_object = Obj;
				return Vector3.zero;
			}
		}
		else
		{// Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return Vector3.zero;
		}

	}

	/// <summary>
	///   Returns the last location read of the center of an object form the point of view of the agent
	///   It is like a memory of one object seen.
	///    This position is updated when:
	///     *The object gets into sensing range 	onEnterSensingArea()
	///     *The object gets out of sensing range 	onExitSensingArea()
	/// 	*The location is read via oiLocation() 
	/// </summary>
	/// <returns>The last location of the object or Vector3.zero if error .</returns>
	/// <param name="Obj">Object.</param>
	public Vector3 oiLastLocation(ISSR_Object Obj)
	{
		oiActionName = "oiLastLocation";
		if (Obj != null)
		{
			return Obj.LastLocation;
		}
		else
		{// Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return Vector3.zero;
		}
	}

	/// <summary>
	///  Return the number of an agent in a team, can be used for internal identification in the team 
	///   (the one they are showing on their textures)
	///  Can be read even if the agent is out of sensing range, 
	///   it is like a "memory" of something the agent saw.
	///   The same as Agent.index but cheching if Agent is null
	/// </summary>
	/// <returns>The number of an agent or -1 if is out of range or not an agent.</returns>
	/// <param name="Obj">Object.</param>
	public int oiAgentNumber(ISSR_Object Agent)
	{
		oiActionName = "oiAgentNumber";
		if (Agent!= null)
		{
			
			if (Agent.type == ISSR_Type.AgentA || Agent.type == ISSR_Type.AgentB) 
			{
				oiError = false;
				return Agent.index;
			}
			else
			{
				oiError = true;
				oiError_code = oiError_codes.OnlyAgentsNumber;
				return -1;
			}
		}
		else
		{ // Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return -1;
		}
	}



	/// <summary>
	///  Returns the name of the GameObject corresponding to this ISSR object, can be used to identify it the scene
	///   Can be read even if the object is out of sensing range, 
	///   it is like a "memory" of something the agent saw.
	///   The same as Obj.Name but checking if Obj is null
	/// </summary>
	/// <returns>A string with the name of the underlying object.</returns>
	/// <param name="Obj">Object.</param>
	public string oiName(ISSR_Object Obj)
	{
		oiActionName = "oiName";
		if (Obj!= null)
		{
			oiError = false;
			return Obj.Name;
		}
		else
		{ // Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return null;
		}
	}

    /// <summary>
    /// Returns object timestamp. Zero if error.
    /// Can be read even if the object is out of sensing range, 
	///  The same as Obj.TimeStamps but checking if Obj is null
    /// </summary>
    /// <param name="Obj"></param>
    /// <returns>Number of seconds since beginning of game</returns>
    public float oiTimeStamp(ISSR_Object Obj)
    {
        oiActionName = "oiTimeStamp";
        if (Obj != null)
        {
            oiError = false;
            return Obj.TimeStamp;
        }
        else
        { // Object is undefined
            oiError = true;
            oiError_code = oiError_codes.Undefined;
            return 0;  // 
        }
    }

    /// <summary>
    ///  Returns true if the object is a wall
    /// </summary>
    /// <returns><c>true</c>, if is obj is a wall, <c>false</c> otherwise.</returns>
    /// <param name="Obj">Object.</param>
    public bool oiIsAWall(ISSR_Object Obj)
	{
		oiActionName = "oiIsWall";
		if (Obj!= null)
		{
			oiError = false;

			switch(Obj.type)
			{
			case ISSR_Type.NorthWall:
			case ISSR_Type.SouthWall:
			case ISSR_Type.EastWall:
			case ISSR_Type.WestWall:
				return true;
			default:
				return false;
			}
		}
		else
		{ // Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return false;
		}
	}
		


	/// <summary>
	///  Given an agent returns the object it is gripping or null otherwise
	///  To obtain the object gripped by this agent use de variable GrippedObject
	/// </summary>
	/// <returns>The object that is gripped by this agent or null if none.</returns>
	/// <param name="Agent">Agent.</param>
	public ISSR_Object oiAgentGrippedObject(ISSR_Object Agent)
	{
		ISSR_Object gripped_object = new ISSR_Object ();

		oiActionName = "oiAgentGrippedObject";
		if (Agent!= null)
		{

			if (Agent.type == ISSR_Type.AgentA || Agent.type == ISSR_Type.AgentB) 
			{
				

				gripped_object = ISSRManager.GetObjectGrippedByAgent (Agent);

				if (gripped_object != null)
				{
					oiError = false;
					return new ISSR_Object (gripped_object);
				}
				else
				{
					oiError = true;
					oiError_code = oiError_codes.NoObjectGripped;
					return null;
				}

			}
			else
			{
				oiError = true;
				oiError_code = oiError_codes.OnlyAgentsGrip;
				return null;
			}
		}
		else
		{ // Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return null;
		}


	}

	/// <summary>
	/// Given an agent returns true if she is gripping and object, false otherwise
	///  For the object gripped by this agent the variable GrippedObject can be read
	/// </summary>
	/// <returns><c>true</c>, if the agent is gripping same <c>false</c> otherwise.</returns>
	/// <param name="Agent">Agent.</param>
	public bool oiIsAgentGripping(ISSR_Object Agent)
	{
		ISSR_Object gripped_object = new ISSR_Object ();

		oiActionName = "oiIsAgentGripping";
		if (Agent!= null)
		{

			if ((Agent.type == ISSR_Type.AgentA) || (Agent.type == ISSR_Type.AgentB) )
			{
				gripped_object = ISSRManager.GetObjectGrippedByAgent (Agent);
				oiError = false;

				//Debug.Log("oiIsAgentGripping: gripped_object is null");
				if (gripped_object != null)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				oiError = true;
				oiError_code = oiError_codes.OnlyAgentsGrip;
				return false;
			}
		}
		else
		{ // Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return false;
		}

	}


	/// <summary>
	///   Returns the number of agents that are gripping the object.
	///    Only Big Stones can be gripped by more than one agent.
	/// </summary>
	/// <returns>The number of gripping agents.</returns>
	/// <param name="Obj">Object of which I want to know how many agents are gripping it.</param>
	public int oiGrippingAgents(ISSR_Object Obj)
	{
		oiActionName = "oiGrippingAgents";
		if (Obj!= null)
		{

			if (oiSensable (Obj))
			{
				oiError = false;

				switch(Obj.type)
				{
				case ISSR_Type.NorthWall:
				case ISSR_Type.SouthWall:
				case ISSR_Type.EastWall:
				case ISSR_Type.WestWall:
					return 0;
				default:
					return ISSRManager.NGrippingAgents (Obj);
				}
			}

			else
			{ // Object not in sensing range
				oiError = true;
				oiError_code = oiError_codes.OutOFSenses;
				return -1;
			}
		}
		else
		{ // Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return -1;
		}
	}



	/// <summary>
	/// Given an object returns true if the object is moving false otherwise.
	/// The object can be any, agent, stone, etc
	/// </summary>
	/// <returns><c>true</c>, if object moving is moving, <c>false</c> otherwise.</returns>
	/// <param name="Obj">Object.</param>
	public bool oiMoving(ISSR_Object Obj)
	{
		oiActionName = "oiMoving";
		if (Obj!= null)
		{

			if (oiSensable (Obj))
			{
				return ISSRManager.ObjectMoving (Obj);
			}

			else
			{ // Object not in sensing range
				oiError = true;
				oiError_code = oiError_codes.OutOFSenses;
				return false;
			}
		}
		else
		{ // Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
			return false;
		}


	}



	/// <summary>
	///  Given an agent returns the direction it is facing, can be used to predict its movement
	///   or for the current agent to dodge an obstacle
	/// </summary>
	/// <returns>The agent direction: Vector3 in the XZ Plane.</returns>
	/// <param name="Agent">Agent.</param>
	public Vector3 oiAgentDirection(ISSR_Object Agent)
	{
		Vector3 direction = Vector3.zero;

		oiActionName = "oiAgentDirection";
		if (Agent!= null)
		{

			if ((Agent.type == ISSR_Type.AgentA) || (Agent.type == ISSR_Type.AgentB) )
			{
				direction = this.AgentGameObject.transform.TransformDirection (Vector3.forward);
			}
			else
			{
				oiError = true;
				oiError_code = oiError_codes.OnlyAgentsDirection;
			}
		}
		else
		{ // Object is undefined
			oiError = true;
			oiError_code = oiError_codes.Undefined;
		}

		return direction;
	}


	/// <summary>
	///  Tells whether an object is an agent if the same team as me
	/// </summary>
	/// <returns><c>true</c>, if the object is an agent in my team, <c>false</c> otherwise.</returns>
	/// <param name="Obj">Object.</param>
	public bool oiIsAgentInMyTeam(ISSR_Object Obj)
	{
		oiActionName="oiIsAgentInMyTeam";

		if (Obj != null)
		{
			if ( Obj.type == Myself.type)
			{
				return true;
			}
			else
			{
				return false;
			}

		}
		else
		{
			oiError = true;
			oiError_code = oiError_codes.Undefined;
		}
		return false;
	}


	/// <summary>
	///  Tells whether an object is an agent but not from the same team
	/// </summary>
	/// <returns><c>true</c>, if the object is an agent of a different team, <c>false</c> otherwise.</returns>
	/// <param name="Obj">Object.</param>
	public bool oiIsAgentInOtherTeam(ISSR_Object Obj)
	{
		oiActionName="oiIsAgentInOtherTeam";

		if (Obj != null)
		{
			if ( (Obj.type == ISSR_Type.AgentA)||(Obj.type==ISSR_Type.AgentB))
			{
				if (Obj.type!= Myself.type)
				{
					return true;
				}
			}
			else
			{
				return false;
			}

		}
		else
		{
			oiError = true;
			oiError_code = oiError_codes.Undefined;
		}
		return false;
	}


	#endregion
	// End of OBJECT INFO SERVICES ###################################


	// ACTION SERVICES ##########################################
	#region ACTION SERVICES

	/// <summary>
	/// True if error calling actions
	/// Template for consulting error:
	/// 
	/// if (acError) Debug.Log(acError_msg(acError_code))
	/// </summary>
	[HideInInspector]
	public bool acError;  // True if error calling actions

	/// <summary>
	/// In case of acError error code of enum type acError_codes
	/// 
	/// if (acError) Debug.Log(acError_msg(acError_code))  or call acCheckError()
	///  
	/// </summary>
	[HideInInspector]
	public acError_codes  acError_code;  // In case of acError error code of enum type acError_codes

	/// <summary>
	/// Action error codes.
	/// </summary>
	public enum acError_codes {
		Undefined,
		OutOFSenses,
		GOUndefined,
		GripWall,
		GripAlready,
		GripMyselfWTF,
		NoGrip,
		ManyStonesMoving,
		AgentNotMoving,
		AgentPushed,
        TimerRunning
	}

	/// <summary>
	/// The Action error msgs.
	/// </summary>
	private string[] acError_msgs = {
		"The object is undefined",
		"The object is out of sensing range",
		"The GameObject is undefined",
		"An Agent cannot Grip a Wall, no climbing allowed, no way out of the Gameyard!",
		"This Agent is already gripping an object, ungrip first, you are so greedy!",
		"Are you kidding? One agent cannot Grip Herself, do you need a hug?",
		"The agent is not gripping any object",
		"The agent cannot move gripping a stone, her team is already moving the maximun number of allowed stones",
		"The agent is not moving",
		"Hey someone is pushing me, it is not my fault!",
        "A timer is already set, wait for it to finish!"
	};

	private string acActionName="";


	/// <summary>
	///  Provides a string with a description of the error occurred when calling an action
	///  The error code is a public variable acError_code of enum type acError_codes, only assigned when there is an error 
	///   calling some agent action, in that case the public variable bool aiError is true
	/// </summary>
	/// <returns>string with description of error.</returns>
	/// <param name="code">Code.</param>
	public string acError_msg( acError_codes code)
	{
		int index;

		index = (int)code;

		if (index < acError_msgs.Length)
		{
			return acError_msgs [index];
		}
		else
		{
			return "Unknown error code. Should use acError_code as parameter";
		}
	}

	/// <summary>
	///  Checks if there has been an error after calling and Action service and calls Debug.LogError() with 
	///   its related message. Returns true if error, false otherwise
	/// </summary>
	public bool acCheckError()
	{
		if (acError) 
		{
            
			
            if (acActionName == "acGripObject")
            {
                if (debug_object!= null) 
                {
                    Debug.LogErrorFormat("{0}@{1}: {2}\n  Obj: {3} pos {4}, dist {5}",
                    Myself.Name, acActionName, acError_msg(acError_code), debug_object.Name,
                    debug_object.LastLocation, ISSRHelp.Distance_from_object_to_me(this, debug_object));
                }
                else
                {
                    Debug.LogErrorFormat("{0}@{1}: {2}\n  Obj: null",
                   Myself.Name, acActionName, acError_msg(acError_code));
                }
                
            }
            else
            {
                Debug.LogErrorFormat("{0}@{1}: {2}", Myself.Name, acActionName, acError_msg(acError_code));
            }
            return true;
		}

		return false;
	}


	/// <summary>
	///   Sends a message from this agent, all agents within Communications Range will hear it
	/// </summary>
	/// <param name="code">Message Code.</param>
	/// <param name="usercode">Message Usercode.</param>
	/// <param name="location">Location (optional).</param>
	/// <param name="fvalue">Fvalue (optional).</param>
	/// <param name="ivalue">Ivalue (optional).</param>
	public void acSendMsg(ISSRMsgCode code, int usercode, Vector3 location=new Vector3 (), float fvalue =0, int ivalue = 0)
	{
		ISSR_Message msg;
		acActionName = "acSendMsg";

		if (this.AgentBehaviour.debug_actioncalls)
		{
			Debug.LogFormat ("ACTION: {0} by {1} Code {2}:{3} loc:{4}, fvalue:{5}, ivalue:{6}", acActionName, Myself.Name, code, usercode, location, fvalue, ivalue);
		}
		msg = new ISSR_Message (this.Myself, code, usercode, location, new ISSR_Object(), fvalue, ivalue);
		this.AgentBehaviour.SendMsg (msg);
		acError = false;
	}

	/// <summary>
	///   Sends a message from this agent carry an object, all agents within Communications Range will hear it
	/// </summary>
	/// <param name="code">Message Code.</param>
	/// <param name="usercode">Message Usercode.</param>
	/// <param name="Obj">Object to transmit/inform about</param>
	/// <param name="location">Location (optional).</param>
	/// <param name="fvalue">Fvalue (optional).</param>
	/// <param name="ivalue">Ivalue (optional).</param>
	public void acSendMsgObj(ISSRMsgCode code, int usercode, ISSR_Object Obj, Vector3 location=new Vector3 (), float fvalue =0, int ivalue = 0)
	{
		ISSR_Message msg;
		acActionName = "acSendMsgObj";
        acError = false;

        if (this.AgentBehaviour.debug_actioncalls)
		{
			Debug.LogFormat ("ACTION: {0} by {1} Code {2}:{3} Obj:({7}) loc:{4}, fvalue:{5}, ivalue:{6}", 
				acActionName, Myself.Name, code, usercode, location, fvalue, ivalue, (Obj!=null)?oiName(Obj):"UNDEFINED" );
		}
		msg = new ISSR_Message (this.Myself, code, usercode, location, new ISSR_Object(Obj), fvalue, ivalue);
		this.AgentBehaviour.SendMsg (msg);
		acError = false;
	}


	/// <summary>
	///  Tries to move the agent to get to the specified location.
	///    -If the agent is not gripping any object it will move
	///    -If the agent is gripping a small stone it will move pushing it, only if the number
	///       of stones being moved by this team is less than the defined maximum
	///    -If the agent is gripping a big stone it will move if several conditions hold:
	///       more agents are trying to move while pushing the stone and the maximum moving 
	///       stones per team has not been reached. Also if some other agents are gripping the 
	///       stone the movement can take place if enough of them are 'pushing' and their pushing
	///       directions do not cancel
	/// </summary>
	/// <param name="location">Location to go to.</param>
	public void acGotoLocation(Vector3 location)
	{
		acActionName = "acGotoLocation";
		acError = false;

		if (this.AgentBehaviour.debug_actioncalls)
		{
			Debug.LogFormat ("ACTION: {0} by {1} location:{2}", acActionName, Myself.Name, location);
		}

		if (this.AgentBehaviour.TestMove ())
		{
			this.AgentBehaviour.SetDestination (location);
		}
		else
		{
			acError = true;
			acError_code = acError_codes.ManyStonesMoving;
		}
	}

	/// <summary>
	///  Tries to move the agent to get to an object
	///    If the agent is gripping a stone it will move only if 
	///    other conditions hold: no other stones moving, number of agents...
	///    With no change the agent will collide with the destination object
	/// </summary>
	/// <param name="obj_to_reach">Object to reach.</param>
	public void acGotoObject( ISSR_Object obj_to_reach)
	{
		acActionName = "acGotoObject";
        acError = false;

        if (this.AgentBehaviour.debug_actioncalls)
		{
			Debug.LogFormat ("ACTION: {0} by {1} Object to reach:({2})", acActionName, Myself.Name, (obj_to_reach!=null) ? oiName(obj_to_reach):"UNDEFINED");
		}

		if (obj_to_reach!= null)
		{
			if (oiSensable (obj_to_reach)) 
			{
				acError = false;

				if (this.AgentBehaviour.TestMove ())
				{
					this.AgentBehaviour.SetDestination (obj_to_reach, false, out acError);
					if (acError)
					{
						acError_code = acError_codes.GOUndefined;
					}
				}
				else
				{
					acError = true;
					acError_code = acError_codes.ManyStonesMoving;
				}
		

			}
			else
			{
				acError = true;
				acError_code = acError_codes.OutOFSenses;
			}
		}
		else
		{ // Object is undefined
			acError = true;
			acError_code = acError_codes.Undefined;
		}
	}

	/// <summary>
	///  The agent will try to get to the given object and grip it 
	///   only an agent not gripping any object can call this function
	///   if the agent is already gripping and object it will give an error.
	/// </summary>
	/// <param name="obj_to_grip">Object to grip.</param>
	public void acGripObject(ISSR_Object obj_to_grip)
	{
		acActionName = "acGripObject";
        acError = false;

        if (this.AgentBehaviour.debug_actioncalls)
		{
			Debug.LogFormat ("ACTION: {0} by {1} Object to grip:({2})", acActionName, Myself.Name, (obj_to_grip!=null) ? oiName(obj_to_grip):"UNDEFINED");
		}


		if (obj_to_grip!= null)
		{  
			if (oiSensable (obj_to_grip)) 
			{

				if (!oiIsAWall(obj_to_grip))
				{
					if (!oiIsAgentGripping (Myself))
					{
						// TODO check that an agent is not trying to grip herself

						if ( !obj_to_grip.Equals (Myself))
						{
							acError = false;
							this.AgentBehaviour.SetDestination (obj_to_grip, true, out acError);
							if (acError)
							{
								acError_code = acError_codes.GOUndefined;
							}
						}
						else
						{
							acError = true;
							acError_code = acError_codes.GripMyselfWTF;
						}

					}
					else
					{
						acError = true;
						acError_code = acError_codes.GripAlready;
					}


				}
				else
				{
					acError = true;
					acError_code = acError_codes.GripWall;
				}

			}
			else
			{
				acError = true;
				acError_code = acError_codes.OutOFSenses;
                debug_object = obj_to_grip;
                //Debug.LogFormat("Object: {0}: location {1}", obj_to_grip.Name, obj_to_grip.LastLocation);
			}
		}
		else
		{ // Object is undefined
			acError = true;
			acError_code = acError_codes.Undefined;
		}
	}

	/// <summary>
	/// If the agent is moving it will stop.
	/// </summary>
	public void acStop()
	{
		acActionName = "acStop";
        acError = false;

        if (this.AgentBehaviour.debug_actioncalls)
		{
			Debug.LogFormat ("ACTION: {0} by {1}", acActionName, Myself.Name);
		}

		if (oiMoving (Myself))
		{
			if (this.AgentBehaviour.AcStopMovement ())
			{
				acError = false;
			}
			else
			{
				acError = true;
				acError_code = acError_codes.AgentPushed;
			}
		}
		else
		{
			acError = false;  
			acError_code = acError_codes.AgentNotMoving;
		}
	}


	/// <summary>
	///  If the agent is gripping an object it will ungrip it.
	/// </summary>
	public void acUngrip()
	{
		acActionName = "acUngrip";
        acError = false;

        if (this.AgentBehaviour.debug_actioncalls)
		{
			Debug.LogFormat ("ACTION: {0} by {1}", acActionName, Myself.Name);
		}

		if (oiIsAgentGripping (Myself)) 
		{
			acError = false;
			this.AgentBehaviour.PerformUngrip ();
		}
		else
		{
			acError = true;
			acError_code = acError_codes.NoGrip;
		}

	}


    /// <summary>
    /// Sets the agente timer to run for delay seconds.
    /// If the timer is already running it will give an error.
    /// </summary>
    /// <param name="delay"></param>
    public void acSetTimer(float delay)
    {
        acActionName = "acSetTimer";
        acError = false;
        if (this.AgentBehaviour.debug_actioncalls)
        {
            Debug.LogFormat("ACTION: {0} by {1}", acActionName, Myself.Name);
        }

        if (!this.AgentBehaviour.timer_running)
        {
            this.AgentBehaviour.ACSetTimer(delay);
        }
        else
        {
            acError = true;
            acError_code = acError_codes.TimerRunning;
        }

    }

	#endregion
	// End of ACTION SERVICES ##########################################

	// End of SERVICES-----------------------------------------






	#region Internal Methods
	// Other internal methods --------------------------------------------------------------------------

	/// <summary>
	/// Initializes a new instance of the <see cref="ISSR_Agent"/> class.
	/// Should be called in the derived class 
	/// </summary>
	public ISSR_Agent() : base()
	{
		//this.Manager  = GameObject.Find ("ISSR_Manager").GetComponent<ISSR_ManagerBehaviour>();
		this.initializing = true;
	}

	/// <summary>
	/// Internal initialization method. Should not be used by the student
	/// </summary>
	/// <param name="AgentGO">Agent GameObject</param>
	/// <param name="Behaviour">Main Behaviour of Agent</param>
	/// <param name="OwnId">Own identifier</param>
	public void BootStrap(ISSR_ManagerBehaviour Manager, GameObject AgentGO, ISSR_AgentBehaviour Behaviour, ISSR_Object OwnObj)
	{
		if (this.initializing)
		{
			this.ISSRManager = Manager;
			this.AgentGameObject = AgentGO;
			this.AgentBehaviour = Behaviour;
			this.Myself = new ISSR_Object(OwnObj);
			this.Myself.LastLocation = this.AgentGameObject.transform.position;

			this.initializing = false;
		}
		return;
	}
	// End of Other internal methods --------------------------------------------------------------------------
	#endregion


	#region Private Fields
	private bool 					initializing;
	private GameObject 				AgentGameObject;
	private ISSR_AgentBehaviour 	AgentBehaviour;

	private ISSR_ManagerBehaviour 	ISSRManager;


	#endregion














}
