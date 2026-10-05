using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static CargadorElementos;


//Los saltos tienem que ser una stat
//Las stats tienen que actualizarse solo si es necesario (Que el servidor se encargue de mandar actualizaciones a un objeto del cliente)
public class Jugador : MonoBehaviour
{
    float velocidadJugador;
    int estatusAnimacionCuerpo;
    int saltos;
    bool grounded;

    float relojActualizacionMovimiento;
    int milisegundosActualizacionMovimiento;

    UIDebugManager UIDebug;
    BoxCollider collisionSuelo;
    CharacterController characterController;
    PlayerInput controlesJugador;
    Animator animator;
    NetworkManager networkManager;

    Vector3 velocidadActual;
    void Start()
    {
        collisionSuelo = transform.GetComponent<BoxCollider>();
        controlesJugador = transform.GetComponent<PlayerInput>();
        animator = transform.GetChild(1).GetComponent<Animator>();
        characterController = transform.GetComponent<CharacterController>();
        try { networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>(); UIDebug = GameObject.FindGameObjectWithTag("UIDebug").GetComponent<UIDebugManager>(); }
        catch { Debug.Log("Corriendo Offline"); }
        
        

        velocidadActual = Vector3.zero;
        velocidadJugador = 3.5f;
        estatusAnimacionCuerpo = 0;
        grounded = false;
        saltos = 1;
        CargarElementosC.JugadorCargado = true;
    }

    void Update()
    {
        velocidadActual.x = controlesJugador.actions["Move"].ReadValue<Vector2>().x * velocidadJugador;
        velocidadActual.z = controlesJugador.actions["Move"].ReadValue<Vector2>().y * velocidadJugador;

        //Frente
        if(controlesJugador.actions["Move"].ReadValue<Vector2>().y > 0)
        {
            estatusAnimacionCuerpo = 1;
        } 
        else if(controlesJugador.actions["Move"].ReadValue<Vector2>().y < 0)
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

        RaycastHit suelo;
        Debug.DrawRay(transform.position + (Vector3.up * 0.1f), Vector3.down * 1);
        if (Physics.SphereCast(transform.position + (Vector3.up*0.1f), 0.3f, Vector3.down * 2, out suelo))
        {
            Debug.Log("Casteado");
            if(suelo.collider.gameObject.tag != "Jugador")
            {
                Debug.Log("Objeto: " + suelo.collider.gameObject.name);
                Debug.Log("Distancia: " + suelo.distance);
                if (suelo.distance <= 0.9)
                {
                    Debug.Log("se hizo true");
                    grounded = true;
                }
                else { grounded = false; }
            }
            
        }

        if (!grounded)
        {
            velocidadActual.y -= 0.15f;
            if(velocidadActual.y > 1)
            {
                estatusAnimacionCuerpo = 5;
            }
            else { estatusAnimacionCuerpo = 6; }
            
        } else
        {
            //Reposo total
            if (velocidadActual.x == 0 && velocidadActual.z == 0) { estatusAnimacionCuerpo = 0; }

            saltos = 1;
        }

        if (animator.GetInteger("EstadoCuerpo") != estatusAnimacionCuerpo)
        {
            animator.SetInteger("EstadoCuerpo", estatusAnimacionCuerpo);
            animator.SetTrigger("CambioCuerpo");
        }

        relojActualizacionMovimiento += Time.deltaTime;
        try {
            if (relojActualizacionMovimiento >= milisegundosActualizacionMovimiento / 1000)
            {
                UIDebug.UltimaActualizacion(Mathf.FloorToInt(relojActualizacionMovimiento * 1000));
                networkManager.MandarCoordenadas(transform.position, estatusAnimacionCuerpo);
                relojActualizacionMovimiento = 0;
            }
        } catch { }
        

        Debug.Log(grounded);
        characterController.Move(velocidadActual * Time.deltaTime);
    }

    public void Saltar(InputAction.CallbackContext callbackContext)
    {
        //Las acciones tienen 3 estados (Started, performed, canceled)
        if (callbackContext.performed)
        {
            if (saltos > 0)
            {
                saltos--;
                characterController.Move(Vector3.up * 0.15f);
                velocidadActual.y = 8;
            }
        }
    }
}
