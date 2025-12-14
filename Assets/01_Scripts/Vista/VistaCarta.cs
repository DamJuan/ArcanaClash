using UnityEngine;
using UnityEngine.EventSystems; 
using TMPro;
using System.Collections;
using UnityEngine.UI;


//Esto pinta el nombre, el coste y la descripción en el objeto 3D.
public class VistaCarta : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerClickHandler
{

    public TextMeshProUGUI NameText;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI LifeText;
    public TextMeshProUGUI DescriptionText;
    public RawImage ImagenArte;
    public RawImage FondoCarta;

    public static VistaCarta cartaEnZoom = null;

    private bool enZoom = false;
    private bool animando = false;
    private Transform puntoZoom;
    private Transform padreOriginal;
    private Vector3 posOriginalLocal;
    private Quaternion rotOriginalLocal;
    private Vector3 escalaOriginalLocal;
    private int siblingIndexOriginal;

    private ModeloCarta miModelo;
    private Vector3 posicionOriginal;
    private Camera camaraPrincipal;
    private bool arrastrando = false;
    private Collider miCollider;
    private CanvasGroup miCanvasGroup;

    public float AlturaAlColocar = 0.05f;
    public float EscalaEnTablero = 0.006f;


    void Start()
    {
        camaraPrincipal = Camera.main;
        miCollider = GetComponent<Collider>();
        miCanvasGroup = GetComponent<CanvasGroup>();

        GameObject objZoom = GameObject.Find("PuntoZoom");
        if (objZoom != null) puntoZoom = objZoom.transform;
    }

