using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ----------------------------------------------------------------------------------------
//  File: ISSR_AgentBehaviour.cs
//   Coded by Enrique Rendón: enriqueblender@gmail.com  for ISSR Unity API
//  Summary:
//  This is the main behavior of an agent,
//   this will call the particular services of an agent implementation
//   in class derived from ISSRAgent
//    
// ----------------------------------------------------------------------------------------
// 


public enum AgentSFX
{
    Grip,
    Grip1,
    Grip2,
    Body_Collision,
    Body_Collision1,
    Body_Collision2,
    Body_Collision_Ungrip,
    Small_Collision,
    Small_Collision1,
    Big_Collision,
    Big_Collision1,
    Many_Collisions,
    Body_Small_Collision,
    Body_Small_Collision1

}

public class ISSR_AgentBehaviour : MonoBehaviour
{


    [Header("Modifiable Agent Settings")]
    public ISSR_Agent AgentDescriptor;

    [Header("Debugging log flags for STUDENT code")]
    public bool draw_destinations;
    public bool debug_actioncalls;
    public bool debug_events;

    [Header("Debugging log flags for INTERNAL code")]
    public bool debug_sensing;
    public bool debug_comms;
    public bool debug_collisions;
    public bool debug_grip_ungrip;
    public bool debug_locomotion;


    [Header("Internal Agent Settings")]
    public List<GameObject> AgentsInCommRange;

    public List<GameObject> ObjectsInSensesRange;

    public GameObject GrippedObject;

    [Tooltip("Set this to true with EnableSensing(true) to start receiving events, initially is set to false")]
    public bool sensing_enabled;// Set this to true with EnableSensing(true) to start receiving events, initially is set to false
    public List<ISSR_Event> IncomingEvents; //  this list holds incoming events to the agent, if dispatching is suspended 
                                            //  the list is not emptied
    [Tooltip("Set this to true with EnableReceiving(true) to start receiving messages, initially is set to false")]
    public bool receiving_enabled;      // Set this to true with EnableReceiving(true) to start receiving messages, initially is set to false
    public List<ISSR_Message> IncomingMsgs; //  this list holds incoming messages to the agent, if dispatching is suspended 
                                            //  the list is not emptied






    [HideInInspector]
    public ISSR_ManagerBehaviour ISSRManager;



    [HideInInspector]
    public ISSR_Object ObjectDescriptor;

    [HideInInspector]
    public GameObject AgentMesh;
    [HideInInspector]
    public SkinnedMeshRenderer AgentMeshRenderer;


    private Rigidbody rb;
    private Animator anim;
    private AudioSource audiosource;

    [SerializeField]
    AudioClip[] sound_effects;

    // Agent Colliders
    [HideInInspector]
    public CapsuleCollider MainCollider;  // Her body
    [HideInInspector]
    public CapsuleCollider FrontCollider; // Front collider to detect collisions with the small object she is pushing
                                          // This las one is disabled when not pushing

    // Blocking related--------------------
    [HideInInspector]
    public Vector3 last_position;
    //[HideInInspector]
    public float distance_covered_in_watch_period;
    //[HideInInspector]
    public bool blocked_in_watch_period; 

    bool running;

    // Timer related-------------------------------------
    public bool timer_running = false;  // Only one timer running at the same time
    float timer_delay = 0;              // Delay of the last timer


    // Many collisions related ------------------------------
    float collisions_observation_period = 1;  // Period of time in which collisions are counted
    int collisions_in_period = 0;  // number of collisions in current period
    int collisions_in_period_trigger = 4; // When this number is reached in a period onManyCollisions() 
                                          //  Gets triggered


    public GameObject especial;

    void Awake()
    {
        CapsuleCollider[] MyColliders;
        //----------------CONTROL---------------------------------------
        this.AgentsInCommRange = new List<GameObject>();
        this.ObjectsInSensesRange = new List<GameObject>();
        this.IncomingEvents = new List<ISSR_Event>();

        //-----------LOCOMOTION AND ANIMATION------------------------
        this.anim = GetComponent<Animator>();
        this.rb = GetComponent<Rigidbody>();
        this.audiosource = GetComponent<AudioSource>();



        running = true;

        MyColliders = GetComponents<CapsuleCollider>();
        this.MainCollider = MyColliders[0];
        this.FrontCollider = MyColliders[1];

        // Reference to ISSRManager
        this.ISSRManager = GameObject.Find("ISSR_Manager").GetComponent<ISSR_ManagerBehaviour>();
        //Debug.Log("Awake Agent");

        last_position = this.gameObject.transform.position;
        this.distance_covered_in_watch_period = 0;
        this.blocked_in_watch_period = false;
    }

    // ISSR_AgentBehaviour of an Agent prefab is disable at startup, after all initialization is performed correctly by 
    //   the manager behaviours are enabled.
    void Start()
    {
        //Debug.Log ("Hello for generic agent behavior Start()");   
        this.receiving_enabled = true;  // Set to false in Start for no messages at startup
        this.sensing_enabled = true;  //  Set to false in Start for no events at startup
        this.AgentDescriptor.GrippedObject = null;
        this.AgentDescriptor.SensableObjects = new List<ISSR_Object>();
        this.AgentDescriptor.current_event = ISSREventType.Undefined;

        // Initial values also for read/write public fields
        this.AgentDescriptor.current_state = ISSRState.Idle;
        this.AgentDescriptor.focus_location = Vector3.zero;
        this.AgentDescriptor.focus_object = null;
        this.AgentDescriptor.focus_agent = null;
        this.AgentDescriptor.Stones = new List<ISSR_Object>();
        this.AgentDescriptor.Agents = new List<ISSR_Object>();
        this.AgentDescriptor.Objects = new List<ISSR_Object>();
        this.AgentDescriptor.Locations = new List<Vector3>();

        this.AgentDescriptor.Valid_Small_Stones = new List<ISSR_Object>();
        this.AgentDescriptor.Invalid_Small_Stones = new List<ISSR_Object>();
        this.AgentDescriptor.Valid_Big_Stones = new List<ISSR_Object>();
        this.AgentDescriptor.Invalid_Big_Stones = new List<ISSR_Object>();
        this.AgentDescriptor.Valid_Locations = new List<Vector3>();
        this.AgentDescriptor.Invalid_Locations = new List<Vector3>();


        AgentDescriptor.Start();
        StartCoroutine(AgentDescriptor.Update());
        StartCoroutine(AnimateHead());
        StartCoroutine(CollisionCounter());

        if (ISSRextra.ActivateSpecial())
        {
            especial.SetActive(true);
        }
    }


    public void EndExecution(bool win)
    {
        StopAllCoroutines();
        running = false;

        // TODO animation of win or loose -------
        this.locomotion_active = false;
        SidewardsPush = 0;
        FrontBackPush = 0;
        this.advancing = false;

        if (win == true)
        {
            anim.SetBool("win", true);
        }
        else
        {
            anim.SetBool("loose", true);
        }
        this.UpdateAnimation();
    }

