using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class especial : MonoBehaviour {

	public GameObject prefab;
	// Use this for initialization
	void Start () {
		if (ISSRextra.ActivateSpecial())
        {
			GameObject.Instantiate(prefab, new Vector3(0, 2, 14.5f), Quaternion.Euler(0, 90, 0));
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
