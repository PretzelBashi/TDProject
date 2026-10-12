
using System.Collections.Generic;
using UnityEngine;
using static CargadorElementos;

public class ControladorJugadores : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Temporario temporario;
    List<Temporario> jugadoresARenderizar;
    NetworkManager networkManager;
    bool corriendo;
    void Start()
    {
        corriendo = false;
        CargarElementosC.ControladorJugadoresCargado = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!corriendo) { return; }
        /*for(int i = 0; jugadoresARenderizar.Count > 0; i++)
        {

        }*/
    }
    public void CrearJugadorFisico(string usuario)
    {

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
        jugadoresARenderizar = jugadoresNuevos;
    }
}
