using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VistaCriatura : MonoBehaviour, IPointerClickHandler
{
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
        if (miModelo != null && LifeText != null)
            LifeText.text = miModelo.VidaActual.ToString();
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

        Debug.Log("1. ¡Clic detectado en VistaCriatura!");

        if (GestorUI.Instancia != null)
        {
            Debug.Log("2. GestorUI encontrado."); // <--- SEGUNDO CHECK
            GestorUI.Instancia.CerrarInspeccion();

            if (miModelo != null)
            {
                Debug.Log("3. Modelo existe. Llamando a inspeccionar..."); // <--- TERCER CHECK
                GestorUI.Instancia.InspeccionarCarta(miModelo, transform.position);
            }
        }
    }
}