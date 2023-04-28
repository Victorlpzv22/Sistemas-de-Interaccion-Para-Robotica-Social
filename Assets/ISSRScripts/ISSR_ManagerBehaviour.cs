
// ----------------------------------------------------------------------------------------
//  File: ISSR_Manager.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//    Main Control Object:
//     central control of Application, responsible also for game end condition (TODO)
//     camera control, interface control
// ----------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System;


//  Type of game
using System.Resources;
using UnityEngine.UI;


public enum ISSR_GameScenario 
{
	Undefined,
	Cooperative,
	Competitive
	// some testing special scenarios,  for small testing assignments?
}




public enum ManagerSFX
{
	start,
	score,
	bip,
	end,
	success,
	failure

};


public class ISSR_ManagerBehaviour : MonoBehaviour 
{

	#region BASIC SETTINGS

	[Header("Basic Settings: CHANGE BEFORE RUN")]  // Parameters for testing and game type

	[Multiline]
	public string ScenarioTitle;

	[SerializeField]
	[Tooltip("How far can agents \"see\" objects")]
	private float _SensingRange;
	public  float SensingRange			// How far can agents "see" objects
	{
		get { return _SensingRange;}
	}

	[SerializeField]
	[Tooltip("How far can agents \"hear\" messages")]
	private float _CommsRange;
	public  float CommsRange			// How far can agents "hear" messages
	{
		get { return _CommsRange;}
	}



	[SerializeField]
	[Tooltip("Maximum amount of cost allowed per Team,\n 0 for unlimited NOT IMPLEMENTED (IGNORE)")]
	private float _CostBudget;
	public  float CostBudget  // Maximum amount of cost allowed per Team, 0 for unlimited
	{
		get { return _CostBudget;}
	}


	[SerializeField]
	[Tooltip("Maximum time to run in seconds,\n 0 for unlimited")]
	private int _TimeBudget;
	public 	int TimeBudget  // Maximum time to run in seconds, 0 for unlimited
	{
		get { return _TimeBudget;}
	}
	// Game finishes when:
	//  *There are no stones left to take to the goal(s)
	//  *Budget of all teams is exhausted  (cost budget is not considered 
	//  *TimeBudget is reached

	[SerializeField]
	[Tooltip("Maximum number of Stones a Team can move simultaneously")]
	private int _MaxStonesMovingPerTeam;
	public int MaxStonesMovingPerTeam
	{
		get { return _MaxStonesMovingPerTeam;}
	}
	#endregion
   // End of BASIC SETTINGS--------------------------------------------------------------

	#region GAME SCENARIO SETTINGS
	// Parameters filled with the information obtained from the scen
	[Header("Game Scenario Settings: DON'T TOUCH")] 
	public ISSR_GameScenario 	TypeOfGame;
	public bool 				TeamApresent;   
	public bool 				TeamBpresent;
	public int 					AgentsPerTeam;
	public int 					TotalPoints;
	public float 				GameyardXDim; // Dimensions in X axis or East West of the Gameyard
	public float 				GameyardZDim; // Dimensions in Z axis or South North of the Gameyard
	#endregion

	[Header("Debugging Log flags")]
	public bool debug_anything;


	#region CAMERA Settings
	bool CameraZenithActive = false;
	Camera MainCamera;
	Camera ZenithCamera;

	public float CameraTiltAngle;
	public float CameraHeight;
	public float CameraZDistance;

	int CameraTeamInfocus;
	int CameraAgentInfocus;

	int CameraAgentAInfocus;
	int CameraAgentBInfocus;

	Vector3 CameraTargetVector;
	GameObject CameraObjectInFocus;


	GameObject OccludingWall;
	GameObject RedArrowBill;
	GameObject BlueArrowBill;

	bool CameraGoalInFocus;

	GameObject CameraTarget;
	bool AnimatingCameraTarget;
	float camera_speed;

	#endregion

	// PRIVATE FIELDS-----------------------------
	#region INTERNAL  FIELDS
	[Header("Internal fields")]
	public bool initerror;
    bool automatic;
    GameObject Tester;
    ISSR_TesterBehaviour TesterBehaviour;

	public  GameObject AgentPrefab;  // An agent  for instantation as 3D Active Object


	private float agent_radius;

	ISSR_ManagerBehaviour MyBehaviour;
	GameObject[] AgentsAMarkers;   // Initial position of Agents of TeamA 
	GameObject[] AgentsBMarkers;   // Initial position of Agents of TeamB 
	GameObject[] SStones;	// All SMALL stones present in scene initially
	GameObject[] BStones;	// All BIG stones present in scene initially
	public ISSR_BStoneBehaviour[] BStonesBehaviors; // Behaviours of big stones
	GameObject[] GoalsA;    // Goals for TeamA  (there should only be one and in case there are agents of type A)
	GameObject[] GoalsB;    // Goals for TeamN  (there should only be one and in case there are agents of type B)

	GameObject[] AgentsA;  // Agents of TeamA
	GameObject[] AgentsB;  // Agents of TeamB
	ISSR_AgentBehaviour[] AgentsABehaviors;
	ISSR_AgentBehaviour[] AgentsBBehaviors;
	GameObject	 Gameyard; 
	GameObject   NorthWall;
	GameObject   SouthWall;
	GameObject   EastWall;
	GameObject   WestWall;

	private bool	TeamARegistered;
	private bool 	TeamBRegistered;

	public  ISSR_TeamDescriptor[] Teams;  // Teams in scene, maximum number is 2

	// Indexes of ISSR_Objects  representing all objects in Game
	public ISSR_Object[] OAgentsA;
	public ISSR_Object[] OAgentsB;
	public ISSR_Object[] OSStones;
	public ISSR_Object[] OBStones;
	public ISSR_Object   OGoalA;
	public ISSR_Object   OGoalB;
	public ISSR_Object ONorthWall;
	public ISSR_Object OSouthWall;
	public ISSR_Object OEastWall;
	public ISSR_Object OWestWall;


	// Other variables.  Materials for agents, teams are 
	public Material[] DefaultMaterialsTeamA;
	public Material[] DefaultMaterialsTeamB;

	#endregion



	#region SOUND RELATED

	AudioSource ManagerAudioSource;

	public AudioClip[] sound_effects;


	#endregion

	#region USER INTERFACE related

	Text  TeamAScoreText;
    Animator TeamAScoreAnim;
	Text  TeamBScoreText;
    Animator TeamBScoreAnim;

    GameObject AAgentSprite;
	GameObject BAgentSprite;
	GameObject ClockSprite;

    GameObject AAgentBlockedText;
    GameObject BAgentBlockedText;

    Text  TeamANameText;
	Text  TeamBNameText;
	Text  TimeLeftText;
	public int 	TimeLeft;

	Text RemainingScore;
	float pointsleft;

	GameObject TimeExhausted;
	GameObject ScenarioNameGO;
	Text	   ScenarioName;
	GameObject StonesGO;
	Text 	   StonesText;
	GameObject TaskCompleted;
	GameObject TeamLongNameGO;
	Text		TeamLongName;
	GameObject RedTeamWins;
	GameObject BlueTeamWins;
	GameObject Cover;
	GameObject StartMenu;
	GameObject Controls;
    bool controls_on;

    // Statistics related
    GameObject  StatisticsPanel;
    bool        statistics_on;
    Text        STScore;
    Text        STSStones;
    Text        STBStones;
    Text        STimeElapsed;
    Text        SNAgents;
   

    GameObject  BlueTeamData;
    Text        SAScore;
    Text        SASStones;
    Text        SABStones;
    Text        SADistance;
    Text        SAMessages;
    Text        SACollisions;
    GameObject  SAMedal;
    Text        SAPercent;

    GameObject  RedTeamData;
    Text        SBScore;
    Text        SBSStones;
    Text        SBBStones;
    Text        SBDistance;
    Text        SBMessages;
    Text        SBCollisions;
    int         SStonesCatched;
    int         BStonesCatched;
    GameObject  SBMedal;
    Text        SBPercent;

    #endregion

    public bool superuser_debugging;

	ISSR_Object ObjInFocus;
	GameObject  GOInFocus;
	int indexObjInFocus;
	ISSR_AgentBehaviour AgentInFocusBehav;
	Vector3 debugdestination = new Vector3 (0, 0, 0);

	#region  TEAM SERVICES Manager side
	// (Team part in ISSR_TeamBehaviour.cs)
	// ##########################################################################################
	// ##########################################################################################
	//--------TEAM SERVICES implementation-------------------------------------------------------
	// ------------------------------------------------------------------------------------------
	// ##########################################################################################
	// ##########################################################################################

	public bool RegisterTeam(string ShortName, string LongName, out ISSR_Type AgentType)
	{
			
		AgentType = ISSR_Type.Undefined; 

		if (initerror)
		{
			Debug.LogError ("MANAGER: Cannot register Team, scene intialization error! ");
			return false;
		}

		if (TeamApresent && !TeamARegistered) 
		{
			Teams [0] = new ISSR_TeamDescriptor (ShortName, LongName, ISSR_Type.AgentA, this.CostBudget, this.AgentsPerTeam);
			AgentType = ISSR_Type.AgentA;
			this.TeamARegistered = true;

			this.AgentsABehaviors = Teams [0].Behaviours;

			Teams [0].TeamRegistered = true;
			Teams [0].Agents = AgentsA;

			this.TypeOfGame = ISSR_GameScenario.Cooperative;    // Just in case the second team is not registered
			return true;
		}

		if (TeamBpresent && !TeamBRegistered)
		{
			Teams [1] = new ISSR_TeamDescriptor (ShortName, LongName, ISSR_Type.AgentB, this.CostBudget, this.AgentsPerTeam);
			AgentType = ISSR_Type.AgentB;
			this.TeamBRegistered = true;
			this.AgentsBBehaviors = Teams [1].Behaviours;

			Teams [1].TeamRegistered = true;
			Teams [1].Agents = AgentsB;

			if (TeamARegistered)
			{
				this.TypeOfGame = ISSR_GameScenario.Competitive;  // Once the second team is registered we are sure it is a competitive environment 
			}
			else
			{
				this.TypeOfGame = ISSR_GameScenario.Cooperative; 
			}

			return true;
		}


	
		Debug.LogErrorFormat ("MANAGER: Cannot register Team, {0} already registered! ", (TeamApresent&& TeamBpresent)? "both teams": "unique team");
	



		return false;
	}


