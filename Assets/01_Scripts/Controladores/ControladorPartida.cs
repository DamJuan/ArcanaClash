using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


//Esto lo que hace es crear la partida, la logica del tablero y pinta casillas
public class ControladorPartida : MonoBehaviour
    {

    public GameObject panelVictoria;
    public GameObject panelDerrota;

    public static ControladorPartida Instancia;

    public List<DatosCarta> bibliotecaVisuales;

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

    public Material materialHologramaIA;

    private bool esTurnoJugador = true;

    [Header("Ajustes de Espaciado")]
    public float separacionAncho = 2.0f;
    public float separacionLargo = 2.0f;
    public float huecoCentral = 0.5f;

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

        StartCoroutine(IniciarPartida());
    }

    public void ActualizarUI()
    {
        if (GestorUI.Instancia != null)
        {
            // Con esto le mando los datos del modeloJugador a la pantalla
            GestorUI.Instancia.ActualizarMana(jugador1.MagiaActual, jugador1.MagiaMaxima);

            GestorUI.Instancia.ActualizarVidas(jugador1.Vida, jugador2.Vida);
        }

        ComprobarEstadoPartida();
    }

    void ComprobarEstadoPartida()
    {
        if (jugador2.Vida <= 0)
        {
            MostrarVictoria();
        }
        else if (jugador1.Vida <= 0)
        {
            MostrarDerrota();
        }
    }

    public void MostrarVictoria()
    {
        panelVictoria.SetActive(true);
        Time.timeScale = 0f;
    }

    public void MostrarDerrota()
    {
        panelDerrota.SetActive(true);
        Time.timeScale = 0f;
    }

    public void VolverAJugar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator IniciarPartida()
    {

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

            for (int i = 0; i < 5; i++)
            {
                RobarCartaJugador();
                yield return new WaitForSeconds(0.2f); // Con esto consigo un efecto visual de robo de cartas
            }
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
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                ModeloCasilla modelo = tableroLogico.ObtenerCasilla(x, y);

                float posX = x * separacionAncho;
                float posZ = y * separacionLargo;

                if (y >= 2)
                {
                    posZ += huecoCentral;
                }

                Vector3 posicionLocal = new Vector3(posX, 0, posZ);

                GameObject nuevaCasilla = Instantiate(PrefabCasilla, ContenedorTablero, false);
                nuevaCasilla.transform.localPosition = posicionLocal;

                nuevaCasilla.name = $"Casilla_{x}_{y}";

                InfoCasilla vistaCasilla = nuevaCasilla.GetComponent<InfoCasilla>();
                if (vistaCasilla != null)
                {
                    vistaCasilla.CoordenadaX = x;
                    vistaCasilla.CoordenadaY = y;
                    vistaCasilla.Configurar(modelo);
                    vistaCasilla.EsTerritorioAliado = (y < 2);
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

        ResolverFaseCombate(jugador1, jugador2, true);

        if (jugador2.Vida <= 0) return;

        esTurnoJugador = false;

        if (GestorUI.Instancia != null) GestorUI.Instancia.ActivarBotonTurno(false);
        StartCoroutine(TurnoIA());
    }

    IEnumerator TurnoIA()
    {
        yield return new WaitForSeconds(Random.Range(1.0f, 2.0f));
        DespertarCriaturasIA();

        jugador2.SubirManaMaximo();
        jugador2.RestaurarMagia();
        jugador2.ReiniciarTurno();
        jugador2.RobarCarta();

        ResolverFaseCombate(jugador2, jugador1, false);
        yield return new WaitForSeconds(0.8f);

        List<ModeloCriatura> cartasEnMano = new List<ModeloCriatura>(jugador2.Mano);
        cartasEnMano.Sort((a, b) => b.Ataque.CompareTo(a.Ataque));

        foreach (ModeloCriatura cartaCandidata in cartasEnMano)
        {
            if (jugador2.MagiaActual >= cartaCandidata.CosteMagia && jugador2.PuedeJugarCarta())
            {
                ModeloCasilla sitio = EvaluarMejorSitioIA();

                if (sitio != null)
                {
                    sitio.ColocarCriatura(cartaCandidata);
                    JugarCartaIA_Visual(cartaCandidata, sitio);

                    jugador2.GastarMagia(cartaCandidata.CosteMagia);
                    jugador2.RegistrarJugada();
                    jugador2.EliminarCartaDeMano(cartaCandidata);

                    yield return new WaitForSeconds(Random.Range(0.6f, 1.2f));
                }
            }
        }

        ComenzarTurnoJugador();
    }
    ModeloCasilla EvaluarMejorSitioIA()
    {
        for (int x = 0; x < 4; x++)
        {
            bool columnaAmenazada = tableroLogico.ObtenerCasilla(x, 0).EstaOcupada || tableroLogico.ObtenerCasilla(x, 1).EstaOcupada;

            if (columnaAmenazada)
            {
                ModeloCasilla casillaDefensa = tableroLogico.ObtenerCasilla(x, 2);
                if (!casillaDefensa.EstaOcupada) return casillaDefensa;

                ModeloCasilla casillaAtras = tableroLogico.ObtenerCasilla(x, 3);
                if (!casillaAtras.EstaOcupada) return casillaAtras;
            }
        }

        for (int x = 0; x < 4; x++)
        {
            bool columnaLibre = !tableroLogico.ObtenerCasilla(x, 0).EstaOcupada && !tableroLogico.ObtenerCasilla(x, 1).EstaOcupada;
            if (columnaLibre)
            {
                ModeloCasilla casillaAtaque = tableroLogico.ObtenerCasilla(x, 2);
                if (!casillaAtaque.EstaOcupada) return casillaAtaque;
            }
        }

        List<ModeloCasilla> vacias = new List<ModeloCasilla>();
        for (int y = 2; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                ModeloCasilla c = tableroLogico.ObtenerCasilla(x, y);
                if (!c.EstaOcupada) vacias.Add(c);
            }
        }

        if (vacias.Count > 0) return vacias[Random.Range(0, vacias.Count)];

        return null;
    }

    void JugarCartaIA_Visual(ModeloCriatura carta, ModeloCasilla casillaLogica)
    {
        GameObject objCasilla = GameObject.Find($"Casilla_{casillaLogica.CoordenadaX}_{casillaLogica.CoordenadaY}");

        if (objCasilla != null)
        {
            GameObject cartaIA = Instantiate(PrefabCarta, objCasilla.transform);

            cartaIA.transform.localScale = new Vector3(0.014f, 0.014f, 0.014f);
            cartaIA.transform.localPosition = new Vector3(0, 0.1f, 0);
            cartaIA.transform.localRotation = Quaternion.Euler(90, 0, 0);

            VistaCarta vista = cartaIA.GetComponent<VistaCarta>();
            if (vista != null)
            {
                vista.Configurar(carta);
            }

            CartaEnTablero scriptTablero = cartaIA.GetComponent<CartaEnTablero>();
            if (scriptTablero != null)
            {
                scriptTablero.ConfigurarCarta(carta, false);
            }

            casillaLogica.AsignarCriatura(carta);

            InfoCasilla info = objCasilla.GetComponent<InfoCasilla>();
            if (info != null) info.RecibirCarta(carta);
        }
    }

    void ComenzarTurnoJugador()
    {

        DespertarCriaturasAliadas();

        jugador1.SubirManaMaximo();
        jugador1.RestaurarMagia();
        jugador1.ReiniciarTurno();

        RobarCartaJugador();
        ActualizarUI();

        esTurnoJugador = true;
        if (GestorUI.Instancia != null) GestorUI.Instancia.ActivarBotonTurno(true);
    }
    void DespertarCriaturasAliadas()
    {
        foreach (Transform hijoCasilla in ContenedorTablero)
        {

            VistaCriatura criatura = hijoCasilla.GetComponentInChildren<VistaCriatura>();

            InfoCasilla infoCasilla = hijoCasilla.GetComponent<InfoCasilla>();

            if (criatura != null && infoCasilla != null && infoCasilla.EsTerritorioAliado)
            {
                criatura.Despertar();
            }
        }
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 2; y++)
            { // Mis filas
                ModeloCasilla c = tableroLogico.ObtenerCasilla(x, y);
                if (c.EstaOcupada) c.CriaturaEnCasilla.PuedeAtacar = true;
            }
        }
    }


    void DespertarCriaturasIA()
    {
        foreach (Transform hijoCasilla in ContenedorTablero)
        {
            InfoCasilla infoCasilla = hijoCasilla.GetComponent<InfoCasilla>();
            VistaCriatura criatura = hijoCasilla.GetComponentInChildren<VistaCriatura>();

            if (criatura != null && infoCasilla != null && !infoCasilla.EsTerritorioAliado)
            {
                criatura.Despertar();
                criatura.PonerEnReposo(false);

                ModeloCasilla modelo = tableroLogico.ObtenerCasilla(infoCasilla.CoordenadaX, infoCasilla.CoordenadaY);
                if (modelo.EstaOcupada)
                {
                    modelo.CriaturaEnCasilla.PuedeAtacar = true;
                }
            }
        }
    }

    public void MatarCriatura(ModeloCriatura criatura, ModeloCasilla casilla)
    {

        casilla.LimpiarCasilla();

        GameObject objCasilla = GameObject.Find($"Casilla_{casilla.CoordenadaX}_{casilla.CoordenadaY}");
        if (objCasilla != null)
        {

            VistaCriatura vista = objCasilla.GetComponentInChildren<VistaCriatura>();
            if (vista != null)
            {
                Destroy(vista.gameObject);
            }
        }
    }

    void ResolverFaseCombate(ModeloJugador atacante, ModeloJugador defensor, bool esTurnoJugador)
    {
        int filaInicio = esTurnoJugador ? 0 : 2;
        int filaFin = esTurnoJugador ? 2 : 4;
        int direccion = esTurnoJugador ? 1 : -1;

 
        for (int x = 0; x < 4; x++)
        {
            for (int y = filaInicio; y < filaFin; y++)
            {
                ModeloCasilla casillaAtacante = tableroLogico.ObtenerCasilla(x, y);

                if (casillaAtacante.EstaOcupada && casillaAtacante.CriaturaEnCasilla.PuedeAtacar)
                {
                    ModeloCriatura bicho = casillaAtacante.CriaturaEnCasilla;

                    ModeloCriatura enemigoEncontrado = null;
                    ModeloCasilla casillaEnemiga = null;

                    int yObjetivoStart = esTurnoJugador ? 2 : 1;
                    int yObjetivoEnd = esTurnoJugador ? 4 : -1;


                    if (esTurnoJugador)
                    {
                        for (int targetY = 2; targetY < 4; targetY++)
                        {
                            ModeloCasilla c = tableroLogico.ObtenerCasilla(x, targetY);
                            if (c.EstaOcupada) { enemigoEncontrado = c.CriaturaEnCasilla; casillaEnemiga = c; break; }
                        }
                    }
                    else
                    {
                        for (int targetY = 1; targetY >= 0; targetY--)
                        {
                            ModeloCasilla c = tableroLogico.ObtenerCasilla(x, targetY);
                            if (c.EstaOcupada) { enemigoEncontrado = c.CriaturaEnCasilla; casillaEnemiga = c; break; }
                        }
                    }

                    if (enemigoEncontrado != null)
                    {
                        enemigoEncontrado.RecibirDanio(bicho.Ataque);

                        GameObject objCasillaEnemiga = GameObject.Find($"Casilla_{casillaEnemiga.CoordenadaX}_{casillaEnemiga.CoordenadaY}");
                        if (objCasillaEnemiga) objCasillaEnemiga.GetComponentInChildren<VistaCriatura>()?.ActualizarVisuales();

                        if (enemigoEncontrado.VidaActual <= 0)
                        {
                            MatarCriatura(enemigoEncontrado, casillaEnemiga);
                        }
                    }
                    else
                    {
                        defensor.RecibirDanio(bicho.Ataque);
                        ActualizarUI();
                    }
                }
            }
        }
    }

    public bool EsTurnoDeJugador { get { return esTurnoJugador; } }
}