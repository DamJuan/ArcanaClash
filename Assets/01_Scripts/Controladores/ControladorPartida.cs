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

    public float SeparacionCartas = 2f; 

    private ModeloTablero tableroLogico;
    private ModeloJugador jugador1;
    private ModeloJugador jugador2;
    public LectorCSV lector;
    public static ControladorPartida Instancia;

    private bool esTurnoJugador = true;

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

        jugador1.RestaurarMagia();

        ActualizarUI();

        GenerarTableroVisual();

        Debug.Log("Repartiendo cartas...");

        StartCoroutine(IniciarPartida());
    }

    public void ActualizarUI()
    {
        if (GestorUI.Instancia != null)
        {
            // Con esto le mando los datos del modeloJugador a la pantalla
            GestorUI.Instancia.ActualizarMana(jugador1.MagiaActual, jugador1.MagiaMaxima);
        }
    }

    IEnumerator IniciarPartida()
    {
        Debug.Log("Cargando cartas...");

        if (lector != null)
        {
            List<ModeloCriatura> todasLasCartas = lector.CargarCartas();

            jugador1.CargarMazo(todasLasCartas);
            jugador1.Barajar();

            jugador2.CargarMazo(todasLasCartas); 
            jugador2.Barajar();

            for (int k = 0; k < 5; k++)
            {
                jugador2.RobarCarta();
            }
            Debug.Log($"La IA empieza con {jugador2.Mano.Count} cartas.");

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
                Vector3 posicionLocal = new Vector3(x * 2.0f, 0, y * 2.0f);

                // Instancio el Prefab
                GameObject nuevaCasilla = Instantiate(PrefabCasilla, ContenedorTablero, false);

                nuevaCasilla.transform.localPosition = posicionLocal;

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

            carta.localPosition = new Vector3(posX, 0, 0);

            carta.localRotation = Quaternion.identity;
        }
    }

    public void BotonPasarTurno()
    {
        if (!esTurnoJugador) return;
        esTurnoJugador = false;

        // Desactivo el boton de pasar turno hasta que sea el turno del jugador de nuevo
        if (GestorUI.Instancia != null) GestorUI.Instancia.ActivarBotonTurno(false);

        StartCoroutine(TurnoIA());

    }

    IEnumerator TurnoIA()
    {
        yield return new WaitForSeconds(1.0f); // Pausa para que se note el cambio de turno

        jugador2.SubirManaMaximo();
        jugador2.RestaurarMagia();
        jugador2.ReiniciarTurno();

        ModeloCriatura cartaRobada = jugador2.RobarCarta();

        Debug.Log($"La IA tiene {jugador2.MagiaActual} maná y {jugador2.Mano.Count} cartas.");

        yield return new WaitForSeconds(1.0f); // Pausa para simular que piensa

        // Bucle de jugar cartas
        // Recorro la mano al revés para poder borrar cartas sin romper el bucle
        for (int i = jugador2.Mano.Count - 1; i >= 0; i--)
        {
            ModeloCriatura cartaCandidata = jugador2.Mano[i];

            if (jugador2.MagiaActual >= cartaCandidata.CosteMagia && jugador2.PuedeJugarCarta())
            {
                ModeloCasilla sitio = EncontrarCasillaVaciaIA();

                if (sitio != null)
                {
                    Debug.Log($"IA juega {cartaCandidata.Nombre} en ({sitio.CoordenadaX},{sitio.CoordenadaY})");

                    JugarCartaIA_Visual(cartaCandidata, sitio);

                    jugador2.GastarMagia(cartaCandidata.CosteMagia);
                    jugador2.RegistrarJugada();
                    jugador2.EliminarCartaDeMano(cartaCandidata);

                    yield return new WaitForSeconds(1.5f); // Pausa entre cartas para simular el movimiento y que no sea instantáneo
                }
            }
        }

        Debug.Log("Fin turno IA. TE TOCA.");

        ComenzarTurnoJugador();
    }

    void ComenzarTurnoJugador()
    {
        jugador1.SubirManaMaximo();
        jugador1.RestaurarMagia();
        jugador1.ReiniciarTurno();

        RobarCartaJugador();

        ActualizarUI();
        esTurnoJugador = true;
        if (GestorUI.Instancia != null) GestorUI.Instancia.ActivarBotonTurno(true);
    }



    // ESTE METODO HACE QUE LA IA JUEGUE UNA CARTA EN UNA CASILLA VACIA DE SU ZONA
    ModeloCasilla EncontrarCasillaVaciaIA()
    {
       
        for (int y = 2; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                ModeloCasilla posibleCasilla = tableroLogico.ObtenerCasilla(x, y);
                if (!posibleCasilla.EstaOcupada)
                {
                    return posibleCasilla;
                }
            }
        }
        return null;
    }

    // METODO PARA QUE LA IA JUEGUE UNA CARTA y ACTUALICE LA PARTE VISUAL
    void JugarCartaIA_Visual(ModeloCriatura carta, ModeloCasilla casillaLogica)
    {
        // Buscamos el objeto visual de la casilla donde vamos a poner la carta lo busco por nombre demomento 
        GameObject objCasilla = GameObject.Find($"Casilla_{casillaLogica.CoordenadaX}_{casillaLogica.CoordenadaY}");

        if (objCasilla != null)
        {
            Transform transformCasilla = objCasilla.transform;

            // Creo la carta directamente en la casilla
            GameObject cartaIA = Instantiate(PrefabCarta, transformCasilla);

            cartaIA.transform.localPosition = Vector3.up * 0.05f;
            cartaIA.transform.localRotation = Quaternion.identity;
            cartaIA.transform.localScale = Vector3.one;

            VistaCarta vista = cartaIA.GetComponent<VistaCarta>();
            if (vista != null)
            {
                vista.Configurar(carta);
                // Quito el componente de interacción para que no se pueda tocar la carta de la IA
                Destroy(vista);
            }

            InfoCasilla info = objCasilla.GetComponent<InfoCasilla>();
            if (info != null) info.RecibirCarta(carta);
        }
    }

}