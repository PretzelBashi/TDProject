
using System.Collections.Generic;
using UnityEngine;

public class ControladorJugadores : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Temporario temporario;
    List<Temporario> jugadoresARenderizar;
    NetworkManager networkManager;
    bool corriendo;
    void Start()
    {
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        networkManager.SolicitarJugadoresConectados();
        corriendo = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!corriendo) { return; }
    }

    public void PrenderApagar(bool opcion)
    {
        corriendo=opcion;
    }

    public void CambiarJugadores(List<Temporario> jugadoresNuevos)
    {
        jugadoresARenderizar = jugadoresNuevos;
    }
}