	// Things to do when an agent is created. CHECKLIST:
	//  -See if the team has room for a new agent: 
	//  -Instatiate the Agent Prefab using the position of next Agent Marker for that Team
	//  -Hide (Deactivate) Marker
	//  -Change parameters of the created GameObject:
	// 		-Tag depending on the team  both for agent and mesh HearA/HearB
	//      -Material according to the index number in TeamDescriptor
	//      -Size of Sense and Comm range in SphereColliders (they are simulation parameters)
	//      -A copy of its ISSR_Object
	//  -Register them in structures of ISSR_Manager:   GameObjects, ISSR_Objects, Their ISSR_AgentBehaviours
	//  -Before starting check if all teams present have some  attached behaviour.
	//  -Finally activate behaviour of all Agents (at the same time)
	//   

	public void CreateAgent( ISSR_Agent AgentDesc, string AgentTag, string OptionalName)
	{

		GameObject Agent;
		ISSR_AgentBehaviour Behaviour;
		int TeamIndex;
		int AgentIndex;
		GameObject[] Agents;
		GameObject[] AgentMarkers;
		ISSR_Object[] OAgents;
		Material[] DefaultMaterials;
		ISSR_AgentBehaviour[] Behaviours;


		if (initerror)
		{
			Debug.LogError ("MANAGER: Cannot create Agent, scene initialization error! ");
			return;
		}

		if (AgentTag== "AgentA")
		{
			TeamIndex = 0;
			Agents = AgentsA;
			OAgents = OAgentsA;
			AgentMarkers = AgentsAMarkers;
			DefaultMaterials = DefaultMaterialsTeamA;
			Behaviours = this.AgentsABehaviors;
		}
		else if (AgentTag == "AgentB")
		{
			TeamIndex = 1;
			Agents = AgentsB;
			OAgents = OAgentsB;
			AgentMarkers = AgentsBMarkers;
			DefaultMaterials = DefaultMaterialsTeamB;
			Behaviours = this.AgentsBBehaviors;
		}
		else
		{
			Debug.LogErrorFormat ("MANAGER: Trying to create Agent with wrong tag: \"{0}\"", AgentTag);
			return;
		}

		AgentIndex = Teams [TeamIndex].NAgents;

		if (Teams[TeamIndex].NAgents < this.AgentsPerTeam) // There is room for an agent in this team
		{
			Vector3 location;
			Quaternion rotation;
			SphereCollider Sc;

			location = AgentMarkers[AgentIndex].transform.position;	
			rotation = AgentMarkers [AgentIndex].transform.rotation;

			//Agent = GameObject.Instantiate (AgentPrefab, location, Quaternion.identity)  as GameObject;
			Agent = GameObject.Instantiate (AgentPrefab, location, rotation)  as GameObject;
			Agents[AgentIndex] = Agent;
			Agents [AgentIndex].tag = AgentTag;
			AgentMarkers [AgentIndex].SetActive (false);
			Behaviour = Agent.GetComponent <ISSR_AgentBehaviour> ();
			Behaviours [AgentIndex] = Behaviour;
			Teams [TeamIndex].Behaviours [AgentIndex] = Behaviour;
			Behaviour.AgentDescriptor = AgentDesc;
			//Behaviour.AgentDescriptor.Myself.LastLocation = new Vector3(location.x, 0, location.z);
			Behaviour.AgentMesh = Agents [AgentIndex].transform.Find ("Bot").gameObject;
			Behaviour.AgentMeshRenderer = Behaviour.AgentMesh.GetComponent <SkinnedMeshRenderer> ();
			Behaviour.AgentMeshRenderer.material = DefaultMaterials[AgentIndex];
			Behaviour.AgentMesh.tag = (Agent.tag=="AgentA")?"SensingA":"SensingB";
			OAgents [AgentIndex] = new ISSR_Object (Agents[AgentIndex], AgentIndex);
			Behaviour.ObjectDescriptor = OAgents [AgentIndex];
			if (OptionalName.Length==0)
			{
				Agents [AgentIndex].name = String.Format ("{0}{1}", Teams[TeamIndex].ShortName, AgentIndex);
			}
			else
			{
				Agents [AgentIndex].name = String.Format ("{0}{1}({2})", Teams[TeamIndex].ShortName, AgentIndex, OptionalName);
			}

			Behaviour.AgentDescriptor.BootStrap (this.MyBehaviour, Agents [AgentIndex], Behaviour, OAgents [AgentIndex]);

			Sc = Agents[AgentIndex].GetComponent<SphereCollider> ();
			Sc.radius = this.CommsRange;
			Sc = Behaviour.AgentMesh.GetComponent<SphereCollider> ();
			Sc.radius = this.SensingRange;

		

			Teams [TeamIndex].NAgents++;




		} // End of There is room for an agent in this team
		else
		{ // There is no room for an agent in this team.
			Debug.LogErrorFormat("MANAGER: Already created {0} agents of type {1} add more Agent Markers.", this.AgentsPerTeam, AgentTag);	
		}

	}


	// ##########################################################################################
	// ##########################################################################################
	//---------------------------------------------------------------------------------
	//----------------En of TEAM Services ---------------------------------------------
	// ##########################################################################################
	#endregion


	#region INTERNAL AUXILIARY Methods


	/// <summary>
	/// Finds the game object in array of game objects, returning and index into de array or -1 if not found.
	/// </summary>
	/// <returns>The object in array.</returns>
	/// <param name="obj">Object.</param>
	/// <param name="array">Array.</param>
	int FindObjInArray( GameObject obj, GameObject[] array)
	{
		int index = 0;


		while  (index < array.Length)
		{
			if (array[index] == obj)
			{
				return index;
			}
			index++;
		}

		return -1;
	}


	/// <summary>
	/// Obtains the ISSR_Object for a GameObject.
	/// </summary>
	/// <returns>The ISSR_Object.</returns>
	/// <param name="obj">GameObject.</param>
	public ISSR_Object GameObject2Object(GameObject obj)
	{
		int i;
		switch(obj.tag)
		{
		case "NorthWall":
			return ONorthWall;
		case "SouthWall":
			return OSouthWall;
		case "EastWall":
			return OEastWall;
		case "WestWall":
			return OWestWall;
		case "GoalA":
			return OGoalA;
		case "GoalB":
			return OGoalB;
		case "SmallStone":
			i = FindObjInArray (obj, SStones);
			if (i != -1) {
				return OSStones [i];
			}
			return null;
		case "BigStone": 
			i = FindObjInArray (obj, BStones);
			if (i != -1) {
				return OBStones [i];
			}
			return null;
		case "AgentA":
			i = FindObjInArray (obj, AgentsA);
			if (i != -1) {
				return OAgentsA [i];
			}
			return null;
		case "AgentB":
			i = FindObjInArray (obj, AgentsB);
			if (i != -1) {
				return OAgentsB [i];
			}
			return null;
		default:
			return null;
		}

	}

	/// <summary>
	/// Obtains Unity GameObject from ISSR_Obkext
	/// </summary>
	/// <returns>The game object.</returns>
	/// <param name="obj">ISSR Object.</param>
	/// <param name="error">Error.</param>
	public GameObject Object2GameObject( ISSR_Object obj, out bool error)
	{
		error = false;

		switch(obj.type)
		{
		case ISSR_Type.AgentA:
			return this.AgentsA [obj.index];
		case ISSR_Type.AgentB:
			return this.AgentsB [obj.index];
		case ISSR_Type.BigStone:
			return  this.BStones [obj.index];
		case ISSR_Type.SmallStone:
			return this.SStones [obj.index];
		case ISSR_Type.GoalA:
			return this.GoalsA [0];
		case ISSR_Type.GoalB:
			return this.GoalsB [0];
		case ISSR_Type.NorthWall:
			return this.NorthWall;
		case ISSR_Type.SouthWall:
			return this.SouthWall;
		case ISSR_Type.EastWall:
			return this.EastWall;
		case ISSR_Type.WestWall:
			return this.WestWall;
		default:
			error = true;
			return null;
		}	
	}




	/// <summary>
	///   Returns the location of the center of an object form the point of view of the agent
	///    the "point of view" refers to the walls : it returns the location of the closer point to the agent on the wall
	/// </summary>
	/// <returns>The r object location.</returns>
	/// <param name="Agent">Agent.</param>
	/// <param name="obj">Object.</param>
	public Vector3 ISSR_ObjectLocation(GameObject Agent, ISSR_Object obj, out bool error)
	{
		Vector3 location = new Vector3 ();

		switch(obj.type)
		{
		case ISSR_Type.AgentA:
			location = this.AgentsA [obj.index].transform.position;
			break;
		case ISSR_Type.AgentB:
			location = this.AgentsB [obj.index].transform.position;
			break;
		case ISSR_Type.BigStone:

			if (this.BStones[obj.index].activeSelf== false)
			{
				error = false;
				return obj.LastLocation;
			}
			else
			{
				location = this.BStones [obj.index].transform.position;
			}	

			break;
		case ISSR_Type.SmallStone:

			if (this.SStones [obj.index].activeSelf== false)
			{
				error = false;
				return obj.LastLocation;
			}
			else
			{
				location = this.SStones [obj.index].transform.position;
			}

			break;
		case ISSR_Type.GoalA:
			location = this.GoalsA [0].transform.position;
			break;
		case ISSR_Type.GoalB:
			location = this.GoalsB [0].transform.position;
			break;
		case ISSR_Type.NorthWall:
			location.x = Agent.transform.position.x;
			location.z = GameyardZDim/2;
			break;
		case ISSR_Type.SouthWall:
			location.x = Agent.transform.position.x;
			location.z = -GameyardZDim/2;
			break;
		case ISSR_Type.EastWall:
			location.z = Agent.transform.position.z;
			location.x = GameyardXDim/2;
			break;
		case ISSR_Type.WestWall:
			location.z = Agent.transform.position.z;
			location.x =- GameyardXDim/2;
			break;
		default:
			error = true;
			return Vector3.zero;
		}

		location.y = 0;
		error = false;
		return location;
	}



	public float DistanceFromAgentCenter(GameObject Agent, ISSR_Object obj, out bool error)
	{
		Vector3 vector;
		float radius = 0;
		vector = ISSR_ObjectLocation (Agent, obj, out  error);

		if (error)
		{
			return 0;
		}

		switch(obj.type)
		{
		case ISSR_Type.AgentA:
		case ISSR_Type.AgentB:
		case ISSR_Type.SmallStone:
		case ISSR_Type.GoalA:
		case ISSR_Type.GoalB:
		case ISSR_Type.BigStone:
		case ISSR_Type.NorthWall:
		case ISSR_Type.SouthWall:
		case ISSR_Type.EastWall:
		case ISSR_Type.WestWall:
			vector -= Agent.transform.position; 
			radius = obj.Radius;
			break;
		default:
			error = true;
			return 0;
		}

		return vector.magnitude-radius;
	}

