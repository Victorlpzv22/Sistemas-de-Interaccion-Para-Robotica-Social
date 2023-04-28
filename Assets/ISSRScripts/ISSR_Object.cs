using UnityEngine;
using System.Collections;
using System;


// ----------------------------------------------------------------------------------------
//  File: ISSR_Object.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//    
// ----------------------------------------------------------------------------------------
// Type of object

public enum ISSR_Type 
{
	Undefined,
	AgentA,
	AgentB,
	SmallStone,
	BigStone,
	GoalA,
	GoalB,
	NorthWall,
	SouthWall,
	EastWall,
	WestWall

};

// Class : ISSR_Object
//  Responsible for representing objects in the game:
//    Agents, Stones, Goals, Walls
[System.Serializable]
public class ISSR_Object : IEquatable<ISSR_Object>
{
	[SerializeField]
	private ISSR_Type _type;    // Type of object :  Always visible
	public	ISSR_Type	type
	{
		get { return _type;}
	}

	private string 		tag;   // Internal

	[SerializeField]
	private GameObject 	GO;  // private but visible in Inspector, not in agent, for debugging purposes

	[SerializeField]        // For agents their number: Always visible
	private int 		_index;
	public	int 		index
	{
		get { return _index;}
	}

	[SerializeField]
	private int 		_InstanceId;
	public  int 		InstanceId
	{
		get { return _InstanceId;}
	}

	public string Name
	{
		get { return this.GO.name;}
	}
	/// <summary>
	/// In this field the location of the center of the object is written 
	///  each time it is read and when entering of exiting the agent sensing range. 
	/// </summary>
	public Vector3 LastLocation;

    /// <summary>
    /// This field has the time in with the object was first seen by OnEnterSensingArea()
    /// </summary>
    public float TimeStamp;

	/// <summary>
	/// Gets the radius of the object used for collision detection.
	/// </summary>
	/// <value>The radius.</value>
	public float Radius
	{
		get 
		{
			switch(this._type)
			{
			case ISSR_Type.AgentA:
			case ISSR_Type.AgentB:
			case ISSR_Type.SmallStone:
				return 0.5f;
			case ISSR_Type.BigStone:
				return 1;
			case ISSR_Type.GoalA:
			case ISSR_Type.GoalB:
				return 0.2f;
			default:
				return 0;
			}
		}
	}

	/// <summary>
	/// Gets the gripping radius of an object
	/// </summary>
	/// <value>The gripping radius.</value>
	public float GrippingRadius
	{
		get 
		{
			switch(this._type)
			{
			case ISSR_Type.AgentA:
			case ISSR_Type.AgentB:
				return 0.3f;
			case ISSR_Type.SmallStone:
				return 0.4f;
			case ISSR_Type.BigStone:
				return 0.9f;
			case ISSR_Type.GoalA:
			case ISSR_Type.GoalB:
				return 0.05f;
			default:
				return 0;
			}
		}
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="ISSR_Object"/> class.
	/// </summary>
	/// <param name="go">GameObject from where it comes, will be hidden.</param>
	/// <param name="index">Index into ISSR_Manager per type Object Tables.</param>
	public ISSR_Object( GameObject go, int index)
	{
		this.GO = go;
		this._index = index;
		this._InstanceId = go.GetInstanceID ();
		this.tag = go.tag;
		this._type = ISSR_Object.Tag2Type (go.tag);
		this.LastLocation = new Vector3 (Mathf.Infinity, 0, Mathf.Infinity);
	}


	/// <summary>
	/// Initializes a new empty instance of the <see cref="ISSR_Object"/> class.
	/// </summary>
	public ISSR_Object () 
	{
		//this.GO = null;
		this._type = ISSR_Type.Undefined;
		this._InstanceId = 0;
		this.LastLocation = new Vector3 ();
    }

	/// <summary>
	/// Copies the contents of obj into this instance of the <see cref="ISSR_Object"/> class.
	/// </summary>
	public ISSR_Object(ISSR_Object obj)
	{
		this.GO 		= obj.GO;
		this._index 	= obj.index;
		if (obj.GO!= null)
		{
			this._InstanceId = obj.GO.GetInstanceID ();
		}
		this.tag 		= obj.tag;
		this._type 		= obj.type;
		this.LastLocation = new Vector3 (obj.LastLocation.x, 0, obj.LastLocation.z);
        this.TimeStamp = obj.TimeStamp;
	}




	/// <summary>
	/// Determines whether the specified <see cref="ISSR_Object"/> is equal to the current <see cref="ISSR_Object"/>.
	/// </summary>
	/// <param name="other">The <see cref="ISSR_Object"/> to compare with the current <see cref="ISSR_Object"/>.</param>
	/// <returns><c>true</c> if the specified <see cref="ISSR_Object"/> is equal to the current <see cref="ISSR_Object"/>;
	/// otherwise, <c>false</c>.</returns>
	public bool Equals(ISSR_Object other)
	{
		//  Debug.Log("ISSRO Equal llamado");
		if (other == null)
		{
			return false;
		}

		else
		{
			return (this.InstanceId == other.InstanceId);
		}

	}




	// Auxiliary Static functions-----------------------------------------------

	public static ISSR_Type Tag2Type( string tag)
	{
		switch (tag)
		{
		case "AgentA":
			return ISSR_Type.AgentA;
		case "AgentB":
			return ISSR_Type.AgentB;
		case "SmallStone":
			return ISSR_Type.SmallStone;
		case "BigStone":
			return ISSR_Type.BigStone;
		case "GoalA":
			return ISSR_Type.GoalA;
		case "GoalB":
			return ISSR_Type.GoalB;
		case "NorthWall":
			return ISSR_Type.NorthWall;
		case "SouthWall":
			return ISSR_Type.SouthWall;
		case "EastWall":
			return ISSR_Type.EastWall;
		case "WestWall":
			return ISSR_Type.WestWall;
		default:
			return ISSR_Type.Undefined;

		}

	}
	// End of Tag2Type()

	public static string Type2Tag( ISSR_Type type)
	{
		switch(type)
		{
		case ISSR_Type.AgentA:
			return "AgentA";
		case ISSR_Type.AgentB:
			return "AgentB";
		case ISSR_Type.BigStone:
			return "BigStone";
		case ISSR_Type.EastWall:
			return "EastWall";
		case ISSR_Type.GoalA:
			return "GoalA";
		case ISSR_Type.GoalB:
			return "GoalB";
		case ISSR_Type.NorthWall:
			return "NorthWall";
		case ISSR_Type.SmallStone:
			return "SmallStone";
		case ISSR_Type.SouthWall:
			return "SouthWall";
		case ISSR_Type.Undefined:
		default:
			return "Ignore";
		}
	}
	// End of Type2Tag





	// End of auxiliary static functions 

	 




	 
	/*------------
	public static ISSR_Object operator ==(ISSR_Object  o1, ISSR_Object  o2)
	{
		if (o1 != null)
		{
			if (o2 != null)
				return (o1.InstanceId== o1.InstanceId);
			else
				return false;
		}
		else
		{
			if (o2!= null)
			{
				return false;
			} 
			else
			{
				return true;
			}
		}
	}

	public static ISSR_Object operator !=(ISSR_Object  o1, ISSR_Object  o2)
	{
		if (o1 != null)
		{
			if (o2 != null)
				return (o1.InstanceId!= o1.InstanceId);
			else
				return true;
		}
		else
		{
			if (o2!= null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
----*/



}
