using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class placaJugador : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    UIManager uiJugador;
    string usuario;
    NetworkManager networkManager;
    TextMeshProUGUI textoPlaca;

    public void IniciarPlaca(string nombre)
    {
        usuario = "";
        uiJugador = GameObject.FindGameObjectWithTag("UIJugador").GetComponent<UIManager>();

        textoPlaca = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        Debug.Log("CREANDO PLACA");
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
        usuario = networkManager.jugadoresEnServidor[nombre].nombreJugador;

        textoPlaca.text = usuario;
        Debug.Log(transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
    }

    // Update is called once per frame
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Entro");
        uiJugador.Mostrar(true);
        uiJugador.CargarStatsUsuario(usuario);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        uiJugador.Mostrar(false);
    }
}
