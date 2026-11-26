using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using static CargadorElementos;


//Los saltos tienem que ser una stat
//Las stats tienen que actualizarse solo si es necesario (Que el servidor se encargue de mandar actualizaciones a un objeto del cliente)
public class Jugador : MonoBehaviour
{
    float velocidadJugador;
    float potenciaSalto;
    int estatusAnimacionCuerpo;
    int saltos;
    int saltosMax;
    bool grounded;
    int gravedad;

    float relojActualizacionMovimiento;
    int milisegundosActualizacionMovimiento;

    UIDebugManager uiDebug;
    UIManager uiManager;
    BoxCollider collisionSuelo;
    Rigidbody rbJugador;
    PlayerInput controlesJugador;
    Animator animator;
    NetworkManager networkManager;
    CamaraJugador camaraJugador;
    ControladorJugadores controladorJugadores;

    Vector3 velocidadActual;
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 120;
    }
    void Start()
    {
        List<SkinnedMeshRenderer> materiales;
        materiales = new List<SkinnedMeshRenderer>();
        controladorJugadores = GameObject.FindGameObjectWithTag("ControladorJugadores").GetComponent<ControladorJugadores>();
        try { networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>(); } catch { }
        
        materiales.Add(transform.GetChild(1).Find("pelo").GetComponent<SkinnedMeshRenderer>());
        materiales.Add(transform.GetChild(1).Find("pecho").GetComponent<SkinnedMeshRenderer>());
        materiales.Add(transform.GetChild(1).Find("piernas").GetComponent<SkinnedMeshRenderer>());
        materiales.Add(transform.GetChild(1).Find("zapatos").GetComponent<SkinnedMeshRenderer>());
        try {
            transform.GetChild(1).Find("pelo").GetComponent<SkinnedMeshRenderer>().sharedMesh = controladorJugadores.pelos[networkManager.jugadorActual.pelo];
            transform.GetChild(1).Find("ojos").GetComponent<SkinnedMeshRenderer>().material.mainTexture = controladorJugadores.ojos[networkManager.jugadorActual.ojos].texture;
            transform.GetChild(1).Find("boca").GetComponent<SkinnedMeshRenderer>().material.mainTexture = controladorJugadores.bocas[networkManager.jugadorActual.boca].texture;


            for (int i = 0; i < materiales.Count; i++)
            {
                foreach (Material material in materiales[i].materials)
                {

                    Debug.Log(networkManager.jugadorActual.color1);
                    ColorUtility.TryParseHtmlString("#" + networkManager.jugadorActual.color1, out Color currentColour);

                    switch (material.name)
                    {
                        case "pelo (Instance)":
                            material.color = currentColour;
                            break;
                        case "traje (Instance)":
                            material.color = currentColour;
                            break;

                        case "trageP2 (Instance)":
                            Debug.Log("Entro traje P2");
                            material.color = new Color(
                                currentColour.r * 0.65f,
                                currentColour.g * 0.65f,
                                currentColour.b * 0.65f,
                                currentColour.a
                            );
                            break;
                    }
                }
            }
        } catch { }
        



        collisionSuelo = transform.GetComponent<BoxCollider>();
        controlesJugador = transform.GetComponent<PlayerInput>();
        animator = transform.GetChild(1).GetComponent<Animator>();
        camaraJugador = GameObject.FindGameObjectWithTag("ObjetoCamara").GetComponent<CamaraJugador>();
        uiManager = GameObject.FindGameObjectWithTag("UIJugador").GetComponent<UIManager>();
        rbJugador = transform.GetComponent<Rigidbody>();
        try
        {
            networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
            uiDebug = GameObject.FindGameObjectWithTag("UIDebug").GetComponent<UIDebugManager>();
        }
        catch { Debug.Log("Corriendo Offline"); }


        gravedad = 25;
        velocidadActual = Vector3.zero;

        estatusAnimacionCuerpo = 0;
        grounded = false;

        velocidadJugador = 2.5f;
        saltosMax = 2;
        saltos = saltosMax;
        potenciaSalto = 7f;

        CargarElementosC.JugadorCargado = true;
    }

    void Update()
    {
        velocidadActual.x = controlesJugador.actions["Move"].ReadValue<Vector2>().x * velocidadJugador;
        velocidadActual.z = controlesJugador.actions["Move"].ReadValue<Vector2>().y * velocidadJugador;

        //Frente
        if (controlesJugador.actions["Move"].ReadValue<Vector2>().y > 0)
        {
            estatusAnimacionCuerpo = 1;
        }
        else if (controlesJugador.actions["Move"].ReadValue<Vector2>().y < 0)
        {
            estatusAnimacionCuerpo = 2;
        }
        //Lados
        if (controlesJugador.actions["Move"].ReadValue<Vector2>().x > 0)
        {
            estatusAnimacionCuerpo = 3;
        }
        else if (controlesJugador.actions["Move"].ReadValue<Vector2>().x < 0)
        {
            estatusAnimacionCuerpo = 4;
        }

        if (!grounded)
        {
            if (rbJugador.linearVelocity.y > 1)
            {
                estatusAnimacionCuerpo = 5;
            }
            else { estatusAnimacionCuerpo = 6; }

        }
        else
        {
            //Reposo total
            if (rbJugador.linearVelocity.x == 0 && rbJugador.linearVelocity.z == 0) { estatusAnimacionCuerpo = 0; }
            saltos = saltosMax;
        }

        if (animator.GetInteger("EstadoCuerpo") != estatusAnimacionCuerpo)
        {
            animator.SetInteger("EstadoCuerpo", estatusAnimacionCuerpo);
            animator.SetTrigger("CambioCuerpo");
        }

        //Debug.Log(grounded);
        Vector3 camaraF = camaraJugador.transform.forward; camaraF.y = 0;
        Vector3 camaraR = camaraJugador.transform.right; camaraR.y = 0;
        velocidadActual = (((camaraF * velocidadActual.z).normalized + (camaraR * velocidadActual.x)).normalized) * velocidadJugador + new Vector3(0, rbJugador.linearVelocity.y, 0);
        //Debug.Log(rbJugador.linearVelocity.y);
        rbJugador.linearVelocity = new Vector3(velocidadActual.x * velocidadJugador, rbJugador.linearVelocity.y, velocidadActual.z * velocidadJugador);
        //rbJugador.MoveRotation(Quaternion.Euler(new Vector3(0, camaraJugador.rotacionCamara.y, 0)));
        transform.rotation = Quaternion.Euler(new Vector3(0, camaraJugador.rotacionCamara.y, 0));

        relojActualizacionMovimiento += Time.deltaTime;
        try
        {
            if (relojActualizacionMovimiento >= milisegundosActualizacionMovimiento / 1000)
            {
                uiDebug.ActualizacionCliente(relojActualizacionMovimiento);
                networkManager.MandarCoordenadas(transform.position, transform.rotation.eulerAngles, estatusAnimacionCuerpo);
                relojActualizacionMovimiento = 0;
            }
        }
        catch { }
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contacto in collision.contacts)
        {
            if (contacto.normal.y > 0.5f)
            {
                grounded = true;
                return;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        grounded = false;
    }

    public void Saltar(InputAction.CallbackContext callbackContext)
    {
        //Las acciones tienen 3 estados (Started, performed, canceled)
        if (callbackContext.started)
        {
            if (saltos > 0)
            {
                saltos--;
                rbJugador.AddForce(Vector3.up * potenciaSalto, ForceMode.Impulse);

            }
        }
    }

    public void ToggleInventario(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            uiManager.ToggleInventarioUI();
        }
    }
}