    public void Configurar(ModeloCarta modelo, Texture imagen = null)
    {
        this.miModelo = modelo;

        // Actualizo los textos visuales con los datos del modelo
        if (NameText != null) NameText.text = modelo.Nombre;
        if (CostText != null) CostText.text = modelo.CosteMagia.ToString();
        if (modelo is ModeloCriatura criatura)
        {
            if (LifeText != null) LifeText.text = criatura.VidaMaxima.ToString();
            if (DescriptionText != null) DescriptionText.text = modelo.Nombre + ": " + criatura.Ataque + " ATK";
        }

        string ruta = "Imagenes/" + modelo.Nombre;

        Texture imagenCargada = Resources.Load<Texture>(ruta);

            if (ImagenArte != null)
            {
                ImagenArte.texture = imagenCargada;
                ImagenArte.color = Color.white;
            }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!arrastrando && !animando && puntoZoom != null)
        {
            if (!enZoom)
            {
                if (cartaEnZoom != null && cartaEnZoom != this)
                {
                    cartaEnZoom.DesactivarZoom();
                }

                ActivarZoom();
            }
            else
            {
                DesactivarZoom();
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        posicionOriginal = transform.position;

        if (ControladorPartida.Instancia != null && !ControladorPartida.Instancia.EsTurnoDeJugador)
        {
            Debug.Log("¡Espera a tu turno!");
            return;
        }

        if (enZoom) DesactivarZoom();

        arrastrando = true;

        if (miCollider != null) miCollider.enabled = false;
        if (miCanvasGroup != null) miCanvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (arrastrando)
        {
            MoverCartaConRaton(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (ControladorPartida.Instancia != null && !ControladorPartida.Instancia.EsTurnoDeJugador)
        {
            transform.position = posicionOriginal;
            if (miCanvasGroup != null)
            {
                miCanvasGroup.alpha = 1f;
                miCanvasGroup.blocksRaycasts = true;
            }

            if (miCollider != null) miCollider.enabled = true;

            return;
        }

        arrastrando = false;

        Ray rayo = camaraPrincipal.ScreenPointToRay(eventData.position);
        RaycastHit golpe;

        int capaTablero = 1 << LayerMask.NameToLayer("Tablero");

        if (Physics.Raycast(rayo, out golpe, Mathf.Infinity, capaTablero))
        {
            InfoCasilla casillaDetectada = golpe.collider.GetComponentInParent<InfoCasilla>();

            ModeloJugador jugador = ControladorPartida.Instancia.JugadorLocal;


            bool esMiTerritorio = casillaDetectada != null && casillaDetectada.EsTerritorioAliado;
            bool estaVacia = casillaDetectada != null && !casillaDetectada.EstaOcupada;
            bool tengoJugadas = jugador.PuedeJugarCarta();
            bool tengoMana = jugador.MagiaActual >= miModelo.CosteMagia;


            if (esMiTerritorio && estaVacia && tengoJugadas && tengoMana)
            {
                JugarCartaEnCasilla(casillaDetectada);

                jugador.GastarMagia(miModelo.CosteMagia);
                jugador.RegistrarJugada();
                ControladorPartida.Instancia.ActualizarUI();

                if (AudioManager.Instancia != null)
                {
                    AudioManager.Instancia.ReproducirSonido(AudioManager.Instancia.SonidoJugarCarta);
                }

                if (miCollider != null) miCollider.enabled = true;
                return;
            }
            else
            {
                // Mensajes de error para saber qué falla
                if (!tengoMana) Debug.Log("¡No tienes suficiente maná!");
                else if (!esMiTerritorio) Debug.Log("Territorio enemigo.");
                else if (!estaVacia) Debug.Log("Casilla ocupada.");
                else if (!tengoJugadas) Debug.Log("Ya has jugado 4 cartas.");
            }
        }

        transform.position = posicionOriginal;
        if (miCanvasGroup != null)
        {
            miCanvasGroup.alpha = 1f;
            miCanvasGroup.blocksRaycasts = true;
        }
        if (miCollider != null) miCollider.enabled = true;
    }

    void ActivarZoom()
    {
        padreOriginal = transform.parent;
        posOriginalLocal = transform.localPosition;
        rotOriginalLocal = transform.localRotation;
        escalaOriginalLocal = transform.localScale;
        siblingIndexOriginal = transform.GetSiblingIndex();

        cartaEnZoom = this;

        transform.SetParent(puntoZoom);

        StartCoroutine(AnimarViaje(Vector3.zero, Quaternion.identity, Vector3.one * 3.0f));
        enZoom = true;
    }

    void DesactivarZoom()
    {

        if (cartaEnZoom == this) cartaEnZoom = null;

        transform.SetParent(padreOriginal);
        transform.SetSiblingIndex(siblingIndexOriginal);

        StartCoroutine(AnimarViaje(posOriginalLocal, rotOriginalLocal, escalaOriginalLocal));

        if (GestorUI.Instancia != null) GestorUI.Instancia.CerrarPanel();

        enZoom = false;
    }

    IEnumerator AnimarViaje(Vector3 posDestino, Quaternion rotDestino, Vector3 escalaDestino)
    {
        animando = true;
        float tiempo = 0;
        float duracion = 0.3f;

        Vector3 posInicial = transform.localPosition;
        Quaternion rotInicial = transform.localRotation;
        Vector3 escalaInicial = transform.localScale;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;

            t = Mathf.SmoothStep(0, 1, t);

            transform.localPosition = Vector3.Lerp(posInicial, posDestino, t);
            transform.localRotation = Quaternion.Lerp(rotInicial, rotDestino, t);
            transform.localScale = Vector3.Lerp(escalaInicial, escalaDestino, t);

            yield return null;
        }

        transform.localPosition = posDestino;
        transform.localRotation = rotDestino;
        transform.localScale = escalaDestino;

        animando = false;
    }

    private void JugarCartaEnCasilla(InfoCasilla casilla)
    {
        transform.position = casilla.transform.position + Vector3.up * AlturaAlColocar;
        transform.SetParent(casilla.transform);
        transform.localRotation = Quaternion.Euler(90, 0, 0);
        transform.localScale = new Vector3(EscalaEnTablero, EscalaEnTablero, EscalaEnTablero);

        gameObject.layer = LayerMask.NameToLayer("Default");

        if (miModelo is ModeloCriatura criatura)
        {
            casilla.RecibirCarta(criatura);
            ControladorPartida.Instancia.JugadorLocal.EliminarCartaDeMano(criatura);
            VistaCriatura scriptMesa = GetComponent<VistaCriatura>();
            if (scriptMesa != null)
            {
                scriptMesa.enabled = true;
                scriptMesa.Inicializar(criatura);
            }
        }

        Destroy(GetComponent<GraphicRaycaster>());
        if (GetComponent<CanvasGroup>() != null) Destroy(GetComponent<CanvasGroup>());
        Destroy(this);
    }

    private void MoverCartaConRaton(Vector2 posicionPantalla)
    {
        //Esto crea plano invisible a la altura de la carta
        Plane planoMovimiento = new Plane(Vector3.up, transform.position);
        Ray rayo = camaraPrincipal.ScreenPointToRay(posicionPantalla);

        float distancia;
        if (planoMovimiento.Raycast(rayo, out distancia))
        {
            Vector3 nuevoPunto = rayo.GetPoint(distancia);
            transform.position = new Vector3(nuevoPunto.x, posicionOriginal.y, nuevoPunto.z);
        }
    }

}