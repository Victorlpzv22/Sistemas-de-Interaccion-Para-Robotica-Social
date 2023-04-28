using UnityEngine;
using System.Collections;

// This Behaviour is attached to the AgentMesh Object, son of the Agent.
//  It has a SphereCollider for Sensing:  it will keep track of all objects within 
//    sensing range of the Agent
// So it maintains a list of all objects in  "sight distance", using OnTriggerEnter() an OnTriggerExit
public class ISSR_AgentSensing : MonoBehaviour  

{
	[HideInInspector]
	public ISSR_ManagerBehaviour ISSRManager;
	[HideInInspector]
	public GameObject 		   MyAgent;
	[HideInInspector]
	public ISSR_AgentBehaviour MyAgentBehavior;

	public bool debug_verbose;

	void Awake()
	{
		// Reference to ISSRManager
		this.ISSRManager = GameObject.Find ("ISSR_Manager").GetComponent<ISSR_ManagerBehaviour>();
		if (this.ISSRManager==null)
		{
			Debug.LogError("No ISSRManager Object");
		}

		this.MyAgent = transform.parent.gameObject;
		this.MyAgentBehavior = MyAgent.GetComponent <ISSR_AgentBehaviour> ();

	}




	void OnTriggerEnter(Collider other)
	{
		ISSR_Object obj;
		Vector3 location;
		bool error;

		if ((other.isTrigger == false) &&(  ISSR_Object.Tag2Type (other.tag)!= ISSR_Type.Undefined))
		{
            // EXPERIMENTAL
         //   if (Vector3.Distance(this.transform.position, other.transform.position) > (ISSRManager.SensingRange + 1))
         //       return; 
		 	
			if (!this.MyAgentBehavior.ObjectsInSensesRange.Contains (other.gameObject))  
			{ // The check is necessary because when an agent is gripped an additional onTrigger() is produced
				this.MyAgentBehavior.ObjectsInSensesRange.Add (other.gameObject);



				if (MyAgentBehavior.debug_sensing)
				{
					Debug.LogFormat ("SENSE {0} STARTS SENSING {1} of type {2}", this.MyAgent.name,  other.name, other.tag);
				}

				// GENERATE EVENT--------------------------------------------------------------------------------
				this.MyAgentBehavior.EnqueueEvent (ISSREventType.onEnterSensingArea, other.gameObject);



				// Find out if the other object is an agent gripping a small stone, goal or agent whose collider are disabled.
				//   if it has entered this agent sensing area it is not detected, we should detect this circunstance
				//    and add that object to the sensing lists
				if ((other.gameObject.tag == "AgentA") || (other.gameObject.tag == "AgentA"))
				{
					ISSR_AgentBehaviour AgentBeh = ISSRManager.GetAgentBehaviour (other.gameObject);
					if ((AgentBeh.GrippedObject!= null) && (AgentBeh.GrippedObject.tag!="BigStone"))// The Agent has some gripped object not a big stone
					{
						if (!this.MyAgentBehavior.ObjectsInSensesRange.Contains (AgentBeh.GrippedObject))
						{
							this.MyAgentBehavior.ObjectsInSensesRange.Add (AgentBeh.GrippedObject);
							this.MyAgentBehavior.EnqueueEvent (ISSREventType.onEnterSensingArea, AgentBeh.GrippedObject);
							if (MyAgentBehavior.debug_sensing)
							{
								Debug.LogFormat ("SENSE {0} STARTS SENSING {1} gripped by {2}", 
									this.MyAgent.name, AgentBeh.GrippedObject.name, other.name, other.tag);
							}
						}
					}
				}

				if ((MyAgentBehavior.debug_sensing) && (debug_verbose))
				{
					// Debugging ------------ distances
					obj = this.MyAgentBehavior.ISSRManager.GameObject2Object (other.gameObject);
					if (obj != null)
					{
						location = this.MyAgentBehavior.AgentDescriptor.oiLocation (obj);
						if (this.MyAgentBehavior.AgentDescriptor.oiError)
						{
							Debug.Log (this.MyAgentBehavior.AgentDescriptor.oiError_msg (this.MyAgentBehavior.AgentDescriptor.oiError_code));
						}
						Debug.LogFormat ("SENSE distance: {0}, location: {1}, error: {2}", 
							this.ISSRManager.DistanceFromAgentCenter (this.MyAgent, obj, out error), location.ToString (), error);

					}

					else
						Debug.Log ("Cannot access ISSR_Object");
				}

			}




		}	
	}


	void OnTriggerExit(Collider other)
	{
		ISSR_Object obj;
		bool error;

		if ((other.isTrigger == false) &&(  ISSR_Object.Tag2Type (other.tag)!= ISSR_Type.Undefined))
		{



			if (this.MyAgentBehavior.ObjectsInSensesRange.Contains (other.gameObject))
			{
				this.MyAgentBehavior.ObjectsInSensesRange.Remove (other.gameObject);

				// GENERATE EVENT--------------------------------------------------------------------------
				this.MyAgentBehavior.EnqueueEvent (ISSREventType.onExitSensingArea, other.gameObject);

				if (MyAgentBehavior.debug_sensing)
				{
					Debug.LogFormat ("SENSE {0} STOPS SENSING {1} of type {2}", this.MyAgent.name,  other.name, other.tag);
					// Debugging

					if (debug_verbose)
					{
						obj = this.MyAgentBehavior.ISSRManager.GameObject2Object (other.gameObject);
						if (obj != null)
							Debug.LogFormat ("SENSE distance: {0}, error: {1}", this.ISSRManager.DistanceFromAgentCenter (this.MyAgent, obj, out error), error);
						else
							Debug.Log ("Cannot access ISSR_Object");
					}

				}


				// Find out if the other object is an agent gripping a small stone, goal or agent whose collider are disabled.
				//   if it has exited this agent sensing area it is not detected, we should detect this circunstance
				//    and remove that object to the sensing lists
				if ((other.gameObject.tag == "AgentA") || (other.gameObject.tag == "AgentA"))
				{
					ISSR_AgentBehaviour AgentBeh = ISSRManager.GetAgentBehaviour (other.gameObject);
					if ((AgentBeh.GrippedObject!= null) &&(AgentBeh.GrippedObject.tag!="BigStone"))// The Agent has some gripped object not a big stone
					{

						if (this.MyAgentBehavior.ObjectsInSensesRange.Contains (AgentBeh.GrippedObject))
						{
							this.MyAgentBehavior.ObjectsInSensesRange.Remove (AgentBeh.GrippedObject);
							this.MyAgentBehavior.EnqueueEvent (ISSREventType.onExitSensingArea, AgentBeh.GrippedObject);

							if (MyAgentBehavior.debug_sensing)
							{
								Debug.LogFormat ("SENSE SENSE {0} STOPS SENSING {1} gripped by {2}", 
									this.MyAgent.name, AgentBeh.GrippedObject.name, other.name, other.tag);
							}	
						}
							
					}
				}
			}








		}	
	}
}
