using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Unity.Collections.Tests.CoreCLR.TestJobs;
using UnityEngine;
using UnityEngine.SceneManagement;


//REESCRIBIR LAS LISTAS RECIBIDAS A DICCIONARIOS

public class Vector3Placeholder
{
    public float x, y, z;
}
public class Temporario
{
    public string nombreJugador;
    public Vector3Placeholder PlaceholderPosicion;
    public Vector3Placeholder PlaceholderRotacion;
    public Vector3 rotacion;
    public Vector3 posicion;
    public float saludMaxima;
    public float saludActual;
    public int nivel;
    public string clase;
    public int animacionActual;
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

public class Item
{
    public string nombre;
    public int id;
    public string descripcion;
    public string vida;
}


public class NetworkManager : MonoBehaviour
{
    UIDebugManager uiDebug;
    UIManager uiManager;
    Temporario jugadorActual;
    
    public Dictionary<string, Temporario> jugadoresEnServidor;

    ControladorJugadores controladorJugadores;

    [SerializeField] private string serverUrl = "http://localhost:3000";
    private SocketIOUnity socket;
    public List<Jugador> jugadores = new List<Jugador>();
    public List<Item> itemsJugador = new List<Item>();

    int estadoDeConeccion; //0 desconectado, 1 conectando, 2 conectado
    string usuarioSeleccionado;
    float relojActualizacionServidor;
    string servidorAConectar;
    bool desconectadoIntencional;

    private void Start()
    {
        Application.runInBackground = true;
        if (GameObject.FindGameObjectsWithTag("NetworkManager").Length > 1) { Destroy(gameObject); }
        uiDebug = GameObject.FindGameObjectWithTag("UIDebug").GetComponent<UIDebugManager>();
        
        jugadoresEnServidor = new Dictionary<string,Temporario>();
        DontDestroyOnLoad(uiDebug);
        DontDestroyOnLoad(gameObject);
        servidorAConectar = "servidor1";
        desconectadoIntencional = false;
        Application.targetFrameRate = 80;
    }

    public void ActualizacionRecibida()
    {
        uiDebug.ActualizacionServidor(relojActualizacionServidor);
        relojActualizacionServidor = 0;
    }

    public Vector3 ConvertirAVector3(Vector3Placeholder posicion)
    {
        return (new Vector3(posicion.x, posicion.y, posicion.z));
    }

    public void ConectarASocket(string username, string contrasena)
    {
        desconectadoIntencional = false;
        var uri = new Uri(serverUrl);
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        // Newtonsoft's serializer is more reliable than the default
        // System.Text.Json one under IL2CPP builds.
        socket.JsonSerializer = new NewtonsoftJsonSerializer();

        Debug.Log("Iniciando");
        RegistrarEventosT1(username, contrasena);

        socket.Connect();
    }

    public void IntentarConectar()
    {
        
    }

    public void SolicitarJugadoresConectados()
    {
        controladorJugadores = GameObject.FindGameObjectWithTag("ControladorJugadores").GetComponent<ControladorJugadores>();
        uiManager = GameObject.FindGameObjectWithTag("UIJugador").GetComponent<UIManager>();
        Debug.Log("Jugadores solicitados");
        socket.Emit("solicitarJugadoresConectados");
    }
    public void MandarCoordenadas(Vector3 posicion, Vector3 rotacion, int animacion)
    {
        socket.Emit("mandarCoordenadasJugador", new { x = posicion.x, y = posicion.y, z = posicion.z }, new { x = rotacion.x, y = rotacion.y, z = rotacion.z }, animacion);
    }

    private void RegistrarEventosT1(string username, string contrasena)
    {
        usuarioSeleccionado = username;
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log("OnReconnectAttempt: " + socket.Id + ", " + e);
        };
        socket.OnReconnectError += (sender, e) =>
        {
            Debug.Log("OnReconnectError: " + socket.Id + ", " + e);
        };
        socket.OnReconnectFailed += (sender, e) =>
        {
            Debug.Log("OnReconnectFailed: " + socket.Id + ", " + e);
        };
        socket.OnReconnected += (sender, e) =>
        {
            Debug.Log("OnReconnected: " + socket.Id + ", " + e);
        };

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
            if (e == "io client disconnect" || e == "io server disconnect")
            {
                Debug.Log("Desconectado: " + e);

                socket?.Dispose();
                jugadoresEnServidor.Clear();
                UnityThread.executeInUpdate(() =>
                {
                    SceneManager.LoadScene(0);
                    uiDebug.ActualizarErrores("DESCONECTADO: " + e);
                });
            } else
            {
                uiDebug.ActualizarErrores("Reconectando...");
            }
        };

        socket.On("loginExitoso", respuesta =>
        {
            UnityThread.executeInUpdate(() =>
            {
                ActualizacionRecibida();
            });
            if (respuesta.GetValue<bool>(0))
            {
                Debug.Log("Login exitoso");
                socket.Emit("conectarJugadorAInstancia", servidorAConectar);
                uiDebug.ActualizarErrores(respuesta.GetValue<string>(1));
                jugadorActual = respuesta.GetValue<Temporario>(2);
            }
            else
            {
                Debug.Log(respuesta.GetValue<string>(1));
                Debug.Log("Login fallo");
                uiDebug.ActualizarErrores(respuesta.GetValue<string>(1));
            }
        });