    // Update is called once per frame
    void Update()
    {

        if (running)
        {
            this.UpdateAgentLocomotion();

            this.UpdateAnimation();

            // Handling of events and messages to ISSR_Agent------------------------------------
            this.PostAllEvents();
        }


    }  // End of Update()



    // --------------AGENT LOCOMOTION--------------------------------------------------
    #region AGENT LOCOMOTION

    public enum Locomotion
    {
        loose,     // Walking freely or going to grip an object
        lightpush, // Pushing a light object:  SmallStone...
        heavypush,  // Pushing a Heavy object: BigStone...
        gripped     // This agent is gripped by other agent
    }
    // Locomotion parameters ------------------------------

    [Header("Locomotion data")]
    public bool locomotion_active = false;
    public Locomotion loco_state = Locomotion.loose;
    Vector3 loco_tmpgoal_location = new Vector3();

    public bool trying_to_grip;
    public bool use_destination_object;
    public ISSR_Object destination_object;
    public GameObject destination_game_object;
    public Vector3 destination_location;

    bool rotating;
    bool advancing;

    float goal_distance_threshold = 0.1f;

    // General locomotion parameters ------------------
    float remaining_angle;
    int update_goal_period = 5;
    int update_goal_tick;

    // Loose locomotion parameters---------------------
    float loose_speed = 2.2f;   // 1.8f
    float loose_rotate_speed = 140; // 100
    float max_angle_rotate_advance = 15; // 10
    float gripping_distance;


    // Light Push locomotion parameters ------------------
    float lightpush_speed = 1.8f;    // 1.2f
    float lightpush_rotate_speed = 80;   // 50
    float max_angle_rotate_advance_lightpush = 20; // 20



    void UpdateAgentLocomotion()
    {
        if (locomotion_active)
        {
            switch (loco_state)
            {
                case Locomotion.loose:
                    UpdateLooseLoco();
                    break;
                case Locomotion.lightpush:
                    UpdateLightPushLoco();
                    break;
                case Locomotion.heavypush:
                    UpdateHeavyPushLoco();
                    break;
            }
        }
    }

    void StopMovement()
    {
        if (locomotion_active)
        {
            this.locomotion_active = false;
            this.advancing = false;
            this.rotating = false;
            ISSR_BStoneBehaviour BStoneBehav;
            this.SidewardsPush = 0;
            this.FrontBackPush = 0;

            if ((this.GrippedObject != null))
            {
                int team = (this.tag == "AgentA") ? 0 : 1;

                if (this.GrippedObject.tag == "SmallStone")
                {
                    if (debug_locomotion)
                    {
                        Debug.LogFormat("LOCO {0} Decrement moving stones for team {1}:{2}", this.AgentDescriptor.Myself.Name, ISSRManager.Teams[team].ShortName,
                            (team == 0) ? "BLUE" : "RED");

                    }

                    ISSRManager.Teams[team].stones_moving--;
                }

                if (this.GrippedObject.tag == "BigStone")
                {
                    BStoneBehav = this.transform.parent.GetComponent<ISSR_BStoneBehaviour>();
                    BStoneBehav.AgentStops(this.AgentDescriptor.Myself, this.gameObject);
                }

                if ((this.GrippedObject.tag == "AgentA") || (this.GrippedObject.tag == "AgentB"))
                {
                    ISSR_AgentBehaviour other_agent_behavior;

                    other_agent_behavior = ISSRManager.GetAgentBehaviour(this.GrippedObject);
                    other_agent_behavior.EnqueueEvent(ISSREventType.onStop);
                }


            }
        }

        //use_destination_object = false;
        //Debug.LogFormat ("{0} STOPS.........................", this.AgentDescriptor.Myself.Name);
    }

    void DrawDestinations()
    {
        if (use_destination_object)
        {
            Debug.DrawLine(this.transform.position + Vector3.up * 0.1f,
            this.loco_tmpgoal_location + Vector3.up * 0.1f, Color.magenta);
        }
        else
        {
            Debug.DrawLine(this.transform.position + Vector3.up * 0.1f,
            this.loco_tmpgoal_location + Vector3.up * 0.1f, Color.gray);
        }
    }

