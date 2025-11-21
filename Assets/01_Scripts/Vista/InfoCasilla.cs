using UnityEngine;
using UnityEngine.EventSystems;



public class InfoCasilla : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public bool EsTerritorioAliado = false;

    public Material MaterialLlanura;
    public Material MaterialMaiz;
    public Material MaterialPantano;

    public Material MaterialResaltado;
    public Material MaterialRojo;

    // Esti cambia el material del objeto 3D ya que el tablero puede tener distintos tipos de terreno.
    private Renderer renderizador;
    private ModeloCasilla modelo;
    private Material materialOriginal;

    // El awake lo uso para obtener el renderizador del objeto.
    void Awake()
    {
        renderizador = GetComponent<Renderer>();
    }

    public void Configurar(ModeloCasilla modeloAsignado)
    {
        this.modelo = modeloAsignado;

        if (this.modelo != null)
        {
            this.modelo.EventoTerrenoCambiado += ActualizarAspecto;
            ActualizarAspecto(this.modelo.TipoActual);
        }
    }

    private void ActualizarAspecto(TipoTerreno nuevoTipo)
    {
        Material materialACambiar = null;

        switch (nuevoTipo)
        {
            case TipoTerreno.Maiz:
                materialACambiar = MaterialMaiz;
                break;
            case TipoTerreno.Pantano:
                materialACambiar = MaterialPantano;
                break;
            case TipoTerreno.Llanura:
            default:
                materialACambiar = MaterialLlanura;
                break;
        }

        if (renderizador != null && materialACambiar != null)
        {
            renderizador.material = materialACambiar;
            materialOriginal = materialACambiar;
        }
    }

   
    public void OnPointerClick(PointerEventData eventData)
    {

        if (modelo == null)
        {
            Debug.LogError("Has hecho clic en una casilla sin datos. Bórrala de la escena: " + gameObject.name);
            return;
        }

        Debug.Log($"Clic en casilla: ({modelo.CoordenadaX}, {modelo.CoordenadaY})");

        // Demomento solo muestro un mensaje en consola.
    }

    void OnDestroy()
    {

        if (this.modelo != null)
        {
            this.modelo.EventoTerrenoCambiado -= ActualizarAspecto;
        }
    }

    public bool EstaOcupada
    {
        get
        {
            if (modelo == null)
            {
                return true;
            }
            return modelo.EstaOcupada;
        }
    }

    public void RecibirCarta(ModeloCriatura carta)
    {
        if (modelo != null)
        {
            modelo.AsignarCriatura(carta);
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!EstaOcupada && EsTerritorioAliado && eventData.dragging)
        {
            renderizador.material = MaterialResaltado;
        }
        else if (!EsTerritorioAliado && eventData.dragging)
        {
            renderizador.material = MaterialRojo;
        }
    }


        public void OnPointerExit(PointerEventData eventData)
    {
        // Volvemos al estado normal
        if (renderizador != null && materialOriginal != null)
        {
            renderizador.material = materialOriginal;
        }
    }
}
