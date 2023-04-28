using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class decor : MonoBehaviour {

	public MeshRenderer rend;
	
	public IEnumerator corutina()
    {
		while (true)
        {
			yield return new WaitForSeconds(1f);
			rend.material.color = Color.white;
			yield return new WaitForSeconds(1f);
			rend.material.color = new Color(0.5f, 0.5f, 0.5f);
		}
		
    }
	// Use this for initialization

	void Start()
    {
		StartCoroutine("corutina");
    }
	
}
