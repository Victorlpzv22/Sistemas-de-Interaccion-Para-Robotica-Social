// 
// Fichero: BillboardState.cs   
// Rota al objeto que lo lleva para 
//   orientar su eje forward a la cámara principal.
//  Icono de estado de agente
//  
//  Derivado de: http://wiki.unity3d.com
//
//  Fecha: 	19 07 2018
//  Autor: 	Enrique Rendón enriqueblender@gmail.com
//
using UnityEngine;
using System.Collections;

public class BillboardState : MonoBehaviour 
{

	public Camera 	MainCamera;
	public bool		active;
    [SerializeField] Material[] StateIconsMaterials;
    int current_material_index = -1;
    [HideInInspector]
    public GameObject MyAgent;
    [HideInInspector]
    public ISSR_AgentBehaviour MyAgentBehavior;
    MeshRenderer meshrenderer;
    [SerializeField] Material error_material;

    // Use this for initialization
    void Awake () 
	{
		this.MainCamera = GameObject.Find ("MainCamera").GetComponent<Camera> ();   // Obtiene cámara principal
		active= true;
        this.MyAgent = transform.parent.gameObject;
        this.MyAgentBehavior = MyAgent.GetComponent<ISSR_AgentBehaviour>();
        this.meshrenderer = GetComponent<MeshRenderer>();

        if (ISSRextra.ActivateSpecial())
        {
            transform.localPosition = Vector3.up * 2.4f;
        }
    }
	
	// Update is called once per frame
	void Update () 
	{
		if (this.active)
		{
            if (this.MyAgentBehavior.AgentDescriptor.current_state != (ISSRState) current_material_index)
            {
                current_material_index = (int) this.MyAgentBehavior.AgentDescriptor.current_state;

                if (current_material_index < StateIconsMaterials.Length)
                {
                    meshrenderer.material = StateIconsMaterials[current_material_index];
                }
                else
                {
                    meshrenderer.material = error_material;
                }
            }
			transform.LookAt(transform.position + MainCamera.transform.rotation * Vector3.forward,
				MainCamera.transform.rotation * Vector3.up);
		}
	}


	public void Activate(bool activate)
	{
		this.active = activate;
        meshrenderer.enabled = activate;
	}
}
