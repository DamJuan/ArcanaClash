using UnityEngine;
using TMPro;

public class TextoFlotante : MonoBehaviour
{
    private TMP_Text textoMesh;
    private Camera camaraPrincipal;
    public float velocidad = 2f;
    public float tiempoVida = 1.5f;

    void Awake()
    {
        textoMesh = GetComponent<TMP_Text>();
        if (textoMesh == null) textoMesh = GetComponentInChildren<TMP_Text>();
        camaraPrincipal = Camera.main;
    }

    public void ConfigurarTexto(string contenido, Color color)
    {
        if (textoMesh == null) textoMesh = GetComponent<TMP_Text>();

        if (textoMesh != null)
        {
            textoMesh.text = contenido;
            textoMesh.color = color;
        }

        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        transform.Translate(Vector3.up * velocidad * Time.deltaTime);

        if (camaraPrincipal != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - camaraPrincipal.transform.position);
        }
    }
}