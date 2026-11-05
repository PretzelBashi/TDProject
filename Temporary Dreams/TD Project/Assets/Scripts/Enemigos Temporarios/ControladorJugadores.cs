
using System.Collections.Generic;

using UnityEngine;
using static CargadorElementos;

public class ControladorJugadores : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject prefabJugador;

    Temporario temporario;
    List<string> jugadoresARenderizar;
    NetworkManager networkManager;
    
    bool corriendo;
    void Start()
    {
        corriendo = false;
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        jugadoresARenderizar = new List<string>();
        CargarElementosC.ControladorJugadoresCargado = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!corriendo) { return; }
        for(int i = 0; i < jugadoresARenderizar.Count; i++)
        {
            Transform objetoJugadorBuffer = networkManager.jugadoresEnServidor[jugadoresARenderizar[i]].objetoJugador.transform;
            Animator animatorBuffer = objetoJugadorBuffer.GetChild(0).GetComponent<Animator>();
            Temporario jugadorServidorBuffer = networkManager.jugadoresEnServidor[jugadoresARenderizar[i]];

            objetoJugadorBuffer.position = jugadorServidorBuffer.posicion;
            objetoJugadorBuffer.rotation = Quaternion.Euler(jugadorServidorBuffer.rotacion);

            if (animatorBuffer.GetInteger("EstadoCuerpo") != jugadorServidorBuffer.animacionActual)
            {
                animatorBuffer.SetInteger("EstadoCuerpo", jugadorServidorBuffer.animacionActual);
                animatorBuffer.SetTrigger("CambioCuerpo");
            }
        }
    }
    public GameObject CrearJugadorFisico(string usuario)
    {
        Debug.Log("Jugador creado");
        GameObject objetoJugador = Instantiate(prefabJugador,Vector3.zero, Quaternion.identity);
        jugadoresARenderizar.Add(usuario);
        return objetoJugador;
    }
    public void DestruirJugadorFisico(string usuario)
    {
        for(int i = 0; i < jugadoresARenderizar.Count; i++)
        {
            if (jugadoresARenderizar[i] == usuario)
            {
                if(networkManager.jugadoresEnServidor[usuario].objetoJugador != null)
                {
                    Destroy(networkManager.jugadoresEnServidor[usuario].objetoJugador);
                    jugadoresARenderizar.RemoveAt(i);
                }
            }
        }
    }
    public void PrenderApagar(bool opcion)
    {
        corriendo=opcion;
    }

    public void ActualizarJugadores()
    {

    }

    public void CambiarJugadores(List<Temporario> jugadoresNuevos)
    {

    }
}
