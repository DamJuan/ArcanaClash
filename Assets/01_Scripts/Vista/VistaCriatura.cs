using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VistaCriatura : MonoBehaviour
{
    [Header("CONTROL VISUAL")]
    public GameObject GrupoVisual2D;
    public GameObject GrupoVisual3D;

    [Header("TEXTOS 3D (Se buscan solos si están vacíos)")]
    public TMP_Text TxtVida3D;
    public TMP_Text TxtAtaque3D;
    public TMP_Text TxtNombre3D;

    public GameObject PrefabTextoDano;
    public GameObject PrefabEfectoMuerte;

    [Header("TEXTOS 2D")]
    public TMP_Text TxtVida2D;
    public TMP_Text TxtAtaque2D;

    private ModeloCriatura miModelo;
    private Animator miAnimator;

    void Awake()
    {
        if (TxtNombre3D == null) TxtNombre3D = BuscarTextoEnHijos("Nombre");
        if (TxtVida3D == null) TxtVida3D = BuscarTextoEnHijos("Vida");

        if (TxtAtaque3D == null)
        {
            TxtAtaque3D = BuscarTextoEnHijos("Ataque");
            if (TxtAtaque3D == null) TxtAtaque3D = BuscarTextoEnHijos("Araque");
        }
        miAnimator = GetComponent<Animator>();
    }

    TMP_Text BuscarTextoEnHijos(string nombreObjeto)
    {
        TMP_Text[] todosLosTextos = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in todosLosTextos)
        {
            if (t.gameObject.name == nombreObjeto) return t;
        }
        return null;
    }


    public void Inicializar(ModeloCriatura modelo)
    {
        this.miModelo = modelo;
        ActualizarVisuales();

        PonerEnReposo(true);
    }

    public void ActualizarVisuales()
    {
        if (miModelo == null) return;

        // TEXTOS 2D
        if (TxtVida2D != null) TxtVida2D.text = miModelo.VidaActual.ToString();
        if (TxtAtaque2D != null) TxtAtaque2D.text = miModelo.Ataque.ToString();

        // TEXTOS 3D
        if (TxtVida3D != null)
        {
            TxtVida3D.text = "❤️" + miModelo.VidaActual.ToString();
        }
        if (TxtAtaque3D != null)
        {
            TxtAtaque3D.text = "⚔️" + miModelo.Ataque.ToString();
        }

        if (TxtNombre3D != null) TxtNombre3D.text = miModelo.Nombre;

    }

    public void PonerEnReposo(bool dormir)
    {
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

            ActualizarVisuales();
        }
    }

    public void Despertar()
    {
        PonerEnReposo(false);
        ActualizarVisuales();

        if (miAnimator != null)
        {
            miAnimator.enabled = true;

            miAnimator.applyRootMotion = false;

            miAnimator.Play("Breathing Idle", 0, 0f);

        }
    }


    public void AnimacionRecibirDano(int cantidad)
    {
        if (PrefabTextoDano != null)
        {
            Vector3 posicion = transform.position + Vector3.up * 1.5f;
            GameObject textoObj = Instantiate(PrefabTextoDano, posicion, Quaternion.identity);

            TextoFlotante scriptTexto = textoObj.GetComponent<TextoFlotante>();
            if (scriptTexto != null) scriptTexto.Configurar(cantidad);
        }

    }

    public void AnimacionMuerte()
    {
        if (PrefabEfectoMuerte != null)
        {
            Instantiate(PrefabEfectoMuerte, transform.position, Quaternion.identity);
        }
    }

}
