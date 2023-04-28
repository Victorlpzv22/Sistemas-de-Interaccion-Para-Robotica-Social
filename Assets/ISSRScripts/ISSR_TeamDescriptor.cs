using UnityEngine;
using System.Collections;


// ----------------------------------------------------------------------------------------
//  File: ISSR_.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//    
// ----------------------------------------------------------------------------------------
[System.Serializable]
public class ISSR_TeamDescriptor
{
	public bool 	TeamRegistered;

	public string 	ShortName;
	public string 	LongName;

	public float  	BudgetLeft;

	//ISSR_Type		AgentsType;
	//string			AgentsTag;
	public int		NAgents;

	public int 	    Score;
    public int BStonesCatched;
    public int SStonesCatched;

    public int 		stones_moving;

    // Statistics related
    public float    distance_covered;
    public bool     blocked;
    public int      number_of_collisions;
    public int      number_of_messages;

	public GameObject[] Agents;
	public ISSR_AgentBehaviour[] Behaviours;


	public ISSR_TeamDescriptor(string ShortName, string LongName, ISSR_Type AgentsType, float BudgetLeft, int NumberOfAgents)
	{
		this.TeamRegistered = false;
		this.ShortName = ShortName;

		if (LongName.Length == 0)
		{
			this.LongName = ShortName; 
		}
		else
		{
			this.LongName = LongName;
		}

		this.stones_moving = 0;
		//this.AgentsType = AgentsType;
		//this.AgentsTag = ISSR_Object.Type2Tag (this.AgentsType);
		this.NAgents = 0;
		this.BudgetLeft = BudgetLeft;
		this.Behaviours = new ISSR_AgentBehaviour[NumberOfAgents];
        this.distance_covered = 0;
        this.number_of_collisions = 0;
        this.blocked = false;
        this.number_of_messages = 0;
        BStonesCatched = 0;
        SStonesCatched = 0;

    }

}