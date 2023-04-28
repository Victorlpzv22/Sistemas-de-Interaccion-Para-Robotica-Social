using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LURA_Equipo4 : ISSR_TeamBehaviour {

    // Use this for initialization
    public override void CreateTeam()
    {
        if (!InitError())
        {
            if (RegisterTeam("LURA", "LURANO"))
            {
                for(int index=0; index<GetNumberOfAgentsInTeam(); index++)
                {
                    CreateAgent(new LURA_Agente4());
                }
            }
        }
       
    }
}
