using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARUL_Equipo7 : ISSR_TeamBehaviour
{

    // Use this for initialization
    public override void CreateTeam()
    {
        if (!InitError())
        {
            if (RegisterTeam("ARUL", "ONARUL"))
            {
                for (int index = 0; index < GetNumberOfAgentsInTeam(); index++)
                {
                    CreateAgent(new ARLU_Agente7());
                }
            }
        }

    }
}
