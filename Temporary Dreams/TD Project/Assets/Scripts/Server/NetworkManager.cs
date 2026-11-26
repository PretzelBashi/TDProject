using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.Tests.CoreCLR.TestJobs;
using UnityEngine;
using UnityEngine.SceneManagement;


//REESCRIBIR LAS LISTAS RECIBIDAS A DICCIONARIOS

public class Vector3Placeholder
{
    public float x, y, z;
}

[Serializable]
public class PlantillaStats
{

}

public class Temporario
{
    public string nombreJugador;
    public Vector3Placeholder PlaceholderPosicion;
    public Vector3Placeholder PlaceholderRotacion;
    public Vector3 rotacion;
    public Vector3 posicion;

    public int nivel;
    public string clase;
    public int animacionActual;
    public GameObject objetoJugador;

    // Informacion general
    public int experiencia = 0;
    public int monedas = 100;
    public int monedasRenacimiento = 0;

    public int pelo = 0;
    public int ojos = 0;
    public int boca = 0;
    public string color1 = "";

    // Salud y mana
    public float saludMaxima;
    public float saludActual;
    public int manaMaxima = 50;
    public int manaActual = 50;

    // Estadisticas actuales
    public int danoFisicoActual = 10;
    public int danoMagicoActual = 10;
    public int armaduraFisicaActual = 5;
    public int armaduraMagicaActual = 5;
    public int armaduraCriticosActual = 0;
    public int penetracionFisicaActual = 5;
    public int penetracionMagicaActual = 5;
    public int aceleracionCasteoActual = 5;

    public float velocidadDeAtaqueActual = 0.5f;
    public int roboVidaActual = 0;
    public int probabilidadCriticoActual = 5;
    public int velocidadActual = 5;
    public int suerteActual = 1;

    // Habilidades de vida mayores
    public int nivelAgriculturaActual = 0;
    public int nivelAlquimiaActual = 0;
    public int nivelCocinaActual = 0;
    public int nivelSastreriaActual = 0;
    public int nivelHerreroActual = 0;
    public int nivelEscritorActual = 0;

    // Habilidades de vida menores
    public int nivelCazaActual = 0;
    public int nivelPescaActual = 0;
    public int nivelMineriaActual = 0;
    public int nivelForrajeadorActual = 0;


    // Habilidades y enfriamientos
    public Dictionary<string, int[]> habilidades = new Dictionary<string, int[]>()
    {
        { "basico", new int[] { 0, 1 } }
    };
    // Extras y efectos
    public Dictionary<string, object> extras = new Dictionary<string, object>();
    public Dictionary<string, object> efectos = new Dictionary<string, object>();

    public int idJugador = 0;

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
    public int rareza;
    public string vida;
}


public class NetworkManager : MonoBehaviour
{
    UIDebugManager uiDebug;
    UIManager uiManager;
    public UIManagerInicio uiManagerInicio;
    public Temporario jugadorActual;
    
    public Dictionary<string, Temporario> jugadoresEnServidor;

    ControladorJugadores controladorJugadores;

    [SerializeField] private string serverUrl = "http://localhost:3000";
    private SocketIOUnity socket;
    public List<Jugador> jugadores = new List<Jugador>();
    public List<Item> itemsJugador = new List<Item>();

    int estadoDeConeccion; //0 desconectado, 1 conectando, 2 conectado
    string usuarioSeleccionado;
    float relojActualizacionServidor;
    public string servidorAConectar;
    bool desconectadoIntencional;
    int casoConectado;
    private void Start()
    {
        Application.runInBackground = true;
        if (GameObject.FindGameObjectsWithTag("NetworkManager").Length > 1) { Destroy(gameObject); }
        uiDebug = GameObject.FindGameObjectWithTag("UIDebug").GetComponent<UIDebugManager>();
        uiManagerInicio = GameObject.FindGameObjectWithTag("UIManagerInicio").GetComponent<UIManagerInicio>();

        casoConectado = 0;
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

    public void ConectarASocket(string username, string contrasena, int caso)
    {
        casoConectado = caso;
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

    public void EliminarPersonaje(string nombre)
    {
        socket.Emit("eliminarPersonaje", nombre);
    }

    public void ElegirPersonaje(string nombre, int pelo, int cara, string color1, string clase)
    {
        socket.Emit("crearPersonaje", nombre, pelo, cara, color1, clase);
    }

    public void ConectarAInstancia(string servidor, string usuario)
    {
        socket.Emit("conectarJugadorAInstancia", servidor, usuario);
    }

    public void SolicitarInventario()
    {
        socket.Emit("solicitarInventario");
    }

    public void CrearPersonaje(string nombre, int pelo, int cara, int boca, string color1)
    {
        socket.Emit("crearPersonaje", nombre, pelo, cara, color1, boca, "Proximamente");
    }

    public void SolicitarJugadoresConectados()
    {
        controladorJugadores = GameObject.FindGameObjectWithTag("ControladorJugadores").GetComponent<ControladorJugadores>();
        uiManager = GameObject.FindGameObjectWithTag("UIJugador").GetComponent<UIManager>();

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
            switch (casoConectado)
            {
                case 0: Debug.Log("caso vacio"); break;
                case 1: socket.Emit("login", username, contrasena); break;
                case 2: socket.Emit("registro", username, contrasena); break;
            }
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
                    if (SceneManager.GetActiveScene().name == "Login")
                    {

                        uiManagerInicio.ReiniciarUI();
                        uiDebug.ActualizarErrores("DESCONECTADO LOGIN: " + e);

                    }
                    else
                    {
                        SceneManager.LoadScene(0);
                        uiDebug.ActualizarErrores("DESCONECTADO: " + e);
                    }
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
                socket.Emit("solicitarPersonajes");
                UnityThread.executeInUpdate(() =>
                {
                    uiDebug.ActualizarInfoLogin(respuesta.GetValue<string>(1));
                    jugadorActual = respuesta.GetValue<Temporario>(2);
                });
                
            }
            else
            {
                Debug.Log(respuesta.GetValue<string>(1));
                Debug.Log("Login fallo");
                UnityThread.executeInUpdate(() =>
                {
                    uiDebug.ActualizarInfoLogin(respuesta.GetValue<string>(1));
                });
            }
        });