    void UpdateLooseLoco()
    {

        float distance;
        bool error;

        if (use_destination_object)  // Update destination if going to or trying to grip moving object
        {
            update_goal_tick++;
            if (update_goal_tick >= update_goal_period)
            {
                update_goal_tick = 0;

                if (this.AgentDescriptor.oiSensable(destination_object))
                {
                    // Object still in sensing range
                    destination_object.LastLocation = ISSRManager.ISSR_ObjectLocation(this.gameObject, destination_object, out error);
                    SetTmpGoalLocation(ISSRManager.ISSR_ObjectLocation(this.gameObject, destination_object, out error));
                }

                else
                {
                    this.StopMovement();
                    EnqueueEvent(ISSREventType.onStop);

                    // Cannot sense the object I was trying to follow or grip
                    EnqueueEvent(ISSREventType.onObjectLost, this.destination_game_object);

                    if (debug_locomotion)
                    {
                        Debug.LogFormat("LOCO {0} could NOT REACH {1} because she lost it", this.AgentDescriptor.Myself.Name, this.destination_object.Name);
                    }

                    if (trying_to_grip)
                    {
                        EnqueueEvent(ISSREventType.onGripFailure, this.destination_game_object);
                        if (debug_grip_ungrip)
                        {
                            Debug.LogFormat("GRIP {0} could NOT GRIP {1} because she lost it", this.AgentDescriptor.Myself.Name, this.destination_object.Name);
                        }
                    }



                    // stop or go to last location??    better stop


                    return;
                }
            }
        }


        if (this.advancing)
        {

            rb.velocity = this.transform.TransformDirection(Vector3.forward) * this.loose_speed;

            anim.SetBool("rotating", false);

            // Check goal 

            distance = (this.transform.position - this.loco_tmpgoal_location).magnitude;


            if (draw_destinations) DrawDestinations();




            if (trying_to_grip)
            {
                this.remaining_angle = this.CalculateRotationAngle(this.transform.TransformDirection(Vector3.forward),
                    this.transform.position, this.loco_tmpgoal_location);

                if ((distance < this.gripping_distance + 0.1f) && (this.remaining_angle < 2))
                {


                    if (debug_grip_ungrip)
                    {
                        int ngripping_agents;
                        bool moving;

                        ngripping_agents = ISSRManager.NGrippingAgents(this.AgentDescriptor.Myself);
                        moving = ISSRManager.ObjectMoving(this.destination_object);

                        Debug.LogFormat("GRIP {0} will try to GRIP {1} which currently has {2} gripping agents and {3} MOVING",
                            this.AgentDescriptor.Myself.Name, this.destination_object.Name, ngripping_agents, moving ? "IS" : "is NOT");

                    }

                    this.StopMovement();
                    EnqueueEvent(ISSREventType.onStop);
                    this.rb.velocity = Vector3.zero; // Force abrupt stop to avoid generating onCollision()


                    if (ISSRManager.TryGrip(this.AgentDescriptor.Myself, this.gameObject,
                        this.destination_object, this.destination_game_object))
                    {

                        if (debug_grip_ungrip)
                        {
                            Debug.LogFormat("GRIP {0} successfully GRIPPED {1}", this.gameObject.name,
                                this.destination_game_object.name);
                        }

                        EnqueueEvent(ISSREventType.onGripSuccess, this.destination_game_object);
                        PlaySfx(AgentSFX.Grip, 3);


                        // Change animation parameters
                        gripping = true;
                        FrontBackPush = 0;
                        SidewardsPush = 0;
                        // loco_state =  has changed inside TryGrip-->DoGrip
                    }
                    else
                    {

                        EnqueueEvent(ISSREventType.onGripFailure, this.destination_game_object);
                        if (debug_grip_ungrip)
                        {
                            Debug.LogFormat("GRIP {0} COULD NOT GRIP {1}", this.gameObject.name,
                                this.destination_game_object.name);
                        }

                    }




                    return;
                }
            }


            if (distance < this.goal_distance_threshold)  // Arrived to destination
            {

                this.StopMovement();
                EnqueueEvent(ISSREventType.onStop);


                if (use_destination_object)
                {
                    // Something strange happened, the agent was trying to reach and object and got to 
                    //  its location without colliding with it or gripping it when in gripping distance
                    Debug.LogErrorFormat("LOCO {0} cannot find {1} though it arrived to its destinations", this.AgentDescriptor.Myself.Name, this.destination_object.Name);
                    EnqueueEvent(ISSREventType.onObjectLost, this.destination_game_object);
                }
                else
                {

                    //  TODO if this is a temporal goal while dodging and object should not post event
                    EnqueueEvent(ISSREventType.onDestArrived);

                }

            }
        }
        else
        {
            float absangle = Mathf.Abs(this.remaining_angle);

            if (absangle < this.max_angle_rotate_advance)
            {
                this.advancing = true;
            }
            else
            {
                anim.SetBool("rotating", true);
            }

        }

        if (this.rotating)
        {
            float absangle = Mathf.Abs(this.remaining_angle);
            float rotateangle;

            if (absangle < this.loose_rotate_speed * Time.deltaTime)
            {
                rotateangle = this.remaining_angle * Time.deltaTime;
            }
            else
            {
                rotateangle = this.loose_rotate_speed * this.remaining_angle / absangle * Time.deltaTime;
            }
            this.transform.Rotate(Vector3.up, rotateangle);

            if (this.advancing)
            {
                this.remaining_angle = this.CalculateRotationAngle(this.transform.TransformDirection(Vector3.forward),
                    this.transform.position, this.loco_tmpgoal_location);
            }
            else
            {
                this.remaining_angle -= rotateangle;
            }
        }


    }


    void UpdateHeavyPushLoco()
    {
        bool error = false;
        ISSR_BStoneBehaviour BStoneBehav;
        float distance;

        if (use_destination_object)
        {
            update_goal_tick++;
            if (update_goal_tick >= update_goal_period)
            {
                update_goal_tick = 0;
                if (this.AgentDescriptor.oiSensable(destination_object))
                {   // Object still in sensing range
                    destination_object.LastLocation = ISSRManager.ISSR_ObjectLocation(this.gameObject, destination_object, out error);
                    SetTmpGoalLocation(ISSRManager.ISSR_ObjectLocation(this.gameObject, destination_object, out error));
                }
                else
                {
                    if (this.AcStopMovement())
                    {
                        BStoneBehav = this.transform.parent.GetComponent<ISSR_BStoneBehaviour>();
                        BStoneBehav.AgentStops(this.AgentDescriptor.Myself, this.gameObject);
                        EnqueueEvent(ISSREventType.onObjectLost, this.destination_game_object);
                    }
                }
            }
        }

        // Check goal 
        distance = (this.transform.parent.position - this.loco_tmpgoal_location).magnitude;

        if (draw_destinations) DrawDestinations();

        if (distance < this.goal_distance_threshold)
        {  // Arrived to destination


            if (this.AcStopMovement())
            {
                BStoneBehav = this.transform.parent.GetComponent<ISSR_BStoneBehaviour>();
                BStoneBehav.AgentStops(this.AgentDescriptor.Myself, this.gameObject);
            }

            if (use_destination_object)
            {
                // Something strange happened, the agent was trying to reach and object and got to 
                //  its location without colliding with it or gripping it when in gripping distance
                Debug.LogFormat("LOCO {0} cannot find {1} though it arrived to its location", this.AgentDescriptor.Myself.Name, this.destination_object.Name);
                EnqueueEvent(ISSREventType.onObjectLost, this.destination_game_object);
            }
            else
            {

                //  TODO if this is a temporal goal while dodging and object should not post event
                EnqueueEvent(ISSREventType.onDestArrived);
            }
        }

    }



    void UpdateLightPushLoco()
    {

        float distance;
        bool error;

        if (use_destination_object)  // Update destination if going to an object (the agent cannot be trying to grip in this state)
        {
            update_goal_tick++;
            if (update_goal_tick >= update_goal_period)
            {
                update_goal_tick = 0;

                if (this.AgentDescriptor.oiSensable(destination_object))
                {   // Object still in sensing range
                    destination_object.LastLocation = ISSRManager.ISSR_ObjectLocation(this.gameObject, destination_object, out error);
                    SetTmpGoalLocation(ISSRManager.ISSR_ObjectLocation(this.gameObject, destination_object, out error));
                }
                else
                {
                    // stop or go to last location??    better stop
                    this.StopMovement();
                    EnqueueEvent(ISSREventType.onStop);



                    EnqueueEvent(ISSREventType.onObjectLost, this.destination_game_object);
                    if (debug_locomotion)
                    {
                        Debug.LogFormat("LOCO {0} could NOT REACH {1} because she lost it", this.AgentDescriptor.Myself.Name, this.destination_object.Name);
                    }


                    return;
                }
            }
        }


        if (this.advancing)
        {

            rb.velocity = this.transform.TransformDirection(Vector3.forward) * this.lightpush_speed;

            // Check goal 
            distance = (this.transform.position - this.loco_tmpgoal_location).magnitude;

            if (draw_destinations) DrawDestinations();

            if (distance < this.goal_distance_threshold)  // Arrived to destination
            {
                this.StopMovement();
                EnqueueEvent(ISSREventType.onStop);



                if (use_destination_object)
                {
                    // Something strange happened, the agent was trying to reach and object and got to 
                    //  its location without colliding with it or gripping it when in gripping distance
                    Debug.LogFormat("LOCO {0} cannot find {1} though it arrived to its destinations", this.AgentDescriptor.Myself.Name, this.destination_object.Name);
                    EnqueueEvent(ISSREventType.onObjectLost, this.destination_game_object);
                }
                else
                {

                    //  TODO if this is a temporal goal while dodging and object should not post event
                    EnqueueEvent(ISSREventType.onDestArrived);
                }

            }
        }
        else
        {
            float absangle = Mathf.Abs(this.remaining_angle);

            if (absangle < this.max_angle_rotate_advance_lightpush)
            {
                this.advancing = true;
            }

        }

        if (this.rotating)
        {
            float absangle = Mathf.Abs(this.remaining_angle);
            float rotateangle;

            if (absangle < this.lightpush_rotate_speed * Time.deltaTime)
            {
                rotateangle = this.remaining_angle * Time.deltaTime;
            }
            else
            {
                rotateangle = this.lightpush_rotate_speed * this.remaining_angle / absangle * Time.deltaTime;
            }
            this.transform.Rotate(Vector3.up, rotateangle);

            if (this.advancing)
            {
                this.remaining_angle = this.CalculateRotationAngle(this.transform.TransformDirection(Vector3.forward),
                    this.transform.position, this.loco_tmpgoal_location);
            }
            else
            {
                this.remaining_angle -= rotateangle;
            }
        }


    }



