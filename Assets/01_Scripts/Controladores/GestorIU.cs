using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GestorUI : MonoBehaviour
{
    public static GestorUI Instancia;

    public GameObject PanelInfo;

    public TextMeshProUGUI TxtNombre;
    public TextMeshProUGUI TxtDescripcion;
    public TextMeshProUGUI TxtCoste;
    public TextMeshProUGUI TxtAtaque;
    public TextMeshProUGUI TxtVida;

    public TextMeshProUGUI TxtManaJuego;
    public Button BtnPasarTurno; 

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);

        //Empiezo con el panel cerrado
        if (PanelInfo != null) PanelInfo.SetActive(false);
    }

    public void ActualizarMana(int actual, int maximo)
    {
        if (TxtManaJuego != null)
        {
            TxtManaJuego.text = $"MANÁ: {actual}/{maximo}";
        }
    }

    public void ActivarBotonTurno(bool activo)
    {
        if (BtnPasarTurno != null) BtnPasarTurno.interactable = activo;
    }

    public void MostrarInformacion(ModeloCarta modelo)
    {
        // Esto abre el panel y rellena los datos
        PanelInfo.SetActive(true);

        TxtNombre.text = modelo.Nombre;
        TxtCoste.text = modelo.CosteMagia.ToString();

        TxtDescripcion.text = "Carta de tipo " + modelo.Tipo;

        if (modelo is ModeloCriatura criatura)
        {
            TxtAtaque.text = criatura.Ataque.ToString();
            TxtVida.text = criatura.VidaActual.ToString();
        }
        else
        {
            TxtAtaque.text = "-";
            TxtVida.text = "-";
        }
    }

    public void CerrarPanel()
    {
        PanelInfo.SetActive(false);
    }
}