	/// <summary>
	/// Gets the agent behaviour.
	/// </summary>
	/// <returns>The agent behaviour.</returns>
	/// <param name="Agent">Agent.</param>
	public ISSR_AgentBehaviour GetAgentBehaviour(GameObject Agent)
	{
		GameObject[] GOArray;
		ISSR_AgentBehaviour[] BehavArray;

		int index;

		if (Agent.tag == "AgentA")
		{
			GOArray = AgentsA;
			BehavArray = AgentsABehaviors;
		}
		else if (Agent.tag == "AgentB")
		{
			GOArray = AgentsB;
			BehavArray = AgentsBBehaviors;
		}
		else
		{
			Debug.LogErrorFormat ("MANAGER: Object \"{0}\" should be An Agent, is a \"{1}\" ", Agent.name, Agent.tag);
			return null;
		}

		index = FindObjInArray (Agent, GOArray);

		if (index != -1)
		{
			return BehavArray [index];
		}
		else
		{
			Debug.LogErrorFormat ("MANAGER: Agent \"{0}\" not found in Agents Array, is a \"{1}\" ", Agent.name, Agent.tag);	
			return null;

		}
	}

	/// <summary>
	/// Gets the Behaviour component of a Big Stone.
	/// </summary>
	/// <returns>The big stone behavior.</returns>
	/// <param name="BStone">BStone GameObject</param>
	ISSR_BStoneBehaviour GetBigStoneBehavior(GameObject BStone)
	{
		int index;

		index = FindObjInArray (BStone, BStones);

		if (index != -1)
		{
			return BStonesBehaviors [index];
		}
		else
		{
			Debug.LogErrorFormat ("MANAGER: Object \"{0}\" not found in Big Stones Array, is a \"{1}\" ", BStone.name, BStone.tag);
			return null;
		}
	}

	public void DestroyStone(GameObject StoneGO)
	{
		
		int team; int agent;
		int index;
		ISSR_BStoneBehaviour BSBehav;
	    // Look for stone object everywhere and remove it  from:
		//  GameObjects in view of every agent
		//  ISSR_Objects in sensables of every agent--->  Better by enqueueing a onExitSensingArea()
		//  -- Change functions that query about a gameobject to signal destroyed  (non sensable)
		//  Global variables of Manager:   set to null


		for (team =0; team < 2; team++)
		{
			if ((Teams[team]!= null) &&(Teams[team].TeamRegistered))
			{
				ISSR_AgentBehaviour[] TeamAgentBehavs;

				ISSR_AgentBehaviour AgentBeh;

				if (team==0)
				{
					TeamAgentBehavs = this.AgentsABehaviors;
				}
				else
				{
					TeamAgentBehavs = this.AgentsBBehaviors;
				}

				for (agent=0; agent < this.AgentsPerTeam; agent++)
				{
					AgentBeh = TeamAgentBehavs [agent];
					if (AgentBeh.ObjectsInSensesRange.Contains (StoneGO))
					{

						AgentBeh.ObjectsInSensesRange.Remove (StoneGO);

						// GENERATE EVENT--------------------------------------------------------------------------
						AgentBeh.EnqueueEvent (ISSREventType.onExitSensingArea, StoneGO);

					}
				}
			}
		}

		switch(StoneGO.tag)
		{

		case "SmallStone":
			index = FindObjInArray (StoneGO, SStones);
			if (index != -1)
			{
				SStones [index].SetActive (false);
			}
			else
			{
				Debug.LogErrorFormat ("MANAGER: Cannot find Stone {0} in Small stones", StoneGO.name);
			}
			break;
		case "BigStone":

			index = FindObjInArray (StoneGO, BStones);

			if (index != -1)
			{
				BSBehav = BStonesBehaviors [index];
				BSBehav.RemoveMe ();
				BStones [index].SetActive (false);
			}
			else
			{
				Debug.LogErrorFormat ("MANAGER: Cannot find Stone {0} in Big stones", StoneGO.name);
			}
			break;
		default:
			Debug.LogErrorFormat ("MANAGER: This Object is not a Stone: {0} ", StoneGO.name);
			break;
		}

		StoneGO.SetActive (false);
	}


	/// <summary>
	/// Given an object determines if this object is in the game
	/// </summary>
	/// <returns><c>true</c>, if object exists <c>false</c> otherwise.</returns>
	/// <param name="obj">Object.</param>
	public bool ObjectExists(ISSR_Object obj)
	{
		bool error;
		GameObject go = Object2GameObject (obj, out error);

		if (go != null)
		{
			return (go.activeSelf);
		}
		else
		{
			return false;
		}

	}

	#endregion

	#region AGENT SERVICES  Manager Side 
	// (Agent part in ISSR_Agent.cs)
	// ##########################################################################################
	// ##########################################################################################
	// ##########################################################################################
	//--------------AGENT SERVICES-----------------------------------------------------
	//---------------------------------------------------------------------------------
	// ##########################################################################################
	// ##########################################################################################


	// TODO Services directory with Cost, all services can be stopped or only services with cost
	// TODO Manager Update monitors the consumed cost in agents to detect unfair or faulty behaviour


	public Vector3 GoalLocation(ISSR_Type agent_type)
	{
		Vector3 location = new Vector3 ();

		switch(agent_type)
		{
		case ISSR_Type.AgentA:
			location = this.GoalsA [0].transform.position;
			return location;
		case ISSR_Type.AgentB:
			location = this.GoalsB [0].transform.position;
			return location;
		default:
			Debug.LogFormat ("MANAGER: {0} is not a goal type", agent_type);
			return Vector3.zero;
		}
	}




	/// <summary>
	///  Obtains the number of Agents that are gripping the given object.
	///    Zero for loose objects
	///    SmallStones, Goals and Agents con only have 1 Agent gripping them
	///    BigStones can have as many as there is room around them
	///    Walls are not grippable
	/// </summary>
	/// <returns>The number gripping agents or zero if none</returns>
	/// <param name="Obj">ISSR Object.</param>
	public int NGrippingAgents(ISSR_Object Obj)
	{
		GameObject GO;
		bool error;

		GO = Object2GameObject (Obj, out error);
		if (error)
		{
			return 0;
		}

		switch(Obj.type)
		{
		case ISSR_Type.SmallStone:
		case ISSR_Type.AgentA:
		case ISSR_Type.AgentB:
		case ISSR_Type.GoalA:
		case ISSR_Type.GoalB:
			return (GO.transform.parent != null) ? 1 : 0;
		case ISSR_Type.NorthWall:
		case ISSR_Type.SouthWall:
		case ISSR_Type.WestWall:
		case ISSR_Type.EastWall:
			return 0;
		case ISSR_Type.BigStone:
			return GO.transform.childCount;
		default:
			return 0;
		}
	}


	/// <summary>
	/// Gets the object gripped by given agent.
	/// </summary>
	/// <returns>The ISSR object gripped by agent.</returns>
	/// <param name="Agent">ISSR Object Agent.</param>
	public ISSR_Object GetObjectGrippedByAgent(ISSR_Object Agent)
	{
		bool error;
		GameObject AgentGO = Object2GameObject (Agent, out error);
		GameObject GrippedGO;
		ISSR_AgentBehaviour AgentBehav;

		AgentBehav = this.GetAgentBehaviour (AgentGO);

		GrippedGO = AgentBehav.GrippedObject;

		if (GrippedGO!= null)
		{
			return GameObject2Object (GrippedGO);
		}
		else
		{
			return	null;
		}


	}



	/// <summary>
	///  Accesses Big Stone Behaviour to determine if it is moving
	/// </summary>
	/// <returns><c>true</c>, if the big stone is moving <c>false</c> otherwise.</returns>
	/// <param name="BStone">Big Stone GameObject.</param>
	public bool BigStoneMoving( GameObject BStone)
	{
		ISSR_BStoneBehaviour Behavior = GetBigStoneBehavior (BStone);
		if (Behavior != null)
		{
			return Behavior.moving;
		}
		else
		{
			Debug.LogErrorFormat ("MANAGER: Object \"{0}\" not found in Big Stones Array, is a \"{1}\" ", BStone.name, BStone.tag);	
			return false;
		}
	}



	/// <summary>
	/// Accesses Agent  to determine if it is moving:
	///   If it has no parent we only have to take a look at is locomotion_active field
	///   If its parent is another agent we call this function on the parent
	///   If the parent is a big stone we access its behavior
	/// </summary>
	/// <returns><c>true</c>, if moving was agented, <c>false</c> otherwise.</returns>
	/// <param name="Agent">Agent.</param>
	public bool AgentMoving(GameObject Agent)
	{
		
		ISSR_AgentBehaviour Behavior = GetAgentBehaviour(Agent);
		GameObject myparent;

		if (Behavior == null)
		{
			Debug.LogErrorFormat ("MANAGER: {0} is not a valid Agent", Agent.name);
			return false;
		}


		if ( Agent.transform.parent== null)
		{
			return Behavior.locomotion_active;
		}
		else
		{
			myparent = Agent.transform.parent.gameObject;

			if ((myparent.tag == "AgentA")|| (myparent.tag == "AgentB"))
			{
				return AgentMoving (myparent);
			}
			else if (myparent.tag == "BigStone")
			{
				return BigStoneMoving (myparent);
			}
			else
			{
				Debug.LogErrorFormat ("MANAGER: Agent \"{0}\" has an unexpected parent \"{1}\" ", Agent.name, myparent.name);	
			}
		}

		return false;
	}



	/// <summary>
	///  Determines if one object is moving
	/// </summary>
	/// <returns><c>true</c>, if the object is moving, <c>false</c> otherwise.</returns>
	/// <param name="Obj">ISSR Object to test.</param>
	public bool ObjectMoving( ISSR_Object Obj)
	{
		bool error;
		GameObject go;

		switch(Obj.type)
		{
		case ISSR_Type.NorthWall:
		case ISSR_Type.SouthWall:
		case ISSR_Type.EastWall:
		case ISSR_Type.WestWall:
			return false;
		case ISSR_Type.SmallStone:
		case ISSR_Type.GoalA:
		case ISSR_Type.GoalB:
			go = Object2GameObject (Obj, out error);
			if (go.transform.parent == null)
			{
				return false;
			}
			else
			{
				return AgentMoving (go.transform.parent.gameObject);
			}
		case ISSR_Type.AgentA:
		case ISSR_Type.AgentB:
			return AgentMoving (Object2GameObject (Obj, out error));
		case ISSR_Type.BigStone:
			return BigStoneMoving (Object2GameObject (Obj, out error));
		default:
			return false;
		}
			
	}

