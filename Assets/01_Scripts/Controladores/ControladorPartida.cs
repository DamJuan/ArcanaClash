using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ControladorPartida : MonoBehaviour
{
    public GameObject panelVictoria;
    public GameObject panelDerrota;

    public static ControladorPartida Instancia;

    public List<DatosCarta> bibliotecaVisuales;

    public GameObject PrefabCasilla;
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

    [Header("Prefabs de Efectos")]
    public GameObject prefabTextoDanio;
    public GameObject prefabTextoCuracion;

    private Dictionary<Vector2Int, InfoCasilla> cacheCasillas;
    private Dictionary<ModeloCriatura, Vector2Int> cacheCriaturas;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);

        cacheCasillas = new Dictionary<Vector2Int, InfoCasilla>();
        cacheCriaturas = new Dictionary<ModeloCriatura, Vector2Int>();
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
                ModeloCriatura cartaIA = jugador2.RobarCarta();
                if (cartaIA != null) cartaIA.EsDelJugador = false;
            }

            for (int i = 0; i < 5; i++)
            {
                RobarCartaJugador();
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    void RobarCartaJugador()
    {
        ModeloCriatura cartaRobada = jugador1.RobarCarta();

        if (cartaRobada != null)
        {
            cartaRobada.EsDelJugador = true;
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

                    cacheCasillas[new Vector2Int(x, y)] = vistaCasilla;
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

        StartCoroutine(SecuenciaTurnoJugador());
    }

    IEnumerator SecuenciaTurnoJugador()
    {
        EjecutarHabilidadesEnTurno(HabilidadCarta.MomentoEjecucion.InicioTurno, true);

        yield return StartCoroutine(ResolverFaseCombate(jugador1, jugador2, true));

        if (jugador2.Vida <= 0) yield break;

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

        ModeloCriatura cartaRobada = jugador2.RobarCarta();
        if (cartaRobada != null) cartaRobada.EsDelJugador = false;

        EjecutarHabilidadesEnTurno(HabilidadCarta.MomentoEjecucion.InicioTurno, false);

        yield return StartCoroutine(ResolverFaseCombate(jugador2, jugador1, false));
        yield return new WaitForSeconds(0.8f);

        yield return StartCoroutine(TurnoIAMejorado());

        ComenzarTurnoJugador();
    }

    IEnumerator TurnoIAMejorado()
    {
        List<ModeloCriatura> cartasEnMano = new List<ModeloCriatura>(jugador2.Mano);

        Dictionary<int, int> amenazasPorColumna = EvaluarAmenazas(false);

        cartasEnMano.Sort((a, b) => {
            int prioridadA = CalcularPrioridadCarta(a, amenazasPorColumna);
            int prioridadB = CalcularPrioridadCarta(b, amenazasPorColumna);
            return prioridadB.CompareTo(prioridadA);
        });

        foreach (ModeloCriatura cartaCandidata in cartasEnMano)
        {
            if (jugador2.MagiaActual >= cartaCandidata.CosteMagia && jugador2.PuedeJugarCarta())
            {
                ModeloCasilla sitio = EvaluarMejorSitioIAMejorada(cartaCandidata, amenazasPorColumna);

                if (sitio != null)
                {
                    sitio.ColocarCriatura(cartaCandidata);
                    JugarCartaIA_Visual(cartaCandidata, sitio);

                    jugador2.GastarMagia(cartaCandidata.CosteMagia);
                    jugador2.RegistrarJugada();
                    jugador2.EliminarCartaDeMano(cartaCandidata);

                    cacheCriaturas[cartaCandidata] = new Vector2Int(sitio.CoordenadaX, sitio.CoordenadaY);

                    cartaCandidata.EjecutarHabilidades(HabilidadCarta.MomentoEjecucion.AlJugar, this);

                    yield return new WaitForSeconds(Random.Range(0.6f, 1.2f));
                }
            }
        }
    }

    private Dictionary<int, int> EvaluarAmenazas(bool paraJugador)
    {
        Dictionary<int, int> amenazas = new Dictionary<int, int>();

        int filaInicio = paraJugador ? 2 : 0;
        int filaFin = paraJugador ? 4 : 2;

        for (int x = 0; x < 4; x++)
        {
            int amenazaTotal = 0;
            for (int y = filaInicio; y < filaFin; y++)
            {
                ModeloCasilla casilla = tableroLogico.ObtenerCasilla(x, y);
                if (casilla.EstaOcupada)
                {
                    ModeloCriatura criatura = casilla.CriaturaEnCasilla;
                    amenazaTotal += criatura.Ataque * criatura.VidaActual;
                }
            }
            amenazas[x] = amenazaTotal;
        }

        return amenazas;
    }

    private int CalcularPrioridadCarta(ModeloCriatura carta, Dictionary<int, int> amenazas)
    {
        int prioridad = 0;

        int amenazaTotal = 0;
        foreach (int valor in amenazas.Values)
        {
            amenazaTotal += valor;
        }

        if (amenazaTotal > 4)
        {
            prioridad += carta.VidaMaxima * 2;
        }
        else
        {
            prioridad += carta.Ataque * 3;
        }

        if (jugador2.MagiaMaxima <= 4)
        {
            prioridad += (5 - carta.CosteMagia) * 5;
        }

        return prioridad;
    }

    ModeloCasilla EvaluarMejorSitioIAMejorada(ModeloCriatura carta, Dictionary<int, int> amenazas)
    {
        List<CasillaPuntuada> casillasConPuntuacion = new List<CasillaPuntuada>();

        for (int x = 0; x < 4; x++)
        {
            for (int y = 2; y < 4; y++)
            {
                ModeloCasilla casilla = tableroLogico.ObtenerCasilla(x, y);
                if (!casilla.EstaOcupada)
                {
                    int puntuacion = EvaluarPosicion(casilla, carta, amenazas);
                    casillasConPuntuacion.Add(new CasillaPuntuada(casilla, puntuacion));
                }
            }
        }

        if (casillasConPuntuacion.Count == 0) return null;

        casillasConPuntuacion.Sort((a, b) => b.puntuacion.CompareTo(a.puntuacion));
        return casillasConPuntuacion[0].casilla;
    }

    private class CasillaPuntuada
    {
        public ModeloCasilla casilla;
        public int puntuacion;

        public CasillaPuntuada(ModeloCasilla c, int p)
        {
            casilla = c;
            puntuacion = p;
        }
    }

    private int EvaluarPosicion(ModeloCasilla casilla, ModeloCriatura carta, Dictionary<int, int> amenazas)
    {
        int puntuacion = 0;
        int x = casilla.CoordenadaX;
        int y = casilla.CoordenadaY;

        if (carta.VidaMaxima >= 6 || carta.Nombre == "Muro" || carta.Nombre == "Guardia")
        {
            if (y == 2) puntuacion += 30;
            else puntuacion -= 10;
        }
        else if (carta.VidaMaxima <= 3)
        {
            if (y == 3) puntuacion += 30;
            else puntuacion -= 10;
        }


        ModeloCasilla casillaEnemigaFrente = tableroLogico.ObtenerCasilla(x, 1);
        ModeloCasilla casillaEnemigaAtras = tableroLogico.ObtenerCasilla(x, 0);

        ModeloCriatura enemigo = null;
        if (casillaEnemigaFrente.EstaOcupada) enemigo = casillaEnemigaFrente.CriaturaEnCasilla;
        else if (casillaEnemigaAtras.EstaOcupada) enemigo = casillaEnemigaAtras.CriaturaEnCasilla;

        if (enemigo != null)
        {
            if (carta.Ataque >= enemigo.VidaActual)
            {
                if (y == 2) puntuacion += 100;
            }

            if (enemigo.Ataque >= carta.VidaActual)
            {
                puntuacion -= 40;
            }

            if (carta.Nombre == "Muro" && enemigo.Ataque > 3 && y == 2)
            {
                puntuacion += 50;
            }
        }
        else
        {
            if (carta.Ataque >= 5) puntuacion += 20;
        }

        bool hayAliadosEnColumna = false;
        for (int checkY = 2; checkY < 4; checkY++)
        {
            if (checkY != y && tableroLogico.ObtenerCasilla(x, checkY).EstaOcupada)
            {
                hayAliadosEnColumna = true;
                break;
            }
        }

        if (!hayAliadosEnColumna) puntuacion += 15;

        return puntuacion;
    }

    ModeloCasilla EvaluarMejorSitioIA()
    {
        for (int x = 0; x < 4; x++)
        {
            bool columnaAmenazada = tableroLogico.ObtenerCasilla(x, 0).EstaOcupada ||
                                    tableroLogico.ObtenerCasilla(x, 1).EstaOcupada;

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
            bool columnaLibre = !tableroLogico.ObtenerCasilla(x, 0).EstaOcupada &&
                                !tableroLogico.ObtenerCasilla(x, 1).EstaOcupada;
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
        InfoCasilla infoCasilla = ObtenerInfoCasilla(casillaLogica.CoordenadaX, casillaLogica.CoordenadaY);

        if (infoCasilla != null)
        {
            GameObject cartaIA = Instantiate(PrefabCarta, infoCasilla.transform);

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

            if (infoCasilla != null) infoCasilla.RecibirCarta(carta);
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
            {
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
        criatura.EjecutarHabilidades(HabilidadCarta.MomentoEjecucion.AlMorir, this);

        casilla.LimpiarCasilla();
        InfoCasilla infoCasilla = ObtenerInfoCasilla(casilla.CoordenadaX, casilla.CoordenadaY);

        if (infoCasilla != null)
        {
            VistaCriatura vista = infoCasilla.GetComponentInChildren<VistaCriatura>();
            if (vista != null)
            {
                vista.AnimacionMuerte();
            }
        }

        if (cacheCriaturas.ContainsKey(criatura))
        {
            cacheCriaturas.Remove(criatura);
        }
    }

    IEnumerator ResolverFaseCombate(ModeloJugador atacante, ModeloJugador defensor, bool esTurnoJugador)
    {
        int filaInicio = esTurnoJugador ? 0 : 2;
        int filaFin = esTurnoJugador ? 2 : 4;

        for (int x = 0; x < 4; x++)
        {
            for (int y = filaInicio; y < filaFin; y++)
            {
                ModeloCasilla casillaAtacante = tableroLogico.ObtenerCasilla(x, y);

                if (casillaAtacante.EstaOcupada && casillaAtacante.CriaturaEnCasilla.VidaActual <= 0)
                {
                    MatarCriatura(casillaAtacante.CriaturaEnCasilla, casillaAtacante);
                    continue;
                }

                if (casillaAtacante.EstaOcupada && casillaAtacante.CriaturaEnCasilla.PuedeAtacar)
                {
                    ModeloCriatura atacanteCreatura = casillaAtacante.CriaturaEnCasilla;
                    ModeloCriatura enemigoEncontrado = null;
                    ModeloCasilla casillaEnemiga = null;

                    if (esTurnoJugador)
                    {
                        for (int targetY = 2; targetY < 4; targetY++)
                        {
                            ModeloCasilla c = tableroLogico.ObtenerCasilla(x, targetY);
                            if (c.EstaOcupada)
                            {
                                enemigoEncontrado = c.CriaturaEnCasilla;
                                casillaEnemiga = c;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int targetY = 1; targetY >= 0; targetY--)
                        {
                            ModeloCasilla c = tableroLogico.ObtenerCasilla(x, targetY);
                            if (c.EstaOcupada)
                            {
                                enemigoEncontrado = c.CriaturaEnCasilla;
                                casillaEnemiga = c;
                                break;
                            }
                        }
                    }

                    if (enemigoEncontrado != null)
                    {
                        atacanteCreatura.EjecutarHabilidades(HabilidadCarta.MomentoEjecucion.AlAtacar, this);

                        int danioTotal = atacanteCreatura.Ataque + atacanteCreatura.ObtenerBonusAtaque(enemigoEncontrado);

                        enemigoEncontrado.RecibirDanio(danioTotal);

                        MostrarTextoDanio(casillaEnemiga, danioTotal);

                        InfoCasilla infoCasillaEnemiga = ObtenerInfoCasilla(casillaEnemiga.CoordenadaX, casillaEnemiga.CoordenadaY);

                        if (infoCasillaEnemiga != null)
                        {
                            VistaCriatura vistaEnemiga = infoCasillaEnemiga.GetComponentInChildren<VistaCriatura>();
                            if (vistaEnemiga != null)
                            {
                                vistaEnemiga.ActualizarVisuales();
                                vistaEnemiga.AnimacionRecibirDano(danioTotal);
                            }
                        }

                        if (enemigoEncontrado.VidaActual <= 0)
                        {
                            MatarCriatura(enemigoEncontrado, casillaEnemiga);
                        }

                        yield return new WaitForSeconds(0.4f);
                    }
                    else
                    {
                        defensor.RecibirDanio(atacanteCreatura.Ataque);
                        ActualizarUI();

                        yield return new WaitForSeconds(0.4f);
                    }
                }
            }
        }
    }

    public ModeloCasilla ObtenerCasillaDeCriatura(ModeloCriatura criatura)
    {
        if (cacheCriaturas.ContainsKey(criatura))
        {
            Vector2Int pos = cacheCriaturas[criatura];
            return tableroLogico.ObtenerCasilla(pos.x, pos.y);
        }

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                ModeloCasilla casilla = tableroLogico.ObtenerCasilla(x, y);
                if (casilla.EstaOcupada && casilla.CriaturaEnCasilla == criatura)
                {
                    cacheCriaturas[criatura] = new Vector2Int(x, y);
                    return casilla;
                }
            }
        }

        return null;
    }

    public List<ModeloCasilla> ObtenerCasillasAdyacentes(ModeloCasilla casilla)
    {
        List<ModeloCasilla> adyacentes = new List<ModeloCasilla>();
        int x = casilla.CoordenadaX;
        int y = casilla.CoordenadaY;

        if (y + 1 < 4) adyacentes.Add(tableroLogico.ObtenerCasilla(x, y + 1));
        if (y - 1 >= 0) adyacentes.Add(tableroLogico.ObtenerCasilla(x, y - 1));
        if (x - 1 >= 0) adyacentes.Add(tableroLogico.ObtenerCasilla(x - 1, y));
        if (x + 1 < 4) adyacentes.Add(tableroLogico.ObtenerCasilla(x + 1, y));

        return adyacentes;
    }

    public void InvocarCriatura(ModeloCriatura criatura, ModeloCasilla casilla, bool esDelJugador)
    {
        criatura.EsDelJugador = esDelJugador;
        casilla.ColocarCriatura(criatura);

        InfoCasilla infoCasilla = ObtenerInfoCasilla(casilla.CoordenadaX, casilla.CoordenadaY);
        if (infoCasilla != null)
        {
            GameObject nuevaCarta = Instantiate(PrefabCarta, infoCasilla.transform);
            nuevaCarta.transform.localScale = new Vector3(0.014f, 0.014f, 0.014f);
            nuevaCarta.transform.localPosition = new Vector3(0, 0.1f, 0);
            nuevaCarta.transform.localRotation = Quaternion.Euler(90, 0, 0);

            VistaCarta vista = nuevaCarta.GetComponent<VistaCarta>();
            if (vista != null) vista.Configurar(criatura);

            CartaEnTablero scriptTablero = nuevaCarta.GetComponent<CartaEnTablero>();
            if (scriptTablero != null) scriptTablero.ConfigurarCarta(criatura, esDelJugador);

            infoCasilla.RecibirCarta(criatura);
            cacheCriaturas[criatura] = new Vector2Int(casilla.CoordenadaX, casilla.CoordenadaY);
        }
    }

    public void MostrarTextoCuracion(ModeloCasilla casilla, int cantidad)
    {
        if (prefabTextoCuracion == null) return;

        InfoCasilla infoCasilla = ObtenerInfoCasilla(casilla.CoordenadaX, casilla.CoordenadaY);
        if (infoCasilla != null)
        {
            GameObject texto = Instantiate(prefabTextoCuracion, infoCasilla.transform);
            texto.transform.localPosition = new Vector3(0, 2.5f, 0);
            TextoFlotante script = texto.GetComponent<TextoFlotante>();
            if (script != null)
            {
                script.ConfigurarTexto($"+{cantidad}", Color.green);
            }
        }
    }

    public void MostrarTextoDanio(ModeloCasilla casilla, int cantidad)
    {
        if (prefabTextoDanio == null) return;

        InfoCasilla infoCasilla = ObtenerInfoCasilla(casilla.CoordenadaX, casilla.CoordenadaY);
        if (infoCasilla != null)
        {
            GameObject texto = Instantiate(prefabTextoDanio, infoCasilla.transform);
            texto.transform.localPosition = new Vector3(0, 2.5f, 0);
            TextoFlotante script = texto.GetComponent<TextoFlotante>();
            if (script != null)
            {
                script.ConfigurarTexto($"-{cantidad}", Color.red);
            }
        }
    }

    public void RobarMana(bool elJugadorRoba, int cantidad)
    {
        if (elJugadorRoba)
        {
            jugador1.RobarMana(cantidad);
            if (jugador2.MagiaActual >= cantidad) jugador2.GastarMagia(cantidad);
        }
        else
        {
            jugador2.RobarMana(cantidad);
            if (jugador1.MagiaActual >= cantidad) jugador1.GastarMagia(cantidad);
        }

        ActualizarUI();
    }

    private void EjecutarHabilidadesEnTurno(HabilidadCarta.MomentoEjecucion momento, bool turnoJugador)
    {
        int filaInicio = turnoJugador ? 0 : 2;
        int filaFin = turnoJugador ? 2 : 4;

        for (int x = 0; x < 4; x++)
        {
            for (int y = filaInicio; y < filaFin; y++)
            {
                ModeloCasilla casilla = tableroLogico.ObtenerCasilla(x, y);
                if (casilla.EstaOcupada) casilla.CriaturaEnCasilla.EjecutarHabilidades(momento, this);
            }
        }
    }

    private InfoCasilla ObtenerInfoCasilla(int x, int y)
    {
        Vector2Int coordenadas = new Vector2Int(x, y);
        if (cacheCasillas.ContainsKey(coordenadas)) return cacheCasillas[coordenadas];
        return null;
    }

    public bool EsTurnoDeJugador { get { return esTurnoJugador; } }
}