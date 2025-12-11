using UnityEngine;
using UnityEngine.EventSystems;

public class InfoCasilla : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Datos Lógicos")]
    public int CoordenadaX;
    public int CoordenadaY;
    public bool EsTerritorioAliado = false;

    [Header("Configuración Visual")]
    public Color ColorInvisible = new Color(1, 1, 1, 0.05f);
    public Color ColorAliado = new Color(0, 1, 1, 0.4f);
    public Color ColorEnemigo = new Color(1, 0, 0, 0.4f);

    private Renderer renderizador;
    private ModeloCasilla modelo;

    void Awake()
    {
        renderizador = GetComponentInChildren<Renderer>();

        if (renderizador != null)
        {
            renderizador.material.color = ColorInvisible;
        }
    }

    public void Configurar(ModeloCasilla modeloAsignado)
    {
        this.modelo = modeloAsignado;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging && renderizador != null)
        {
            if (!EstaOcupada && EsTerritorioAliado)
            {
                renderizador.material.color = ColorAliado;
            }
            else
            {
                renderizador.material.color = ColorEnemigo;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (renderizador != null)
        {
            renderizador.material.color = ColorInvisible;
        }
    }

    public bool EstaOcupada
    {
        get
        {
            if (modelo == null) return true;
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
}