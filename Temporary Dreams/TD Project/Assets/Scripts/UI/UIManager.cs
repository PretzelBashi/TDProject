using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CargadorElementos;

public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject placaJugadorPrefab;
    public GameObject prefabItem;
    public Sprite[] spritesObjetos;
    Transform placasContenido;
    Transform inventarioContenido;
    Transform statsJugador;
    Transform inventario;
    Transform statsObjeto;
    

    NetworkManager networkManager;

    List<Temporario> listaJugadores;
    List<Transform> placasJugadores;
    List<Transform> itemsCargados;

    TextMeshProUGUI[] statsJugadorSeleccion;

    bool inventarioAbierto = false;
    // Update is called once per frame
    public void Mostrar(bool estado, int menu)
    {
        CanvasGroup elementoCambiar = new CanvasGroup();
        switch (menu)
        {
            case 0:
                elementoCambiar = statsJugador.GetComponent<CanvasGroup>();
                break;
            case 1:
                elementoCambiar = statsObjeto.GetComponent<CanvasGroup>();
                break;
        }

        if (estado) {
            elementoCambiar.alpha = 1.0f;
            elementoCambiar.blocksRaycasts = true;
        } else
        {
            elementoCambiar.alpha = 0f;
            elementoCambiar.blocksRaycasts = false;
        }
        
    }
    private void Start()
    {
        statsJugador = transform.GetChild(1);
        inventario = transform.GetChild(2);
        statsObjeto = transform.GetChild(3);

        placasContenido = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0);
        inventarioContenido = inventario.GetChild(0).GetChild(0).GetChild(0);


        statsJugadorSeleccion = new TextMeshProUGUI[statsJugador.childCount];
        placasJugadores = new List<Transform>();
        itemsCargados = new List<Transform>();
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

        for(int i = 0; i < placasJugadores.Count; i++)
        {
            Destroy(placasJugadores[i].gameObject);
        }
        placasJugadores.Clear();

        foreach (KeyValuePair<string, Temporario> jugador in networkManager.jugadoresEnServidor)
        {
            GameObject placaCreada = Instantiate(placaJugadorPrefab, placasContenido);
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
    public void ToggleInventarioUI()
    {
        if (inventarioAbierto)
        {
            Mostrar(false, 1);
        } else
        {
            Mostrar(true, 1);
            CargarInventario();
        }
    }

    public void CargarInventario()
    {
        for(int i = 0;i < networkManager.itemsJugador.Count; i++)
        {
            GameObject placaObjeto = Instantiate(prefabItem, inventarioContenido);
            placaObjeto.transform.GetComponent<Image>().sprite = spritesObjetos[networkManager.itemsJugador[i].id];
            itemsCargados.Add(placaObjeto.transform);
        }
    }
    public void CargarStatsItem(int index)
    {
        Item item = networkManager.itemsJugador[index];
        statsObjeto.GetChild(0).GetComponent<TextMeshProUGUI>().text = item.nombre;
        statsObjeto.GetChild(1).GetComponent<TextMeshProUGUI>().text = item.descripcion;
        statsObjeto.GetChild(2).GetComponent<TextMeshProUGUI>().text = item.vida;
        statsObjeto.GetChild(3).GetComponent<Image>().sprite = spritesObjetos[item.id];
    }
}