	/// <summary>
	/// Tries to perform a grip of an Agent to an object.
	///   The agent must be already within gripping distance of the object.
	///   If the defined conditions for gripping are satisfied, the object is gripped and true returned
	/// </summary>
	/// <returns><c>true</c>, if grip is posible returns true an the grip is performed, <c>false</c> otherwise.</returns>
	/// <param name="Agent">Agent ISSR Object  of the agent that tries to grip the object</param>
	/// <param name="AgentGO">GameObject of the Agent.</param>
	/// <param name="Obj2Grip">ISSR Object of the object to grip.</param>
	/// <param name="Obj2GripGO">Game Object of the object to grip.</param>
	public bool TryGrip(ISSR_Object Agent, GameObject AgentGO, ISSR_Object Obj2Grip, GameObject Obj2GripGO)
	{

		int ngripping_agents;
		bool moving;

		ngripping_agents = this.NGrippingAgents (Obj2Grip);
		moving = ObjectMoving (Obj2Grip);



		// TODO revise decision on when an agent can grip an object.
		// Currently an agent can only grip a small object (Small stone, goal or agent)
		//  when it is not gripped by other agent.
		// An agent can only be gripped when he is not moving and he is not gripping anything and he is not already gripped 
		// He can always grip a Big Stone

		switch(Obj2Grip.type)
		{
		case ISSR_Type.BigStone:
			DoGrip (Agent, AgentGO, Obj2Grip, Obj2GripGO);
			return true;
		case ISSR_Type.SmallStone:
		case ISSR_Type.GoalA:
		case ISSR_Type.GoalB:
			if (ngripping_agents == 0)
			{
				DoGrip (Agent, AgentGO, Obj2Grip, Obj2GripGO);
				return true;
			}
			else
			{
				return false;
			}
		case ISSR_Type.AgentA:
		case ISSR_Type.AgentB:
			if ((ngripping_agents == 0 )&& (!moving )&& (GetObjectGrippedByAgent(Obj2Grip)==null))
			{// An agent can only be gripped when he is not moving and 
				// he is not gripping anything and he is not already gripped 
				DoGrip (Agent, AgentGO, Obj2Grip, Obj2GripGO);
				return true;
				}
			else
			{
				return false;
			}
		default:
			return false;

		}

	}


	/// <summary>
	///  Performs actions associated to ungrip an object an agent is already gripping
	/// </summary>
	/// <param name="Agent">Gripping Agent.</param>
	/// <param name="AgentGO">Gripping Agent. GameObject</param>
	/// <param name="Obj2Grip">Gripped Object</param>
	/// <param name="Obj2GripGO">Gripped Object GameObject</param>
	public void UndoGrip(ISSR_Object Agent, GameObject AgentGO, ISSR_Object ObjGripped, GameObject ObjGrippedGO)
	{
		ISSR_AgentBehaviour AgentBehav = GetAgentBehaviour (AgentGO);
		ISSR_AgentBehaviour GrippedAgentBehav; 
		Rigidbody rb_gripped_object;
		CapsuleCollider cc_gripped_object;
		ISSR_BStoneBehaviour BStoneBehav;

		switch(ObjGripped.type)
		{
		case ISSR_Type.BigStone:

			BStoneBehav = BStonesBehaviors [ObjGripped.index];
			// When ungripping big stone
			// Agent removes herself as child of BigStone, 
			// BigStone should be informed and the locomotion type of agent changes to loose

			BStoneBehav.AgentUngrips (AgentGO, Agent);

			AgentBehav.loco_state = ISSR_AgentBehaviour.Locomotion.loose;
			break;
		case ISSR_Type.SmallStone:
		case ISSR_Type.GoalA:
		case ISSR_Type.GoalB:

			// When ungripping a small object
			// The object should remove the agent as its parent
			ObjGrippedGO.transform.parent = null;

			// The locomotion mode of the gripping agent changes to:
			AgentBehav.loco_state = ISSR_AgentBehaviour.Locomotion.loose;

			// It is necessary to enable the small object physics components
			rb_gripped_object = ObjGrippedGO.GetComponent <Rigidbody> ();
			cc_gripped_object = ObjGrippedGO.GetComponent <CapsuleCollider> ();

			// Enable rigid body and collider of small object
			//rb_gripped_object.isKinematic = false;
			cc_gripped_object.enabled = true;


			//Add rigid body component  // Not necessary


			// Diseble gripping agent front collider
			AgentBehav.FrontCollider.enabled = false;
			break;
		case ISSR_Type.AgentA:
		case ISSR_Type.AgentB:
			// When ungripping an agent the actions are the following:

			// GRIPPING AGENT-----------------------------
			AgentBehav.loco_state = ISSR_AgentBehaviour.Locomotion.loose;

			// Disable gripping agent front collider
			AgentBehav.FrontCollider.enabled = false; 



			// GRIPPED AGENT-----------------------------
			// The agent gripped removes the agent as her parent
			ObjGrippedGO.transform.parent = null;

			// It is necessary to enable the gripped agent physics components
			rb_gripped_object = ObjGrippedGO.GetComponent <Rigidbody> ();
			rb_gripped_object.isKinematic = false;

			GrippedAgentBehav = GetAgentBehaviour (ObjGrippedGO);   // Now Behaviour of Gripped Agent
			GrippedAgentBehav.MainCollider.enabled = true;

			// Also the gripped agent changes its locomotion state
			GrippedAgentBehav.loco_state = ISSR_AgentBehaviour.Locomotion.loose;


			break;
		default:
			Debug.LogErrorFormat ("MANAGER: {0} Cannot ungrip {1}", AgentGO.name, ObjGripped.Name);
			break;
		}

		AgentBehav.GrippedObject = null;
		AgentBehav.AgentDescriptor.GrippedObject = null;
	}

	/// <summary>
	///   Performs actions associated to Gripping an object, 
	///   the object satisfies conditions to be gripped by this agent
	///    and is within gripping distance     
	/// </summary>
	/// <param name="Agent">ISSR_Object of Agent that is going to grip </param>
	/// <param name="AgentGO">GameObject of Agent that is going to grip </param>
	/// <param name="Obj2Grip">ISSR_Object of Object to Grip</param>
	/// <param name="Obj2GripGO">GameObject of Object to Grip</param>
	public void DoGrip(ISSR_Object Agent, GameObject AgentGO, ISSR_Object Obj2Grip, GameObject Obj2GripGO)
	{
		ISSR_AgentBehaviour AgentBehav = GetAgentBehaviour (AgentGO);
		Rigidbody rb_gripped_object;
		CapsuleCollider cc_gripped_object;
		ISSR_BStoneBehaviour BStoneBehav;
		Vector3 center;

		AgentBehav.GrippedObject = Obj2GripGO;
		AgentBehav.AgentDescriptor.GrippedObject = Obj2Grip;

		switch(Obj2Grip.type)
		{
		case ISSR_Type.BigStone:

			BStoneBehav = BStonesBehaviors [Obj2Grip.index];

			BStoneBehav.AgentGrips (AgentGO, Agent);
			// When gripping big stone
			// Agent gets to be a child of BigStone, 
			// BigStone should be informed and the locomotion type of agent changes to heavypush:

			AgentBehav.loco_state = ISSR_AgentBehaviour.Locomotion.heavypush;
			break;
		case ISSR_Type.SmallStone:
		case ISSR_Type.GoalA:
		case ISSR_Type.GoalB:
			// When gripping a small object
			// The object gets the agent as its parent
			Obj2GripGO.transform.parent = AgentGO.transform;
			// The locomotion mode of the gripping agent changes to:
			AgentBehav.loco_state = ISSR_AgentBehaviour.Locomotion.lightpush;

			// It is necessary to disable the small object physics components
			rb_gripped_object = Obj2GripGO.GetComponent <Rigidbody> ();
			cc_gripped_object = Obj2GripGO.GetComponent <CapsuleCollider> ();

			// Disable rigid body and collider of small object
			//rb_gripped_object.isKinematic = true;

			// Change radius and distance to agent of capsule collider collider

			cc_gripped_object.enabled = false;




			// Enable gripping agent front collider
			AgentBehav.FrontCollider.radius = Obj2Grip.Radius;
			center = AgentBehav.FrontCollider.center;
			center.z = Obj2Grip.GrippingRadius + 0.7f;
			AgentBehav.FrontCollider.center = center;

			AgentBehav.FrontCollider.enabled = true;



			break;
		case ISSR_Type.AgentA:
		case ISSR_Type.AgentB:
			// When gripping an agent the actions are the following:

			// GRIPPING AGENT-----------------------------
			AgentBehav.loco_state = ISSR_AgentBehaviour.Locomotion.lightpush;

			// Enable gripping agent front collider
			AgentBehav.FrontCollider.enabled = true;



			// GRIPPED AGENT-----------------------------
			// The agent gripped gets the agent as her parent
			Obj2GripGO.transform.parent = AgentGO.transform;

			// It is necessary to disable the gripped agent physics components
			rb_gripped_object = Obj2GripGO.GetComponent <Rigidbody> ();
			rb_gripped_object.isKinematic = true;

			AgentBehav = GetAgentBehaviour (Obj2GripGO);   // Now Behaviour of Gripped Agent
			AgentBehav.MainCollider.enabled = false;

			// Also the gripped agent changes its locomotion state
			AgentBehav.loco_state = ISSR_AgentBehaviour.Locomotion.gripped;


			break;
		default:
			AgentBehav.GrippedObject = null;
			AgentBehav.AgentDescriptor.GrippedObject = null;
			Debug.LogErrorFormat ("MANAGER: {0} Cannot grip {1}", AgentGO.name, Obj2Grip.Name);
			break;
		}
	}	

	// ##########################################################################################
	// ##########################################################################################
	// ##########################################################################################
	//---------------------------------------------------------------------------------
	//-----------END of AGENT SERVICES------------------------------------------------
	// ##########################################################################################
	// ##########################################################################################
	#endregion


	// --------------INIT Methods------------------------------------------
	//----------------------------------------------------------------------------------
	#region INIT Methods
	// InspectScene():   Responsible for
	//   Reading objects in scene to determine:
	//    -Agents and teams:   Objects with tags TeamA and TeamB
	//    -Number of agents in one team == 0     cooperative scenario
	//    -Number of agents TeamA == TeamB != 0  competitive scenario
	//    -Stones and total prize.
	//    -Size of play yard


