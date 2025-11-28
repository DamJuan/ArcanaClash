using UnityEngine;
using UnityEngine.EventSystems; 
using TMPro;
using System.Collections;


//Esto pinta el nombre, el coste y la descripción en el objeto 3D.
public class VistaCarta : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerClickHandler
{

    public TextMeshPro NameText;
    public TextMeshPro CostText;
    public Renderer ImagenArte;
    public TextMeshPro LifeText;
    public TextMeshPro DescriptionText;

    public Vector3 OffsetTextoDrag = new Vector3(0, 0.1f, 0);
    public Vector3 OffsetTextoZoom = new Vector3(0, -0.02f, 0);

    private Vector3 posBaseNombre;
    private Vector3 posBaseCoste;
    private Vector3 posBaseVida;
    private Vector3 posBaseDesc;

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
        GuardarPosicionesBaseTexto();
    }

    void GuardarPosicionesBaseTexto()
    {
        if (NameText != null) posBaseNombre = NameText.transform.localPosition;
        if (CostText != null) posBaseCoste = CostText.transform.localPosition;
        if (LifeText != null) posBaseVida = LifeText.transform.localPosition;
        if (DescriptionText != null) posBaseDesc = DescriptionText.transform.localPosition;
    }


    // Método para inicializar la carta visual con sus datos reales lo llama el Controlador cuando roba una carta.

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

        Texture imagenCargada = Resources.Load<Texture>("Imagenes/" + modelo.Nombre);

        if (imagenCargada != null && ImagenArte != null)
        {
            ImagenArte.material.mainTexture = imagenCargada;
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Solo mostramos info si NO estamos arrastrando la carta
        if (!arrastrando && !animando && puntoZoom != null)
        {
            Debug.Log("Clic en carta: " + miModelo.Nombre);

            if (!enZoom)
            {
                ActivarZoom();
            }
            else
            {
                DesactivarZoom();
            }

            if (GestorUI.Instancia != null)
            {
                GestorUI.Instancia.MostrarInformacion(miModelo);
            }
            else
            {
                Debug.LogWarning("No encuentro el GestorUI. ¿Está creado en la escena?");
            }
        }
    }

    void ActivarZoom()
    {
        padreOriginal = transform.parent;
        posOriginalLocal = transform.localPosition;
        rotOriginalLocal = transform.localRotation;
        escalaOriginalLocal = transform.localScale;
        siblingIndexOriginal = transform.GetSiblingIndex();

        transform.SetParent(puntoZoom);

        StartCoroutine(AnimarViaje(Vector3.zero, Quaternion.identity, Vector3.one * 1.5f));

        if (GestorUI.Instancia != null) GestorUI.Instancia.MostrarInformacion(miModelo);

        AplicarOffsetTextos(OffsetTextoZoom);

        enZoom = true;
    }

    void DesactivarZoom()
    {
        transform.SetParent(padreOriginal);
        transform.SetSiblingIndex(siblingIndexOriginal);

        StartCoroutine(AnimarViaje(posOriginalLocal, rotOriginalLocal, escalaOriginalLocal));

        if (GestorUI.Instancia != null) GestorUI.Instancia.CerrarPanel();

        RestaurarTextos();

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


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (enZoom) DesactivarZoom();

        arrastrando = true;
        posicionOriginal = transform.position;

        if (miCollider != null) miCollider.enabled = false;
        AplicarOffsetTextos(OffsetTextoDrag);
    }

    public void OnDrag(PointerEventData eventData) {
        if (arrastrando)
        {
            MoverCartaConRaton(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData) { 
        arrastrando = false;

        RestaurarTextos();

        Ray rayo = camaraPrincipal.ScreenPointToRay(eventData.position);
        RaycastHit golpe;

        if (Physics.Raycast(rayo, out golpe))
        {

            Debug.Log("He tocado: " + golpe.collider.name);

            InfoCasilla casillaDetectada = golpe.collider.GetComponent<InfoCasilla>();

            // Verifico si el jugador puede jugar la carta
            bool quedanJugadas = ControladorPartida.Instancia.JugadorLocal.PuedeJugarCarta();

            if (casillaDetectada != null && !casillaDetectada.EstaOcupada && casillaDetectada.EsTerritorioAliado && quedanJugadas)
            {
                JugarCartaEnCasilla(casillaDetectada);

                //Aqui resto una jugada al jugador
                ControladorPartida.Instancia.JugadorLocal.RegistrarJugada();
                if (miCollider != null) miCollider.enabled = true;
                return;
            }
        }
        transform.position = posicionOriginal;
        if (miCollider != null) miCollider.enabled = true;

        Debug.Log("Carta soltada en la nada. Volviendo...");

    }

    void AplicarOffsetTextos(Vector3 offset)
    {
        if (NameText != null) NameText.transform.localPosition = posBaseNombre + offset;
        if (CostText != null) CostText.transform.localPosition = posBaseCoste + offset;
        if (LifeText != null) LifeText.transform.localPosition = posBaseVida + offset;
        if (DescriptionText != null) DescriptionText.transform.localPosition = posBaseDesc + offset;
    }

    void RestaurarTextos()
    {
        if (NameText != null) NameText.transform.localPosition = posBaseNombre;
        if (CostText != null) CostText.transform.localPosition = posBaseCoste;
        if (LifeText != null) LifeText.transform.localPosition = posBaseVida;
        if (DescriptionText != null) DescriptionText.transform.localPosition = posBaseDesc;
    }

    private void JugarCartaEnCasilla(InfoCasilla casilla)
    {
        Debug.Log("¡Carta jugada en la casilla: " + casilla.name + "!");

        // Muevo la carta visualmente encima de la casilla
        transform.position = casilla.transform.position + Vector3.up * 0.05f;

        // Hago que la carta sea hija de la casilla
        transform.SetParent(casilla.transform);

        // Reseteo la rotación para que se quede plana en el suelo
        transform.rotation = Quaternion.identity;

        transform.localScale = Vector3.one;

        // Con esto le digo a la casilla que tiene algo
        if (miModelo is ModeloCriatura criatura)
        {
            casilla.RecibirCarta(criatura);
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