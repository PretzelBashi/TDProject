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
    float temporizadorMenu;
    bool menuAbierto;

    public void IniciarPlaca(string nombre)
    {
        usuario = nombre;
        temporizadorMenu = 0;
        menuAbierto = false;
        uiJugador = GameObject.FindGameObjectWithTag("UIJugador").GetComponent<UIManager>();

        textoPlaca = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();


        textoPlaca.text = usuario;
    }

    private void Update()
    {
        temporizadorMenu += Time.deltaTime;
        if (menuAbierto && temporizadorMenu > 0.5)
        {
            uiJugador.CargarStatsUsuario(usuario);
            temporizadorMenu = 0;
        }
    }

    // Update is called once per frame
    public void OnPointerEnter(PointerEventData eventData)
    {
        menuAbierto = true;
        uiJugador.Mostrar(true, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        menuAbierto = false;
        uiJugador.Mostrar(false, 0);
    }
}
