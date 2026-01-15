using UnityEngine;

public class CartaEnTablero : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Material shaderAliado;
    public Material shaderEnemigo;
    public GameObject capsulaDefault;

    private string _nombreModelo;
    private bool _esAliado;


    public void ConfigurarCarta(ModeloCriatura datos, bool esAliado)
    {
        this._nombreModelo = datos.NombreModelo3D;
        this._esAliado = esAliado;

        CancelInvoke();
        Invoke("Despertar", 2.0f);
    }

    private void Despertar()
    {
        GameObject prefabAInstanciar = null;

        if (!string.IsNullOrEmpty(_nombreModelo))
        {
            prefabAInstanciar = Resources.Load<GameObject>(_nombreModelo);
        }

        if (prefabAInstanciar == null)
        {
            prefabAInstanciar = capsulaDefault;
        }

        if (prefabAInstanciar != null)
        {

            GameObject criatura = Instantiate(prefabAInstanciar, transform.position, Quaternion.identity, transform.parent);

            criatura.transform.localScale = Vector3.one;
            criatura.transform.localPosition = Vector3.zero;

            Material matFinal = _esAliado ? shaderAliado : shaderEnemigo;
            PintarRecursivo(criatura, matFinal);


            Destroy(gameObject);
        }
    }
    void PintarRecursivo(GameObject obj, Material mat)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            r.material = mat;
        }
    }
}