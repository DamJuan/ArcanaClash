using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GestorUI : MonoBehaviour
{
    public static GestorUI Instancia;

    public Transform PuntoObservacion; 
    public GameObject PrefabCartaVisual;
    private GameObject cartaClonada;


    public TextMeshProUGUI TxtManaJuego;
    public Button BtnPasarTurno;
    public TextMeshProUGUI TxtVidaJugador;
    public TextMeshProUGUI TxtVidaEnemigo;

    public GameObject PanelInfo;
    public GameObject PanelFinPartida;
    public TextMeshProUGUI TxtMensajeFin;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);

        //Empiezo con el panel cerrado
        if (PanelFinPartida != null) PanelFinPartida.SetActive(false);
    }

    public void InspeccionarCarta(ModeloCarta modelo, Vector3 posicionInicial)
    {
        CerrarInspeccion();

        cartaClonada = Instantiate(PrefabCartaVisual, posicionInicial, Quaternion.Euler(90, 0, 0));

        
        Destroy(cartaClonada.GetComponent<VistaCarta>());
        Destroy(cartaClonada.GetComponent<VistaCriatura>());
        Destroy(cartaClonada.GetComponent<Collider>()); 

        VistaCarta vistaTemp = cartaClonada.GetComponent<VistaCarta>();
        if (vistaTemp != null) vistaTemp.Configurar(modelo);

        // 5. Iniciamos la animación de viaje hacia la cámara
        StartCoroutine(AnimarViajeCarta(cartaClonada.transform));
    }

    IEnumerator AnimarViajeCarta(Transform carta)
    {
        float tiempo = 0;
        float duracion = 0.3f;
        Vector3 inicioPos = carta.position;
        Quaternion inicioRot = carta.rotation;
        Vector3 inicioEscala = carta.localScale;

   
        Vector3 escalaFinal = new Vector3(0.008f, 0.008f, 0.008f);

        while (tiempo < duracion)
        {
            if (carta == null) yield break;

            tiempo += Time.deltaTime;
            float t = tiempo / duracion;
 
            t = t * t * (3f - 2f * t);

            carta.position = Vector3.Lerp(inicioPos, PuntoObservacion.position, t);
            carta.rotation = Quaternion.Lerp(inicioRot, PuntoObservacion.rotation, t);
            carta.localScale = Vector3.Lerp(inicioEscala, escalaFinal, t);

            yield return null;
        }

        if (carta != null)
        {
            carta.position = PuntoObservacion.position;
            carta.rotation = PuntoObservacion.rotation;
        }
    }

    public void CerrarInspeccion()
    {
        if (cartaClonada != null)
        {
            Destroy(cartaClonada);
            cartaClonada = null;
        }
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
        CerrarInspeccion();

        if (PanelInfo != null) PanelInfo.SetActive(false);
    }

}