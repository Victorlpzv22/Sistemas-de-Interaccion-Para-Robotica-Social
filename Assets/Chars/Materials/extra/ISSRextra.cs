using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public  class ISSRextra  {

	// Use this for initialization
	public static bool ActivateSpecial () 
	{
		DateTime today = DateTime.Now;
		bool especial = false;


		if (today.Month == 12)
		{
			especial = true;
		}
		else if ((today.Month == 1) && (today.Day <= 15))
		{
			especial = true;

		}

	   return especial;
	}
	
	
}
