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
    public Sprite[] spritesMarcos;

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
            case 2:
                elementoCambiar = inventario.GetComponent<CanvasGroup>();
                break;
        }

        if (estado) {
            elementoCambiar.alpha = 1.0f;
            elementoCambiar.blocksRaycasts = true;
            elementoCambiar.interactable = true;
        } else
        {
            elementoCambiar.alpha = 0f;
            elementoCambiar.blocksRaycasts = false;
            elementoCambiar.interactable = false;
        }
        
    }
    private void Start()
    {
        statsJugador = transform.GetChild(1);
        inventario = transform.GetChild(2);
        statsObjeto = transform.GetChild(3);

        placasContenido = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0);
        inventarioContenido = inventario.GetChild(2).GetChild(0).GetChild(0);


        statsJugadorSeleccion = new TextMeshProUGUI[statsJugador.childCount];
        placasJugadores = new List<Transform>();
        itemsCargados = new List<Transform>();
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();


        for (int i = 0; i < statsJugador.childCount; i++)
        {
            statsJugadorSeleccion[i] = statsJugador.GetChild(i).GetComponent<TextMeshProUGUI>();
        }


        CargarElementosC.UIManagerCargado = true;
    }

    public void CargarPlacas()
    {
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
            Mostrar(false, 2);
            for (int i = 0; i < itemsCargados.Count; i++)
            {
                Destroy(itemsCargados[i].gameObject);
            }
            itemsCargados.Clear();
            inventarioAbierto = false;

        } else
        {
            Mostrar(true, 2);
            networkManager.SolicitarInventario();
            inventarioAbierto = true;
        }
    }

    public void CargarInventario()
    {
        Debug.Log("Inventario cargado");
        Debug.Log(networkManager.itemsJugador.Count);
        for(int i = 0;i < networkManager.itemsJugador.Count; i++)
        {
            GameObject placaObjeto = Instantiate(prefabItem, inventarioContenido);
            Debug.Log(networkManager.itemsJugador[i].rareza);
            placaObjeto.transform.GetComponent<Image>().sprite = spritesObjetos[networkManager.itemsJugador[i].id];
            placaObjeto.transform.GetChild(1).GetComponent<Image>().sprite = spritesMarcos[networkManager.itemsJugador[i].rareza];
            placaObjeto.transform.GetComponent<scriptInvObjeto>().index = i;
            itemsCargados.Add(placaObjeto.transform);
        }
    }
    public void CargarStatsItem(int index)
    {
        Item item = networkManager.itemsJugador[index];
        statsObjeto.GetChild(0).GetComponent<Image>().sprite = spritesObjetos[item.id];
        statsObjeto.GetChild(1).GetComponent<Image>().sprite = spritesMarcos[item.rareza];
        statsObjeto.GetChild(2).GetComponent<TextMeshProUGUI>().text = item.nombre;
        statsObjeto.GetChild(3).GetComponent<TextMeshProUGUI>().text = item.descripcion;
        statsObjeto.GetChild(4).GetComponent<TextMeshProUGUI>().text = item.vida;

    }
}
