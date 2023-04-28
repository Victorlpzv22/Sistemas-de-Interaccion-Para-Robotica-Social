using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// ----------------------------------------------------------------------------------------
//  File: ISSR_BStoneBehaviour.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//    Behaviour of Big Stone, deals with control of gripping agents, pushing agents and
//     the resulting events and eventually the movement of the stone
// ----------------------------------------------------------------------------------------

[System.Serializable]
class ISSR_AgentPush
{
	public 	ISSR_Object Agent;
	public  GameObject 	AgentGO;
	public 	float 		StartPushTime;
	public 	Vector3 	DesiredDestination;
	public  bool 		timeout;


	public ISSR_AgentPush(ISSR_Object Agent, GameObject AgentGO, Vector3 destination)
	{
		this.Agent = new ISSR_Object (Agent);
		this.AgentGO = AgentGO;
		this.StartPushTime = Time.time;
		this.DesiredDestination = destination;
		Debug.LogFormat ("Push time: {0} for {1}", this.StartPushTime, Agent.Name);
		this.timeout = false;
	}
}


public class ISSR_BStoneBehaviour : MonoBehaviour {

	public bool debug;
	public bool moving = false;
    public bool game_active = true;
	ISSR_ManagerBehaviour ISSRManager;
	public List<GameObject> GrippingAgentsGO; 
	public List<ISSR_Object> GrippingAgents;
	public List<ISSR_AgentBehaviour> GABehaviours;
	public List<Rigidbody> GARigidBodies;
	public Rigidbody rb;

	[SerializeField]
	List<ISSR_AgentPush> CurrentPushes;
	int PushingAgentsA;   // Number of pushing agents of each team, to count moving stones.
	int PushingAgentsB;
	float  MaxWaitTime;   // Duration of push waiting for other agents to cooperate (removed after this time)
	float  PushVectorMinMag; // Minimum magnitude of resulting pushing vector to start moving
	Vector3 AggregatedPushVector;

	void Awake()
	{
		// Reference to ISSRManager
		this.ISSRManager = GameObject.Find ("ISSR_Manager").GetComponent<ISSR_ManagerBehaviour>();
		this.GrippingAgentsGO= new List<GameObject> ();
		this.GrippingAgents = new List<ISSR_Object> ();
		this.GABehaviours = new List<ISSR_AgentBehaviour> ();
		this.GARigidBodies = new List<Rigidbody> ();

		this.CurrentPushes = new List<ISSR_AgentPush> ();
		this.rb = this.gameObject.GetComponent<Rigidbody> ();
	}
	// Use this for initialization
	void Start () 
	{
		this.MaxWaitTime = 1f;
		this.PushVectorMinMag = 0.5f;
		this.PushingAgentsA = 0;
		this.PushingAgentsB = 0;
        this.game_active = true;
	}



	void UpdateLists()
	{
		
		this.GrippingAgents.RemoveRange (0, this.GrippingAgents.Count);
		this.GABehaviours.RemoveRange (0, this.GABehaviours.Count);
		this.GARigidBodies.RemoveRange (0, this.GARigidBodies.Count);

		foreach(GameObject agent in this.GrippingAgentsGO)
		{
			this.GrippingAgents.Add (ISSRManager.GameObject2Object (agent));
			this.GABehaviours.Add (ISSRManager.GetAgentBehaviour (agent));
			this.GARigidBodies.Add (agent.GetComponent <Rigidbody>());
		}
	}

	public void AgentGrips(GameObject agentGO, ISSR_Object agent)
	{
		int i;
		agentGO.transform.parent = this.transform;


		if (!this.GrippingAgentsGO.Contains (agentGO))
		{
			// Inform all other gripping agents
			for (i=0; i < GrippingAgentsGO.Count; i++)
			{


				if (this.GABehaviours [i].debug_grip_ungrip)
				{
					Debug.LogFormat ("GRIP {0} knows {1} gripped {2}", this.GABehaviours [i].AgentDescriptor.Myself.Name, agentGO.name, this.name);
				}
				this.GABehaviours [i].EnqueueEvent (ISSREventType.onAnotherAgentGripped, agentGO);
			}


			this.GrippingAgentsGO.Add (agentGO);
			this.UpdateLists ();
		}



	}

