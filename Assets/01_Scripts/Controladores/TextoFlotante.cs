using UnityEngine;
using TMPro; // Necesario

public class TextoFlotante : MonoBehaviour
{
    public float velocidad = 2f;
    public float tiempoVida = 1f;

    private TMP_Text textoMesh;
    private Color colorOriginal;

    void Awake()
    {
        textoMesh = GetComponent<TMP_Text>();

        if (textoMesh == null)
        {
            Debug.LogError("¡Falta el componente de texto!");
            return;
        }

        colorOriginal = textoMesh.color;
    }

    public void Configurar(int cantidad)
    {
        if (textoMesh != null)
            textoMesh.text = "-" + cantidad.ToString();

        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        transform.Translate(Vector3.up * velocidad * Time.deltaTime);

        if (textoMesh != null)
        {
            float alpha = textoMesh.color.a - (Time.deltaTime / tiempoVida);
            textoMesh.color = new Color(colorOriginal.r, colorOriginal.g, colorOriginal.b, alpha);
        }

        if (Camera.main != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}