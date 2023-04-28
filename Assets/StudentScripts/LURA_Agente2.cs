using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LURA_Agente2 : ISSR_Agent {

    public override void Start(){
        Debug.LogFormat("{0}: comienza", Myself.Name);
    }

    public override IEnumerator Update(){
        while (true){
            yield return new WaitForSeconds(0.5f);
            current_event = ISSREventType.onTickElapsed;
            current_state = AgentStateMachine();
        }
    }

    public override void onEnterSensingArea(ISSR_Object obj){ // Comienzo a ver objeto obj
        // Acaba de entrar un objeto en el ‘campo visual’ del agente.
        object_just_seen = obj; // Anotamos objeto que vemos
        if (obj.type == ISSR_Type.SmallStone){ // Si es una piedra pequeña
            if (!Valid_Small_Stones.Contains((obj))){ // si obj no está en la lista
                Valid_Small_Stones.Add(obj); // Añadir obj a la lista
                Debug.LogFormat("{0}: nueva piedra pequeña '{1}', la anoto", Myself.Name, obj.Name);
            }
        }
        current_state = AgentStateMachine();
    }

    public override void onGripSuccess(ISSR_Object obj_gripped){ //Objeto agarrado por agente
        // Acabo de agarrar un objeto: acGripObject() completado con éxito
        Debug.LogFormat("{0}: agarra '{1}'", Myself.Name, obj_gripped.Name);
        current_state = AgentStateMachine(); // Llamar a máquina de estados
    }

    public override void onGObjectScored(ISSR_Object stone_that_scored){ 
        // Acabo de puntuar/ meter una piedra en la meta
        Debug.LogFormat("{0}: piedra '{1}' metida en meta",
        Myself.Name, stone_that_scored.Name);
        current_state = AgentStateMachine(); // Llamar a máquina de estados
    }

    public ISSRState AgentStateMachine(){ // Función principal de máquina de estados
        ISSRState next_state = current_state; // estado de salida, en principio igual
        switch (current_state) // Según el estado
        {
            case ISSRState.Idle:
                next_state = SF_Idle();
                break;
            case ISSRState.GoingToGripSmallStone:
                next_state = SF_GoingToGripSmallStone();
                break;
            case ISSRState.GoingToGoalWithSmallStone:
                next_state = SF_GoingToGoalWithSmallStone();
                break;
            default:
                Debug.LogErrorFormat("{0}: estado {1} no considerado", Myself.Name, current_state);
                break;
        }
        if (current_state != next_state){ // Si ha cambiado el estado
            Debug.LogWarningFormat("{0}: Estado '{1}'-->'{2}' por evento '{3}'", Myself.Name, current_state, next_state, current_event);
        }
        return next_state;
    }

    public ISSRState SF_Idle(){ // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event){ // Según el evento
            case ISSREventType.onTickElapsed:
                focus_object = ISSRHelp.GetCloserToMeObjectInList(this, Valid_Small_Stones,
                ISSR_Type.SmallStone); // coger de lista objeto más cercano a agente 
                                       // que sea de tipo SmallStone, se convierte en el objeto de interés:
                if (focus_object != null){ // Si hay alguno (focus_object está definido)
                    Debug.LogFormat("{0}: trata de coger '{1}'", Myself.Name, focus_object.Name);
                    acGripObject(focus_object); // intento agarrar esa piedra pequeña cercana
                    if (acCheckError()){ // si hay error en la acción: 
                    
                        next_state = ISSRState.Error; // Señala error
                    }
                    else{
                        // En caso de que no haya error: 
                        next_state = ISSRState.GoingToGripSmallStone; // cambio a estado siguiente
                    }
                }
                else // focus_object es null, no hay más piedras disponibles
                {
                    Debug.LogFormat("{0}: no conozco más piedras pequeñas", Myself.Name);
                    next_state = ISSRState.End; // Fin del proceso
                }
                break;
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }

    public ISSRState SF_GoingToGripSmallStone(){ // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event){ // Según el evento
            case ISSREventType.onGripSuccess:
                Valid_Small_Stones.Remove(focus_object);
                acGotoLocation(iMyGoalLocation());
                if (acCheckError()){ // si hay error en la acción:
                    next_state = ISSRState.Error; // Señala error
                }
                else{
                    // En caso de que no haya error: 
                    next_state = ISSRState.GoingToGoalWithSmallStone; // cambio a estado siguiente
                }
                break;
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }

    public ISSRState SF_GoingToGoalWithSmallStone(){ // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event){ // Según el evento
            case ISSREventType.onGObjectScored:
                next_state = ISSRState.Idle;
                break;
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }
}