	public void AgentUngrips(GameObject agentGO, ISSR_Object agent)
	{
		int i;
		ISSR_AgentPush found_push;


		if (this.GrippingAgentsGO.Contains (agentGO))
		{
			this.GrippingAgentsGO.Remove (agentGO);
			agentGO.transform.parent = null;

			this.UpdateLists ();
			// Inform all other gripping agents
			for (i=0; i < GrippingAgentsGO.Count; i++)
			{
				
				if (this.GABehaviours [i].debug_grip_ungrip)
				{
					Debug.LogFormat ("GRIP {0} knows {1} ungripped {2}", this.GABehaviours [i].AgentDescriptor.Myself.Name, agentGO.name, this.name);
				}
				this.GABehaviours[i].EnqueueEvent (ISSREventType.onAnotherAgentUngripped, agentGO);
			}

			found_push = FindPush (agent);
			if (found_push != null)
			{
				this.CurrentPushes.Remove (found_push);
				if (agent.type == ISSR_Type.AgentA)
				{
					this.PushingAgentsA--;
				}
				else
				{
					this.PushingAgentsB--;
				}
			}

		}
	}


	public void RemoveMe()
	{
		int i;
		// Things to do for all agents :
		// Ungrip and event of ungrip
		// score event

		if ((PushingAgentsA > 0) &&  (this.moving))
		{
			ISSRManager.Teams [0].stones_moving--;
		}

		if ((PushingAgentsB > 0) && (this.moving))
		{
			ISSRManager.Teams [1].stones_moving--;
		}



		for (i=0; i < this.GrippingAgentsGO.Count; i++)
		{

			this.GABehaviours [i].gripping = false;
			this.GABehaviours [i].SidewardsPush = 0;
			this.GABehaviours [i].FrontBackPush = 0;
			this.GABehaviours [i].locomotion_active = false;
			this.GABehaviours [i].GrippedObject = null;
			this.GABehaviours [i].AgentDescriptor.GrippedObject = null;

			this.GABehaviours [i].loco_state = ISSR_AgentBehaviour.Locomotion.loose;
			this.GrippingAgentsGO [i].transform.parent = null;


			if (this.GABehaviours [i].debug_grip_ungrip)
			{
				Debug.LogFormat ("GRIP {0} ungrips {1}", this.GABehaviours [i].AgentDescriptor.Myself.Name,  this.name);
			}

			this.GABehaviours [i].EnqueueEvent (ISSREventType.onUngrip, this.gameObject);




			if (this.GABehaviours [i].debug_locomotion)
			{
				Debug.LogFormat ("LOCO {0} scored with {1}", this.GABehaviours [i].AgentDescriptor.Myself.Name, this.name);
			}
			this.GABehaviours [i].EnqueueEvent (ISSREventType.onGObjectScored, this.gameObject);

		}

	}

    public void GameEnded()
    {
        this.game_active = false;
        StopAllCoroutines();
    }


	ISSR_AgentPush FindPush(ISSR_Object Agent)
	{
		ISSR_AgentPush found_push= null;

		foreach( ISSR_AgentPush push in CurrentPushes)
		{
			if (push.Agent.Equals(Agent))
			{
				found_push = push;
				break;
			}
		}

		return found_push;
	}


	/// <summary>
	/// Looks for an agent in the gripping agents of this stone
	///  return index into GrippingAgentsGO list or -1 if not faond
	/// </summary>
	/// <returns>Index  gripping agent.</returns>
	/// <param name="Agent">Agent to look for.</param>
	int FindGrippingAgent(GameObject Agent)
	{
		int index = -1;

		foreach( GameObject gripping_agent in this.GrippingAgentsGO)
		{
			if (gripping_agent == Agent)
			{
				index = GrippingAgentsGO.IndexOf (gripping_agent);
				break;
			}
		}

		return index;
	}

	public void AgentPushes(ISSR_Object Agent, GameObject AgentGO,  Vector3 destination)
	{
		ISSR_AgentPush found_push;
		// First check if agent is already pushing 
		found_push = this.FindPush(Agent);

		if (debug)
		{
			Debug.LogFormat ("{0} pushes to {1}", Agent.Name, destination);
		}

		if (this.FindGrippingAgent(AgentGO)!= -1)
		{
			// If stone already moving    update push of agent
			if (this.moving)
			{
				if (found_push!= null)
				{
					found_push.DesiredDestination = destination;
					found_push.StartPushTime = Time.time;
				}
				else
				{
					CurrentPushes.Add (new ISSR_AgentPush (Agent, AgentGO, destination));
					if (Agent.type == ISSR_Type.AgentA)
					{
						this.PushingAgentsA++;
						if (PushingAgentsA==1)
						{
							ISSRManager.Teams [0].stones_moving++;
						}
					}
					else
					{
						this.PushingAgentsB++;
						if (PushingAgentsB==1)
						{
							ISSRManager.Teams [1].stones_moving++;
						}
					}
				}

			}
			else
			{
				if (found_push == null)
				{   // If stone is not moving add push with time stamp only in case the agent did not do that already
					CurrentPushes.Add (new ISSR_AgentPush (Agent, AgentGO, destination));
					if (Agent.type == ISSR_Type.AgentA)
					{
						this.PushingAgentsA++;
					}
					else
					{
						this.PushingAgentsB++;
					}
				}
			}
		}
		else
		{
			Debug.LogFormat ("Agent {0} is not gripping big stone {1}, CANNOT PUSH!!!", AgentGO.name, this.name);
		}




	}
		

