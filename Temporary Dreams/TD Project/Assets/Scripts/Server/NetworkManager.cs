using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Attach this to a persistent object (e.g. an empty GameObject marked
// DontDestroyOnLoad) so the connection survives scene loads.

public class Vector3Placeholder
{
    public float x, y, z;
}
public class Temporario
{
    public string nombreJugador;
    public Vector3Placeholder PlaceholderPosicion;
    public Vector3 posicion;
    public float saludMaxima;
    public float saludActual;
    public int nivel;
    public string clase;
    public GameObject objetoJugador;
    
    public Temporario(string nombre, int nivel, int saludMaxima, int saludActual, string clase,GameObject objetoJugador, Vector3Placeholder posicion)
    {
        this.nombreJugador = nombre;
        this.objetoJugador = objetoJugador;
        this.PlaceholderPosicion = posicion;
        this.nivel = nivel;
        this.saludMaxima = saludMaxima;
        this.saludActual = saludActual;
        this.clase = clase;
    }
}


public class NetworkManager : MonoBehaviour
{
    UIDebugManager uiManager;
    Temporario jugadorActual;
    List<Temporario> jugadoresCargados;
    ControladorJugadores controladorJugadores;

    [SerializeField] private string serverUrl = "http://localhost:3000";
    private SocketIOUnity socket;
    public List<Jugador> jugadores = new List<Jugador>();

    private void Start()
    {
        uiManager = GameObject.FindGameObjectWithTag("UIDebug").GetComponent<UIDebugManager>();
        jugadoresCargados = new List<Temporario>();
        DontDestroyOnLoad(uiManager);
        DontDestroyOnLoad(gameObject);
    }

    public Vector3 ConvertirAVector3(Vector3Placeholder posicion)
    {
        return (new Vector3(posicion.x, posicion.y, posicion.z));
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
                jugadorActual = respuesta.GetValue<Temporario>(2);
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

        socket.On("recibirJugadoresConectados", response =>
        {
            Debug.Log("Jugadores Recibidos");
            if (response.GetValue<List<Temporario>>() != null)  
            {
                Debug.Log(response.GetValue<List<Temporario>>().Count);
                jugadoresCargados.Clear();

                jugadoresCargados.AddRange(
                    response.GetValue<List<Temporario>>()
                );

                for (int i = 0; i < jugadoresCargados.Count; i++)
                {
                    jugadoresCargados[i].posicion = ConvertirAVector3(jugadoresCargados[i].PlaceholderPosicion);
                }
                controladorJugadores.PrenderApagar(true);
            }
            Debug.Log(jugadoresCargados[0].nombreJugador);
        });

        socket.On("solicitarJugadoresEnRango", response =>
        {
            if (response.GetValue< List<Temporario>>() != null)
            {
                jugadoresCargados.Clear();

                jugadoresCargados.AddRange(
                    response.GetValue<List<Temporario>>()
                );

                for (int i = 0; i < jugadoresCargados.Count; i++)
                {
                    jugadoresCargados[i].posicion = ConvertirAVector3(jugadoresCargados[i].PlaceholderPosicion);
                }
                controladorJugadores.PrenderApagar(true);
            }
            Debug.Log(jugadoresCargados[0].nombreJugador);
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

    public void SolicitarJugadoresConectados()
    {
        controladorJugadores = GameObject.FindGameObjectWithTag("ControladorJugadores").GetComponent<ControladorJugadores>();
        Debug.Log("Jugadores solicitados");
        socket.Emit("solicitarJugadoresConectados");
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