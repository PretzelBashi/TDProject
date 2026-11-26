using TMPro;
using UnityEngine;

public class BotonLogIn : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public NetworkManager NetworkManager;
    // Update is called once per frame
    private void Start()
    {
        NetworkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>();
    }
    public void IntentarLogIn(int caso)
    {
        string nombre = transform.parent.GetChild(0).GetComponent<TMP_InputField>().text;
        string contrasena = transform.parent.GetChild(1).GetComponent<TMP_InputField>().text;
        Debug.Log(nombre + " " + contrasena);
        NetworkManager.ConectarASocket(nombre, contrasena, caso);
    }
    void Update()
    {
        
    }
}