	void InspectScene()
	{
		

		BoxCollider YardBoxCollider;
		this.Gameyard = GameObject.Find ("Gameyard");
		YardBoxCollider = this.Gameyard.GetComponent <BoxCollider> ();
		this.GameyardXDim = YardBoxCollider.size.x;
		this.GameyardZDim = YardBoxCollider.size.z;



		this.NorthWall = GameObject.Find ("NorthWall");
		this.SouthWall = GameObject.Find ("SouthWall");
		this.EastWall = GameObject.Find ("EastWall");
		this.WestWall = GameObject.Find ("WestWall");


		AgentsAMarkers = GameObject.FindGameObjectsWithTag ("AgentA");
		// Check that there should be no Objects with tag "AgentA" but empties with "TeamA" tag
		if (AgentsAMarkers.Length>0)
		{
			Debug.LogError ("MANAGER: There should be no objects with \"AgentA\" tag in the scene,\n Use empty objects with \"TeamA\" tag for placing Agents of A Team");
			return;
		}

		AgentsBMarkers = GameObject.FindGameObjectsWithTag ("AgentB");
		// Check that there should be no Objects with tag "AgentB" but empties with "TeamB" tag
		if (AgentsBMarkers.Length>0)
		{
			Debug.LogError ("MANAGER: There should be no objects with \"AgentB\" tag in the scene,\n Use empty objects with \"TeamB\" tag for placing Agents of B Team");
			return;
		}


		// Read initial position of agents for TeamA
		AgentsAMarkers = GameObject.FindGameObjectsWithTag ("AgentA_Marker");

		// Read initial position of agents for TeamB
		AgentsBMarkers = GameObject.FindGameObjectsWithTag ("AgentB_Marker");


		// Look for stones and goals
		SStones = GameObject.FindGameObjectsWithTag ("SmallStone");

		BStones = GameObject.FindGameObjectsWithTag ("BigStone");

		// Creates behavior references for Big Stones
		if (BStones.Length > 0)
		{
			this.BStonesBehaviors = new ISSR_BStoneBehaviour[BStones.Length];
		}

		GoalsA = GameObject.FindGameObjectsWithTag ("GoalA");

		GoalsB = GameObject.FindGameObjectsWithTag ("GoalB");

		// Set all objects to Y=0 and TODO put them inside the gameyard if they are not
		this.ForceCorrectInitialTransforms ();

		// Deduce a correct set of objects:
		//  -Number of agents in a team must be either zero if the team is not present 
		//  -If both teams are present they must have the same number of agents, from 1 to 10
		//  -If a team is present their goal should also be present
		//  -There should be at least one agent


		if (AgentsAMarkers.Length > 0)
		{
			this.TeamApresent = true;
		}

		if (AgentsBMarkers.Length > 0)
		{
			this.TeamBpresent = true;
		}

		this.TypeOfGame = ISSR_GameScenario.Cooperative;  // By default we intend a cooperative environment, only one team of agents

		if ((this.TeamApresent)&&(this.TeamBpresent))
		{
			
			this.TypeOfGame = ISSR_GameScenario.Competitive;

			if (AgentsAMarkers.Length!= AgentsBMarkers.Length)
			{
				Debug.LogErrorFormat ("MANAGER: It is NOT FAIR!! number of Agents in TeamA ({0}) is different\n from the number of Agents in TeamB ({1})", 
					AgentsAMarkers.Length, AgentsBMarkers.Length);
				
				return;
			}
			else
			{
				this.AgentsPerTeam = AgentsAMarkers.Length; // Or AgentsB.Length

			}
			
		}
		else
		{
			if ((!this.TeamApresent)&&(!this.TeamBpresent))
			{
				Debug.LogError("MANAGER: No objects with TeamA nor TeamB tag present in scene, NO AGENTS, NO GAME!");
				return;
			}

			this.TypeOfGame = ISSR_GameScenario.Cooperative;

			if (this.TeamApresent)
			{
				this.AgentsPerTeam = AgentsAMarkers.Length;
			}
			else
			{
				this.AgentsPerTeam = AgentsBMarkers.Length;
			}
		}

		this.TypeOfGame = ISSR_GameScenario.Undefined;

		// Checking goals ------------------------------------------

		switch (GoalsA.Length)   // GOAL A BLUE TEAM #####################################
		{
		case 0:
			if (this.TeamApresent)
			{
				Debug.LogError ("MANAGER: There is no goal for TeamA! (BLUE color banner)");
				return;
			}
			break;

		case 1:
			if (!this.TeamApresent)
			{
				Debug.LogWarning ("MANAGER: Disabling GoalA (BLUE color banner) because there are no TeamA Agents, could only cause problems");
				GoalsA [0].SetActive (false);
			}
			break;
		default:
			
			Debug.LogError ("MANAGER: DON'T CHEAT More than one Goal for TeamA! (BLUE color banner) is not allowed");
			return;
		}  // end of checking of Goal A Blue TeamA


		switch (GoalsB.Length)   // GOAL B GREEN TEAM #####################################
		{
		case 0:
			if (this.TeamBpresent)
			{
				Debug.LogError ("MANAGER: There is no goal for TeamB! (RED color banner)");
				return;
			}
			break;

		case 1:
			if (!this.TeamBpresent)
			{
				Debug.LogWarning ("MANAGER: Disabling GoalB (RED color banner) because there are no TeamB Agents, could only cause problems");
				GoalsB [0].SetActive (false);
			}
			break;
		default:
			
			Debug.LogError ("MANAGER: DON'T CHEAT More than one Goal for TeamB! (RED color banners) is not allowed");
			return;

		}  // end of checking of Goal B Green TeamB


		// Checking stones----------------------------------------------

		this.TotalPoints = 0;

		this.TotalPoints += SStones.Length;


        if (this.AgentsPerTeam > 1)
        {
            this.TotalPoints += (BStones.Length * 3);
        }
        else
        {
            if (BStones.Length > 0) // AgentsPerTeam == 1 and Big Stones present
            {
                Debug.LogWarning("MANAGER: a single agent cannot collect Big Stones");
            }
        }
        
		

		if (this.TotalPoints==0)
		{
			Debug.LogError ("MANAGER: There are NO STONES in the Play Yard, NO STONES NO GAME");
			return;
		}

	
		// Create Empty GameObjects for Agents
		if (TeamApresent)
		{
			this.AgentsA = new GameObject[this.AgentsPerTeam];
			this.OAgentsA = new ISSR_Object[this.AgentsPerTeam];
		}

		if (TeamBpresent)
		{
			this.AgentsB = new GameObject[this.AgentsPerTeam];
			this.OAgentsB = new ISSR_Object[this.AgentsPerTeam];
		}

		for (int i=0; i < BStones.Length; i++)
		{
			BStonesBehaviors[i] = BStones [i].GetComponent <ISSR_BStoneBehaviour> ();
		}

		// TODO read Gameyard dimensions and Wall objects


		this.initerror = false;
	}
	//  End OF InspectScene() -------------------------------------------------------------------------

	// Checking objects: All Y coordinates should be zero and no parent child relationship should be among them
		
	void ForceCorrectInitialTransforms()
	{
		Vector3 pos;

		// TODO Force objects inside the scene if out of Gameyard dimensions, check if no problem por putting stone at y=0
		foreach (GameObject obj in AgentsAMarkers)
		{

			pos = obj.transform.position;
			pos.y = 0;
			obj.transform.position = pos;
		}

		foreach (GameObject obj in AgentsBMarkers)
		{
			pos = obj.transform.position;
			pos.y = 0;
			obj.transform.position = pos;
		}

		foreach (GameObject obj in SStones)
		{
			pos = obj.transform.position;
			pos.y = 0;
			obj.transform.position = pos;
			obj.transform.parent = null;
		}

		foreach (GameObject obj in BStones)
		{
			pos = obj.transform.position;
			pos.y = 0;
			obj.transform.position = pos;
			obj.transform.parent = null;
		}

		foreach (GameObject obj in GoalsA)
		{
			pos = obj.transform.position;
			pos.y = 0;
			obj.transform.position = pos;
			obj.transform.parent = null;
		}
		foreach (GameObject obj in GoalsB)
		{
			pos = obj.transform.position;
			pos.y = 0;
			obj.transform.position = pos;
			obj.transform.parent = null;
		}
	}
	// End of ForceCorrectInitialTransforms()


	// Create ISSR_OBjects for identifying every object in scene
	void CreateISSRObjects()
	{
		if (SStones.Length>0)
		{
			OSStones = new ISSR_Object[SStones.Length];
			for (int i=0 ; i < SStones.Length; i++)
			{
				OSStones [i] = new ISSR_Object (SStones [i], i);
			}
		}

		if (BStones.Length>0)
		{
			OBStones = new ISSR_Object[BStones.Length];
			for (int i=0 ; i < BStones.Length; i++)
			{
				OBStones [i] = new ISSR_Object (BStones [i], i);
			}
		}


		if (TeamApresent)
		{
			OGoalA = new ISSR_Object (GoalsA [0], 0);
		}

		if (TeamBpresent)
		{
			OGoalB = new ISSR_Object (GoalsB [0], 0);
		}

		ONorthWall = new ISSR_Object (NorthWall, 0);
		OSouthWall = new ISSR_Object (SouthWall, 0);
		OEastWall = new ISSR_Object (EastWall, 0);
		OWestWall = new ISSR_Object (WestWall, 0);
	}
	// End of void CreateISSRObjects()----------------------------------------------------------


    void InitError()
    {
        if (automatic)
        {
            this.TesterBehaviour.InitError();
        }
    }

    bool TeamValid(int ti)
    {
        bool valid = false;

        if (ti ==0)
        {
            valid = TeamApresent & TeamARegistered;
        }
        else
        {
            valid = TeamBpresent & TeamBRegistered;
        }
        return valid;
    }
	// Obtain information from scene and prepare everything

	void Awake()
	{
        Tester = GameObject.FindGameObjectWithTag("Tester");

        if (Tester != null)
        {
            TesterBehaviour = Tester.GetComponent<ISSR_TesterBehaviour>();
            automatic = true;
        }
        else
            automatic = false;

		this.MyBehaviour = this.GetComponent<ISSR_ManagerBehaviour> ();
		this.initerror = true;
		this.InspectScene ();
		if (this.initerror)
		{
            InitError();

            return;
		}

		this.ManagerAudioSource = this.GetComponent <AudioSource> ();
		this.CreateISSRObjects ();

		this.Teams = new ISSR_TeamDescriptor[2];



	}

	void HideNotices()
	{
		TimeExhausted.SetActive (false);
		StonesGO.SetActive (false);
		TaskCompleted.SetActive (false);
		RedTeamWins.SetActive (false);
		BlueTeamWins.SetActive (false);
		Cover.SetActive (false);
        StatisticsPanel.SetActive(false);
        statistics_on = false;
		StartMenu.SetActive (false);
		Controls.SetActive (false);
        controls_on = false;
		TeamLongNameGO.SetActive (false);
		ScenarioNameGO.SetActive (false);
	}


