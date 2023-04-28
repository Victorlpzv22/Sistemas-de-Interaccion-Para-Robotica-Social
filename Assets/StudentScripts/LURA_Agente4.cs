using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LURA_Agente4 : ISSR_Agent {

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
            if (oiGrippingAgents(obj) > 0)
            { // si obj no está en la lista
                SStoneIsAvailable(obj, false);
               
            }
            else { SStoneIsAvailable(obj, true); }
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

     ISSRState AgentStateMachine(){ // Función principal de máquina de estados
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
            case ISSRState.AvoidingObstacle:
                next_state = SF_AvoidingObstacle();
                break;
            case ISSRState.WaitforNoStonesMoving:
                next_state = SF_WaitforNoStonesMoving();
                break;
            case ISSRState.End:
                break;
            case ISSRState.Error:
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

     ISSRState ProcessCollision()  // Procesar colisi�n 
    {
        ISSRState next_state = current_state;

        switch (current_state)
        {
            case ISSRState.GoingToGripSmallStone:
                last_state = current_state;
                acGotoLocation(ISSRHelp.CalculateSafeLocation(this,colliding_object));
                if (acCheckError())
                {
                    next_state = ISSRState.Error;
                }
                else
                {
                    next_state = ISSRState.AvoidingObstacle;
                }
                break;
            case ISSRState.GoingToGoalWithSmallStone:
                last_state = current_state;
                acGotoLocation(ISSRHelp.CalculateSafeLocation(this, colliding_object));
                if (acCheckError())
                {
                    next_state = ISSRState.Error;
                }
                else
                {
                    next_state = ISSRState.AvoidingObstacle;
                }
                break;
                
            case ISSRState.AvoidingObstacle:
                acGotoLocation(ISSRHelp.CalculateSafeLocation(this, colliding_object));
                if (acCheckError())
                {
                    next_state = ISSRState.Error;
                }
                else
                {
                    next_state = ISSRState.AvoidingObstacle;
                }
                break;
                
            default:
                Debug.LogErrorFormat("ProcessCollision() en {0}, estado {1} no considerado al colisionar", Myself.Name, current_state);
                break;
        }

        return ISSRState.AvoidingObstacle;
    }

    ISSRState ResumeAfterCollision() // Versi�n de Pr�ctica 3, completar en las siguientes
    { // Continuar con lo que se estaba haciendo en el momento de la colisi�n.
        ISSRState next_state = current_state;

        switch (last_state)  // Seg�n estado anterior 
        {
            case ISSRState.GoingToGripSmallStone:
                next_state = GetSStone(focus_object);  // Volver a pedir coger piedra o ir a su lugar
                break;
            case ISSRState.GoingToGoalWithSmallStone:
                if (iMovingStonesInMyTeam() == 0) { 
                acGotoLocation(iMyGoalLocation());  // volver a pedir ir a la meta
                if (acCheckError())
                {
                    next_state = ISSRState.Error;
                }
                else
                {
                    next_state = ISSRState.GoingToGoalWithSmallStone;
                }
                }
                else
                {
                    next_state = ISSRState.WaitforNoStonesMoving;
                }
                break;
            default:
                Debug.LogErrorFormat("{0}, estado {1} no considerado al volver de colisi�n", Myself.Name, last_state);
                break;
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
                    next_state = GetSStone(focus_object);
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

    private ISSRState ProcessStone()
    {
        ISSRState next_state;
        if (oiSensable(focus_object))
        {
            Debug.LogFormat("{0}: '{1}' visible, trato de cogerla", Myself.Name,
            focus_object.Name);
            acGripObject(focus_object); // intento agarrar esa piedra pequeña cercana
        }
        else
        {
            Debug.LogFormat("{0}: '{1}' fuera de la vista, voy a su última posición",
            Myself.Name, focus_object.Name);
            acGotoLocation(oiLastLocation(focus_object));
        }
        if (acCheckError()) // si hay error en la acción:
        {
            next_state = ISSRState.Error; // Señala error
        }
        else
        { // En caso de que no haya error:
            next_state = ISSRState.GoingToGripSmallStone; // cambio a estado siguiente
        }

        return next_state;
    }

     ISSRState SF_GoingToGripSmallStone(){ // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event){ // Según el evento
            case ISSREventType.onGripSuccess:
                SStoneIsAvailable(focus_object,false);
                next_state= ISSRState.WaitforNoStonesMoving;
                
                break;
            case ISSREventType.onEnterSensingArea:
                if (object_just_seen.Equals(focus_object)) // veo justo la piedra que 'recuerdo'
                {
                   GetSStone(focus_object);
                    
                }
                break;
            case ISSREventType.onCollision:
                if (colliding_object.Equals(focus_object)) { next_state = GetSStone(focus_object); }
                else { next_state=ProcessCollision(); }
                break;
            case ISSREventType.onDestArrived:
                SStoneIsAvailable(focus_object, false);
                next_state = ISSRState.Idle;
                break;
            case ISSREventType.onGripFailure:
                SStoneIsAvailable(focus_object, false);
                next_state = ISSRState.Idle;
                Debug.LogWarningFormat("AQUI");
                break;
            case ISSREventType.onObjectLost:
                SStoneIsAvailable(focus_object, false);
                next_state = ISSRState.Idle;
                Debug.LogWarningFormat("AQUI2");
                break;
            case ISSREventType.onTickElapsed:
                next_state = GetSStone(focus_object);
                break;
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }

     ISSRState SF_GoingToGoalWithSmallStone(){ // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event){ // Según el evento
            case ISSREventType.onGObjectScored:
                next_state = ISSRState.Idle;
                Debug.LogFormat("{0} ha entregado en la meta la piedra : {1}", Myself.Name, focus_object.Name);
                break;
            case ISSREventType.onCollision:
                next_state=ProcessCollision();
                break;
            case ISSREventType.onGObjectCollision:
                next_state = ProcessCollision();
                break;
            
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }

    ISSRState SF_AvoidingObstacle()
    { // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event)
        { // Según el evento
            case ISSREventType.onGObjectCollision:
                next_state=ProcessCollision();
                break;
            case ISSREventType.onCollision:
                next_state=ProcessCollision();
                break;
            case ISSREventType.onDestArrived:
                next_state=ResumeAfterCollision();
                break;
            case ISSREventType.onGObjectScored:
                next_state = ISSRState.Idle;
                Debug.LogFormat("{0} ha entregado en la meta la piedra : {1}", Myself.Name, focus_object.Name);
                break;
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }

    ISSRState SF_WaitforNoStonesMoving()
    { // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event)
        { // Según el evento
            case ISSREventType.onTickElapsed:
                if (iMovingStonesInMyTeam() == 0)
                {
                    acGotoLocation(iMyGoalLocation());
                    if (acCheckError())
                    { // si hay error en la acción:
                        next_state = ISSRState.Error; // Señala error
                    }
                    else
                    {
                        // En caso de que no haya error: 
                        next_state = ISSRState.GoingToGoalWithSmallStone; // cambio a estado siguiente
                        Debug.LogFormat("{0} ha agarrado la piedra : {1}", Myself.Name, focus_object.Name);
                    }
                }

                    break;
            case ISSREventType.onUngrip:
                SStoneIsAvailable(focus_object, true);
                GetSStone(focus_object);
                break;
            
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }

    public override void onCollision(ISSR_Object obj_that_collided_with_me)
    {
        colliding_object = obj_that_collided_with_me;
        current_state = AgentStateMachine();
    }

    public override void onGObjectCollision(ISSR_Object obj_that_collided_with_me)
    {
        colliding_object = obj_that_collided_with_me;
        current_state = AgentStateMachine();
    }
    public override void onDestArrived()
    {
        current_state = AgentStateMachine();
    }

    void SStoneIsAvailable(ISSR_Object stone, bool available)
    {
        if (available) {
            if (!Valid_Small_Stones.Contains(stone))
            {
                Valid_Small_Stones.Add(stone);
            }
        }
        else {
            if (Valid_Small_Stones.Contains(stone)) {
                Valid_Small_Stones.Remove(stone);
             }
           
        }
    }

    ISSRState GetSStone(ISSR_Object stone)
    {
        ISSRState estado_salida;


        if (oiSensable(stone))
        {
            if (oiGrippingAgents(stone) > 0)
            {
                SStoneIsAvailable(stone, false);
                acStop();
                if (acCheckError()) // si hay error en la acción:
                {
                    estado_salida = ISSRState.Error; // Señala error
                }
                else
                { // En caso de que no haya error:
                    estado_salida = ISSRState.Idle; // cambio a estado siguiente
                }
            }
            else
            {
                acGripObject(stone);

                if (acCheckError()) // si hay error en la acción:
                {
                    estado_salida = ISSRState.Error; // Señala error
                }
                else
                { // En caso de que no haya error:
                    estado_salida = ISSRState.GoingToGripSmallStone; // cambio a estado siguiente
                }
            }
        }
        else
        {
            if(Vector3.Distance(oiLastLocation(Myself), oiLastLocation(stone)) > iSensingRange()){ 
            acGotoLocation(oiLastLocation(stone));

            if (acCheckError()) // si hay error en la acción:
            {
                estado_salida = ISSRState.Error; // Señala error
            }
            else
            { // En caso de que no haya error:
                estado_salida = ISSRState.GoingToGripSmallStone; // cambio a estado siguiente
            }
        }
            else{
                SStoneIsAvailable(stone, false);
                acStop();
                if (acCheckError()) // si hay error en la acción:
                {
                    estado_salida = ISSRState.Error; // Señala error
                }
                else
                { // En caso de que no haya error:
                    estado_salida = ISSRState.Idle; // cambio a estado siguiente
                }

            }
        }
        return estado_salida;
    }
    public override void onGripFailure(ISSR_Object obj_I_wanted_to_grip)
    {
        current_state = AgentStateMachine();
    }

    public override void onObjectLost(ISSR_Object obj_I_wanted_to_grip)
    {
        current_state = AgentStateMachine();
    }
    public override void onUngrip(ISSR_Object obj_I_wanted_to_grip)
    {
        current_state = AgentStateMachine();
    }
}