	public void AgentStops(ISSR_Object Agent, GameObject AgentGO)
	{
		// remove agent push if any
		ISSR_AgentPush found_push;

		if (this.FindGrippingAgent (AgentGO) != -1) 
		{
			// First check if agent is already pushing 
			found_push = this.FindPush (Agent);

			if (found_push != null)
			{
				CurrentPushes.Remove (found_push);
				if (Agent.type == ISSR_Type.AgentA)
				{
					this.PushingAgentsA--;
					if (PushingAgentsA==0)
					{
						//ISSRManager.Teams [0].stones_moving--;
					}
				}
				else
				{
					this.PushingAgentsB--;
					if (PushingAgentsB==0)
					{
						//ISSRManager.Teams [1].stones_moving--;
					}
				}
			}

		}
		else
		{
			Debug.LogFormat ("Agent {0} is not gripping big stone {1}, it should not inform of its stop", AgentGO.name, this.name);
		}
	}

	void StopBStoneMovement(Collision other)
	{
		

		this.CurrentPushes.RemoveRange (0, CurrentPushes.Count);
		this.rb.isKinematic = true;
		this.rb.velocity = Vector3.zero;

		foreach(Rigidbody rba in GARigidBodies)
		{
			rba.velocity  = Vector3.zero;
		}

		if ((PushingAgentsA > 0) && (this.moving))
		{
			ISSRManager.Teams [0].stones_moving--;
			PushingAgentsA = 0;
		}
		if ((PushingAgentsB > 0) && (this.moving))
		{
			ISSRManager.Teams [1].stones_moving--;
			PushingAgentsB = 0;
		}
        this.moving = false;

        foreach ( ISSR_AgentBehaviour Behav in GABehaviours)
		{
			Behav.locomotion_active = false;
			Behav.SidewardsPush = 0;
			Behav.FrontBackPush = 0;
			if (other != null)
			{
				Behav.EnqueueEvent (ISSREventType.onGObjectCollision, other.gameObject);
			}
			Behav.EnqueueEvent (ISSREventType.onStop);
		}

	}

	void Update () 
	{
		
		// Tasks   If moving update movement 
		//         If not moving revise pushes:
		//           -Start moving and send events to agents if necessary:   
		//           -Stop moving  and send events
		//  Accounting of moving stones in team(s)

		if (this.moving)
		{
			// Check if conditions for moving still hold::  DoConditionsForMovingHold()
			//  if conditions for moving HOLD:   update movement UpdateStoneMovement()
			//     Theses are:
			//        number of agents pushing *3 > total mass (3 stone) 1x number of gripping agents 
			//        sum of vectors to destinations normalized   has a magnitud > threshold (PushVectorMinMag)
			//  if not:  stop, signal onStop to all gripping agents,
			//    remove count of moving stones for teams,
			//    set moving to false
			//    remove all pushes

			if (DoConditionsForMovingHold())
			{
				UpdateStoneMovement ();
			}
			else
			{
				StopBStoneMovement (null); // Stop but nor due to collision
			}
		}
		else
		{
			// Check if conditions for moving hold: DoConditionsForMovingHold()
			//  if conditions for moving hold :  start moving, signal agents onStartsMoving, 
			//   increase moving stone for teams, set moving to true

			if (DoConditionsForMovingHold ())
			{
				// Start to move
				bool can_move;

				can_move = false;

				if ((PushingAgentsA > 0) && (ISSRManager.Teams [0].stones_moving < ISSRManager.MaxStonesMovingPerTeam))
				{
					can_move = true;
				}

				if ((PushingAgentsB > 0) && (ISSRManager.Teams [1].stones_moving < ISSRManager.MaxStonesMovingPerTeam))
				{
					can_move = true;
				}

				// Only if can move

				if (can_move)
				{
					this.moving = true;
					this.rb.isKinematic = false;

					if (PushingAgentsA > 0)
					{
						ISSRManager.Teams [0].stones_moving++;
					}
					if (PushingAgentsB > 0)
					{
						ISSRManager.Teams [1].stones_moving++;
					}
					foreach ( ISSR_AgentBehaviour Behav in GABehaviours)
					{
						Behav.EnqueueEvent (ISSREventType.onStartsMoving);
					}
					UpdateStoneMovement ();

				}
				else
				{
					CheckPushesTimeOut ();
				}

			}
			else
			{
				// Check pushes timeout

				CheckPushesTimeOut ();
			}

			// If not moving revise time of pushes and remove pushes for which to much time has elapsed: MaxWaitTime
		}
	}