	public IEnumerator MainMenu()
	{
		bool start_game = false;
		//bool controls_on = false;

		StartMenu.SetActive (true);

        start_game = automatic;

		while (!start_game)
		{
			if (Input.anyKeyDown)
			{
				StartMenu.SetActive (false);
			}

			if (Input.GetKeyDown (KeyCode.Return))
			{
				start_game = true;
                Controls.SetActive(false);
            }

			yield return null;
		}

		StartMenu.SetActive (false);
		Controls.SetActive (false);
        controls_on = false;
        StatisticsPanel.SetActive(false);
        statistics_on = false;
		ScenarioNameGO.SetActive (true);
		ScenarioName.text = String.Format ("{0}\nStarts...", this.ScenarioTitle);
		StartGame ();

	}

	void SetupUI()
	{
		AAgentSprite = GameObject.Find ("AAgentSprite");

		BAgentSprite = GameObject.Find ("BAgentSprite");

        AAgentBlockedText = GameObject.Find("AAgentBlocked");
        AAgentBlockedText.SetActive(false);
        BAgentBlockedText = GameObject.Find("BAgentBlocked");
        BAgentBlockedText.SetActive(false);

        ClockSprite = GameObject.Find ("ClockSprite");
		TeamAScoreText = GameObject.Find ("TeamAScore").GetComponent<Text> ();
        TeamAScoreAnim = GameObject.Find("TeamAScore").GetComponent<Animator>();
		TeamBScoreText = GameObject.Find ("TeamBScore").GetComponent<Text> ();
        TeamBScoreAnim = GameObject.Find("TeamBScore").GetComponent<Animator>();
        TeamANameText = GameObject.Find ("TeamAName").GetComponent<Text> ();
		TeamBNameText = GameObject.Find ("TeamBName").GetComponent<Text> ();
		RemainingScore = GameObject.Find ("RemainingScore").GetComponent<Text> ();
		TimeLeftText = GameObject.Find ("RemainingTime").GetComponent<Text> ();
		Cover = GameObject.Find ("Cover");
		StartMenu = GameObject.Find ("StartMenu");
		Controls = GameObject.Find ("Controls");

		TimeExhausted = GameObject.Find ("TimeExhausted");
		ScenarioNameGO =  GameObject.Find ("ScenarioName");
		ScenarioName = ScenarioNameGO.GetComponent<Text> ();
		StonesText = GameObject.Find ("StonesText").GetComponent<Text>();
		StonesGO = GameObject.Find ("StonesText");
		TaskCompleted = GameObject.Find ("TaskCompleted");
		RedTeamWins= GameObject.Find ("RedTeamWins");
		BlueTeamWins= GameObject.Find ("BlueTeamWins");
		TeamLongNameGO = GameObject.Find ("TeamLongName");
		TeamLongName = TeamLongNameGO.GetComponent<Text> ();

        StatisticsPanel = GameObject.Find("StatisticsPanel");
        GetStatsReferences();

        HideNotices ();

		if (ScenarioTitle.Length!=0)
		{
			ScenarioName.text = this.ScenarioTitle;
			ScenarioNameGO.SetActive (true);
		}

		if (!TeamApresent)
		{
			AAgentSprite.SetActive (false);
			TeamAScoreText.gameObject.SetActive (false);
			TeamANameText.gameObject.SetActive (false);
		}
		else
		{
			TeamANameText.text = Teams [0].ShortName;


		}

		if (!TeamBpresent)
		{
			BAgentSprite.SetActive (false);
			TeamBScoreText.gameObject.SetActive (false);
			TeamBNameText.gameObject.SetActive (false);
		}
		else
		{
			TeamBNameText.text = Teams [1].ShortName;
		}

		if (this.TimeBudget!=0)
		{
			this.TimeLeft = this.TimeBudget;
			TimeLeftTextUpdate ();

		}
		else
		{
			TimeLeftText.gameObject.SetActive (false);
			ClockSprite.SetActive (false);
		}

		this.pointsleft = this.TotalPoints;
		this.RemainingScore.text = this.pointsleft.ToString ();

	}

    public void UpdateStats()
    {
        if (StatisticsPanel.activeSelf)
        {
            int time_elapsed = TimeBudget - TimeLeft;
            int minutes = time_elapsed / 60;
            int seconds = time_elapsed - (minutes * 60);
            STimeElapsed.text = string.Format("{0,2:D2}:{1,2:D2}", minutes, seconds);

            STScore.text = string.Format("{1}/{0}", this.TotalPoints, this.TotalPoints-this.pointsleft);
            STSStones.text = string.Format("{1}/{0}", this.SStones.Length, this.SStonesCatched);
            STBStones.text = string.Format("{1}/{0}", this.BStones.Length, this.BStonesCatched);

            if (TeamApresent)
            {
                SAScore.text = string.Format("{1}/{0}", this.TotalPoints, Teams[0].Score);
                SASStones.text = string.Format("{1}/{0}", this.SStones.Length, Teams[0].SStonesCatched);
                SABStones.text = string.Format("{1}/{0}", this.BStones.Length, Teams[0].BStonesCatched);
                SADistance.text = string.Format("{0:0.0}", Teams[0].distance_covered);
                SACollisions.text = Teams[0].number_of_collisions.ToString();
                SAMessages.text = Teams[0].number_of_messages.ToString();
                if (!TeamBpresent) SAPercent.text = string.Format("{0:0.0}%", 100f * Teams[0].Score / this.TotalPoints);

            }

            if (TeamBpresent)
            {
                SBScore.text = string.Format("{1}/{0}", this.TotalPoints, Teams[1].Score);
                SBSStones.text = string.Format("{1}/{0}", this.SStones.Length, Teams[1].SStonesCatched);
                SBBStones.text = string.Format("{1}/{0}", this.BStones.Length, Teams[1].BStonesCatched);
                SBDistance.text = string.Format("{0:0.0}", Teams[1].distance_covered);
                SBCollisions.text = Teams[1].number_of_collisions.ToString();
                SBMessages.text = Teams[1].number_of_messages.ToString();
                if (!TeamApresent) SBPercent.text = string.Format("{0:0.0}%", 100f * Teams[1].Score / this.TotalPoints);
            }

            if ((TeamApresent) && (TeamBpresent))
            {
                if (Teams[0].Score> Teams[1].Score)
                {
                    SAMedal.SetActive(true);
                    SBMedal.SetActive(false);
                }
                else
                {
                    if (Teams[0].Score < Teams[1].Score)
                    {
                        SAMedal.SetActive(false);
                        SBMedal.SetActive(true);
                    }
                    else
                    {
                        SAMedal.SetActive(false);
                        SBMedal.SetActive(false);
                    }
                }
            }

        }
    }

    public void SetUpStats()
    {
        STScore.text = string.Format("0/{0}", this.TotalPoints);
        STSStones.text = string.Format("0/{0}", this.SStones.Length);
        STBStones.text = string.Format("0/{0}", this.BStones.Length);
        SNAgents.text = this.AgentsPerTeam.ToString();
        SStonesCatched = 0;
        BStonesCatched = 0;
        SAMedal.SetActive(false);
        SBMedal.SetActive(false);
        SAPercent.text = "";
        SBPercent.text = "";

        Transform RedTeamT = RedTeamData.transform;
        Transform BlueTeamT = BlueTeamData.transform;
        Vector3 pos = RedTeamT.position;
        if (!TeamApresent)
        {
            
            RedTeamT.position = BlueTeamT.position;
        }
        BlueTeamData.SetActive(TeamApresent);
        RedTeamData.SetActive(TeamBpresent);



    }

    public void GetStatsReferences()
    {
        STScore = GameObject.Find("STScore").GetComponent<Text>();
        STSStones = GameObject.Find("STSStones").GetComponent<Text>(); 
        STBStones = GameObject.Find("STBStones").GetComponent<Text>(); 
        STimeElapsed = GameObject.Find("STimeElapsed").GetComponent<Text>();
        SNAgents = GameObject.Find("SNAgents").GetComponent<Text>();

        BlueTeamData = GameObject.Find("BlueTeamData");
        SAScore = GameObject.Find("SAScore").GetComponent<Text>(); 
        SASStones = GameObject.Find("SASStones").GetComponent<Text>(); 
        SABStones = GameObject.Find("SABStones").GetComponent<Text>(); 
        SADistance = GameObject.Find("SADistance").GetComponent<Text>(); 
        SAMessages = GameObject.Find("SAMessages").GetComponent<Text>(); 
        SACollisions = GameObject.Find("SACollisions").GetComponent<Text>(); 
        SAMedal = GameObject.Find("SAMedal");
        SAPercent = GameObject.Find("SAPercent").GetComponent<Text>();


        RedTeamData = GameObject.Find("RedTeamData") ;
        SBScore = GameObject.Find("SBScore").GetComponent<Text>(); 
        SBSStones = GameObject.Find("SBSStones").GetComponent<Text>(); 
        SBBStones = GameObject.Find("SBBStones").GetComponent<Text>(); 
        SBDistance = GameObject.Find("SBDistance").GetComponent<Text>(); 
        SBMessages = GameObject.Find("SBMessages").GetComponent<Text>(); 
        SBCollisions = GameObject.Find("SBCollisions").GetComponent<Text>();
        SBMedal = GameObject.Find("SBMedal");
        SBPercent = GameObject.Find("SBPercent").GetComponent<Text>();
    }


    // I have altered Scripts execution order for the Manager to be executed after all the other scripts
    //   so that in the case of Start it is executed after the TeamManagers
    void Start () 
	{


	
		Debug.Log("Start of ISSR Manager");

		if (ISSRextra.ActivateSpecial())
        {
			Debug.Log("Especial");
        }
        else
        {
			Debug.Log("NO Especial");
        }

		if (this.initerror)
		{
			Debug.LogError ("MANAGER: No Start after Awake because of initerror");
			return;
		}

		// Check if teams are correctly registered---------------------------------------

		if (this.TypeOfGame == ISSR_GameScenario.Undefined)
		{
			this.initerror = true;
			Debug.LogError ("MANAGER: No team has been registered");
            InitError();
            return;
		}

		if (this.TypeOfGame == ISSR_GameScenario.Cooperative)
		{
			if (this.TeamApresent && this.TeamBpresent)
			{
				this.initerror = true;
				Debug.LogError ("MANAGER: Markers for two teams where available and only one was registered");
                InitError();
                return;
			}
		}

		this.SetupUI ();

		this.SetupCamera ();

        this.SetUpStats();

		StartCoroutine (MainMenu ());


	}


