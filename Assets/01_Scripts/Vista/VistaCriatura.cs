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

    [Header("TEXTOS 2D")]
    public TMP_Text TxtVida2D;
    public TMP_Text TxtAtaque2D;

    private ModeloCriatura miModelo;

    // --- ESTO ES LO NUEVO: BUSCADOR AUTOMÁTICO ---
    void Awake()
    {
        // Si las casillas están vacías, el script busca objetos hijos que se llamen así
        if (TxtNombre3D == null) TxtNombre3D = BuscarTextoEnHijos("Nombre");
        if (TxtVida3D == null) TxtVida3D = BuscarTextoEnHijos("Vida");

        // He puesto "Araque" también porque vi en tu foto que se llamaba así
        if (TxtAtaque3D == null)
        {
            TxtAtaque3D = BuscarTextoEnHijos("Ataque");
            if (TxtAtaque3D == null) TxtAtaque3D = BuscarTextoEnHijos("Araque"); // Por si acaso
        }
    }

    // Función auxiliar para buscar recursivamente
    TMP_Text BuscarTextoEnHijos(string nombreObjeto)
    {
        // Busca en todos los hijos y nietos
        TMP_Text[] todosLosTextos = GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in todosLosTextos)
        {
            if (t.gameObject.name == nombreObjeto) return t;
        }
        return null;
    }
    // ---------------------------------------------

    public void Inicializar(ModeloCriatura modelo)
    {
        this.miModelo = modelo;
        ActualizarVisuales();

        // Empieza dormida (2D) hasta que CartaEnTablero la despierte
        PonerEnReposo(true);
    }

    public void ActualizarVisuales()
    {
        if (miModelo == null) return;

        // TEXTOS 2D
        if (TxtVida2D != null) TxtVida2D.text = miModelo.VidaActual.ToString();
        if (TxtAtaque2D != null) TxtAtaque2D.text = miModelo.Ataque.ToString();

        // TEXTOS 3D
        if (TxtVida3D != null) TxtVida3D.text = miModelo.VidaActual.ToString();
        if (TxtAtaque3D != null) TxtAtaque3D.text = miModelo.Ataque.ToString();
        if (TxtNombre3D != null) TxtNombre3D.text = miModelo.Nombre;

        // Debug para confirmar que escribe
        // Debug.Log($"Escribiendo en 3D -> Vida: {miModelo.VidaActual}, Nombre: {miModelo.Nombre}");
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

            // Forzamos actualización al despertar
            ActualizarVisuales();
        }
    }

    public void Despertar()
    {
        PonerEnReposo(false);
    }
}