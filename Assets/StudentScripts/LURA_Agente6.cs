using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LURA_Agente6 : ISSR_Agent {
    enum LURA_MsgCode
    {
        AvailableStone,
        NonAvailableStone,
        LetsGoToGoal
    }

    public override void Start(){
        Debug.LogFormat("{0}: comienza", Myself.Name);
    }

    public override IEnumerator Update(){
        while (true){
            yield return new WaitForSeconds(0.5f);
            current_event = ISSREventType.onTickElapsed;
            current_state = AgentStateMachine();
            Share();
        }
    }

    public override void onEnterSensingArea(ISSR_Object obj){ // Comienzo a ver objeto obj
        // Acaba de entrar un objeto en el ‘campo visual’ del agente.
        object_just_seen = obj; // Anotamos objeto que vemos
        if (obj.type == ISSR_Type.SmallStone) { // Si es una piedra pequeña
            obj.TimeStamp = Time.time;
            if (oiGrippingAgents(obj) > 0)
            { // si obj no está en la lista

                SStoneIsAvailable(obj, false);

            }
            else { SStoneIsAvailable(obj, true); }
        } else if (obj.type == ISSR_Type.BigStone)
        {
           
            if (oiGrippingAgents(obj) >= 2)
            { // si obj no está en la lista

                BStoneIsAvailable(obj, false);

            }
            else { BStoneIsAvailable(obj, true); }
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
            case ISSRState.WaitforNoStonesMoving:
                next_state = SF_WaitforNoStonesMoving();
                break;
            case ISSRState.GoingToGripBigStone:
                next_state = SF_GoingToGripBigStone();
                break;
            case ISSRState.WaitingForHelpToMoveBigStone:
                next_state = SF_WaitingForHelpToMoveBigStone();
                break;
            case ISSRState.WaitforNoStonesMovingBigStone:
                next_state = SF_WaitforNoStonesMovingBigStone();
                break;
            case ISSRState.GoingToGoalWithBigStone:
                next_state = SF_GoingToGoalWithBigStone();
                break;
            case ISSRState.AvoidingObstacle:
                next_state = SF_AvoidingObstacle();
                break;
            case ISSRState.SleepingAfterCollisions:
                next_state = SF_SleepingAfterCollisions();
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
            case ISSRState.GoingToGripBigStone:
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
            case ISSRState.GoingToGoalWithBigStone:
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
            case ISSRState.GoingToGripBigStone:
                next_state = GetBStone(focus_object);  // Volver a pedir coger piedra o ir a su lugar
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
                //focus_object = ISSRHelp.GetCloserToMeObjectInList(this, Valid_Small_Stones, ISSR_Type.SmallStone); // coger de lista objeto más cercano a agente  
                focus_object = ISSRHelp.Get_next_available_stone_closer_to_me(this);
                                       // que sea de tipo SmallStone, se convierte en el objeto de interés:
                if (focus_object != null){ // Si hay alguno (focus_object está definido)
                    if (focus_object.type == ISSR_Type.BigStone) {
                        Debug.LogWarningFormat("Lo que quieras");
                        next_state = GetBStone(focus_object); }
                    else { next_state = GetSStone(focus_object); }
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
            Debug.LogFormat("{0}: '{1}' visible, trato de cogerla", Myself.Name, focus_object.Name);
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
                focus_object.TimeStamp = Time.time;
                SStoneIsAvailable(focus_object,false);
                next_state= ISSRState.WaitforNoStonesMoving;
                break;
            case ISSREventType.onEnterSensingArea:
                if (object_just_seen.Equals(focus_object)) // veo justo la piedra que 'recuerdo'
                {
                    next_state = GetSStone(focus_object);
                }
                break;
            case ISSREventType.onCollision:
                if (colliding_object.Equals(focus_object)) { next_state = GetSStone(focus_object); }
                else { next_state=ProcessCollision(); }
                break;
            case ISSREventType.onDestArrived:
                focus_object.TimeStamp = Time.time;
                SStoneIsAvailable(focus_object, false);
                next_state = ISSRState.Idle;
                break;
            case ISSREventType.onGripFailure:
                focus_object.TimeStamp = Time.time;
                SStoneIsAvailable(focus_object, false);
                next_state = ISSRState.Idle;
                break;
            case ISSREventType.onObjectLost:
                focus_object.TimeStamp = Time.time;
                SStoneIsAvailable(focus_object, false);
                next_state = ISSRState.Idle;
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
            case ISSREventType.onManyCollisions:
                acSetTimer(UnityEngine.Random.value * 2f);
                next_state = ISSRState.SleepingAfterCollisions;
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
                focus_object.TimeStamp = Time.time;
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

    public override void onPushTimeOut(ISSR_Object gripped_big_stone)
    {
        current_state = AgentStateMachine();
    }

    public override void onAnotherAgentGripped(ISSR_Object gripped_big_stone)
    {
        current_state = AgentStateMachine();
    }

    public override void onAnotherAgentUngripped(ISSR_Object gripped_big_stone)
    {
        current_state = AgentStateMachine();
    }

    public override void onDestArrived()
    {
        current_state = AgentStateMachine();
    }

    void SStoneIsAvailable(ISSR_Object stone, bool available)
    {
        ISSRHelp.UpdateStoneLists(stone, available, Valid_Small_Stones, Invalid_Small_Stones);
    }

    void BStoneIsAvailable(ISSR_Object stone, bool available)
    {
        ISSRHelp.UpdateStoneLists(stone, available, Valid_Big_Stones, Invalid_Big_Stones);
    }

    ISSRState GetSStone(ISSR_Object stone)
    {
        ISSRState estado_salida;


        if (oiSensable(stone))
        {
            if (oiGrippingAgents(stone) > 0) //condicion de no disponibilidad
            {
                focus_object.TimeStamp = Time.time;
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
                focus_object.TimeStamp = Time.time;
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

    ISSRState GetBStone(ISSR_Object stone)
    {
        ISSRState estado_salida;


        if (oiSensable(stone))
        {
            if (oiGrippingAgents(stone) >=2) //condicion de no disponibilidad
            {
                focus_object.TimeStamp = Time.time;
                BStoneIsAvailable(stone, false);
                acStop();
                if (acCheckError()) // si hay error en la acción:
                {
                    estado_salida = ISSRState.Error; // Señala error
                }
                else
                { // En caso de que no haya error:
                    estado_salida = ISSRState.GoingToGripBigStone; // cambio a estado siguiente
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
                    estado_salida = ISSRState.GoingToGripBigStone; // cambio a estado siguiente
                }
            }
        }
        else
        {
            if (Vector3.Distance(oiLastLocation(Myself), oiLastLocation(stone)) > iSensingRange())
            {
                acGotoLocation(oiLastLocation(stone));

                if (acCheckError()) // si hay error en la acción:
                {
                    estado_salida = ISSRState.Error; // Señala error
                }
                else
                { // En caso de que no haya error:
                    estado_salida = ISSRState.GoingToGripBigStone; // cambio a estado siguiente
                }
            }
            else
            {
                focus_object.TimeStamp = Time.time;
                BStoneIsAvailable(stone, false);
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

    public override void onManyCollisions()
    {
        current_state = AgentStateMachine();
    }

    public override void onTimerOut(float delay)
    {
        current_state = AgentStateMachine();
    }

    void Share()
    {
        foreach (ISSR_Object piedra in Valid_Small_Stones)
        {
            acSendMsgObj(0, (int)LURA_MsgCode.AvailableStone, piedra);
        }

        foreach (ISSR_Object piedra in Invalid_Small_Stones)
        {
            acSendMsgObj(0, (int)LURA_MsgCode.NonAvailableStone, piedra);
        }
        foreach (ISSR_Object piedra in Valid_Big_Stones)
        {
            acSendMsgObj(0, (int)LURA_MsgCode.AvailableStone, piedra);
        }
        foreach (ISSR_Object piedra in Invalid_Big_Stones)
        {
            acSendMsgObj(0, (int)LURA_MsgCode.NonAvailableStone, piedra);
        }
    }

    void ProcessMessage(ISSR_Message msg)
    {
      
        if (msg.usercode == (int)LURA_MsgCode.AvailableStone) {
            if (msg.Obj.type == ISSR_Type.BigStone) { BStoneIsAvailable(msg.Obj, true); }
            else { SStoneIsAvailable(msg.Obj, true); }
           
        }
        if (msg.usercode == (int)LURA_MsgCode.NonAvailableStone)
        {
            if (msg.Obj.type == ISSR_Type.BigStone) { BStoneIsAvailable(msg.Obj, false); }
            else { SStoneIsAvailable(msg.Obj, false); }
        }
        
        if (msg.usercode == (int)LURA_MsgCode.LetsGoToGoal && msg_obj.Equals(focus_object))
        {
            current_state = AgentStateMachine();
        }
    }

    public override void onMsgArrived(ISSR_Message msg)
    {
        ProcessMessage(msg);
    }

    ISSRState SF_SleepingAfterCollisions()
    { // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event)
        { // Según el evento
            case ISSREventType.onTimerOut:
                next_state = ResumeAfterCollision();
                break;
            case ISSREventType.onUngrip:
                focus_object.TimeStamp = Time.time;
                SStoneIsAvailable(focus_object,true);
                GetSStone(focus_object);
                break;

            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }

    ISSRState SF_GoingToGripBigStone()
    { // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event)
        { // Según el evento
            case ISSREventType.onEnterSensingArea:
                if (object_just_seen.Equals(focus_object)) // veo justo la piedra que 'recuerdo'
                {
                    next_state = GetBStone(focus_object);
                }
                break;
            case ISSREventType.onGripSuccess:
                if(oiGrippingAgents(focus_object) > 1)
                {
                    focus_object.TimeStamp = Time.time;
                    BStoneIsAvailable(focus_object, false);
                    next_state = ISSRState.WaitforNoStonesMovingBigStone;
                }
                else
                {
                    next_state = ISSRState.WaitingForHelpToMoveBigStone;
                }
                break;
            case ISSREventType.onCollision:
                if (!colliding_object.Equals(focus_object)) {
                    next_state = ProcessCollision();
                }
                else
                {
                    next_state = GetBStone(focus_object);
                }
                break;
            case ISSREventType.onGripFailure:
                focus_object.TimeStamp = Time.time;
                BStoneIsAvailable(focus_object, false);
                next_state = ISSRState.Idle;
                break;
            case ISSREventType.onObjectLost:
                focus_object.TimeStamp = Time.time;
                BStoneIsAvailable(focus_object, false);
                next_state = ISSRState.Idle;
                break;
            case ISSREventType.onDestArrived:
                focus_object.TimeStamp = Time.time;
                BStoneIsAvailable(focus_object, false);
                next_state = ISSRState.Idle;
                break;
            case ISSREventType.onTickElapsed:
                next_state = GetBStone(focus_object);
                break;
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }

    ISSRState SF_WaitingForHelpToMoveBigStone()
    { // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event)
        { // Según el evento
            case ISSREventType.onAnotherAgentGripped:
                focus_object.TimeStamp = Time.time;
                BStoneIsAvailable(focus_object, false);
                next_state = ISSRState.WaitforNoStonesMovingBigStone;
                break;
            case ISSREventType.onUngrip:
                next_state = GetBStone(focus_object);
                break;
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }

    ISSRState SF_WaitforNoStonesMovingBigStone()
    { // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event)
        { // Según el evento
            case ISSREventType.onAnotherAgentUngripped:
                focus_object.TimeStamp = Time.time;
                BStoneIsAvailable(focus_object, true);
                next_state = ISSRState.WaitingForHelpToMoveBigStone;
                break;
            case ISSREventType.onTickElapsed:
                if (iMovingStonesInMyTeam() == 0)
                {
                    acSendMsgObj(0, (int)LURA_MsgCode.LetsGoToGoal, focus_object);
                    acGotoLocation(iMyGoalLocation());
                    if (acCheckError()) // si hay error en la acción:
                    {
                        next_state = ISSRState.Error; // Señala error
                    }
                    else
                    { // En caso de que no haya error:
                        next_state = ISSRState.GoingToGoalWithBigStone; // cambio a estado siguiente
                    }
                }
                break;
            case ISSREventType.onMsgArrived:
                if (user_msg_code.Equals(LURA_MsgCode.LetsGoToGoal)) {
                    if (msg_obj.Equals(focus_object) || msg_obj.Equals(GrippedObject)) {
                        acGotoLocation(iMyGoalLocation());
                        if (acCheckError()) // si hay error en la acción:
                        {
                            next_state = ISSRState.Error; // Señala error
                        }
                        else
                        { // En caso de que no haya error:
                            next_state = ISSRState.GoingToGoalWithBigStone; // cambio a estado siguiente
                        }
                    }
                }
                break;
            case ISSREventType.onUngrip:
                next_state = GetBStone(focus_object);
                focus_object.TimeStamp = Time.time;
                BStoneIsAvailable(focus_object, true);
                break;
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }

    ISSRState SF_GoingToGoalWithBigStone()
    { // SF “State Function”
        ISSRState next_state = current_state;
        switch (current_event)
        { // Según el evento
            case ISSREventType.onGObjectScored:
                next_state = ISSRState.Idle;
                break;
            case ISSREventType.onPushTimeOut:
                next_state = ISSRState.WaitforNoStonesMovingBigStone;
                break;
            default:
                Debug.LogWarningFormat("{0} Evento '{1}' no considerado en estado '{2}'", Myself.Name, current_event, current_state);
                break;
        }
        return next_state;
    }
}


