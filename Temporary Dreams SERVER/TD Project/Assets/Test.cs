using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Entro");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Salio");
    }
}
