using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GestorUI : MonoBehaviour
{
    public static GestorUI Instancia;

    public GameObject PanelInfo;

    public TextMeshProUGUI TxtManaJuego;
    public Button BtnPasarTurno;

    public TextMeshProUGUI TxtVidaJugador;
    public TextMeshProUGUI TxtVidaEnemigo;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);

        //Empiezo con el panel cerrado
        if (PanelInfo != null) PanelInfo.SetActive(false);
    }

    public void ActualizarVidas(int vidaJugador, int vidaEnemigo)
    {
        if (TxtVidaJugador != null) TxtVidaJugador.text = "YO: " + vidaJugador;
        if (TxtVidaEnemigo != null) TxtVidaEnemigo.text = "RIVAL: " + vidaEnemigo;
    }

    public void ActualizarMana(int actual, int maximo)
    {
        if (TxtManaJuego != null)
        {
            TxtManaJuego.text = $"{actual}/{maximo}";
        }
    }

    public void ActivarBotonTurno(bool activo)
    {
        if (BtnPasarTurno != null) BtnPasarTurno.interactable = activo;
    }
    public void CerrarPanel()
    {
        if (PanelInfo != null)
        {
            PanelInfo.SetActive(false);
        }
    }
}