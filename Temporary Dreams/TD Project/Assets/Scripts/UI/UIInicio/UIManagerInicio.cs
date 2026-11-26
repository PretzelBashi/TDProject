
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerInicio : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Transform pantallaMain;
    Transform pantallaRegistro;
    Transform pantallaElegir;
    Transform pantallaCrear;
    Transform pantallaServidores;
    Transform pantallaLogin;
    ColorPickerControl colorPicker;

    NetworkManager networkManager;
    string personajeElegido;
    string servidorElegido;

    bool[] personajesCargados = { false, false, false };

    int ojosActual = 0;
    int peloActual = 0;
    int bocaActual = 0;

    TextMeshProUGUI contadorPelo;
    TextMeshProUGUI contadorOjos;
    TextMeshProUGUI contadorBoca;
    TMP_InputField textoNombre;

    [SerializeField]
    Mesh[] pelos;
    [SerializeField]
    Sprite[] ojos;
    [SerializeField]
    Sprite[] bocas;
    [SerializeField]
    Transform modeloJugador;
    void Start()
    {
        
        pantallaMain = transform.GetChild(0);
        pantallaLogin = transform.GetChild(1);
        pantallaRegistro = transform.GetChild(2);
        pantallaElegir = transform.GetChild(3);
        pantallaCrear = transform.GetChild(4);
        pantallaServidores = transform.GetChild(5);
        colorPicker = pantallaCrear.Find("colorPicker").GetComponent<ColorPickerControl>();
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        networkManager.uiManagerInicio = transform.GetComponent<UIManagerInicio>();
        modeloJugador.Find("pelo").GetComponent<SkinnedMeshRenderer>().sharedMesh = pelos[peloActual];
        modeloJugador.Find("ojos").GetComponent<SkinnedMeshRenderer>().material.mainTexture = ojos[ojosActual].texture;
        modeloJugador.Find("boca").GetComponent<SkinnedMeshRenderer>().material.mainTexture = bocas[bocaActual].texture;

        contadorPelo = pantallaCrear.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>();
        contadorOjos = pantallaCrear.transform.GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>();
        contadorBoca = pantallaCrear.transform.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>();
        textoNombre = pantallaCrear.transform.Find("nombreUsuario").GetComponent<TMP_InputField>();
    }
    public void AumentarPelo()
    {
        if (peloActual >= pelos.Length-1)
        {
            peloActual = 0;
        }
        else
        {
            peloActual++;
        }

        modeloJugador.Find("pelo").GetComponent<SkinnedMeshRenderer>().sharedMesh = pelos[peloActual];
        contadorPelo.text = (peloActual+1).ToString();
    }
    public void ReducirPelo() {
        if (peloActual <= 0)
        {
            peloActual = pelos.Length - 1;
        }
        else
        {
            peloActual--;
        }

        modeloJugador.Find("pelo").GetComponent<SkinnedMeshRenderer>().sharedMesh = pelos[peloActual];
        contadorPelo.text = (peloActual+1).ToString();
    }

    public void AumentarOjos()
    {
        if (ojosActual >= ojos.Length - 1)
        {
            ojosActual = 0;
        }
        else
        {
            ojosActual++;
        }

        modeloJugador.Find("ojos").GetComponent<SkinnedMeshRenderer>().material.mainTexture = ojos[ojosActual].texture;
        contadorOjos.text = (ojosActual+1).ToString();
    }
    public void ReducirOjos()
    {
        if (ojosActual <= 0)
        {
            ojosActual = ojos.Length - 1;
        }
        else
        {
            ojosActual--;
        }

        modeloJugador.Find("ojos").GetComponent<SkinnedMeshRenderer>().material.mainTexture = ojos[ojosActual].texture;
        contadorOjos.text = (ojosActual + 1).ToString();
    }

    public void AumentarBoca()
    {
        if (bocaActual >= bocas.Length - 1)
        {
            bocaActual = 0;
        }
        else
        {
            bocaActual++;
        }

        modeloJugador.Find("boca").GetComponent<SkinnedMeshRenderer>().material.mainTexture = bocas[bocaActual].texture;
        contadorBoca.text = (bocaActual + 1).ToString();

    }
    public void ReducirBoca()
    {
        if (bocaActual <= 0)
        {
            bocaActual = bocas.Length - 1;
        }
        else
        {
            bocaActual--;
        }

        modeloJugador.Find("boca").GetComponent<SkinnedMeshRenderer>().material.mainTexture = bocas[bocaActual].texture;
        contadorBoca.text = (bocaActual + 1).ToString();
    }
    
    public void CrearPersonaje()
    {
        if (textoNombre.text == "" || textoNombre.text == null) { return; }
        Debug.Log(" " + textoNombre.text + " " + int.Parse(contadorPelo.text) + " " + int.Parse(contadorOjos.text) + " " + ColorUtility.ToHtmlStringRGB(colorPicker.currentColour));
        networkManager.CrearPersonaje(textoNombre.text, peloActual, ojosActual, bocaActual, ColorUtility.ToHtmlStringRGB(colorPicker.currentColour)); 
    }
    public void ReiniciarUI()
    {
        Ocultar(3); Ocultar(4); Ocultar(5);
    }
    public void ElegirPersonaje(string nombre)
    {
        personajeElegido = nombre;
        Ocultar(3);
        Mostrar(5);
    }

    public void EliminarPersonaje(string nombre)
    {
        networkManager.EliminarPersonaje(nombre);

    }

    public void IniciarServidor(string servidor)
    {
        servidorElegido = servidor;
        networkManager.ConectarAInstancia(servidorElegido, personajeElegido);
    }

    public void IniciarPersonajes(Temporario[] personajes)
    {
        Transform[] personajesL = { pantallaElegir.GetChild(0), pantallaElegir.GetChild(1), pantallaElegir.GetChild(2) };
        for (int i = 0; i < personajesL.Length; i++)
        {
            Debug.Log("holaAAAA " + personajesL.Length);
            try
            {
                Debug.Log(personajes[i].nombreJugador);
                string nombre = personajes[i].nombreJugador;
                personajesL[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = personajes[i].nombreJugador;
                personajesL[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = personajes[i].nivel.ToString();
                personajesL[i].GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners();
                personajesL[i].GetChild(2).GetComponent<Button>().onClick.AddListener(() => ElegirPersonaje(nombre));
                personajesL[i].GetChild(3).GetComponent<Button>().onClick.RemoveAllListeners();
                personajesL[i].GetChild(3).GetComponent<Button>().onClick.AddListener(() => EliminarPersonaje(nombre));
                personajesCargados[i] = true;
            }
            catch
            {
                personajesL[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = "Vacio";
                personajesL[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }

        }
    }
    public void Mostrar(int menu)
    {
        CanvasGroup elementoCambiar = new CanvasGroup();
        switch (menu)
        {
            case 0:
                elementoCambiar = pantallaMain.GetComponent<CanvasGroup>();
                break;
            case 1:
                elementoCambiar = pantallaLogin.GetComponent<CanvasGroup>();
                break;
            case 2:
                elementoCambiar = pantallaRegistro.GetComponent<CanvasGroup>();
                break;
            case 3:
                elementoCambiar = pantallaElegir.GetComponent<CanvasGroup>();
                break;
            case 4:
                elementoCambiar = pantallaCrear.GetComponent<CanvasGroup>();
                break;
            case 5:
                elementoCambiar = pantallaServidores.GetComponent<CanvasGroup>();
                break;
        }

        elementoCambiar.alpha = 1.0f;
        elementoCambiar.blocksRaycasts = true;
        elementoCambiar.interactable = true;
    }

    public void Ocultar(int menu)
    {
        CanvasGroup elementoCambiar = new CanvasGroup();
        switch (menu)
        {
            case 0:
                elementoCambiar = pantallaMain.GetComponent<CanvasGroup>();
                break;
            case 1:
                elementoCambiar = pantallaLogin.GetComponent<CanvasGroup>();
                break;
            case 2:
                elementoCambiar = pantallaRegistro.GetComponent<CanvasGroup>();
                break;
            case 3:
                elementoCambiar = pantallaElegir.GetComponent<CanvasGroup>();
                break;
            case 4:
                elementoCambiar = pantallaCrear.GetComponent<CanvasGroup>();
                break;
            case 5:
                elementoCambiar = pantallaServidores.GetComponent<CanvasGroup>();
                break;
        }

        elementoCambiar.alpha = 0f;
        elementoCambiar.blocksRaycasts = false;
        elementoCambiar.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
