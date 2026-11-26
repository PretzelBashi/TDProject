
using System.Collections.Generic;

using UnityEngine;
using static CargadorElementos;

public class ControladorJugadores : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject prefabJugador;

    Temporario temporario;
    List<string> jugadoresARenderizar;
    NetworkManager networkManager;

    [SerializeField]
    public Mesh[] pelos;
    [SerializeField]
    public Sprite[] ojos;
    [SerializeField]
    public Sprite[] bocas;
    [SerializeField]
    public Transform modeloJugador;

    bool corriendo;
    void Start()
    {
        corriendo = false;
        try { networkManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<NetworkManager>(); } catch { }
        
        jugadoresARenderizar = new List<string>();
        CargarElementosC.ControladorJugadoresCargado = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!corriendo) { return; }
        for(int i = 0; i < jugadoresARenderizar.Count; i++)
        {
            Transform objetoJugadorBuffer = networkManager.jugadoresEnServidor[jugadoresARenderizar[i]].objetoJugador.transform;
            Animator animatorBuffer = objetoJugadorBuffer.GetChild(0).GetComponent<Animator>();
            Temporario jugadorServidorBuffer = networkManager.jugadoresEnServidor[jugadoresARenderizar[i]];

            objetoJugadorBuffer.position = jugadorServidorBuffer.posicion;
            objetoJugadorBuffer.rotation = Quaternion.Euler(jugadorServidorBuffer.rotacion);

            if (animatorBuffer.GetInteger("EstadoCuerpo") != jugadorServidorBuffer.animacionActual)
            {
                animatorBuffer.SetInteger("EstadoCuerpo", jugadorServidorBuffer.animacionActual);
                animatorBuffer.SetTrigger("CambioCuerpo");
            }
        }
    }
    public GameObject CrearJugadorFisico(string usuario, Temporario usuarioAparecer)
    {
        List<SkinnedMeshRenderer> materiales;
        materiales = new List<SkinnedMeshRenderer>();

        Debug.Log("Jugador creado");
        GameObject objetoJugador = Instantiate(prefabJugador,Vector3.zero, Quaternion.identity);
        materiales.Add(objetoJugador.transform.GetChild(0).Find("pelo").GetComponent<SkinnedMeshRenderer>());
        materiales.Add(objetoJugador.transform.GetChild(0).Find("pecho").GetComponent<SkinnedMeshRenderer>());
        materiales.Add(objetoJugador.transform.GetChild(0).Find("piernas").GetComponent<SkinnedMeshRenderer>());
        materiales.Add(objetoJugador.transform.GetChild(0).Find("zapatos").GetComponent<SkinnedMeshRenderer>());

        objetoJugador.transform.GetChild(0).Find("pelo").GetComponent<SkinnedMeshRenderer>().sharedMesh = pelos[usuarioAparecer.pelo];
        objetoJugador.transform.GetChild(0).Find("ojos").GetComponent<SkinnedMeshRenderer>().material.mainTexture = ojos[usuarioAparecer.ojos].texture;
        objetoJugador.transform.GetChild(0).Find("boca").GetComponent<SkinnedMeshRenderer>().material.mainTexture = bocas[usuarioAparecer.boca].texture;

        for (int i = 0; i < materiales.Count; i++)
        {
            foreach (Material material in materiales[i].materials)
            {

                Debug.Log(material.name);
                ColorUtility.TryParseHtmlString("#" + usuarioAparecer.color1, out Color currentColour);

                switch (material.name)
                {
                    case "pelo (Instance)":
                        material.color = currentColour;
                        break;
                    case "traje (Instance)":    
                        material.color = currentColour;
                        break;

                    case "trageP2 (Instance)":
                        Debug.Log("Entro traje P2");
                        material.color = new Color(
                            currentColour.r * 0.65f,
                            currentColour.g * 0.65f,
                            currentColour.b * 0.65f,
                            currentColour.a
                        );
                        break;
                }
            }
        }

        jugadoresARenderizar.Add(usuario);
        return objetoJugador;
    }
    public void DestruirJugadorFisico(string usuario)
    {
        for(int i = 0; i < jugadoresARenderizar.Count; i++)
        {
            if (jugadoresARenderizar[i] == usuario)
            {
                if(networkManager.jugadoresEnServidor[usuario].objetoJugador != null)
                {
                    Destroy(networkManager.jugadoresEnServidor[usuario].objetoJugador);
                    jugadoresARenderizar.RemoveAt(i);
                }
            }
        }
    }
    public void PrenderApagar(bool opcion)
    {
        corriendo=opcion;
    }

    public void ActualizarJugadores()
    {

    }

    public void CambiarJugadores(List<Temporario> jugadoresNuevos)
    {

    }
}
