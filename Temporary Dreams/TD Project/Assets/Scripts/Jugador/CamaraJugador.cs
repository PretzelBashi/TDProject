using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraJugador : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector3 rotacionCamara;
    PlayerInput controlesJugador;
    float sensibilidad;
    void Start()
    {
        sensibilidad = 0.2f;
        controlesJugador = GameObject.FindGameObjectWithTag("Jugador").GetComponent<PlayerInput>();
        rotacionCamara = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        rotacionCamara.y += controlesJugador.actions["Look"].ReadValue<Vector2>().x * sensibilidad;
        rotacionCamara.x += controlesJugador.actions["Look"].ReadValue<Vector2>().y * -sensibilidad;

        transform.rotation = Quaternion.Euler(rotacionCamara);
    }
}
