using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VistaCriatura : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public TextMeshProUGUI LifeText;
    public RawImage ImagenArte;        // Para ponerla en gris si duerme

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
            if (LifeText != null) LifeText.text = miModelo.VidaActual.ToString();
        }
    }

    public void PonerEnReposo(bool dormir)
    {
        estaDormida = dormir;

        if (ImagenArte != null)
        {
            ImagenArte.color = dormir ? Color.gray : Color.white;
        }
    }

    public void Despertar()
    {
        PonerEnReposo(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (miModelo != null && GestorUI.Instancia != null)
        {
            GestorUI.Instancia.InspeccionarCarta(miModelo, transform.position);

            if (AudioManager.Instancia != null)
                AudioManager.Instancia.ReproducirSonido(AudioManager.Instancia.SonidoJugarCarta);
        }
    }
}