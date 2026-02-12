using UnityEngine;
using TMPro;
using System.Collections;

public class VistaCriatura : MonoBehaviour
{
    private ModeloCriatura modeloAsociado;

    [Header("Referencias UI - Asignar o se buscarán automáticamente")]
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoAtaque;
    public GameObject indicadorEscudo;

    [Header("Referencias 3D")]
    public GameObject modelo3D;
    public Material materialNormal;
    public Material materialDormido;
    public Material materialHerido;

    [Header("Efectos - Opcionales")]
    public ParticleSystem particulasAtaque;
    public ParticleSystem particulasMuerte;
    public ParticleSystem particulasCuracion;

    private Renderer rendererModelo;
    private bool estaDormido = true;
    private Animator animator;

    void Awake()
    {
        BuscarComponentesAutomaticamente();

        if (modelo3D != null)
        {
            rendererModelo = modelo3D.GetComponent<Renderer>();
            animator = modelo3D.GetComponent<Animator>();
        }

        if (indicadorEscudo != null)
        {
            indicadorEscudo.SetActive(false);
        }
    }

    private void BuscarComponentesAutomaticamente()
    {
        if (textoNombre == null || textoVida == null || textoAtaque == null)
        {
            TextMeshProUGUI[] textosEncontrados = GetComponentsInChildren<TextMeshProUGUI>();

            foreach (TextMeshProUGUI texto in textosEncontrados)
            {
                string nombreLower = texto.gameObject.name.ToLower();

                if (textoNombre == null && (nombreLower.Contains("nombre") || nombreLower.Contains("name") || nombreLower.Contains("titulo")))
                {
                    textoNombre = texto;
                }
                else if (textoVida == null && (nombreLower.Contains("vida") || nombreLower.Contains("hp") || nombreLower.Contains("life")))
                {
                    textoVida = texto;
                }
                else if (textoAtaque == null && (nombreLower.Contains("ataque") || nombreLower.Contains("atk") || nombreLower.Contains("attack")))
                {
                    textoAtaque = texto;
                }
            }

            if ((textoNombre == null || textoVida == null || textoAtaque == null) && textosEncontrados.Length >= 3)
            {
                if (textoNombre == null) textoNombre = textosEncontrados[0];
                if (textoVida == null) textoVida = textosEncontrados[1];
                if (textoAtaque == null) textoAtaque = textosEncontrados[2];
            }
            else if ((textoVida == null || textoAtaque == null) && textosEncontrados.Length >= 2)
            {
                if (textoVida == null) textoVida = textosEncontrados[0];
                if (textoAtaque == null) textoAtaque = textosEncontrados[1];
            }
            else if (textoVida == null && textosEncontrados.Length >= 1)
            {
                textoVida = textosEncontrados[0];
            }
        }
    }

    public void Inicializar(ModeloCriatura modelo)
    {
        this.modeloAsociado = modelo;

        if (textoNombre == null || textoVida == null || textoAtaque == null)
        {
            BuscarComponentesAutomaticamente();
        }

        ActualizarVisuales();
        ActualizarIconoHabilidades();
        PonerEnReposo(true);
    }

    public void ActualizarVisuales()
    {
        if (modeloAsociado == null)
        {
            return;
        }

        if (textoNombre != null)
        {
            textoNombre.text = modeloAsociado.Nombre;
        }

        if (textoVida != null)
        {
            int vidaVisual = Mathf.Max(0, modeloAsociado.VidaActual);

            textoVida.text = "❤️ " + vidaVisual.ToString();

            float porcentajeVida = (float)modeloAsociado.VidaActual / modeloAsociado.VidaMaxima;
            if (porcentajeVida <= 0.3f)
            {
                textoVida.color = Color.red;
            }
            else if (porcentajeVida <= 0.6f)
            {
                textoVida.color = Color.yellow;
            }
            else
            {
                textoVida.color = Color.white;
            }
        }

        if (textoAtaque != null)
        {
            textoAtaque.text = "⚔️ " + modeloAsociado.Ataque.ToString();
        }

        if (rendererModelo != null && materialHerido != null)
        {
            float porcentajeVida = (float)modeloAsociado.VidaActual / modeloAsociado.VidaMaxima;
            if (porcentajeVida <= 0.5f && !estaDormido)
            {
                rendererModelo.material = materialHerido;
            }
            else if (!estaDormido && materialNormal != null)
            {
                rendererModelo.material = materialNormal;
            }
        }
    }

