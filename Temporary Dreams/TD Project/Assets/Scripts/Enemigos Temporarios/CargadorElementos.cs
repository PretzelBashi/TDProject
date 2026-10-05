using UnityEngine;

public static class CargarElementosC
{
    public static int elementosInicializados = 0;

    //
    public static bool UIManagerCargado = false;
    public static bool ControladorJugadoresCargado = false;
    public static bool JugadorCargado = false;

    public static bool escenaCargada = false;
}
public class CargadorElementos : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    NetworkManager networkManager;
 
    // Update is called once per frame
    void Update()
    {

        if (CargarElementosC.escenaCargada) { return ; }
        if (CargarElementosC.UIManagerCargado && CargarElementosC.ControladorJugadoresCargado && CargarElementosC.JugadorCargado)
        {
            networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
            networkManager.SolicitarJugadoresConectados();
            CargarElementosC.escenaCargada = true;
        }
    }
}