	public void StartGame()
	{
		ISSR_TeamDescriptor Team;

		//---------------------------------------------------------------------
		// Enable behaviours, inside each ISSR_AgentBahavior calls agents start functions and start coroutines
		for (int t=0; t < 2; t++)
		{
			Team = this.Teams [t];

			if ((Team!= null)&&(Team.TeamRegistered))
			{
				Debug.LogFormat ("MANAGER: Activating behaviours for \"{0}\"({1}) Team, they will be the {2} ones.", 
					Team.LongName, Team.ShortName, (t==0)?"BLUE":"RED");
				for (int i=0; i < Team.NAgents; i++)
				{
					Team.Behaviours [i].enabled = true;  // This automatically launches the Start() functions of agents
				}
			}

		}

		if (this.TimeBudget != 0)
		{
			StartCoroutine (TimeUpdateCorroutine());
		}

		StartCoroutine (WaitAndRemoveTitle ());

		this.PlaySfx (ManagerSFX.start);
        StartCoroutine(AgentsWatchCorroutine());

        if (automatic)
        {
            TesterBehaviour.GameParams(this.AgentsPerTeam, (TeamValid(0))? Teams[0].ShortName:"",
                  (TeamValid(1)) ? Teams[1].ShortName:"", this.TimeBudget, this.TotalPoints, this.BStones.Length, this.SStones.Length);
        }
	}

	#endregion
	// -------------end of INIT METHODS

	#region  CAMERA MANAGEMENT

	void SetupCamera()
	{
		this.ZenithCamera = GameObject.Find ("ZenithCamera").GetComponent<Camera> ();
		this.MainCamera   = GameObject.Find ("MainCamera").GetComponent<Camera> ();
		this.CameraTarget = GameObject.Find ("CameraTarget");
		this.AnimatingCameraTarget = true;
		this.camera_speed = 20;

		this.ZenithCamera.enabled = false;
		this.MainCamera.enabled = true;
		this.CameraZenithActive = false;

		this.RedArrowBill = GameObject.Find ("RedArrowBill");
		this.BlueArrowBill = GameObject.Find ("BlueArrowBill");
		this.OccludingWall = GameObject.Find ("OccludingWall");

		if (TeamApresent )
		{
			this.CameraTeamInfocus = 0;
		}
		else
		{
			this.CameraTeamInfocus = 1;
		}

		this.CameraAgentInfocus = 0;
		this.CameraAgentAInfocus = 0;
		this.CameraAgentBInfocus = 0;

		this.MainCamera.transform.rotation = Quaternion.Euler (new Vector3 (this.CameraTiltAngle, 0, 0));
		this.CameraTargetVector = new Vector3 (0, this.CameraHeight, -this.CameraZDistance);
		//Debug.LogFormat ("Vector: {0}", this.CameraTargetVector);

		this.CameraObjectInFocus = GetAgentInFocus();
		this.OccludingWall.SetActive (false);

	}

	GameObject GetAgentInFocus()
	{
		//GOInFocus = null;

		if (this.CameraTeamInfocus ==0)
		{
			this.AgentInFocusBehav = AgentsABehaviors [this.CameraAgentInfocus];


			return AgentsA [this.CameraAgentInfocus];
		}
		else
		{
			this.AgentInFocusBehav = AgentsBBehaviors [this.CameraAgentInfocus];

			return AgentsB [this.CameraAgentInfocus];
		}
	}

	void ToggleMap()
	{
		this.CameraZenithActive = !this.CameraZenithActive;

		if (this.CameraZenithActive)
		{
			this.ZenithCamera.enabled = true;
			this.MainCamera.enabled = false;
			this.OccludingWall.SetActive (true);
			this.RedArrowBill.SetActive (false);
		}
		else
		{
			this.ZenithCamera.enabled = false;
			this.MainCamera.enabled = true;
			this.OccludingWall.SetActive (false);
			this.RedArrowBill.SetActive (true);
		}
	}

	void StartAnimatingCameraTarget()
	{
		this.AnimatingCameraTarget = true;
		this.camera_speed = 30;
        RedArrowBill.SetActive(true);
    }

	void UpdateCamera()
	{

		if (Input.GetKeyDown(KeyCode.M))
		{
			ToggleMap ();
		}

		if (Input.GetKeyDown (KeyCode.A)) 
		{
			if (this.CameraZenithActive)
			{
				ToggleMap ();
			}

			if (!this.CameraGoalInFocus)
			{
				this.CameraAgentInfocus++;

				if (this.CameraAgentInfocus == this.AgentsPerTeam)
				{
					this.CameraAgentInfocus = 0;
				}
			}
			else
			{
				this.CameraGoalInFocus = false;
			}


			this.CameraObjectInFocus = GetAgentInFocus();
			StartAnimatingCameraTarget ();
		}

		if (Input.GetKeyDown (KeyCode.G)) 
		{
			if (this.CameraZenithActive)
			{
				ToggleMap ();
			}

			if (this.CameraGoalInFocus)
			{
				this.CameraObjectInFocus = GetAgentInFocus();
				this.CameraGoalInFocus = false;
				StartAnimatingCameraTarget ();
			}
			else
			{
				if (this.CameraTeamInfocus == 0) 
				{
					this.CameraObjectInFocus = GoalsA [0];
				}
				else
				{
					this.CameraObjectInFocus = GoalsB[0];
				}
				this.CameraGoalInFocus = true;
				StartAnimatingCameraTarget ();
			}
		}

		if (Input.GetKeyDown (KeyCode.T)) 
		{
			if (this.CameraZenithActive)
			{
				ToggleMap ();
			}

			if (TypeOfGame == ISSR_GameScenario.Competitive)
			{
				if (this.CameraTeamInfocus==0)
				{
					this.CameraAgentAInfocus = this.CameraAgentInfocus;
					this.CameraAgentInfocus = this.CameraAgentBInfocus;
					this.CameraTeamInfocus = 1;
				}
				else
				{
					this.CameraAgentBInfocus = this.CameraAgentInfocus;
					this.CameraAgentInfocus = this.CameraAgentAInfocus;
					this.CameraTeamInfocus = 0;
				}
				this.CameraObjectInFocus = GetAgentInFocus();
				StartAnimatingCameraTarget ();
			}
		}


		if (CameraObjectInFocus!= null)
		{
			this.RedArrowBill.transform.position = this.CameraObjectInFocus.transform.position + Vector3.up * 2.5f;
		}

		if (AnimatingCameraTarget)
		{
			Vector3 trayectory;

			trayectory = CameraObjectInFocus.transform.position - CameraTarget.transform.position;

			if (trayectory.magnitude < 0.05f)
			{
				this.AnimatingCameraTarget = false;
                StartCoroutine(WaitToHideCameraTarget());
                
			}
			else
			{
                float fraction = this.camera_speed * Time.deltaTime / trayectory.magnitude;

                this.CameraTarget.transform.position = Vector3.Lerp(this.CameraTarget.transform.position, CameraObjectInFocus.transform.position, fraction);
			}
		}

		if (superuser_debugging)
		{
			SuperuserDebugging ();
		}
	}

	void SuperuserDebugging()
	{
		bool error;

		// Object on focus-------------------------------------------------
		if (Input.GetKeyDown (KeyCode.O))
		{
			if (this.CameraGoalInFocus)
			{
				GOInFocus = CameraObjectInFocus;
			}

			else
			{
				if (GOInFocus == null)
				{
					indexObjInFocus = 0;

				}
				else
				{
					indexObjInFocus++;
				}

				if (this.AgentInFocusBehav.AgentDescriptor.SensableObjects.Count > indexObjInFocus)
				{
					this.ObjInFocus = this.AgentInFocusBehav.AgentDescriptor.SensableObjects [indexObjInFocus];
					this.GOInFocus = Object2GameObject (this.ObjInFocus,out error);
				}
				else
				{
					if (this.AgentInFocusBehav.AgentDescriptor.SensableObjects.Count>0)
					{
						this.ObjInFocus = this.AgentInFocusBehav.AgentDescriptor.SensableObjects [0];
						this.GOInFocus = Object2GameObject (this.ObjInFocus, out error);
						indexObjInFocus = 0;
					}
				}


			}

		}

		if (GOInFocus!= null)
		{
			this.BlueArrowBill.transform.position = this.GOInFocus.transform.position + Vector3.up * 2.25f;
		}

		// Agent movement -----------------------------------------------
		if (Input.GetKeyDown (KeyCode.UpArrow))
		{
			debugdestination = this.CameraObjectInFocus.transform.position + Vector3.forward * 4;
			this.AgentInFocusBehav.AgentDescriptor.acGotoLocation (debugdestination);
		}
		if (Input.GetKeyDown (KeyCode.DownArrow))
		{
			debugdestination = this.CameraObjectInFocus.transform.position - Vector3.forward * 4;
			this.AgentInFocusBehav.AgentDescriptor.acGotoLocation (debugdestination);
		}
		if (Input.GetKeyDown (KeyCode.LeftArrow))
		{
			debugdestination = this.CameraObjectInFocus.transform.position - Vector3.right * 4;
			this.AgentInFocusBehav.AgentDescriptor.acGotoLocation (debugdestination);
		}
		if (Input.GetKeyDown (KeyCode.RightArrow))
		{
			debugdestination = this.CameraObjectInFocus.transform.position + Vector3.right * 4;
			this.AgentInFocusBehav.AgentDescriptor.acGotoLocation (debugdestination);
			this.AgentInFocusBehav.AgentDescriptor.acCheckError ();
		}

		if (Input.GetKeyDown (KeyCode.Alpha0))
		{
			if (GOInFocus!= null)
			{
				ObjInFocus = GameObject2Object (GOInFocus);
				this.AgentInFocusBehav.AgentDescriptor.acGripObject (ObjInFocus);
				this.AgentInFocusBehav.AgentDescriptor.acCheckError ();
			}
		}

		if (Input.GetKeyDown (KeyCode.P))
		{
			if (GOInFocus!= null)
			{
				ObjInFocus = GameObject2Object (GOInFocus);

				if ((ObjInFocus.type == ISSR_Type.GoalA) || (ObjInFocus.type == ISSR_Type.GoalB))
				{
					debugdestination = this.GOInFocus.transform.position;
					this.AgentInFocusBehav.AgentDescriptor.acGotoLocation (debugdestination);
					this.AgentInFocusBehav.AgentDescriptor.acCheckError ();
				} 
				else 
				{
					this.AgentInFocusBehav.AgentDescriptor.acGotoObject (ObjInFocus);
					Debug.LogFormat ("SUPER: {0} wants to go to {1}", this.AgentInFocusBehav.AgentDescriptor.Myself.Name, ObjInFocus.Name);
					this.AgentInFocusBehav.AgentDescriptor.acCheckError ();
				}
			}
		}

		if (Input.GetKeyDown (KeyCode.U))
		{
			this.AgentInFocusBehav.AgentDescriptor.acUngrip ();
			this.AgentInFocusBehav.AgentDescriptor.acCheckError ();

		}

		if (Input.GetKeyDown (KeyCode.I))
		{
			this.AgentInFocusBehav.AgentDescriptor.acStop ();
			this.AgentInFocusBehav.AgentDescriptor.acCheckError ();
		}

	}
	#endregion

