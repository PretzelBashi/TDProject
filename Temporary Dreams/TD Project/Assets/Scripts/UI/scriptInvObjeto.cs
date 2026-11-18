using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class scriptInvObjeto : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    int id = 0;
    int index = 0;
    bool menuAbierto = false;
    TextMeshProUGUI nombre;
    TextMeshProUGUI descripcion;
    TextMeshProUGUI vida;
    Image foto;
    UIManager uiJugador;
    public Sprite sprite;
    
    void Start()
    {
        foto = transform.GetChild(3).GetComponent<Image>();
        foto.sprite = sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        menuAbierto = true;
        uiJugador.Mostrar(true, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        menuAbierto = false;
        uiJugador.Mostrar(false, 1);
    }
}