    // Auxiliary vector and movement functions-----------------------

    /// <summary>
    /// Calculates the rotation angle between the forward direction of a character and the direction from its position to the destination position.
    /// </summary>
    /// <returns>The rotation angle that apply via Rotate will make the character point from origin to destination</returns>
    /// <param name="forward">Forward direction of the character transformed according to its transform (global coordinates) </param>
    /// <param name="origin">Origin location  of the character in space.</param>
    /// <param name="destination">Destination location to where the character needs to go/point.</param>
    float CalculateRotationAngle(Vector3 forward, Vector3 origin, Vector3 destination)
    {
        float angle;
        Vector3 dir = (destination - origin).normalized;
        dir.y = 0;


        //angle = Mathf.Asin(Vector3.Cross(forward, dir).y);

        angle = Mathf.Atan2(Vector3.Cross(forward, dir).y, Vector3.Dot(forward, dir)) * Mathf.Rad2Deg;



        return angle;
    }

    public bool StartToMove()
    {
        bool canmove;
        bool startsmoving;
        int team = (this.tag == "AgentA") ? 0 : 1;
        GameObject myparent;
        ISSR_AgentBehaviour other_agent_behaviour;

        canmove = TestMove();

        if (canmove)
        {

            if (this.locomotion_active == true)
            {
                startsmoving = false;
            }
            else
            {
                startsmoving = true;
            }

            switch (loco_state)
            {
                case Locomotion.loose:

                    this.locomotion_active = true;

                    if (startsmoving)
                    {
                        EnqueueEvent(ISSREventType.onStartsMoving);

                        if (debug_locomotion)
                        {
                            Debug.LogFormat("LOCO {0} Starts to move in loose locomotion", this.AgentDescriptor.Myself.Name);
                        }
                    }

                    break;
                case Locomotion.lightpush:

                    if (startsmoving)
                    {
                        this.locomotion_active = true;

                        if (debug_locomotion)
                        {
                            Debug.LogFormat("LOCO {0} Starts to move in lightpush locomotion", this.AgentDescriptor.Myself.Name);
                        }

                        if (this.GrippedObject.tag == "SmallStone")
                        {
                            if (debug_locomotion)
                            {
                                Debug.LogFormat("LOCO {0} Increment moving stones for team {1}:{2}", this.AgentDescriptor.Myself.Name, ISSRManager.Teams[team].ShortName,
                                    (team == 0) ? "BLUE" : "RED");
                            }

                            ISSRManager.Teams[team].stones_moving = ISSRManager.Teams[team].stones_moving + 1;
                        }
                        else if ((this.GrippedObject.tag == "AgentA") || (this.GrippedObject.tag == "AgentB"))
                        {  // If I start moving while gripping another agent, she will receive and onStartsMoving event
                            other_agent_behaviour = ISSRManager.GetAgentBehaviour(this.GrippedObject);
                            other_agent_behaviour.EnqueueEvent(ISSREventType.onStartsMoving);
                        }

                        EnqueueEvent(ISSREventType.onStartsMoving);

                    }

                    break;
                case Locomotion.gripped:
                    this.loco_state = Locomotion.loose;
                    // Parent must ungrip me
                    myparent = this.transform.parent.gameObject;
                    other_agent_behaviour = ISSRManager.GetAgentBehaviour(myparent);

                    other_agent_behaviour.PerformUngrip();

                    if (debug_locomotion)
                    {
                        Debug.LogFormat("LOCO {0} gets free from {1} grip!", this.AgentDescriptor.Myself.Name, myparent.name);
                    }

                    if (ISSRManager.AgentMoving(myparent))
                    {
                        other_agent_behaviour.StopMovement();
                        other_agent_behaviour.EnqueueEvent(ISSREventType.onStop);

                    }
                    else
                    {
                        EnqueueEvent(ISSREventType.onStartsMoving);
                        if (debug_locomotion)
                        {
                            Debug.LogFormat("LOCO {0} starts moving in loose locomotion", this.AgentDescriptor.Myself.Name);
                        }
                    }


                    this.locomotion_active = true;
                    break;
                case Locomotion.heavypush:

                    this.locomotion_active = true;


                    break;
            }





        }
        else
        {
            Debug.LogErrorFormat("LOCO {0} cannot move", this.name);
        }



        return canmove;
    }