	// ----------Game State Management
	#region GAME MANAGEMENT


	void LateUpdate()
	{
		if (!this.CameraZenithActive)
		{
			if (CameraObjectInFocus!= null)
			{
				// this.MainCamera.transform.position = this.CameraObjectInFocus.transform.position + this.CameraTargetVector;
				if (!AnimatingCameraTarget)
				{
					this.CameraTarget.transform.position = this.CameraObjectInFocus.transform.position;

				}

				this.MainCamera.transform.position = this.CameraTarget.transform.position + this.CameraTargetVector;

			}
		}
	}

	// Update is called once per frame
	void Update () 
	{
	
		if (this.initerror)
		{
			return;
		}

		UpdateCamera ();

        if (Input.GetKeyDown(KeyCode.C))
        {
            controls_on = !controls_on;
            Controls.SetActive(controls_on);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            statistics_on = !statistics_on;
            StatisticsPanel.SetActive(statistics_on);
            if (statistics_on)
            {
                this.UpdateStats();
            }
        }

    }

	public IEnumerator TimeUpdateCorroutine()
	{
		while (this.TimeLeft > 0)
		{
			yield return new WaitForSeconds (1);
			TimeLeft--;
			TimeLeftTextUpdate ();
		}
		this.EndGame ();
	}

	public void TimeLeftTextUpdate()
	{
		int minutes;
		int seconds;

		if (TimeLeft < 10)
		{
			this.PlaySfx (ManagerSFX.bip);
			TimeLeftText.color = new Color (1, 0, 0);
		}
		minutes = TimeLeft / 60;
		seconds = TimeLeft - (minutes * 60);
		TimeLeftText.text = string.Format ("{0,2:D2}:{1,2:D2}", minutes, seconds); 
	}


	public void PlaySfx( ManagerSFX effect)
	{
		this.ManagerAudioSource.clip = this.sound_effects [(int)effect];
		this.ManagerAudioSource.Play ();
	}

	public void EndGame()
	{
		
		StopAllCoroutines ();

		StartCoroutine (EndofGameCoroutine());
        

    }

	public IEnumerator EndofGameCoroutine()
	{
		int i;
		bool TeamAWins= false, TeamBWins= false;

		yield return new WaitForSeconds (0.7f);
		this.PlaySfx (ManagerSFX.end);

        

        yield return new WaitForSeconds (1.3f);



		// Win Loose Message
		if ((TimeBudget!=0)&&(TimeLeft == 0))
		{
			this.TimeExhausted.SetActive (true);
		}

		StonesGO.SetActive (true);
		if (pointsleft != 0)
		{
			StonesText.text =  string.Format("{0} Remaining points :(", pointsleft);
		}

		if (TypeOfGame == ISSR_GameScenario.Cooperative)
		{  // Cooperativo, un solo equipo
			if (pointsleft ==0)
			{
				this.TaskCompleted.SetActive (true);
				if (TeamApresent)
				{
					BlueTeamWins.SetActive (true);
					TeamLongName.text = String.Format( "{0}: \"{1}\"", Teams[0].ShortName, Teams [0].LongName);
					TeamLongName.color = new Color (0, 0, 1);
					TeamAWins = true;
				}
				else
				{
					RedTeamWins.SetActive (true);
					TeamLongName.text = String.Format( "{0}:\"{1}\"", Teams[1].ShortName, Teams [1].LongName);
					TeamLongName.color = new Color (1, 0, 0);
					TeamBWins = true;
				}
				TeamLongNameGO.SetActive (true);

				this.PlaySfx (ManagerSFX.success);
			}
			else
			{
				this.PlaySfx (ManagerSFX.failure);
			}
		}
        else // Competitivo, dos equipos
        { 
			if (Teams[0].Score > Teams[1].Score)
			{
				BlueTeamWins.SetActive (true);
				TeamLongName.text = String.Format( "{0}: \"{1}\"", Teams[0].ShortName, Teams [0].LongName);
				TeamLongName.color = new Color (0, 0, 1);
				TeamLongNameGO.SetActive (true);
				TeamAWins = true;
			}
			else if (Teams[0].Score < Teams[1].Score)
			{
				RedTeamWins.SetActive (true);
				TeamLongName.text = String.Format( "{0}: \"{1}\"", Teams[1].ShortName, Teams [1].LongName);
				TeamLongName.color = new Color (1, 0, 0);
				TeamLongNameGO.SetActive (true);
				TeamBWins = true;
			}
			else 
			{
				TeamLongName.text = "Draw: Same Score";
				TeamLongName.color = new Color (0.3f, 0, 0.3f);
				TeamLongNameGO.SetActive (true);
				TeamAWins = true;
				TeamBWins = true;
			}
		}



        for (i=0; i < BStonesBehaviors.Length; i++)
        {
            BStonesBehaviors[i].GameEnded();
        }

		if (TeamApresent)
		{
			for (i=0; i < AgentsABehaviors.Length; i++)
			{
				AgentsABehaviors [i].EndExecution (TeamAWins);
			}
		}

		if (TeamBpresent)
		{
			for (i=0; i < AgentsBBehaviors.Length; i++)
			{
				AgentsBBehaviors [i].EndExecution (TeamBWins);
			}
		}


		superuser_debugging = false;  // Disable actions if any

        

        // After a pause removes finish Notices
        StartCoroutine (WaitAndRemoveNotices ());


	}

	public IEnumerator WaitAndRemoveNotices()
	{
		yield return new WaitForSeconds (2);
        if (automatic)
        {
            HideNotices();
            TesterBehaviour.ExecData(this.TimeBudget - this.TimeLeft,
               TeamValid(0) ? Teams[0].Score : 0, TeamValid(1) ? Teams[1].Score : 0,
               TeamValid(0) ? Teams[0].SStonesCatched : 0, TeamValid(1) ? Teams[1].SStonesCatched : 0,
               TeamValid(0) ? Teams[0].BStonesCatched : 0, TeamValid(1) ? Teams[1].BStonesCatched : 0);
           
        }
        else
        {
            yield return new WaitForSeconds(3);
            HideNotices();
        }
        
	}


    public IEnumerator WaitToHideCameraTarget()
    {
        yield return new WaitForSeconds(0.5f);
        RedArrowBill.SetActive(false);
    }



	public IEnumerator WaitAndRemoveTitle()
	{
		yield return new WaitForSeconds (2);
		ScenarioNameGO.SetActive (false);
	}

    public IEnumerator AgentsWatchCorroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.2f);
            // Eack 1/10 of a second measures distances covered and detects blocked agents
            AgentsWatch();
        }
    }

    int agent_watch_tick;
    int agents_watch_period = 20;  //  agents_watch_period*0.2 seconds : Watch period
    float min_distance_covered_in_watch_period = 1;

    public void AgentsWatch()
    {
        
        ISSR_TeamDescriptor team;
        ISSR_AgentBehaviour[] AgentBeh;
        Vector3 position;
        float distance;
        int number_of_blocked_agents;
        int number_of_blocked_teams =0;

        agent_watch_tick++;

        for (int t=0; t < 2;  t++ )
        {
            team = Teams[t];

            if ((team!=null) && team.TeamRegistered)  
            {
                number_of_blocked_agents = 0;
                if (t == 0)
                    AgentBeh = AgentsABehaviors;
                else
                    AgentBeh = AgentsBBehaviors;


                for (int ai =0; ai< AgentBeh.Length; ai++)
                {
                    position = AgentBeh[ai].transform.position;

                    distance = (position - AgentBeh[ai].last_position).magnitude;
                    AgentBeh[ai].last_position = position;
                    team.distance_covered += distance;

                    AgentBeh[ai].distance_covered_in_watch_period += distance;

                    if (agent_watch_tick == agents_watch_period)
                    {
                        

                        if (AgentBeh[ai].distance_covered_in_watch_period < min_distance_covered_in_watch_period)
                        {
                            AgentBeh[ai].blocked_in_watch_period = true;
                            number_of_blocked_agents++;
                        }
                        else
                        {
                            AgentBeh[ai].blocked_in_watch_period = false;
                        }
                        AgentBeh[ai].distance_covered_in_watch_period = 0;
                    }
                }  
                // For agents in team

                if (number_of_blocked_agents== AgentBeh.Length)
                {
                    team.blocked = true;
                    number_of_blocked_teams++;
                    if (t==0)
                    {
                        AAgentBlockedText.SetActive(true);
                    }
                    else
                    {
                        BAgentBlockedText.SetActive(true);
                    }

                    
                }

            }
        }

        if (agents_watch_period % 2 == 0)
        {
            if (statistics_on)
            {
                UpdateStats();
            }
        }

        if (agent_watch_tick == agents_watch_period)
            agent_watch_tick = 0;


        if (number_of_blocked_teams>0)
        {
            if (this.TypeOfGame == ISSR_GameScenario.Cooperative)
            {
                EndGame();
            }
            else
            {
                if (number_of_blocked_teams ==2)
                {
                    EndGame();
                }
            }
        }


    }


  

	public void ScoreUpdate(GameObject agent, GameObject stone, GameObject goal)
	{
		int points;



		points = (stone.tag == "SmallStone") ? 1 : 3;
		this.PlaySfx (ManagerSFX.score);
		if (agent!= null)
		{
			Debug.LogFormat ("GAME {0} scored with {1} ({2} points) in {3}", agent.name, stone.name, 
				points, goal.name);
		}
		else
		{
			Debug.LogFormat ("GAME {0} scored ({1} points) in {2}", stone.name, 
				points, goal.name);
		}

	
		if (goal.tag=="GoalA")
		{
			Teams [0].Score += points;
            if (stone.tag == "SmallStone")
            {
                Teams[0].SStonesCatched++;
            }
            else
            {
                Teams[0].BStonesCatched++;
            }

                TeamAScoreText.text = Teams [0].Score.ToString ();
            TeamAScoreAnim.SetTrigger("scale");
		}
		else
		{
			Teams [1].Score += points;
            if (stone.tag == "SmallStone")
            {
                Teams[1].SStonesCatched++;
            }
            else
            {
                Teams[1].BStonesCatched++;
            }
            TeamBScoreText.text = Teams [1].Score.ToString ();
            TeamBScoreAnim.SetTrigger("scale");
        }

        if (stone.tag == "SmallStone")
        {
            SStonesCatched++;
        }
        else
        {
            BStonesCatched++;
        }

        this.pointsleft -= points;
		this.RemainingScore.text = this.pointsleft.ToString ();

		if (this.pointsleft ==0)
		{
            this.UpdateStats();
            this.EndGame ();
		}
	}


	#endregion
	// END OF GAME MANAGEMENT Methods (game state, etc.)-----------------------------------------------------





}   /// End of ISSRManagerBehaviour