        socket.On("conexionAInstanciaExitosa", respuesta =>
        {
            UnityThread.executeInUpdate(() =>
            {
                ActualizacionRecibida();
            });

            Debug.Log("Instancia recibida" + respuesta);
            if (respuesta.GetValue<bool>())
            {
                UnityThread.executeInUpdate(() =>
                {
                    SceneManager.LoadScene("Mundo 1");
                });
            } else
            {
                Debug.Log("Conexion a instancia rechazada");
                uiDebug.ActualizarErrores("El servidor no existe");
            }
        });

        socket.On("recibirJugadoresConectados", response =>
        {
            UnityThread.executeInUpdate(() =>
            {
                ActualizacionRecibida();
            });
            
            Dictionary<string, Temporario> listaJugadoresRecibida = response.GetValue<Dictionary<string, Temporario>>();
            Debug.Log(listaJugadoresRecibida.Count);
            if (listaJugadoresRecibida[usuarioSeleccionado] != null)
            {
                jugadorActual = listaJugadoresRecibida[usuarioSeleccionado];
            }
            if (listaJugadoresRecibida != null)  
            {
                Debug.Log(response.GetValue<Dictionary<string, Temporario>>().Count);
                jugadoresEnServidor.Clear();

                

                foreach (KeyValuePair<string, Temporario> usuario in listaJugadoresRecibida)
                {
                    usuario.Value.posicion = ConvertirAVector3(usuario.Value.PlaceholderPosicion);
                    usuario.Value.rotacion = ConvertirAVector3(usuario.Value.PlaceholderRotacion);
                    

                    if (usuario.Value.nombreJugador == jugadorActual.nombreJugador)
                    {
                        continue;
                    }
                    UnityThread.executeInUpdate(() =>
                    {
                         usuario.Value.objetoJugador = controladorJugadores.CrearJugadorFisico(usuario.Value.nombreJugador);
                    });

                    Debug.Log("Jugador cargado: " + usuario.Value.nombreJugador);
                }

                jugadoresEnServidor = listaJugadoresRecibida;
                controladorJugadores.PrenderApagar(true);
                UnityThread.executeInUpdate(() =>
                {
                    uiManager.CargarPlacas();
                });
                RegistrarEventosT2();
            }
        });

        socket.On("abrirInventario", response =>
        {

        });
    }
    public void AgregarQuitarJugadores(string accion, string usuario, Temporario temporario)
    {
        switch (accion)
        {
            case "agregar":
                UnityThread.executeInUpdate(() =>
                {
                    jugadoresEnServidor[usuario] = temporario;
                    jugadoresEnServidor[usuario].objetoJugador = controladorJugadores.CrearJugadorFisico(usuario);
                    uiManager.CargarPlacas();
                });
                break;
            case "quitar":
                UnityThread.executeInUpdate(() =>
                {
                    controladorJugadores.DestruirJugadorFisico(usuario);
                    jugadoresEnServidor.Remove(usuario);
                    uiManager.CargarPlacas();
                });
                break;

            default: Debug.Log("AQJ incorrecto"); break;
        }
    }
    public void RegistrarEventosT2()
    {
        socket.On("jugadorNuevoConectado", jugador =>
        {
            Temporario nuevoJugador = jugador.GetValue<Temporario>();

            if (!jugadoresEnServidor.ContainsKey(nuevoJugador.nombreJugador)){
                AgregarQuitarJugadores("agregar", nuevoJugador.nombreJugador, nuevoJugador);
            }
        });

        socket.On("jugadorDesconectado", jugador =>
        {
            string nuevoJugador = jugador.GetValue<string>();

            if (jugadoresEnServidor.ContainsKey(nuevoJugador))
            {
                AgregarQuitarJugadores("quitar", nuevoJugador, null);
            }
        });

        socket.On("recibirPosicionesJugadores", jugadoresConectados =>
        {
            UnityThread.executeInUpdate(() =>
            {
                ActualizacionRecibida();
            });
            bool actualizarUI = false;
            Dictionary<string, Temporario> listaJugadoresRecibida = jugadoresConectados.GetValue<Dictionary<string, Temporario>>();
            foreach (KeyValuePair<string, Temporario> usuario in listaJugadoresRecibida)
            {
                if (jugadoresEnServidor.ContainsKey(usuario.Key))
                {
                    jugadoresEnServidor[usuario.Key].posicion = ConvertirAVector3(usuario.Value.PlaceholderPosicion);
                    jugadoresEnServidor[usuario.Key].rotacion = ConvertirAVector3(usuario.Value.PlaceholderRotacion);
                    jugadoresEnServidor[usuario.Key].animacionActual = usuario.Value.animacionActual;
                } else
                {
                    //Debug.Log("Jugador perdido");
                }
            }
        });

        socket.On("solicitarJugadoresEnRango", response =>
        {

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

    public void Desconectar()
    {
        socket?.Disconnect();
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