    public bool TestMove()
    {
        int team = (this.tag == "AgentA") ? 0 : 1;

        if (ISSRManager.AgentMoving(this.gameObject))
        {
            return true;
        }

        switch (loco_state)
        {
            case Locomotion.loose:
            case Locomotion.gripped:
                return true;
            case Locomotion.lightpush:

                if (GrippedObject.tag == "SmallStone")
                {

                    if (ISSRManager.Teams[team].stones_moving < ISSRManager.MaxStonesMovingPerTeam)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            case Locomotion.heavypush:
                if (ISSRManager.Teams[team].stones_moving < ISSRManager.MaxStonesMovingPerTeam)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            default:
                return false;

        }
    }

    #endregion
    //---------------End of AGENT LOCOMOTION-------------------------------------------



    // --- SERVICES IMPLEMENTATION --------------------------------------------
    #region SERVICES 



    public bool AcStopMovement()
    {

        if (this.gameObject.transform.parent != null)
        {
            if ((this.gameObject.transform.parent.gameObject.tag == "AgentA") || (this.gameObject.transform.parent.gameObject.tag == "AgentB"))
            {
                return false;
            }
            StopMovement();
        }
        else
        {
            StopMovement();
        }

        EnqueueEvent(ISSREventType.onStop);
        return true;
    }



    public void SetDestination(Vector3 destination)
    {
        if (StartToMove())
        {
            this.use_destination_object = false;
            SetArmsOutStretched(false);   // When walking arms are at the sides of the body
            this.trying_to_grip = false;
            SetTmpGoalLocation(destination);
        }

    }


    public void SetDestination(ISSR_Object dst_obj, bool wantsToGripIt, out bool error)
    {


        if (StartToMove())
        {

            this.trying_to_grip = wantsToGripIt;
            this.use_destination_object = true;
            this.destination_object = dst_obj;
            this.destination_game_object = ISSRManager.Object2GameObject(dst_obj, out error);

            if (error)
            {
                return;
            }

            if (wantsToGripIt)
            {
                this.gripping_distance = 0.7f + dst_obj.GrippingRadius; // The exact gripping distance is arms length (0.7) + object gripping radius
            }
            SetTmpGoalLocation(ISSRManager.ISSR_ObjectLocation(this.gameObject, dst_obj, out error));
        }
        else
        {
            error = true;
        }

    }

    public void PerformUngrip()
    {

        ISSR_Object grippedobject;
        GameObject grippedGO;

        grippedobject = ISSRManager.GameObject2Object(this.GrippedObject);
        grippedGO = this.GrippedObject;

        if (debug_grip_ungrip)
        {
            Debug.LogFormat("GRIP {0} UNGRIPS {1}", this.gameObject.name, this.GrippedObject.name);
        }

        ISSRManager.UndoGrip(this.AgentDescriptor.Myself, this.gameObject, grippedobject, this.GrippedObject);

        gripping = false;
        FrontBackPush = 0;
        SidewardsPush = 0;

        EnqueueEvent(ISSREventType.onUngrip, grippedGO);
    }


    public void SetTmpGoalLocation(Vector3 tmpdest)
    {
        float absangle;
        ISSR_BStoneBehaviour BStoneBehav;

        switch (loco_state)
        {
            case Locomotion.gripped:
                // Make  my gripping agent ungrip
                break;
            case Locomotion.loose:

                this.AgentDescriptor.dest_location = tmpdest;

                this.remaining_angle = this.CalculateRotationAngle(this.transform.TransformDirection(Vector3.forward),
                    this.transform.position, tmpdest);

                absangle = Mathf.Abs(this.remaining_angle);
                this.loco_tmpgoal_location = tmpdest;

                if (absangle < this.max_angle_rotate_advance)
                {
                    this.advancing = true;
                }
                else
                {
                    this.advancing = false;
                }

                this.rotating = true;


                break;
            case Locomotion.lightpush:


                this.remaining_angle = this.CalculateRotationAngle(this.transform.TransformDirection(Vector3.forward),
                    this.transform.position, tmpdest);

                absangle = Mathf.Abs(this.remaining_angle);
                this.loco_tmpgoal_location = tmpdest;
                this.AgentDescriptor.dest_location = tmpdest;

                if (absangle < this.max_angle_rotate_advance_lightpush)
                {
                    this.advancing = true;
                }
                else
                {
                    this.advancing = false;
                }

                this.rotating = true;
                break;
            default:

                this.loco_tmpgoal_location = tmpdest;
                this.AgentDescriptor.dest_location = tmpdest;
                BStoneBehav = ISSRManager.BStonesBehaviors[this.AgentDescriptor.GrippedObject.index];
                BStoneBehav.AgentPushes(this.AgentDescriptor.Myself, this.gameObject, tmpdest);

                break;
        }


    }

    // Timer related---------------------------------
    IEnumerator AgentTimer()
    {
        yield return new WaitForSeconds(timer_delay);
        EnqueueEvent(ISSREventType.onTimerOut);
    }

    public void ACSetTimer(float delay)
    {
        timer_delay = delay;
        timer_running = true;
        StartCoroutine(AgentTimer());
    }

    // Many collisions related
    IEnumerator CollisionCounter()
    {
        while (true)
        {
            collisions_in_period = 0;
            yield return new WaitForSeconds(collisions_observation_period);
            //if (collisions_in_period > 0)
            //    Debug.LogFormat("Collisions in period {0}", collisions_in_period);

            if (collisions_in_period >= collisions_in_period_trigger)
            {
                EnqueueEvent(ISSREventType.onManyCollisions);
                PlaySfx(AgentSFX.Many_Collisions);
                //Debug.Log("on many collisions");
            }
        }

    }


    void IncreaseCollisionsInPeriod()
    {
        collisions_in_period++;
        if (collisions_in_period >= collisions_in_period_trigger)
        {
            EnqueueEvent(ISSREventType.onManyCollisions);
            PlaySfx(AgentSFX.Many_Collisions);
            //Debug.Log("on many collisions");
            collisions_in_period = 0;
        }
        
    }

    #endregion
    // End of--- SERVICES IMPLEMENTATION --------------------------------------------


    // -------------AGENT EVENT HANDLING------------------------------------------------
    #region EVENT HANDLING
    /// <summary>
    /// Enqueues an event in the incoming events queue. Version using a GameObject
    /// </summary>
    /// <param name="type">Type.</param>
    /// <param name="go">Go.</param>
    public void EnqueueEvent(ISSREventType type, GameObject go = null)
    {
        ISSR_Object Obj;
        ISSR_Event theevent;
        bool error;

        switch (type)
        {
            case ISSREventType.onEnterSensingArea:
            case ISSREventType.onExitSensingArea:
            case ISSREventType.onGripSuccess:
            case ISSREventType.onGripFailure:
            case ISSREventType.onObjectLost:
            case ISSREventType.onCollision:
            case ISSREventType.onGObjectScored:
            case ISSREventType.onUngrip:
            case ISSREventType.onGObjectCollision:
            case ISSREventType.onAnotherAgentGripped:
            case ISSREventType.onAnotherAgentUngripped:
            case ISSREventType.onPushTimeOut:
                Obj = ISSRManager.GameObject2Object(go);
                Obj.LastLocation = ISSRManager.ISSR_ObjectLocation(this.gameObject, Obj, out error);
                if (error)
                {
                    Debug.LogError("Unknown error reading object location ");
                }
                else
                {
                    theevent = new ISSR_Event(type, Obj);
                    if (type == ISSREventType.onTimerOut)
                    {
                        theevent.f = timer_delay;
                    }
                    this.IncomingEvents.Add(theevent);
                }


                break;
            case ISSREventType.onStop:
            case ISSREventType.onDestArrived:
            case ISSREventType.onStartsMoving:
            case ISSREventType.onManyCollisions:
            case ISSREventType.onTimerOut:

                theevent = new ISSR_Event(type, null);
                this.IncomingEvents.Add(theevent);
                break;
            default:
                Debug.LogErrorFormat("Unknown event type: {0}", type);
                break;
        }
    }


    void PostAllEvents()
    {
        if (receiving_enabled)
        {
            ISSR_Message msg;

            while (this.IncomingMsgs.Count > 0)
            {
                msg = this.IncomingMsgs[0];
                this.AgentDescriptor.current_event = ISSREventType.onMsgArrived;
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onMsgArrived@{0} rcv msg {1} from {2}", this.AgentDescriptor.Myself.Name, msg.usercode, msg.Sender.Name);
                }

                this.AgentDescriptor.user_msg_code = msg.usercode;
                this.AgentDescriptor.msg_obj = msg.Obj;
                this.AgentDescriptor.msg_location = msg.location;
                this.AgentDescriptor.onMsgArrived(msg);
                
                this.IncomingMsgs.RemoveAt(0);
            }
        }

        if (sensing_enabled)
        {
            ISSR_Event ev;

            while (this.IncomingEvents.Count > 0)
            {
                ev = this.IncomingEvents[0];
                this.PostEvent(ev);
                this.IncomingEvents.RemoveAt(0);
            }
        }
    }

