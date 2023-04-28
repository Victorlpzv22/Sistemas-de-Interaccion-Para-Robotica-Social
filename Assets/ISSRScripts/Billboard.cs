// Clase 23 PVU   Control de NPCs / IA (II)
//
// Fichero: Billboard.cs   
// Rota al objeto que lo lleva para 
//   orientar su eje forward a la cámara principal.
//  
//  Sacado de: http://wiki.unity3d.com
//
//  Fecha: 	25 06 2016
//  Autor: 	Enrique Rendón enriqueblender@gmail.com
//  Profe:  Jaume Castells Carles
//
using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour 
{

	public Camera 	MainCamera;
	public bool		active; 

	// Use this for initialization
	void Awake () 
	{
		this.MainCamera = GameObject.Find ("MainCamera").GetComponent<Camera> ();   // Obtiene cámara principal
		active= true;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (this.active)
		{
			transform.LookAt(transform.position + MainCamera.transform.rotation * Vector3.forward,
				MainCamera.transform.rotation * Vector3.up);
		}
	}


	public void Activate(bool activate)
	{
		this.active = activate;
	}
}