    private void ActualizarIconoHabilidades()
    {
        if (modeloAsociado == null) return;

        if (indicadorEscudo != null)
        {
            bool tieneEscudoActivo = modeloAsociado.TieneHabilidad<HabilidadEscudo>() &&
                                     modeloAsociado.escudo != null &&
                                     modeloAsociado.escudo.EscudoActivo;

            indicadorEscudo.SetActive(tieneEscudoActivo);
        }
    }

    public void Despertar()
    {
        estaDormido = false;

        if (rendererModelo != null && materialNormal != null)
        {
            rendererModelo.material = materialNormal;
        }

        if (animator != null)
        {
            animator.SetTrigger("Despertar");
        }
    }

    public void PonerEnReposo(bool dormido)
    {
        estaDormido = dormido;

        if (rendererModelo != null)
        {
            if (dormido && materialDormido != null)
            {
                rendererModelo.material = materialDormido;
            }
            else if (!dormido && materialNormal != null)
            {
                rendererModelo.material = materialNormal;
            }
        }
    }

    public void AnimacionRecibirDano(int cantidad)
    {
        if (animator != null)
        {
            animator.SetTrigger("RecibirDano");
        }

        StartCoroutine(SacudirModelo());
        ActualizarVisuales();
    }

    public void AnimacionCuracion(int cantidad)
    {
        if (particulasCuracion != null)
        {
            particulasCuracion.Play();
        }

        StartCoroutine(BrilloVerde());
        ActualizarVisuales();
    }

    public void AnimacionAtaque()
    {
        if (animator != null)
        {
            animator.SetTrigger("Atacar");
        }

        if (particulasAtaque != null)
        {
            particulasAtaque.Play();
        }
    }

    public void AnimacionMuerte()
    {
        if (animator != null)
        {
            animator.SetTrigger("Morir");
        }

        if (particulasMuerte != null)
        {
            particulasMuerte.Play();
        }

        StartCoroutine(FadeOut());
    }

    private IEnumerator SacudirModelo()
    {
        if (modelo3D == null) yield break;

        Vector3 posicionOriginal = modelo3D.transform.localPosition;
        float duracion = 0.3f;
        float intensidad = 0.1f;
        float tiempo = 0;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float x = Random.Range(-intensidad, intensidad);
            float z = Random.Range(-intensidad, intensidad);
            modelo3D.transform.localPosition = posicionOriginal + new Vector3(x, 0, z);
            yield return null;
        }

        modelo3D.transform.localPosition = posicionOriginal;
    }

    private IEnumerator BrilloVerde()
    {
        if (rendererModelo == null) yield break;

        Material materialOriginal = rendererModelo.material;
        Color colorOriginal = materialOriginal.color;
        float duracion = 0.5f;
        float tiempo = 0;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.PingPong(tiempo * 4, 1);
            materialOriginal.color = Color.Lerp(colorOriginal, Color.green, t * 0.5f);
            yield return null;
        }

        materialOriginal.color = colorOriginal;
    }

    private IEnumerator FadeOut()
    {
        if (rendererModelo == null)
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
            yield break;
        }

        float duracion = 1f;
        float tiempo = 0;

        Material mat = rendererModelo.material;
        Color colorInicial = mat.color;

        bool modoTransparenteActivado = ConfigurarMaterialTransparente(mat);

        if (!modoTransparenteActivado)
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
            yield break;
        }

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float alpha = 1 - (tiempo / duracion);
            Color nuevoColor = colorInicial;
            nuevoColor.a = alpha;
            mat.color = nuevoColor;

            transform.position += Vector3.down * Time.deltaTime;

            yield return null;
        }

        Destroy(gameObject);
    }

    private bool ConfigurarMaterialTransparente(Material mat)
    {
        if (mat == null) return false;

        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            return true;
        }

        return false;
    }

    public ModeloCriatura ObtenerModelo()
    {
        return modeloAsociado;
    }
}