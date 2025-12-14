using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VistaCriatura : MonoBehaviour, IPointerClickHandler
{
    [Header("CONTROL VISUAL")]
    public GameObject GrupoVisual2D;
    public GameObject GrupoVisual3D;

    [Header("TEXTOS DEL HOLOGRAMA (3D)")]
    public TextMeshProUGUI TxtVida3D;
    public TextMeshProUGUI TxtAtaque3D;
    public TextMeshProUGUI TxtNombre3D;

    [Header("TEXTOS DE LA CARTA (2D)")]
    public TextMeshProUGUI TxtVida2D;
    public TextMeshProUGUI TxtAtaque2D;

    private ModeloCriatura miModelo;
    private bool estaDormida = true;

    public void Inicializar(ModeloCriatura modelo)
    {
        this.miModelo = modelo;

        ActualizarVisuales();

        PonerEnReposo(true);
    }

    public void ActualizarVisuales()
    {
        if (miModelo != null)
        {
            if (TxtVida2D != null) TxtVida2D.text = miModelo.VidaActual.ToString();
            if (TxtAtaque2D != null) TxtAtaque2D.text = miModelo.Ataque.ToString();

            if (TxtVida3D != null) TxtVida3D.text = miModelo.VidaActual.ToString();
            if (TxtAtaque3D != null) TxtAtaque3D.text = miModelo.Ataque.ToString();
            if (TxtNombre3D != null) TxtNombre3D.text = miModelo.Nombre;
        }
    }

    public void PonerEnReposo(bool dormir)
    {
        estaDormida = dormir;

        if (dormir)
        {
            if (GrupoVisual2D != null) GrupoVisual2D.SetActive(true);
            if (GrupoVisual3D != null) GrupoVisual3D.SetActive(false);

            transform.localRotation = Quaternion.Euler(90, 0, 0);
        }
        else
        {
            if (GrupoVisual2D != null) GrupoVisual2D.SetActive(false);
            if (GrupoVisual3D != null) GrupoVisual3D.SetActive(true);

            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void Despertar()
    {
        PonerEnReposo(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GestorUI.Instancia != null && miModelo != null)
        {
            GestorUI.Instancia.CerrarInspeccion();
            GestorUI.Instancia.InspeccionarCarta(miModelo, transform.position);
        }
    }


}