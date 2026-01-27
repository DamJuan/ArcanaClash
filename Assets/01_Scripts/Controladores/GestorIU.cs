using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        if (PanelInfo != null) PanelInfo.SetActive(false);
    }

    public void InspeccionarCarta(ModeloCarta modelo, Vector3 posicionInicial)
    {
        CerrarInspeccion();

        if (PrefabCartaVisual != null && PuntoObservacion != null)
        {

            cartaClonada = Instantiate(PrefabCartaVisual, posicionInicial, Quaternion.Euler(90, 0, 0));

            VistaCarta vistaCarta = cartaClonada.GetComponent<VistaCarta>();
            if (vistaCarta != null)
            {
                vistaCarta.Configurar(modelo);
                Destroy(vistaCarta); 
                Destroy(cartaClonada.GetComponent<Collider>());
                Destroy(cartaClonada.GetComponent<CanvasGroup>());
            }

            if (AudioManager.Instancia != null)
                AudioManager.Instancia.ReproducirSonido(AudioManager.Instancia.SonidoJugarCarta);

            StartCoroutine(AnimarViajeCarta(cartaClonada.transform));

        }
        
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
        if (TxtVidaJugador != null) TxtVidaJugador.text = "" + vidaJugador;
        if (TxtVidaEnemigo != null) TxtVidaEnemigo.text = "" + vidaEnemigo;
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

    public void MostrarPantallaFin(bool victoria)
    {
        if (PanelFinPartida != null)
        {
            PanelFinPartida.SetActive(true);

            if (TxtMensajeFin != null)
            {
                if (victoria)
                {
                    TxtMensajeFin.text = "¡VICTORIA!";
                    TxtMensajeFin.color = Color.green;
                }
                else
                {
                    TxtMensajeFin.text = "DERROTA...";
                    TxtMensajeFin.color = Color.red;
                }
            }
        }
    }

    public void ReiniciarJuego()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Update()
    {
        if (cartaClonada != null)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.GetComponent<VistaCriatura>() == null)
                    {
                        CerrarInspeccion();
                    } 
                }
                else
                {
                    CerrarInspeccion();
                }
            }
        }
    }

}