	void CheckPushesTimeOut()
	{
		ISSR_AgentBehaviour Behav;
		ISSR_AgentPush push;
		int i;

		//foreach (ISSR_AgentPush push in CurrentPushes) 
		for (i=0; i < CurrentPushes.Count; i++)
		{
			push = CurrentPushes [i];
			if ((push.StartPushTime + this.MaxWaitTime) < Time.time)
			{

				//Debug.LogFormat ("Push time: {0}, current time: {1}, max: {2}", push.StartPushTime, Time.time, this.MaxWaitTime);
				Behav = ISSRManager.GetAgentBehaviour (push.AgentGO);
				Behav.EnqueueEvent (ISSREventType.onPushTimeOut, this.gameObject);
				CurrentPushes.Remove (push);
				Behav.locomotion_active = false;

			}
		}
	}


	bool DoConditionsForMovingHold()
	{
		//     Theses are:
		//        number of agents pushing *3 > total mass (3 stone) 1x number of gripping agents 
		//        sum of vectors to destinations normalized   has a magnitud > threshold (PushVectorMinMag)
        if (!game_active)
        {
            return false;
        }


		if (this.CurrentPushes.Count*3 < 3+ this.GrippingAgentsGO.Count)  // Pushing force is not enough to move everything???
		{
			return false;
		}
		else
		{
			// Now check the aggregatted push vector. 
			CalculateAggregatedPush();
			if (AggregatedPushVector.magnitude < this.PushVectorMinMag)
			{
				return false;
			}
		}
		return true;
	}

	void CalculateAggregatedPush()
	{
		AggregatedPushVector = Vector3.zero;
		Vector3 onepush;

		foreach( ISSR_AgentPush push in CurrentPushes)
		{
			onepush = push.DesiredDestination-this.gameObject.transform.position;
			//onepush = push.DesiredDestination-push.AgentGO.transform.position;
			//Debug.LogFormat ("onepush {0} = {1} - {2}", onepush, push.DesiredDestination, this.gameObject.transform.position);
			AggregatedPushVector += onepush.normalized;
		}

		//Debug.LogFormat("{0} aggregated", AggregatedPushVector);

	}

	void UpdateStoneMovement()
	{
		//Vector3 velocity;
		int i;
		Vector3 agent_forward;
		ISSR_AgentBehaviour Behav;
        float velocity_factor = 1f;

		float desired_speed = AggregatedPushVector.magnitude;

		AggregatedPushVector = AggregatedPushVector.normalized;

		this.rb.velocity = AggregatedPushVector * velocity_factor;



		foreach(Rigidbody rba in GARigidBodies)
		{
			rba.velocity  = AggregatedPushVector * velocity_factor;
		}

		for (i=0; i < CurrentPushes.Count; i++)
		{
			Vector3 cross;
			Behav = ISSRManager.GetAgentBehaviour( this.CurrentPushes [i].AgentGO);
			agent_forward = this.CurrentPushes [i].AgentGO.transform.TransformDirection (Vector3.forward);
			Behav.FrontBackPush = Vector3.Dot (agent_forward, AggregatedPushVector);
			cross =  Vector3.Cross (agent_forward, AggregatedPushVector);
			Behav.SidewardsPush = cross.y;
		}

		//velocity = AggregatedPushVector * 1;

		//rb.MovePosition (this.transform.position + velocity*Time.deltaTime);
	}


	void OnCollisionEnter(Collision other)
	{
		
		if ((other.gameObject.tag == "GoalA")|| (other.gameObject.tag=="GoalB"))
		{
			// Scoring 
			ISSRManager.ScoreUpdate (null, this.gameObject, other.gameObject);
			ISSRManager.DestroyStone(this.gameObject);
			// from here on it does not continue.  Object Inactive
		}
		else if (other.gameObject.tag != "Ignore")
		{
			StopBStoneMovement (other);
		}
	}
}
