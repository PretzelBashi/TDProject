using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class scriptInvObjeto : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int index = 0;
    bool menuAbierto;
    public Sprite sprite;
    
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager uiManager = GameObject.FindGameObjectWithTag("UIJugador").GetComponent<UIManager>();
        uiManager.Mostrar(true, 1);
        uiManager.CargarStatsItem(index);
        menuAbierto = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        menuAbierto = false;
        GameObject.FindGameObjectWithTag("UIJugador").GetComponent<UIManager>().Mostrar(false, 1);
    }
}