    void PostEvent(ISSR_Event ev)
    {
        this.AgentDescriptor.current_event = ev.Type;
        ISSR_Object newobject = null;
        int t = (this.tag == "AgentA") ? 0 : 1;

        if (ev.Obj != null)
        {
            newobject = new ISSR_Object(ev.Obj);
        }

        switch (ev.Type)
        {
            case ISSREventType.onEnterSensingArea:
                this.AgentDescriptor.SensableObjects.Add(newobject);
                newobject.TimeStamp = Time.time;
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onEnterSensingArea@{0} begins to sense {1}", this.AgentDescriptor.Myself.Name, newobject.Name);
                }
                this.AgentDescriptor.onEnterSensingArea(newobject);
                
                break;
            case ISSREventType.onExitSensingArea:
                //newobject.TimeStamp = Time.time;// TODO no estoy seguro
                this.AgentDescriptor.SensableObjects.Remove(newobject);
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onExitSensingArea@{0} finishes sensing {1}", this.AgentDescriptor.Myself.Name, newobject.Name);
                }
                this.AgentDescriptor.onExitSensingArea(newobject);
                
                break;
            case ISSREventType.onMsgArrived:
                Debug.LogErrorFormat("onMsgArrived@{0} event type \"{1}\" implemented in PostAllEvents()",
                    this.AgentDescriptor.Myself.Name, ev.Type);
                break;
            case ISSREventType.onGripSuccess:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onGripSuccess@{0} has GRIPPED  {1}", this.AgentDescriptor.Myself.Name, ev.Obj.Name);
                }
                this.AgentDescriptor.onGripSuccess(new ISSR_Object(ev.Obj));
                break;
            case ISSREventType.onGripFailure:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onGripFailure@{0} COULD NOT grip  {1}", this.AgentDescriptor.Myself.Name, ev.Obj.Name);
                }
                this.AgentDescriptor.onGripFailure(new ISSR_Object(ev.Obj));
                break;
            case ISSREventType.onObjectLost:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onObjectLost@{0} COULD NOT reach  {1}", this.AgentDescriptor.Myself.Name, ev.Obj.Name);
                }
                this.AgentDescriptor.onObjectLost(new ISSR_Object(ev.Obj));
                break;
            case ISSREventType.onCollision:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onCollision@{0} has COLLIDED with {1}", this.AgentDescriptor.Myself.Name, ev.Obj.Name);
                }
                int team = (this.tag == "AgentA") ? 0 : 1;
                ISSRManager.Teams[team].number_of_collisions++;
                this.AgentDescriptor.onCollision(new ISSR_Object(ev.Obj));
                break;
            case ISSREventType.onGObjectScored:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onGObjectScored@{0} has SCORED with {1} in goal", this.AgentDescriptor.Myself.Name, ev.Obj.Name);
                }
                this.AgentDescriptor.onGObjectScored(new ISSR_Object(ev.Obj));
                break;
            case ISSREventType.onUngrip:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onUngrip@{0} UNGRIPPED  {1}", this.AgentDescriptor.Myself.Name, ev.Obj.Name);
                }
                this.AgentDescriptor.onUngrip(new ISSR_Object(ev.Obj));
                break;
            case ISSREventType.onGObjectCollision:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onGObjectCollision@{0} carried by {1} COLLIDED with {2}", this.AgentDescriptor.GrippedObject.Name,
                        this.AgentDescriptor.Myself.Name, ev.Obj.Name);
                }
                
                ISSRManager.Teams[t].number_of_collisions++;
                this.AgentDescriptor.onGObjectCollision(new ISSR_Object(ev.Obj));
                break;

            case ISSREventType.onManyCollisions:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onManyCollsions@{0}", this.AgentDescriptor.Myself.Name);
                }
                this.AgentDescriptor.onManyCollisions();
                break;
            case ISSREventType.onStartsMoving:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onStartsMoving@{0} starts MOVING", this.AgentDescriptor.Myself.Name);
                }
                this.AgentDescriptor.onStartsMoving();
                break;
            case ISSREventType.onStop:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onStop@{0} has STOPPED", this.AgentDescriptor.Myself.Name);
                }
                this.AgentDescriptor.onStop();
                break;
            case ISSREventType.onDestArrived:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onDestArrived@{0} REACHED destination set.", this.AgentDescriptor.Myself.Name);
                }
                this.AgentDescriptor.onDestArrived();
                break;
            case ISSREventType.onAnotherAgentUngripped:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onAnotherAgentUngripped@{0} another agent ({1}) GRIPPED the stone I am gripping",
                        this.AgentDescriptor.Myself.Name, ev.Obj.Name);
                }
                this.AgentDescriptor.onAnotherAgentUngripped(new ISSR_Object(ev.Obj));
                break;
            case ISSREventType.onAnotherAgentGripped:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT onAnotherAgentGripped@{0} another agent ({1}) UNGRIPPED the stone I am gripping",
                        this.AgentDescriptor.Myself.Name, ev.Obj.Name);
                }
                this.AgentDescriptor.onAnotherAgentGripped(new ISSR_Object(ev.Obj));
                break;
            case ISSREventType.onPushTimeOut:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT @{0} Timeout trying to push bigtone {1}", this.AgentDescriptor.Myself.Name, ev.Obj.Name);
                }
                this.AgentDescriptor.onPushTimeOut(new ISSR_Object(ev.Obj));
                break;
            case ISSREventType.onTimerOut:
                if (debug_events)
                {
                    Debug.LogFormat("EVENT @{0} Timer Timeout", this.AgentDescriptor.Myself.Name);
                }
                timer_running = false;
                this.AgentDescriptor.onTimerOut(timer_delay);
                break;
            default:
                Debug.LogErrorFormat("EVENT {0} event type \"{1}\" not implemented", this.AgentDescriptor.Myself.Name, ev.Type);
                break;
        }
    }

    /// <summary>
    /// Enqueues a  message in the incoming messages queue of this agent
    /// </summary>
    /// <param name="msg">Message.</param>
    public void EnqueueMsg(ISSR_Message msg)
    {
        IncomingMsgs.Add(new ISSR_Message(msg));
    }

    /// <summary>
    /// Sends the message to all agents within communications range
    /// </summary>
    /// <param name="msg">Message.</param>
    public void SendMsg(ISSR_Message msg)
    {
        int team = (this.tag == "AgentA") ? 0 : 1;
        ISSRManager.Teams[team].number_of_messages++;
        foreach (GameObject receiver in AgentsInCommRange)
        {
            receiver.SendMessage("EnqueueMsg", msg, SendMessageOptions.DontRequireReceiver);
        }

    }

    //-----------------------------Event Handlers  for communications 



    // Keeps track of Agents within "hearing" communications distance:  Adding Agents entering area
    void OnTriggerEnter(Collider other)
    {
        //Vector3 distance;
        // Ignore of other GameObjects save the Agents 
        if (other.tag == "AgentA" || other.tag == "AgentB")
        {
            // Only react to a non-trigger Collider (the capsule collider of the Agent)
            if (other.isTrigger == false)
            {
                //distance = this.transform.position - other.transform.position;

                // Unfortunately this OnTriggerEnter triggers both for the Comm Sphere and the Sensing Sphere
                // Only add to list if not already added
                if (!this.AgentsInCommRange.Contains(other.gameObject))
                {
                    if (debug_comms)
                    {
                        Debug.LogFormat("COMMS {0} STARTS HEARING {1}", this.name, other.name);
                    }

                    this.AgentsInCommRange.Add(other.gameObject);
                }


            }

        }
    }

    // Keeps track of Agents within "hearing" communications distance:  Removing Agents exiting area
    void OnTriggerExit(Collider other)
    {
        Vector3 distance;


        // Ignore of other GameObjects save the Agents 
        if (other.tag == "AgentA" || other.tag == "AgentB")
        {
            // Only react to a non-trigger Collider (the capsule collider of the Agent)
            if (other.isTrigger == false)
            {
                distance = this.transform.position - other.transform.position;

                //Debug.LogFormat ("{0} OnTriggerExit() {1}: dist: {2}", this.name, other.name, distance.magnitude);
                if (distance.magnitude >= ISSRManager.CommsRange)
                {
                    if (debug_comms)
                    {
                        Debug.LogFormat("COMMS {0} STOPS HEARING {1}", this.name, other.name);
                    }

                    this.AgentsInCommRange.Remove(other.gameObject);

                }

            }

        }
    }


    void CollisionSortOut(Collision collision, out bool collided_with_body, out bool collided_with_gripped_object)
    {
        if (this.GrippedObject == null)
        {
            collided_with_body = true;
            collided_with_gripped_object = false;
        }
        else
        {
            Vector3 point;

            collided_with_body = false;
            collided_with_gripped_object = false;

            foreach (ContactPoint contact in collision.contacts)
            {

                point = contact.point;
                point.y = 0;


                point -= this.transform.position;

                if (point.magnitude > 0.55f)
                {
                    collided_with_gripped_object = true;

                    if (debug_collisions)
                    {
                        Debug.LogFormat("COLL {0} GRIPPED object ({1}) collided WITH {2}",
                            this.name, this.GrippedObject.name, contact.otherCollider.name);
                    }
                }
                else
                {
                    collided_with_body = true;

                    if (debug_collisions)
                    {
                        Debug.LogFormat("COLL {0} BODY collided WITH {1}", this.name, contact.otherCollider.name);
                    }
                }

            }
        }
    }


    void OnCollisionStay(Collision collision)
    {
        // TODO move agent backwards to avoid collision
        if (collision.gameObject.tag != "Ignore")
        {
            this.ForceSeparation(collision);
        }
    }


    // ---------------------Event Handlers fo collisions
    void OnCollisionEnter(Collision collision)
    {

        bool collided_with_body = false;
        bool collided_with_gripped_object = false;

        bool agent_moving = false;

        bool should_stop = false;
        bool should_ungrip = false;

        bool carrying_stone = false;
        bool carrying_goal = false;

        bool colliding_with_goal = false;
        bool colliding_with_stone = false;

        bool colliding_with_agent_carrying_goal = false;
        ISSR_AgentBehaviour CollidingAgentBehav;

        bool should_score = false;

        GameObject stone = null;
        GameObject goal = null;

        //GameObject myobject= null;

        if (collision.gameObject.tag != "Ignore")
        {

            agent_moving = ISSRManager.AgentMoving(this.gameObject);

            

            // Check collisions between this agent and other objects and the object she is gripping and other objects
            CollisionSortOut(collision, out collided_with_body, out collided_with_gripped_object);


            // First of all obtain some flags about what she has collided with and what she is carrying. 
            if ((collision.gameObject.tag == "GoalA") || (collision.gameObject.tag == "GoalB"))
            {
                colliding_with_goal = true;
                goal = collision.gameObject;
            }

            if ((collision.gameObject.tag == "SmallStone") || (collision.gameObject.tag == "BigStone"))
            {
                colliding_with_stone = true;
                stone = collision.gameObject;
            }


            if ((collision.gameObject.tag == "AgentA") || (collision.gameObject.tag == "AgentB"))
            {
                CollidingAgentBehav = ISSRManager.GetAgentBehaviour(collision.gameObject);

                if (CollidingAgentBehav.GrippedObject != null)
                {
                    if ((CollidingAgentBehav.GrippedObject.tag == "GoalA") || (CollidingAgentBehav.GrippedObject.tag == "GoalB"))
                    {
                        colliding_with_agent_carrying_goal = true;
                        goal = CollidingAgentBehav.GrippedObject;
                    }
                }

            }

            // Only if carrying an object
            if (GrippedObject != null)
            {

                // Get information for special cases
                if ((GrippedObject.tag == "SmallStone") || (GrippedObject.tag == "BigStone"))
                {
                    carrying_stone = true;
                    stone = this.GrippedObject;
                }

                if ((GrippedObject.tag == "GoalA") || (GrippedObject.tag == "GoalB"))
                {
                    carrying_goal = true;
                    goal = this.GrippedObject;
                }

            }



            // Collision with body of this agent---------------------------
            if (collided_with_body)
            {


                if (agent_moving)
                {

                    if ((carrying_goal) && (colliding_with_stone))
                    {
                        should_stop = false;
                        should_score = true;
                    }
                    else if ((colliding_with_goal) && (carrying_stone))  // 2018---
                    {
                        should_stop = false;
                        should_score = true;
                    }
                    else
                    {
                        should_stop = true;
                        EnqueueEvent(ISSREventType.onCollision, collision.gameObject);
                        IncreaseCollisionsInPeriod();

                        if ((collision.gameObject.tag == "AgentA") || (collision.gameObject.tag == "AgentB"))
                        {
                            PlaySfx(AgentSFX.Body_Collision, 3);
                        }
                        else
                        {
                            PlaySfx(AgentSFX.Body_Small_Collision, 2);
                        }

                    }

                    
                }
                else
                {
                    if (this.GrippedObject != null)
                    {
                        should_ungrip = true;
                        PlaySfx(AgentSFX.Body_Collision_Ungrip);
                    }
                    else
                    {
                        PlaySfx(AgentSFX.Body_Collision, 3);
                    }

                    EnqueueEvent(ISSREventType.onCollision, collision.gameObject);
                    IncreaseCollisionsInPeriod();
                }

                // Special case-------Agent collides with goal and she is carrying a stone-------
                // ---------should stop ungrip and score

                if (carrying_stone && colliding_with_goal)
                {
                    should_ungrip = true;
                    should_score = true;
                }

            }
            // End of Collision with body of this agent

            // Collided with gripped object--------------------------
            if (collided_with_gripped_object)
            {


                if (agent_moving)
                {
                    if ((carrying_goal) && (colliding_with_stone))
                    {
                        should_stop = false;
                    }
                    else
                    {
                        should_stop = true;
                    }
                }




                if ((carrying_stone && colliding_with_goal) || (carrying_stone && colliding_with_agent_carrying_goal))
                {
                    should_ungrip = true;
                }

                if ((carrying_stone && colliding_with_goal) || (carrying_goal && colliding_with_stone)
                    || (carrying_stone && colliding_with_agent_carrying_goal))
                {
                    should_score = true;
                }
                else
                {
                    EnqueueEvent(ISSREventType.onGObjectCollision, collision.gameObject);
                    IncreaseCollisionsInPeriod();
                    if ((GrippedObject.tag == "BigStone") || (collision.gameObject.tag == "BigStone"))
                    {
                        PlaySfx(AgentSFX.Big_Collision, 2);
                    }
                    else
                    {
                        //if ((collision.gameObject.tag != "AgentA") && (collision.gameObject.tag != "AgentB"))
                        // {
                        PlaySfx(AgentSFX.Small_Collision, 2);
                        // }

                    }

                }


            }
            // End of Collided with gripped object--------------------------

            if (should_stop)
            {
                this.StopMovement();
                EnqueueEvent(ISSREventType.onStop, collision.gameObject);
            }

            if ((should_score) && (stone.tag == "BigStone"))
            {   // In case of big stones the perform the ungrip of all agents when scoring, they also receive 
                //  onUngrip() and onGObjectScored() 
                should_ungrip = false;
            }

            if (should_ungrip)
            {

                PerformUngrip();

            }


            if (should_score)
            {

                ISSRManager.ScoreUpdate(this.gameObject, stone, goal);
                if (stone.tag != "BigStone")
                {
                    EnqueueEvent(ISSREventType.onGObjectScored, stone);
                }
                ISSRManager.DestroyStone(stone);
            }

            if (debug_collisions)
            {
                Debug.LogFormat("COLL {0}({3})@{1}{2} {4}{5}{6}{7}{8}{9}{10}{11}", this.AgentDescriptor.Myself.Name,
                    (collided_with_body) ? "Body" : "", (collided_with_gripped_object) ? "GObject" : "",
                    (agent_moving) ? "Moving" : "Not moving", (carrying_stone) ? ", Carrying STONE" : "", (carrying_goal) ? ", Carrying GOAL" : "",
                    (colliding_with_goal) ? ", against GOAL" : "", (colliding_with_stone) ? ", against STONE" : "",
                    (colliding_with_agent_carrying_goal) ? ", against Agent carrying Goal" : "",
                    (should_stop) ? ", will STOP" : "", (should_ungrip) ? ", will UNGRIP" : "", (should_score) ? ", will SCORE" : "");

            }

            if (agent_moving)
            {
                this.ForceSeparation(collision);
            }
        }

    }


    void ForceSeparation(Collision col)
    {
        Vector3 direction;
        float factor = 0.02f;
        switch (col.gameObject.tag)
        {
            case "WestWall":
                direction = Vector3.right;
                break;
            case "EastWall":
                direction = Vector3.left;
                break;
            case "NorthWall":
                direction = Vector3.back;
                break;
            case "SouthWall":
                direction = Vector3.forward;
                break;
            default:
                direction = this.gameObject.transform.position - col.gameObject.transform.position;
                direction.Normalize();
                break;
        }

        this.rb.position = this.gameObject.transform.position + direction * factor;
    }
    #endregion
    // End of AGENT EVENT HANDLING----------------------------------------------------


    // ANIMATION  Attributes and Methods------------------------------------------------
    #region ANIMATION  Attributes and Methods

    float ArmsStretched;
    public bool gripping;
    public float SidewardsPush;
    public float FrontBackPush;

    public void SetArmsOutStretched(bool armsout)
    {
        float value;

        if (armsout)
        {
            value = 1;
        }
        else
        {
            value = 0;
        }
        this.ArmsStretched = value;
        anim.SetFloat("StretchArms", this.ArmsStretched, 0.7f, Time.deltaTime);

    }


    void UpdateAnimation()
    {
        switch (loco_state)
        {
            case Locomotion.loose:
                if (this.advancing)
                {
                    anim.SetFloat("Speed", 1);
                }
                else
                {
                    anim.SetFloat("Speed", 0);
                }
                break;
            case Locomotion.lightpush:
                if (this.advancing)
                {
                    FrontBackPush = 1;
                }
                else
                {
                    FrontBackPush = 0;
                }
                break;
        }

        anim.SetBool("gripping", gripping);
        anim.SetFloat("SidewardsPush", SidewardsPush);
        anim.SetFloat("FrontBackPush", FrontBackPush);

        SetArmsOutStretched(this.trying_to_grip);  // Only when walking

    }

    IEnumerator AnimateHead()
    {
        float waittime;
        int headrotation;
        while (true)
        {
            waittime = 4 + Random.value * 5;
            yield return new WaitForSeconds(waittime);

            headrotation = (Random.value < 0.5f) ? 0 : 1;

            if (headrotation == 0)
            {
                anim.SetBool("HeadLeft", true);
            }
            else
            {
                anim.SetBool("HeadRight", true);
            }

            yield return new WaitForSeconds(3.6f);
            anim.SetBool("HeadLeft", false);
            anim.SetBool("HeadRight", false);


            yield return new WaitForSeconds(4);

        }

    }

    // ANIMATION  Attributes and Methods------------------------------------------------

    #endregion


    public void PlaySfx(AgentSFX effect)
    {

        if (this.audiosource.isPlaying)
        {
            this.audiosource.Stop();
        }


        if (!this.audiosource.isPlaying)
        {
            this.audiosource.clip = this.sound_effects[(int)effect];
            this.audiosource.Play();
        }

    }

    public void PlaySfx(AgentSFX base_effect, int range)
    {
        int effect;
        if (range == 1)
        {
            PlaySfx(base_effect);
        }
        else
        {
            effect = Random.Range((int) base_effect, (int)(base_effect+range));
            PlaySfx((AgentSFX) effect);
        }
    }
}
