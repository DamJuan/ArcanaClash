using UnityEngine;
using UnityEngine.EventSystems; 
using TMPro; // He visto que para poner textos en 3D se usa TextMeshPro


//Esto pinta el nombre, el coste y la descripción en el objeto 3D.
public class VistaCarta : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler, IBeginDragHandler
{

    public TextMeshPro NameText;
    public TextMeshPro CostText;
    public Renderer ImagenArte;
    public TextMeshPro LifeText;
    public TextMeshPro DescriptionText;


    private ModeloCarta miModelo;
    private Vector3 posicionOriginal;
    private Camera camaraPrincipal;
    private bool arrastrando = false;
    private Collider miCollider;

    void Start()
    {
        camaraPrincipal = Camera.main;
        miCollider = GetComponent<Collider>();
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

    public void OnPointerDown(PointerEventData eventData) {
        posicionOriginal = transform.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        arrastrando = true;

        // Apago el collider para que el rayo atraviese la carta
        if (miCollider != null) miCollider.enabled = false;
    }

    public void OnDrag(PointerEventData eventData) {
        if (arrastrando)
        {
            MoverCartaConRaton(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData) { 
        arrastrando = false;

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
            else if (casillaDetectada != null && casillaDetectada.EstaOcupada)
            {
                Debug.Log("¡Esa casilla ya está ocupada!");
            }
        }
        transform.position = posicionOriginal;
        if (miCollider != null) miCollider.enabled = true;

        Debug.Log("Carta soltada en la nada. Volviendo...");

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