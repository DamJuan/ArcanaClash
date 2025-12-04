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

    void Start()
    {
        camaraPrincipal = Camera.main;
        miCollider = GetComponent<Collider>();
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

        Debug.Log($"Buscando imagen en: '{ruta}'");

        Texture imagenCargada = Resources.Load<Texture>(ruta);

        if (imagenCargada == null)
        {
            Debug.LogError($" FALLO: No existe el archivo en 'Assets/Resources/{ruta}'");
        }
        else
        {
            Debug.Log($" ÉXITO: Imagen encontrada. Asignando a RawImage...");
            if (ImagenArte != null)
            {
                ImagenArte.texture = imagenCargada;
                ImagenArte.color = Color.white;
            }
            else Debug.LogError(" FALLO: La variable 'ImagenArte' está vacía en el Prefab.");
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
        if (enZoom) DesactivarZoom();

        arrastrando = true;
        posicionOriginal = transform.position;

        if (miCollider != null) miCollider.enabled = false;
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
        arrastrando = false;
        Ray rayo = camaraPrincipal.ScreenPointToRay(eventData.position);
        RaycastHit golpe;

        if (Physics.Raycast(rayo, out golpe))
        {
            InfoCasilla casillaDetectada = golpe.collider.GetComponent<InfoCasilla>();
            bool quedanJugadas = ControladorPartida.Instancia.JugadorLocal.PuedeJugarCarta();

            if (casillaDetectada != null && !casillaDetectada.EstaOcupada && casillaDetectada.EsTerritorioAliado && quedanJugadas)
            {
                JugarCartaEnCasilla(casillaDetectada);
                ControladorPartida.Instancia.JugadorLocal.RegistrarJugada();
                if (miCollider != null) miCollider.enabled = true;
                return;
            }
        }

        transform.position = posicionOriginal;
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
        Debug.Log("¡Carta jugada en la casilla: " + casilla.name + "!");

        // Muevo la carta visualmente encima de la casilla
        transform.position = casilla.transform.position + Vector3.up * 0.05f;

        // Hago que la carta sea hija de la casilla
        transform.SetParent(casilla.transform);

        // Reseteo la rotación para que se quede plana en el suelo
        transform.rotation = Quaternion.Euler(0, 0, 0);

        transform.localScale = Vector3.one;

        // Con esto le digo a la casilla que tiene algo
        if (miModelo is ModeloCriatura criatura)
        {
            casilla.RecibirCarta(criatura);
            ControladorPartida.Instancia.JugadorLocal.EliminarCartaDeMano(criatura);
        }

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