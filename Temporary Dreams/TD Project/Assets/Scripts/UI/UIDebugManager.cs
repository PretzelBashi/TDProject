using TMPro;
using UnityEngine;

public class UIDebugManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    TextMeshProUGUI ultimaActualizacionMS;
    TextMeshProUGUI displayErrores;
    float timerUltimaActualizacion;
    void Start()
    {
        ultimaActualizacionMS = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        displayErrores = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
    }
    public void UltimaActualizacion(int ms)
    {
        ultimaActualizacionMS.text = ms.ToString();
        ultimaActualizacionMS.color = Color.green;
    }

    public void ActualizarErrores(string error)
    {
        displayErrores.text = error;
    }


    // Update is called once per frame
    void Update()
    {
        timerUltimaActualizacion += Time.deltaTime;
        if (timerUltimaActualizacion > 0.010)
        {
            ultimaActualizacionMS.color = Color.yellow;
        } else if(timerUltimaActualizacion < 0.030)
        {
            ultimaActualizacionMS.color = Color.red;
        }
    }
}
