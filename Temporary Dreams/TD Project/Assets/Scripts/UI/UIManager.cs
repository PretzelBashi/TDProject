using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static CargadorElementos;

public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject placaJugadorPrefab;
    NetworkManager networkManager;
    List<Transform> placasJugadores;
    List<Temporario> listaJugadores;
    Transform contenido;
    Transform statsJugador;

    TextMeshProUGUI[] statsJugadorSeleccion;
    // Update is called once per frame
    public void Mostrar(bool estado)
    {
        if (estado) { 
            statsJugador.GetComponent<CanvasGroup>().alpha = 1.0f;
            statsJugador.GetComponent<CanvasGroup>().blocksRaycasts = true;
        } else
        {
            statsJugador.GetComponent<CanvasGroup>().alpha = 0f;
            statsJugador.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        
    }
    private void Start()
    {
        contenido = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0);
        statsJugador = transform.GetChild(1);
        statsJugadorSeleccion = new TextMeshProUGUI[statsJugador.childCount];
        placasJugadores = new List<Transform>();
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();

        Debug.Log("UI Manager iniciado");
        for (int i = 0; i < statsJugador.childCount; i++)
        {
            statsJugadorSeleccion[i] = statsJugador.GetChild(i).GetComponent<TextMeshProUGUI>();
        }


        CargarElementosC.UIManagerCargado = true;
    }

    public void CargarPlacas()
    {
        Debug.Log("Cargando placas");
        Debug.Log(networkManager.jugadoresEnServidor.Count);
        foreach (KeyValuePair<string, Temporario> jugador in networkManager.jugadoresEnServidor)
        {
            Debug.Log(jugador.Key);
            GameObject placaCreada = Instantiate(placaJugadorPrefab, contenido);
            Debug.Log(placaCreada.name);
            placasJugadores.Add(placaCreada.transform);
            placaCreada.GetComponent<placaJugador>().IniciarPlaca(jugador.Key);
            
            
        }
    }
    public void CargarStatsUsuario(string nombre)
    {
        Temporario usuario = networkManager.jugadoresEnServidor[nombre];
        statsJugadorSeleccion[0].text = usuario.nombreJugador;
        statsJugadorSeleccion[1].text = usuario.nivel.ToString();
        statsJugadorSeleccion[2].text = usuario.saludMaxima.ToString();
        statsJugadorSeleccion[3].text = usuario.saludActual.ToString();
        statsJugadorSeleccion[4].text = "x:" + usuario.posicion.x + " y:" + usuario.posicion.y + " z:" + usuario.posicion.z;
    }
}
