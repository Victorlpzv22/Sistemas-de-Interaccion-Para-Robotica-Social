using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LURA_Agente1 : ISSR_Agent {

    public override void Start()
    {
        Debug.LogFormat("{0}: comienza", Myself.Name);
    }
    public override IEnumerator Update()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
        }
    }

    public override void onEnterSensingArea(ISSR_Object obj)
    {
        if(obj.type==ISSR_Type.SmallStone)
        {
            Debug.LogFormat("{0}: veo{1}, voy a por ella", Myself.Name, obj.Name);
            acGripObject(obj);
        }
    }

    public override void onGripSuccess(ISSR_Object obj_gripped)
    {
        Debug.LogFormat("{0}: piedra{1} cogida, voy a la meta con ella", Myself.Name, obj_gripped.Name);
        acGotoLocation(iMyGoalLocation());
    }

    public override void onGObjectScored(ISSR_Object stone_that_scored)
    {
        Debug.LogFormat("{0}: piedra{1} metida en meta", Myself.Name, stone_that_scored.Name);
    }
}
