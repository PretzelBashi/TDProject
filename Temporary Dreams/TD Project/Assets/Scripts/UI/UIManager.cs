using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static CargadorElementos;

public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject placaJugadorPrefab;
    Transform contenido;
    Transform statsJugador;

    NetworkManager networkManager;

    List<Temporario> listaJugadores;
    List<Transform> placasJugadores;
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
        for(int i = 0; i< placasJugadores.Count; i++)
        {
            Destroy(placasJugadores[i].gameObject);
            placasJugadores.Clear();
        }

        foreach (KeyValuePair<string, Temporario> jugador in networkManager.jugadoresEnServidor)
        {
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
        statsJugadorSeleccion[4].text = "x:" + Mathf.FloorToInt(usuario.posicion.x) + " y:" + Mathf.FloorToInt(usuario.posicion.y)
            + " z:" + Mathf.FloorToInt(usuario.posicion.z);
    }
}
