using UnityEngine;
using System.Collections;

// ----------------------------------------------------------------------------------------
//  File: ISSR_Message.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//    Definition of Message Syntax
// ----------------------------------------------------------------------------------------

[System.Serializable]
public class ISSR_Message {

	/// <summary>
	/// The sender of the message with its field Lastlocation just updated in the moment of sending
	/// so that the receiver can know where the sender was in the moment it sent the message
	/// </summary>

	[SerializeField]
	private ISSR_Object 	_Sender;		
	public  ISSR_Object     Sender
	{
		get { return _Sender;}
		set { _Sender = new ISSR_Object(value);}
	}

	public ISSRMsgCode 	code;    	// Main code
	public int			usercode;	// User defined code

	// A vector, posibly a location in the Gameyard
	[SerializeField]
	private Vector3		_location;	
	public  Vector3		location
	{
		get { return _location;}
		set { _location = new Vector3 (value.x, value.y, value.z);}
	}

	// An object descriptor:     
	/// <summary>
	/// Important notice: the object is sent in its state
	/// </summary>
	[SerializeField]
	private ISSR_Object 	_Obj;		
	public  ISSR_Object     Obj
	{
		get { return _Obj;}
		set { _Obj = new ISSR_Object(value);}
	}

	public float		fvalue;		// A float value
	public int			ivalue;		// An integer value



	/// <summary>
	/// Initializes a new instance of the <see cref="ISSR_Message"/> class.
	/// </summary>
	/// <param name="code">Code.</param>
	/// <param name="ucode">Ucode.</param>
	/// <param name="location">Location.</param>
	/// <param name="Obj">Object.</param>
	/// <param name="fvalue">Fvalue.</param>
	/// <param name="ivalue">Ivalue.</param>
	public ISSR_Message(ISSR_Object Sender, ISSRMsgCode code, int ucode, Vector3 location, ISSR_Object Obj, float fvalue, int ivalue)
	{
		this._Sender = Sender;
		this.code = code;
		this.usercode = ucode;
		this.location = location;
		this.Obj = Obj;
		this.fvalue = fvalue;
		this.ivalue = ivalue;
	}

	/// <summary>
	/// Initializes an empty new instance of the <see cref="ISSR_Message"/> class.
	/// </summary>
	public ISSR_Message()
	{
		this._Sender = new ISSR_Object ();
		this.Obj = new ISSR_Object ();
		this._location = new Vector3 ();
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="ISSR_Message"/> class.
	///   as a copy of a previous message
	/// </summary>
	/// <param name="msg">Message.</param>
	public ISSR_Message(ISSR_Message msg)
	{
		this._Sender = msg.Sender;
		this.code = msg.code;
		this.usercode = msg.usercode;
		this.location = msg.location;
		this.Obj = msg.Obj;
		this.fvalue = msg.fvalue;
		this.ivalue = msg.ivalue;
	}

}

