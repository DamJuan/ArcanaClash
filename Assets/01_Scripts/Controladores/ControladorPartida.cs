using UnityEngine;
using System.Collections.Generic;
using System.Collections;


//Esto lo que hace es crear la partida, la logica del tablero y pinta casillas
public class ControladorPartida : MonoBehaviour
{

    public GameObject PrefabCasilla;

    // Esto lo hago para tener orden en las casillas
    public Transform ContenedorTablero;

    public GameObject PrefabCarta;
    public Transform ContenedorManoJugador;

    //Esto es para ver tipo abanico las cartas en la mano
    public float SeparacionCartas = 0.8f; 
    public float CurvaAltura = 0.2f;      
    public float RotacionAbanico = 5f;

    private ModeloTablero tableroLogico;
    private ModeloJugador jugador1;
    private ModeloJugador jugador2;
    public LectorCSV lector;
    public static ControladorPartida Instancia;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }
    public ModeloJugador JugadorLocal { get { return jugador1; } }

    void Start()
    {
        tableroLogico = new ModeloTablero();

        jugador1 = new ModeloJugador("Jugador 1");
        jugador2 = new ModeloJugador("La IA malvada");

        GenerarTableroVisual();

        Debug.Log("Repartiendo cartas...");

        StartCoroutine(IniciarPartida());
    }

    IEnumerator IniciarPartida()
    {
        Debug.Log("Cargando cartas...");

        if (lector != null)
        {
            List<ModeloCriatura> todasLasCartas = lector.CargarCartas();

            jugador1.CargarMazo(todasLasCartas);

            jugador1.Barajar();

            for (int i = 0; i < 5; i++)
            {
                RobarCartaJugador();
                yield return new WaitForSeconds(0.2f); // Con esto consigo un efecto visual de robo de cartas
            }
        }
        else
        {
            Debug.LogError("Falta asignar el LectorCSV en el GameController");
        }
    }

    void RobarCartaJugador()
    {
        ModeloCriatura cartaRobada = jugador1.RobarCarta();

        if (cartaRobada != null)
        {
            CrearCartaVisual(cartaRobada);
        }
    }

    void CrearCartaVisual(ModeloCriatura cartaModelo)
    {
        if (PrefabCarta != null && ContenedorManoJugador != null)
        {
            GameObject cartaObj = Instantiate(PrefabCarta, ContenedorManoJugador);

            VistaCarta vista = cartaObj.GetComponent<VistaCarta>();
            if (vista != null)
            {
                vista.Configurar(cartaModelo);
            }
            OrganizarMano(); 
        }
    }

        void GenerarTableroVisual()
    {
        // Bucle para recorrer todas las posiciones del tablero
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                // Cojo el dato lógico de esta posición
                ModeloCasilla modelo = tableroLogico.ObtenerCasilla(x, y);

                // Calculo dónde poner el cubo
                Vector3 posicion = new Vector3(x * 2.0f, 0, y * 2.0f);

                // Instancio el Prefab
                GameObject nuevaCasilla = Instantiate(PrefabCasilla, posicion, Quaternion.identity);

                // La hago hija del contenedor para tener orden
                if (ContenedorTablero != null) nuevaCasilla.transform.SetParent(ContenedorTablero);
                nuevaCasilla.name = $"Casilla_{x}_{y}";

                // Conecto la Vista con el Modelo
                InfoCasilla vistaCasilla = nuevaCasilla.GetComponent<InfoCasilla>();
                if (vistaCasilla != null)
                {
                    vistaCasilla.Configurar(modelo);
                    // Fila 0 y 1 son del jugador 1 la fila 2 y 3 del jugador 2
                    if (y < 2)
                    {
                        vistaCasilla.EsTerritorioAliado = true;
                    }
                    else
                    {
                        vistaCasilla.EsTerritorioAliado = false;
                    }
                }
            }
        }
    }

    void OrganizarMano()
    {
        int cantidad = ContenedorManoJugador.childCount;

        for (int i = 0; i < cantidad; i++)
        {
            Transform carta = ContenedorManoJugador.GetChild(i);

            // Calculo el centro de la mano
            float centro = (cantidad - 1) / 2f;
            float diferencia = i - centro;

            float posX = diferencia * SeparacionCartas;

            float posY = Mathf.Abs(diferencia) * -CurvaAltura;

            carta.localPosition = new Vector3(posX, posY, 0);

            float rotZ = diferencia * -RotacionAbanico;
            carta.localRotation = Quaternion.Euler(0, 0, rotZ);
        }
    }

}