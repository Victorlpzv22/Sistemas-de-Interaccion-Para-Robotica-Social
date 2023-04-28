using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
//  File: ISSR_TeamBehaviour.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//     Base class for a Team of Agents Used to Registera Team and Create Agents
//  Each student team has to define a Team Class derived from this one
// ----------------------------------------------------------------------------------------

[System.Serializable]
public abstract class ISSR_TeamBehaviour : MonoBehaviour {

	private ISSR_ManagerBehaviour ISSRManager;

	private ISSR_Type AgentsType;
	private string	  AgentsTag;




	// TEAM SERVICES---------------------------------------------
	#region TEAM SERVICES


	/// <summary>
	///  This function must be code by the student's in the derived class
	///   only this one is requested (see examples)
	///  Creates a team of agents:
	///   -Registers a team  using RegisterTeam()
	///   -Creates agents with behaviors derived from ISSR_Agent
	///     using GetNumberOfAgentsInTeam() to know how much agents has the team
	///     and calling CreateAgent()
	///    Should not register a team or create agents if ISSR_InitError() is true
	/// </summary>
	public abstract void CreateTeam ();   // Must be coded by students



	/// <summary>
	/// Returns True if error in Scene Initialization (no team should be registered)
	/// </summary>
	/// <returns><c>true</c>, if there was an error initializing, <c>false</c> otherwise.</returns>
	public bool InitError()
	{
		return this.ISSRManager.initerror;
	}



	/// <summary>
	///  Registers a team in the scene:  Al marker locations of one color will be assigned to 
	///   the new team for the initial locations of its agents. You cannot know in advance which
	///   team will you get. At most two teams can be registered per game, provided that the same
	///   number of markers for red or blue agents are present.
	///   Returns true if success.
	/// </summary>
	/// <returns><c>true</c>, if team was registered, <c>false</c> otherwise.</returns>
	/// <param name="ShortName">Short name: Exactly 4 characters, it will be shown on the game interface
	///    that should be used as a prefix for all of your classes and other public symbols.</param>
	/// <param name="LongName">A Longer name for the team.</param>
	/// 
	public bool RegisterTeam(string ShortName, string LongName="")
	{
		
		if (ShortName.Length != 4) 
		{
			Debug.LogErrorFormat ("Team ShortName must have exactly 4 characters, \"{0}\" is {1} chars long", ShortName, ShortName.Length);
		}

		// Perhaps check aditional conditions like valid identifier
	
		if (!ISSRManager.RegisterTeam(ShortName, LongName, out this.AgentsType)) 
		{
			return false;
		}
		else
		{
			this.AgentsTag = ISSR_Object.Type2Tag (this.AgentsType);
		}

		return !InitError();
	}

	//TODO some service for obtaining marker locations so that there can be a strategy when creating agents
	//TODO in that case CreateAgent() will also change to allow choosing particular locations.


	/// <summary>
	/// Gets the number of agents per team. 
	///  It is the number of agent markers found.
	///  Should be used to create that number of agents.
	/// </summary>
	/// <returns>The number of agents in team.</returns>
	public int GetNumberOfAgentsInTeam()
	{
		
		return ISSRManager.AgentsPerTeam;
	}


	/// <summary>
	/// Creates an agent. It will be created in one of the locations of the 
	///   markers set for our team. You cannot know in advance where each agent will be located.
	///   You need to provide your agent behaviour in the forma of class derived from ISSR_Agent.
	///   The name of the GameObjects in the scene will be AAAAn  
	///     where AAAA is the short name for the team and n is a number from zero to 9.
	/// </summary>
	/// <param name="AgentDesc">Agent descriptor, the behaviour of the agent, ISSR_Agent</param>
	/// <param name="OptionalName">Optional name to be added to the default name of agents.</param>
	public void CreateAgent( ISSR_Agent AgentDesc, string OptionalName="")
	{
		this.ISSRManager.CreateAgent (AgentDesc, AgentsTag, OptionalName);
	}

	#endregion


	// Internal Functions---------------------------------------------
	#region INTERNAL METHODS
	void Awake()
	{
		// Reference to ISSRManager
		this.ISSRManager = GameObject.Find ("ISSR_Manager").GetComponent<ISSR_ManagerBehaviour>();
		if (this.ISSRManager==null)
		{
			Debug.LogError("No ISSRManager Object");
		}


	}

	// Use this for initialization
	void Start () 
	{
		Debug.Log("Start of Team behaviour");
		this.CreateTeam ();
	} 

	#endregion





}