        socket.On("registroExitoso", respuesta =>
        {
            UnityThread.executeInUpdate(() =>
            {
                ActualizacionRecibida();
            });
            if (respuesta.GetValue<bool>(0))
            {
                Debug.Log("Registro exitoso");
                UnityThread.executeInUpdate(() =>
                {
                    uiDebug.ActualizarInfoLogin(respuesta.GetValue<string>(1));
                });
            }
            else
            {
                Debug.Log(respuesta.GetValue<string>(1));
                Debug.Log("Registro fallo");
                UnityThread.executeInUpdate(() =>
                {
                    uiDebug.ActualizarInfoLogin(respuesta.GetValue<string>(1));
                });
            }
        });

        socket.On("personajesRecibidos", personajes =>
        {
            UnityThread.executeInUpdate(() =>
            {
                uiManagerInicio.Ocultar(1);
                uiManagerInicio.Ocultar(2);
                uiManagerInicio.Mostrar(3);
                Debug.Log(personajes.GetValue<Temporario[]>().Length);
                uiManagerInicio.IniciarPersonajes(personajes.GetValue<Temporario[]>());
            });
        });

        socket.On("personajeCreado", respuesta =>
        {
            UnityThread.executeInUpdate(() =>
            {
                ActualizacionRecibida();
            });

            if (respuesta.GetValue<bool>())
            {
                UnityThread.executeInUpdate(() =>
                {
                    uiDebug.ActualizarErrores("Personaje creado!");
                    uiManagerInicio.Ocultar(4);
                    socket.Emit("solicitarPersonajes");

                });
            }
            else
            {
                Debug.Log("Creacion rechazada");
                uiDebug.ActualizarErrores("Fallo la creacion, maximo de personajes alcanzado");
            }
        });

        socket.On("conexionAInstanciaExitosa", respuesta =>
        {
            UnityThread.executeInUpdate(() =>
            {
                ActualizacionRecibida();
            });
            jugadorActual = respuesta.GetValue<Temporario>(1);
            Debug.Log("Instancia recibida" + respuesta);
            
            if (respuesta.GetValue<bool>(0))
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
            //Debug.Log(listaJugadoresRecibida.Count);
            if (listaJugadoresRecibida[usuarioSeleccionado] != null)
            {
                jugadorActual = listaJugadoresRecibida[usuarioSeleccionado];
            }
            if (listaJugadoresRecibida != null)  
            {
                //Debug.Log(response.GetValue<Dictionary<string, Temporario>>().Count);
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
                        Temporario usuarioAparecer = usuario.Value;
                         usuario.Value.objetoJugador = controladorJugadores.CrearJugadorFisico(usuario.Value.nombreJugador, usuarioAparecer);
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
                    jugadoresEnServidor[usuario].objetoJugador = controladorJugadores.CrearJugadorFisico(usuario, temporario);
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

        socket.On("recibirInventario", inventario =>
        {
            itemsJugador.Clear();
            Item[] objetos = inventario.GetValue<Item[]>();
            Debug.Log(objetos.Length);
            for (int i = 0; i < objetos.Length; i++)
            {
                itemsJugador.Add(objetos[i]);
                Debug.Log(objetos[i].nombre);
            }
            UnityThread.executeInUpdate(() =>
            {
                uiManager.CargarInventario();
            });
            
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
        if (socket != null && socket.Connected)
        {
            socket.Disconnect();
        }
        else
        {
            jugadoresEnServidor.Clear();
            Debug.Log("ES NULO");

            UnityThread.executeInUpdate(() =>
            {
                if (SceneManager.GetActiveScene().name == "Login")
                {
                    Debug.Log("ES NULO1");
                    uiManagerInicio.ReiniciarUI();
                    uiDebug.ActualizarErrores("DESCONECTADO LOGIN: ");

                }
                else
                {
                    Debug.Log("ES NULO2");
                    SceneManager.LoadScene(0);
                    uiDebug.ActualizarErrores("DESCONECTADO: ");
                }
            });
        }
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