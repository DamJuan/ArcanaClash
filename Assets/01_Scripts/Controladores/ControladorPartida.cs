using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


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

    public Material materialHologramaIA;

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

                    vistaCasilla.CoordenadaX = x;
                    vistaCasilla.CoordenadaY = y;

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

        ResolverFaseCombate(jugador1, jugador2, true);

        if (jugador2.Vida <= 0) { Debug.Log("¡HAS GANADO!"); return; }

        esTurnoJugador = false;

        // Desactivo el boton de pasar turno hasta que sea el turno del jugador de nuevo
        if (GestorUI.Instancia != null) GestorUI.Instancia.ActivarBotonTurno(false);
        StartCoroutine(TurnoIA());

    }

    IEnumerator TurnoIA()
    {
        yield return new WaitForSeconds(1.0f); // Pausa para que se note el cambio de turno

        DespertarCriaturasIA();

        jugador2.SubirManaMaximo();
        jugador2.RestaurarMagia();
        jugador2.ReiniciarTurno();

        ModeloCriatura cartaRobada = jugador2.RobarCarta();

        ResolverFaseCombate(jugador2, jugador1, false);
        if (jugador1.Vida <= 0) { Debug.Log(" HAS PERDIDO..."); yield break; }

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

                    JugarCartaIA_Visual(cartaCandidata, sitio);

                    jugador2.GastarMagia(cartaCandidata.CosteMagia);
                    jugador2.RegistrarJugada();
                    jugador2.EliminarCartaDeMano(cartaCandidata);

                    yield return new WaitForSeconds(1.5f); // Pausa entre cartas para simular el movimiento y que no sea instantáneo
                }
            }
        }

        ComenzarTurnoJugador();
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

    void JugarCartaIA_Visual(ModeloCriatura carta, ModeloCasilla casillaLogica)
    {
        GameObject objCasilla = GameObject.Find($"Casilla_{casillaLogica.CoordenadaX}_{casillaLogica.CoordenadaY}");

        if (objCasilla != null)
        {
            Transform transformCasilla = objCasilla.transform;

            GameObject cartaIA = Instantiate(PrefabCarta, transformCasilla);

            cartaIA.transform.localPosition = Vector3.up * 0.7f;
            cartaIA.transform.localRotation = Quaternion.Euler(90, 0, 0);
            cartaIA.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            cartaIA.layer = LayerMask.NameToLayer("Default");

            if (AudioManager.Instancia != null)
            {
                AudioManager.Instancia.ReproducirSonido(AudioManager.Instancia.SonidoJugarCarta);
            }

            VistaCarta vista = cartaIA.GetComponent<VistaCarta>();
            if (vista != null)
            {
                vista.Configurar(carta);
                Destroy(vista);
            }

            VistaCriatura vistaCriatura = cartaIA.GetComponent<VistaCriatura>();
            if (vistaCriatura != null)
            {
                vistaCriatura.enabled = true;
                vistaCriatura.Inicializar(carta);

                if (materialHologramaIA != null)
                {
                    Renderer[] renderers = cartaIA.GetComponentsInChildren<Renderer>(true);
                    foreach (Renderer r in renderers)
                    {
                        if (r is MeshRenderer || r is SkinnedMeshRenderer)
                        {
                            r.material = materialHologramaIA;
                        }
                    }
                }

                vistaCriatura.PonerEnReposo(true);
            }

            Destroy(cartaIA.GetComponent<GraphicRaycaster>());
            if (cartaIA.GetComponent<CanvasGroup>() != null) Destroy(cartaIA.GetComponent<CanvasGroup>());

            BoxCollider col = cartaIA.GetComponent<BoxCollider>();
            if (col != null)
            {
                col.enabled = true;
                col.size = new Vector3(col.size.x, col.size.y, 10f);
            }

            InfoCasilla info = objCasilla.GetComponent<InfoCasilla>();
            if (info != null) info.RecibirCarta(carta);
        }
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
        // Defino qué filas son de quién
        int filaInicio = esTurnoJugador ? 0 : 2;
        int filaFin = esTurnoJugador ? 2 : 4;
        int direccion = esTurnoJugador ? 1 : -1;

 
        for (int x = 0; x < 4; x++)
        {
            for (int y = filaInicio; y < filaFin; y++)
            {
                ModeloCasilla casillaAtacante = tableroLogico.ObtenerCasilla(x, y);

                // Si hay bicho y está despierto
                if (casillaAtacante.EstaOcupada && casillaAtacante.CriaturaEnCasilla.PuedeAtacar)
                {
                    ModeloCriatura bicho = casillaAtacante.CriaturaEnCasilla;

                    // Busco objetivo para atacar
                    ModeloCriatura enemigoEncontrado = null;
                    ModeloCasilla casillaEnemiga = null;

                    // Miro las casillas que tiene delante en su misma columna
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

                    // --- RESOLVER EL GOLPE ---
                    if (enemigoEncontrado != null)
                    {
                        // Golpe a criatura
                        Debug.Log($"y golpea a {enemigoEncontrado.Nombre} por {bicho.Ataque} daño!");
                        enemigoEncontrado.RecibirDanio(bicho.Ataque);

                        // Actualizo visuales del enemigo (Vida)
                        GameObject objCasillaEnemiga = GameObject.Find($"Casilla_{casillaEnemiga.CoordenadaX}_{casillaEnemiga.CoordenadaY}");
                        if (objCasillaEnemiga) objCasillaEnemiga.GetComponentInChildren<VistaCriatura>()?.ActualizarVisuales();

                        if (enemigoEncontrado.VidaActual <= 0)
                        {
                            MatarCriatura(enemigoEncontrado, casillaEnemiga);
                        }
                    }
                    else
                    {
                        // Golpe directo al Jugador
                        Debug.Log($"y golpea DIRECTO al Rival por {bicho.Ataque} daño!");
                        defensor.RecibirDanio(bicho.Ataque);
                        ActualizarUI();
                    }
                }
            }
        }
    }

    public bool EsTurnoDeJugador { get { return esTurnoJugador; } }




}