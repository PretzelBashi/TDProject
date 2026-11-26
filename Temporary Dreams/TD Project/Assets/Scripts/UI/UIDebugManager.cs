using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIDebugManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    TextMeshProUGUI actualizacionClienteMS;
    TextMeshProUGUI actualizacionServidorMS;
    TextMeshProUGUI displayErrores;
    TextMeshProUGUI displayInfoLogin;
    TextMeshProUGUI fps;
    float timerActualizacionCliente;
    float timerActualizacionServidor;
    void Start()
    {
        if (GameObject.FindGameObjectsWithTag("UIDebug").Length > 1) { Destroy(gameObject); }
        actualizacionClienteMS = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        actualizacionServidorMS = transform.GetChild(0).GetChild(3).GetComponent<TextMeshProUGUI>();
        fps = transform.GetChild(0).GetChild(4).GetComponent<TextMeshProUGUI>();
        displayErrores = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        displayInfoLogin = transform.GetChild(0).GetChild(5).GetComponent<TextMeshProUGUI>();
        timerActualizacionCliente = 0;
        timerActualizacionServidor = 0;
    }
    public void ActualizacionCliente(float ms)
    {
        int newMS = Mathf.FloorToInt(ms * 1000);
        actualizacionClienteMS.text = "AC: " + newMS.ToString();
        actualizacionClienteMS.color = Color.green;
    }

    public void ActualizacionServidor(float ms)
    {
        int newMS = Mathf.FloorToInt(ms * 1000);
        actualizacionServidorMS.text = "AS: " + newMS.ToString();
        actualizacionServidorMS.color = Color.green;
    }

    public void ActualizarErrores(string error)
    {
        displayErrores.text = error;
    }

    public void ActualizarInfoLogin(string error)
    {
        displayInfoLogin.text = error;
    }

    // Update is called once per frame
    void Update()
    {
        timerActualizacionCliente += Time.deltaTime;
        if (timerActualizacionCliente > 0.010)
        {
            actualizacionClienteMS.color = Color.yellow;
        } else if(timerActualizacionCliente < 0.030)
        {
            actualizacionClienteMS.color = Color.red;
        }

        timerActualizacionServidor += Time.deltaTime;
        if (timerActualizacionServidor > 0.010)
        {
            actualizacionServidorMS.color = Color.yellow;
        }
        else if (timerActualizacionServidor < 0.030)
        {
            actualizacionServidorMS.color = Color.red;
        }

        fps.text ="FPS: " + (int)(1.0f / Time.deltaTime);
    }
}
