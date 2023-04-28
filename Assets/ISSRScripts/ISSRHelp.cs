using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ISSRHelp  {

    /// <summary>
    ///  Calculates a safe location to go after collision of the agent with the given object.
    ///  This location is a  orthogonal direction to the coliding direction and trying to avoid it.
    /// </summary>
    /// <param name="a"> The Agent that callas</param>
    /// <param name="colliding_object">Object that has just collided with agent 
    ///  or object she was carring</param>
    /// <returns> Vector3 A safe location to move to avoiding the colling_object</returns>
    public static Vector3 CalculateSafeLocation(ISSR_Agent a, ISSR_Object colliding_object)
    {  // Parámetro, el objeto que ha colisionado oc
        Vector3 SafeLocation;
        Vector3 collider_direction; // dc dirección de colisión, a oc desde agente o piedra 
        Vector3 collision;  // Vector perpendicular a anterior y dirección de avance
        Vector3 new_direction; // nd Dirección de movimiento para apartarse de oc  
                               //  da  es  oiAgentDirection (Myself)

        // Obtener dc Dirección hacia obstáculo, respecto a agente o piedra 
        if (a.current_event == ISSREventType.onCollision)
        { // Si es el agente el que ha chocado con el obstáculo
          // Vector desde el agente al objeto que ha colisionado
            collider_direction = a.oiLocation(colliding_object) - a.oiLocation(a.Myself);
        }
        else  // current_event == ISSREventType.onGObjectCollision
        { // Si es el objeto agarrado por el agente el que ha chocado
          // Vector desde el objeto agarrado por el agente al objeto que ha colisionado
            collider_direction = a.oiLocation(colliding_object) - a.oiLocation(a.GrippedObject);
        }


        collision = Vector3.Cross(collider_direction, a.oiAgentDirection(a.Myself));
        // El signo del vector resultado es proporcional al seno del ángulo que forman:
        // Vector en dirección Y positivo si agente tiene que ir hacia la izquierda, 
        // según su dirección de avance, negativo si tiene que ir a su derecha

        collision.Normalize();  // Normalizarlo para que tenga longitud 1

        new_direction = Vector3.Cross(collision, collider_direction.normalized);
        // Dirección perpendicular a vector vertical y la dirección de colisión 
        // será en la dirección de apartarse, tendrá longitud 1

        if (colliding_object.type == ISSR_Type.BigStone)
        {  // Si la piedra es grande me muevo dos unidades 
            SafeLocation = a.oiLocation(a.Myself) + 2 * new_direction.normalized;

        }
        else
        {  // Si es un objeto pequeño como piedra pequeña, agente o meta 
           // me muevo una unidad más una cantidad aleatoria entre 0 y 1
            SafeLocation = a.oiLocation(a.Myself) + (1 + Random.value) * new_direction.normalized;
        }
        return SafeLocation;
    }

    /// <summary>
    /// Counts the number of objects of a given type in a given list
    /// </summary>
    /// <param name="ObjList">List of objects provided</param>
    /// <param name="obj_type">Type of Object to count in list</param>
    /// <returns>number of objects found</returns>
    public static int NumberOfObjectsOfTypeInList(List<ISSR_Object> ObjList, ISSR_Type obj_type)
    {
        int count = 0;

        foreach (ISSR_Object obj in ObjList)
        {
            if (obj.type == obj_type)
                count++;
        }

        return count;
    }


    /// <summary>
    /// Obtains distance from the calling agent to an object
    /// </summary>
    /// <param name="a">The Agent</param>
    /// <param name="obj">The Object (if null returns +infinity)
    /// if the object is not sensable will use its last know location</param>
    /// <returns>Distance to object</returns>
    public static float Distance_from_object_to_me(ISSR_Agent a, ISSR_Object obj)  
    { // Distancia de un objeto a la meta
        float distance = 0;
        Vector3 my_location = a.oiLocation(a.Myself);
        Vector3 obj_location;

        if (obj == null)
            return Mathf.Infinity;

        if (a.oiSensable(obj)) // Visible
            obj_location = a.oiLocation(obj);
        else // no visible, puede no ser cierto
            obj_location = obj.LastLocation;

        distance = (my_location - obj_location).magnitude;

        return distance;
    }


    /// <summary>
    /// Obtains distance from object to the calling agent goal
    /// </summary>
    /// <param name="a">The Agent.</param>
    /// <param name="obj">The Object (if null returns +infinity)
    /// if the object is not sensable will use its last know location</param></param>
    /// <returns>Distance of object to Goal</returns>
    public static float Distance_from_object_to_goal(ISSR_Agent a, ISSR_Object obj)
    { // Distancia de un objeto a la meta
        float distance = 0;
        Vector3 goal_location = a.iMyGoalLocation();
        Vector3 obj_location;

        if (obj == null)
            return Mathf.Infinity;

        if (a.oiSensable(obj)) // Visible
            obj_location = a.oiLocation(obj);
        else // no visible, puede no ser cierto
            obj_location = obj.LastLocation;

        distance = (goal_location - obj_location).magnitude;

        return distance;
    }

    /// <summary>
    /// Gets the object of the specified type which is closer to the given agent in the provided list.
    /// </summary>
    /// <returns>The closer object in list  or null if none was found </returns>
    /// <param name="a">ISSR_Agent.</param> 
    /// <param name="ObjList">Object list.</param>
    /// <param name="obj_type">Object type to look for.</param>
    public static ISSR_Object GetCloserToMeObjectInList(ISSR_Agent a, List<ISSR_Object> ObjList, ISSR_Type obj_type)
	{
		ISSR_Object obj_found = null;
		float mindistance = Mathf.Infinity;
		float distance;
		Vector3 to_obj;

		if (ObjList.Count!=0)  // If the list is not empty
		{
			foreach (ISSR_Object obj in ObjList) // Take a look at every object
			{
				if (obj.type == obj_type)  // Only if it is the type I am looking for
				{
					if (a.oiSensable(obj))  // If it is within sensing range (I can see it)
					{
						distance = a.oiDistanceToMe (obj); // Obtain how far it is from me
					}

					else
					{  // In case it is not in sensing range, use its last location
						to_obj = (obj.LastLocation - a.oiLocation (a.Myself));
						distance = to_obj.magnitude;  // To calculate distance to me
					}

					if (distance < mindistance)  // If this distance is less than any other
					{
						mindistance = distance; // Update minimum distance
						obj_found = obj;   // This is the best object for the moment
					}
				}  // End of object of the type I want
			} // End of foreach
		} // End of there are objects in the list

		return obj_found;
	}

    /// <summary>
    /// Gets object in the list that is closer to the goal of this agent
    /// </summary>
    /// <param name="a">The Agent.</param>
    /// <param name="ObjList">Object list.</param>
    /// <param name="obj_type">Object type to look for.</param>
    /// <returns></returns>
    public static ISSR_Object GetCloserToGoalObjectInList(ISSR_Agent a, List<ISSR_Object> ObjList, ISSR_Type obj_type)
    {
        ISSR_Object obj_found = null;
        float mindistance = Mathf.Infinity;
        float distance;
        Vector3 to_obj;

        if (ObjList.Count != 0)  // If the list is not empty
        {
            foreach (ISSR_Object obj in ObjList) // Take a look at every object
            {
                if (obj.type == obj_type)  // Only if it is the type I am looking for
                {
                    if (a.oiSensable(obj))  // If it is within sensing range (I can see it)
                    {
                        to_obj = (a.oiLocation(obj) - a.iMyGoalLocation());
                    }
                    else
                    {  // In case it is not in sensing range, use its last location
                        to_obj = (obj.LastLocation - a.iMyGoalLocation());
                    }
                    distance = to_obj.magnitude;  // To calculate distance to me
                    if (distance < mindistance)  // If this distance is less than any other
                    {
                        mindistance = distance; // Update minimum distance
                        obj_found = obj;   // This is the best object for the moment
                    }
                }  // End of object of the type I want
            } // End of foreach
        } // End of there are objects in the list

        return obj_found;
    }


    /// <summary>
    /// Gets the location in the list that is closer to the agent
    /// </summary>
    /// <returns>Gets the location in list which is closer to the given agent.</returns>
    /// <param name="a">The Agent.</param>
    /// <param name="LocList">Location list.</param>
    /// <param name="remaining_elements">Output Remaining elements to detect when list is empty</param>
    public static Vector3 GetCloserToMeLocationInList(ISSR_Agent a, List<Vector3> LocList, out int remaining_elements)
	{
		Vector3 closest_location = new Vector3(0,0,0);
		float mindistance = Mathf.Infinity;
		float distance;
		Vector3 to_point;

		remaining_elements = LocList.Count;

		if (remaining_elements > 0)
		{
			foreach( Vector3 location in LocList)
			{
				to_point = (location - a.oiLocation (a.Myself));
				distance = to_point.magnitude;

				if (distance < mindistance)
				{
					mindistance = distance;
					closest_location = location;
				}
			}
		}

		return closest_location;
	}

    /// <summary>
    /// Gets location in the list that is closer to the goal of this agent
    /// </summary>
    /// <param name="a">The Agent.</param>
    /// <param name="LocList">Location list.</param>
    /// <param name="remaining_elements">Output Remaining elements to detect when list is empty</param>
    /// <returns></returns>
    public static Vector3 GetCloserToGoalLocationInList(ISSR_Agent a, List<Vector3> LocList, out int remaining_elements)
    {
        Vector3 closest_location = new Vector3(0, 0, 0);
        float mindistance = Mathf.Infinity;
        float distance;
        Vector3 to_point;
        Vector3 goal_location = a.iMyGoalLocation();

        remaining_elements = LocList.Count;

        if (remaining_elements > 0)
        {
            foreach (Vector3 location in LocList)
            {
                to_point = (location - goal_location);
                distance = to_point.magnitude;

                if (distance < mindistance)
                {
                    mindistance = distance;
                    closest_location = location;
                }
            }
        }

        return closest_location;
    }


    
    /// <summary>
    /// Searches Agent's Valid_Small_Stones and Valid_Big_Stones lists for the stone which is closer to the given Agent
    /// </summary>
    /// <param name="a">The Agent.</param>
    /// <returns>Closer to given Agent stone or null if none found</returns>
    public static ISSR_Object Get_next_available_stone_closer_to_me(ISSR_Agent a)  
    {  // Consigue piedra disponible  más próxima a la meta 
        ISSR_Object next_stone, small_stone, big_stone;
        // Coge la piedra más próxima a la meta de cada lista
        small_stone = GetCloserToMeObjectInList(a, a.Valid_Small_Stones, ISSR_Type.SmallStone);
        big_stone = GetCloserToMeObjectInList(a, a.Valid_Big_Stones, ISSR_Type.BigStone);

        next_stone = null;

        if (small_stone == null)  // Si no hay piedras pequeñas
        {
            next_stone = big_stone; // Cojo la grande
        }

        if (big_stone == null)  // Si no hay piedras grandes
        {
            next_stone = small_stone; // Cojo la pequeña
        }

        // Si tengo una pequeña y una grande, cojo la más próxima a la meta.
        if ((small_stone != null) && (big_stone != null))
        {
            float dist_sstone = Distance_from_object_to_me(a, small_stone);
            float dist_bstone = Distance_from_object_to_me(a, big_stone);

            if (dist_sstone < dist_bstone)
            {
                next_stone = small_stone;
            }
            else
            {
                next_stone = big_stone;
            }
        }

        return next_stone;
    }

    /// <summary>
    /// Searches Agent's Valid_Small_Stones and Valid_Big_Stones lists for the stone which is closer to the GOAL of the given Agent
    /// </summary>
    /// <param name="a">The Agent.</param>
    /// <returns>Closer to given Agent's Goal stone or null if none found</returns>
    public static ISSR_Object Get_next_available_stone_closer_to_goal(ISSR_Agent a) 
    {  // Consigue piedra disponible  más próxima a la meta 
        ISSR_Object next_stone, small_stone, big_stone;
        // Coge la piedra más próxima a la meta de cada lista
        small_stone = GetCloserToGoalObjectInList(a, a.Valid_Small_Stones, ISSR_Type.SmallStone);
        big_stone = GetCloserToGoalObjectInList(a, a.Valid_Big_Stones, ISSR_Type.BigStone);

        next_stone = null;

        if (small_stone == null)  // Si no hay piedras pequeñas
        {
            next_stone = big_stone; // Cojo la grande
        }

        if (big_stone == null)  // Si no hay piedras grandes
        {
            next_stone = small_stone; // Cojo la pequeña
        }

        // Si tengo una pequeña y una grande, cojo la más próxima a la meta.
        if ((small_stone != null) && (big_stone != null))
        {
            float dist_sstone = Distance_from_object_to_goal(a,small_stone);
            float dist_bstone = Distance_from_object_to_goal(a, big_stone);

            if (dist_sstone < dist_bstone)
            {
                next_stone = small_stone;
            }
            else
            {
                next_stone = big_stone;
            }
        }

        return next_stone;
    }

    /// <summary>
    /// Updates a list of objects with the most recent version of them.
    ///   the given obj is add to the list if it is not in it
    ///   if the object is in the list timestamps are checked 
    ///     and it is only update if obj has a higher timestamp than the one in the list
    /// </summary>
    /// <param name="obj">object to be tested for update</param>
    /// <param name="obj_list">List where the object should be stored or updated.</param>
    public static void UpdateObjectList(ISSR_Object obj, List<ISSR_Object> obj_list)
    {

       
        if (!obj_list.Contains(obj))
        {  // the list does not contain the object, add it.
            obj_list.Add(obj);
        }
        else
        {  // the list contains a version of the object, check timestamps
            int index = obj_list.IndexOf(obj);
            // If the version in the list is older than the version in obj, it is updated.
            if (obj_list[index].TimeStamp < obj.TimeStamp)
            {
                obj_list[index] = obj;
            }
        }
    }

    /// <summary>
    ///  Updates two stone lists: for valid and invalid stones, with the most up-to-date information
    ///  A stone is given and a boolean (available)
    ///    true to include stone in validlist, false to include it in invalid list
    ///  If the stone is not in any of the two lists it is added to the one stated by available
    ///   else according to its timestamp and the timestamp of its instance in the list
    ///    un update is performed: if the stone is more recent it is considered, discarded otherwise
    /// </summary>
    /// <param name="stone">Stone to be considered for update</param>
    /// <param name="available">true if the stone is valid, false if invalid</param>
    /// <param name="ValidList">List of valid stones</param>
    /// <param name="InvalidList">List of invalid stones</param>
    public static void UpdateStoneLists(ISSR_Object stone, bool available, List<ISSR_Object> ValidList, List<ISSR_Object> InvalidList)  
    {
        float last_news_when;  // For known stone: Timestamp of stone in the list
        bool last_news_available = false; // For known stone: true is stone was in validlist, false if it was in invalid list

        if ((ValidList.Contains(stone)) || (InvalidList.Contains(stone)))
        {  // The stone is known to the lists
            int index;
            ISSR_Object known_stone;

            if (ValidList.Contains(stone))
            {  // This is a valid stone according to the lists
                last_news_available = true;  // valid
                index = ValidList.IndexOf(stone);
                known_stone = ValidList[index];
                last_news_when = known_stone.TimeStamp;
                if (last_news_when > stone.TimeStamp)
                    return;  // The information in the list is more recent: return

                if (available == last_news_available)
                {  // If it continues to be valid
                    ValidList[index] = stone; // Update information on stone
                }
                else
                {   //  If it is no longer available, remove it from one list, insert it in the other one
                    ValidList.RemoveAt(index);
                    InvalidList.Add(stone);
                }
            }
            else
            {// This is a NON valid stone according to the lists
                last_news_available = false;  // invalid
                index = InvalidList.IndexOf(stone);
                known_stone = InvalidList[index];
                last_news_when = known_stone.TimeStamp;
                if (last_news_when > stone.TimeStamp)
                    return; //  The information in the list is more recent: return

                if (available == last_news_available)
                { // If it continues to be NOT valid
                    InvalidList[index] = stone; // Update information on stone
                }
                else
                {//  If it is NOW available, remove it from one list, insert it in the other one
                    InvalidList.RemoveAt(index);
                    ValidList.Add(stone);
                }
            }




        }
        else
        {  // Stone is unknown to the lists, it is added to the list stated by available.
            if (available)
            {
                ValidList.Add(stone);
            }
            else
            {
                InvalidList.Add(stone);
            }
        }

    }



    /// <summary>
    ///  Calculates a direction orthogonal to the given path.
    ///  The path is given by two points: From and To.
    ///  From the given reference Point moving in the calculated direction 
    ///  the distance from the path will increase. 
    /// </summary>
    /// <param name="From">One point in the path</param>
    /// <param name="To"> Another ppint in the path</param>
    /// <param name="Point">The reference point</param>
    /// <returns></returns>
    public static Vector3 AwayFromPathDirection(Vector3 From, Vector3 To, Vector3 Point)
    {
        Vector3 direction = Vector3.zero;
        Vector3 orthodir;
        From.y = 0;
        To.y = 0;
        Point.y = 0;

        orthodir = Vector3.Cross(Vector3.up, From - To);

        if (Vector3.Dot(From, orthodir) > Vector3.Dot(Point, orthodir))
        {
            direction = -orthodir.normalized;
        }
        else
        {
            direction = orthodir.normalized;
        }


        return direction;
    }
    

    /// <summary>
    /// Adds a matrix of locations to Valid_Locations list. If one agent visits the all its perception 
    /// will have covered the whole Gameyard. It is not necessary to get to the exact location, 1.6 meters
    /// distance is enough. Also the Invalid_Locations list is emptied.
    /// </summary>
    /// <param name="a">Calling agent</param>
    public static void SetupScoutingLocations(ISSR_Agent a)  
    {
        int mx, mz;    // Dimensions of locations matrix
        float ox, oz; //  Offsets for first locations on rows and columns of matrices
        float unit = a.iSensingRange();
        Vector3 location;

        // Remove all locations from list of non visited locations (locations to explore)
        a.Valid_Locations.RemoveAll(x => true);
        // Remove all locations from list of visited locations 
        a.Invalid_Locations.RemoveAll(x => true);

        mx = (int)Mathf.Floor(a.iGameyardXDim() / unit);
        mz = (int)Mathf.Floor(a.iGameyardZDim() / unit);
        ox = (a.iGameyardXDim() - mx * unit) / 2;
        oz = (a.iGameyardZDim() - mz * unit) / 2;

        // Double loop for adding locations to 
        for (int x = 0; x < mx + 1; x++)
        {
            for (int z = 0; z < mz + 1; z++)
            {
                location = new Vector3(ox + x * unit - a.iGameyardXDim() / 2, 0,
                    oz + z * unit - a.iGameyardZDim() / 2);
                a.Valid_Locations.Add(location); // Add location to explore to matrix
            }
        }
    }

    /// <summary>
    ///  Looks in Valid_Locations for the location closest to the agent, if it is closer than 1.6 meters
    ///   the location is removed from Valid_Locations and inserted in Invalid_Locations. In that case 
    ///   it returns true, false if no location is removed. This function should be 
    ///   called periodically from a moving agent to update the list of visited locations (invalid) 
    ///   and non visited locations (valid). 
    /// </summary>
    /// <param name="a">Calling agent</param>
    /// <returns>True if location closer to agent by less than 1.6 units was removed</returns>
    public static bool UpdateVisitedScoutingLocation(ISSR_Agent a)
    {
        int remain;
        float distance;
        bool closer_location_removed = false;

        if (a.Valid_Locations.Count > 0) // If there are non visited locations 
        {
            // Get closest location to agent
            Vector3 location = ISSRHelp.GetCloserToMeLocationInList(a, a.Valid_Locations, out remain);
            //Measures distance
            distance = (a.oiLocation(a.Myself) - location).magnitude;
            if (distance < 1.6f) // If agent is close enough to see its sorroundings
            {  // Remove location from unvisited locations and add it to visited locations.
                a.Valid_Locations.Remove(location);
                a.Invalid_Locations.Add(location);
                closer_location_removed = true;  // Closest location has been removed (visited)
            }

        }
        return closer_location_removed;
    }
}


