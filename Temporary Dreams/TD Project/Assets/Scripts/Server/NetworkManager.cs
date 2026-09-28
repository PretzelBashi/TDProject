using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SocketIOUnity;

// Attach this to a persistent object (e.g. an empty GameObject marked
// DontDestroyOnLoad) so the connection survives scene loads.
public class TemporarioTemp
{
    string nombre;
    Vector3 posicion;
}
public class Temporario
{
    string nombre;
    string idSocket;
    GameObject objetoJugador;
    Vector3 posicion;

    public Temporario(string nombre, string idSocket,GameObject objetoJugador, Vector3 posicion)
    {
        this.nombre = nombre;
        this.idSocket = idSocket;
        this.objetoJugador = objetoJugador;
        this.posicion = posicion;
    }
}
public class NetworkManager : MonoBehaviour
{
    UIDebugManager uiManager;

    [SerializeField] private string serverUrl = "http://localhost:3000";
    private SocketIOUnity socket;
    public List<Jugador> jugadores = new List<Jugador>();

    private void Start()
    {
        uiManager = GameObject.FindGameObjectWithTag("UIDebug").GetComponent<UIDebugManager>();
        DontDestroyOnLoad(uiManager);
        DontDestroyOnLoad(gameObject);
    }

    public void ConectarASocket(string username, string contrasena)
    {
        var uri = new Uri(serverUrl);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        // Newtonsoft's serializer is more reliable than the default
        // System.Text.Json one under IL2CPP builds.
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        Debug.Log("Iniciando");
        RegistrarEventos(username, contrasena);

        socket.Connect();
        SendChat("Teto mi vieja we");
    }

    public void MandarCoordenadas(Vector3 posicion, int animacion)
    {
        socket.Emit("mandarCoordenadas", new { x = posicion.x, y = posicion.y, z = posicion.z }, animacion);
    }

    private void RegistrarEventos(string username, string contrasena)
    {
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Conectado como " + socket.Id + " con el nombre " + username);
            socket.Emit("login", username, contrasena);
        };

        socket.OnError += (sender, e) =>
        {
            Debug.Log("Error");
        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("Disconnected: " + e);
            uiManager.ActualizarErrores("DESCONECTADO");
        };

        socket.On("loginExitoso", respuesta =>
        {
            
            if (respuesta.GetValue<bool>(0))
            {
                socket.Emit("conectarJugadorAInstancia", "servidor1mundo1");
                uiManager.ActualizarErrores(respuesta.GetValue<string>(1));
            }
            else
            {
                Debug.Log(respuesta.GetValue<string>(1));
                uiManager.ActualizarErrores(respuesta.GetValue<string>(1));
            }
        });

        socket.On("conexionAInstanciaExitosa", respuesta =>
        {
            Debug.Log("Instancia recibida" + respuesta);
            if (respuesta.GetValue<bool>())
            {
                Debug.Log("Escena");

                UnityThread.executeInUpdate(() =>
                {
                    SceneManager.LoadScene("Mundo 1");
                });
            } else
            {
                uiManager.ActualizarErrores("El servidor no existe");
            }
        });

        socket.On("movimientoJugadores", response =>
        {
            var data = response.GetValue<PlayerMoveData>();
            Debug.Log($"Player {data.id} moved to {data.x}, {data.y}, {data.z}");
            // e.g. FindRemotePlayer(data.id)?.SetTargetPosition(...)
        });

        socket.On("playerJoined", response =>
        {
            var data = response.GetValue<PlayerMoveData>();
            Debug.Log($"Player joined: {data.id}");
        });

        socket.On("playerLeft", response =>
        {
            var id = response.GetValue<string>();
            Debug.Log($"Player left: {id}");
        });

        socket.On("chat", response =>
        {
            var data = response.GetValue<ChatMessage>();
            Debug.Log($"[{data.id}] {data.message}");
        });
    }
    public void SendChat(string message)
    {
        socket.Emit("chat", message);
    }

    void OnDestroy()
    {
        socket?.Disconnect();
        socket?.Dispose();
    }

    [Serializable]
    private class PlayerMoveData
    {
        public string id;
        public float x, y, z;
    }

    [Serializable]
    private class ChatMessage
    {
        public string id;
        public string message;
    }
}