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
    public Vector3 posicion;
    public float saludMaxima;
    public float saludActual;
    public int nivel;
    public string clase;
    public int animacionActual = 0;
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
    UIDebugManager uiDebug;
    UIManager uiManager;
    Temporario jugadorActual;
    
    public Dictionary<string, Temporario> jugadoresEnServidor;

    ControladorJugadores controladorJugadores;

    [SerializeField] private string serverUrl = "http://localhost:3000";
    private SocketIOUnity socket;
    public List<Jugador> jugadores = new List<Jugador>();

    int estadoDeConeccion; //0 desconectado, 1 conectando, 2 conectado
    float relojActualizacionServidor;

    private void Start()
    {
        if (GameObject.FindGameObjectsWithTag("NetworkManager").Length > 1) { Destroy(gameObject); }
        uiDebug = GameObject.FindGameObjectWithTag("UIDebug").GetComponent<UIDebugManager>();
        
        jugadoresEnServidor = new Dictionary<string,Temporario>();
        DontDestroyOnLoad(uiDebug);
        DontDestroyOnLoad(gameObject);

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
    public void MandarCoordenadas(Vector3 posicion, int animacion)
    {
        socket.Emit("mandarCoordenadasJugador", new { x = posicion.x, y = posicion.y, z = posicion.z }, animacion);
    }

    private void RegistrarEventosT1(string username, string contrasena)
    {
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
            Debug.Log("Disconnected: " + e);
            
            socket?.Dispose();
            UnityThread.executeInUpdate(() =>
            {
                SceneManager.LoadScene(0);
                uiDebug.ActualizarErrores("DESCONECTADO");
            });
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
                socket.Emit("conectarJugadorAInstancia", "servidor1mundo1");
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

            if (listaJugadoresRecibida != null)  
            {
                Debug.Log(response.GetValue<Dictionary<string, Temporario>>().Count);
                jugadoresEnServidor.Clear();

                

                foreach (KeyValuePair<string, Temporario> usuario in listaJugadoresRecibida)
                {
                    usuario.Value.posicion = ConvertirAVector3(usuario.Value.PlaceholderPosicion);
                    UnityThread.executeInUpdate(() =>
                    {
                        controladorJugadores.CrearJugadorFisico(usuario.Value.nombreJugador);
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
    }

    public void RegistrarEventosT2()
    {
        socket.On("recibirPosicionesJugadores", response =>
        {
            UnityThread.executeInUpdate(() =>
            {
                ActualizacionRecibida();
            });
            bool actualizarUI = false;
            Debug.Log("Posiciones recibidas");
            if (response.GetValue<Dictionary<string, Temporario>>() != null)
            {
                Dictionary<string, Temporario> listaJugadoresRecibida = response.GetValue<Dictionary<string, Temporario>>();
                Debug.Log(listaJugadoresRecibida.Count);
                Debug.Log("Jugadores en servidor: " + jugadoresEnServidor.Count);
                if(listaJugadoresRecibida.Count != jugadoresEnServidor.Count)
                {
                    if (listaJugadoresRecibida.Count > jugadoresEnServidor.Count)
                    {
                        actualizarUI = true;
                        foreach (KeyValuePair<string, Temporario> usuario in listaJugadoresRecibida)
                        {
                            if (!jugadoresEnServidor.ContainsKey(usuario.Key))
                            {
                                jugadoresEnServidor[usuario.Key] = usuario.Value;
                            }
                        }
                    }
                    else if (listaJugadoresRecibida.Count < jugadoresEnServidor.Count)
                    {
                        List<string> keysRemover = new List<string>();
                        actualizarUI = true;
                        foreach (KeyValuePair<string, Temporario> usuario in jugadoresEnServidor)
                        {
                            if (!listaJugadoresRecibida.ContainsKey(usuario.Key))
                            {
                                keysRemover.Add(usuario.Key);
                                Debug.Log("Jugador desconectado");
                            }
                        }

                        for(int i = 0; i < keysRemover.Count; i++)
                        {
                            jugadoresEnServidor.Remove(keysRemover[i]);
                        }
                    }
                }


                foreach (KeyValuePair<string, Temporario> usuario in listaJugadoresRecibida)
                {
                    usuario.Value.posicion = ConvertirAVector3(usuario.Value.PlaceholderPosicion);
                    jugadoresEnServidor[usuario.Key].posicion = usuario.Value.posicion;
                }

                if (actualizarUI)
                {
                    UnityThread.executeInUpdate(() =>
                    {
                        uiManager.CargarPlacas();
                    });
                }

                /*
                    foreach (KeyValuePair<string, Temporario> usuario in listaJugadoresRecibida)
                    {
                        if (jugadoresEnServidor.ContainsKey(usuario.Key))
                        {
                            usuario.Value.posicion = ConvertirAVector3(usuario.Value.PlaceholderPosicion);
                            jugadoresEnServidor[usuario.Key].posicion = usuario.Value.posicion;
                        }
                        else
                        {
                            Debug.Log("Jugador nuevo detectado");
                            actualizarUI = true;
                            UnityThread.executeInUpdate(() =>
                            {
                                controladorJugadores.CrearJugadorFisico(usuario.Key);
                            });
                            jugadoresEnServidor.Add(usuario.Key, usuario.Value);
                            usuario.Value.posicion = ConvertirAVector3(usuario.Value.PlaceholderPosicion);
                            jugadoresEnServidor[usuario.Key].posicion = usuario.Value.posicion;
                        }
                    }

                if (listaJugadoresRecibida.Count != jugadoresEnServidor.Count)
                {
                    foreach (KeyValuePair<string, Temporario> usuario in jugadoresEnServidor)
                    {
                        if (!listaJugadoresRecibida.ContainsKey(usuario.Value.nombreJugador))
                        {
                            Debug.Log("Jugador desconectado detectado");
                            jugadoresEnServidor.Remove(usuario.Key);
                            actualizarUI = true;
                        }
                    }

                } */
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