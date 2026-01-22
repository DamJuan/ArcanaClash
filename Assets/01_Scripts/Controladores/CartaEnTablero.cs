using UnityEngine;
using TMPro;

public class CartaEnTablero : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Material shaderAliado;
    public Material shaderEnemigo;
    public GameObject capsulaDefault;

    private ModeloCriatura _datosCompletos;
    private bool _esAliado;

    public void ConfigurarCarta(ModeloCriatura datos, bool esAliado)
    {
        this._datosCompletos = datos;
        this._esAliado = esAliado;

        CancelInvoke();
        Invoke("DespertarModelo", 2.0f);
    }

    private void DespertarModelo()
    {
        GameObject prefabAInstanciar = null;
        string nombreModelo = _datosCompletos.NombreModelo3D;

        if (!string.IsNullOrEmpty(nombreModelo))
        {
            prefabAInstanciar = Resources.Load<GameObject>(nombreModelo);
        }

        if (prefabAInstanciar == null)
        {
            prefabAInstanciar = capsulaDefault;
        }

        if (prefabAInstanciar != null)
        {
  
            Quaternion rotacion = _esAliado ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

            GameObject nuevoObjeto = Instantiate(prefabAInstanciar, transform.position, rotacion, transform.parent);

            nuevoObjeto.transform.localScale = Vector3.one;
            nuevoObjeto.transform.localPosition = Vector3.zero;

            VistaCriatura scriptVisual = nuevoObjeto.GetComponentInChildren<VistaCriatura>(true);

            if (scriptVisual != null)
            {
                scriptVisual.Inicializar(_datosCompletos);
                scriptVisual.Despertar();
            }

            Material matFinal = _esAliado ? shaderAliado : shaderEnemigo;
            PintarRecursivo(nuevoObjeto, matFinal);

            Destroy(gameObject);
        }
    }

    void PintarRecursivo(GameObject obj, Material mat)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            if (r.GetComponent<TMP_Text>() == null)
            {
                r.material = mat;
            }
        }
    }
}