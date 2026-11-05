using UnityEngine;

public class pruebaCubo : MonoBehaviour
{
    public float distancia = 5f;
    public float velocidad = 2f;

    private Vector3 posicionInicial;
    private Vector3 posicionFinal;
    private bool yendoAdelante = true;

    void Start()
    {
        posicionInicial = transform.position;
        posicionFinal = posicionInicial + transform.forward * distancia;
    }

    void Update()
    {
        Vector3 destino = yendoAdelante ? posicionFinal : posicionInicial;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destino,
            velocidad * Time.deltaTime
        );

        if (transform.position == destino)
        {
            yendoAdelante = !yendoAdelante;
        }
    }
}
