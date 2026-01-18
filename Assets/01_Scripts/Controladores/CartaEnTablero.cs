using UnityEngine;
using TMPro;

public class CartaEnTablero : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Material shaderAliado;
    public Material shaderEnemigo;
    public GameObject capsulaDefault;

    // Aquí guardamos los datos que vinieron del CSV
    private ModeloCriatura _datosCompletos;
    private bool _esAliado;

    public void ConfigurarCarta(ModeloCriatura datos, bool esAliado)
    {
        this._datosCompletos = datos;
        this._esAliado = esAliado;

        CancelInvoke();
        // Esperamos 2 segundos antes de transformar
        Invoke("DespertarModelo", 2.0f);
    }

    private void DespertarModelo()
    {
        GameObject prefabAInstanciar = null;
        string nombreModelo = _datosCompletos.NombreModelo3D;

        // 1. Intentamos cargar el modelo específico (ej: "PrefabRey")
        if (!string.IsNullOrEmpty(nombreModelo))
        {
            prefabAInstanciar = Resources.Load<GameObject>(nombreModelo);
        }

        // 2. Si no existe o falló, usamos la CÁPSULA (Igual que hace el enemigo)
        if (prefabAInstanciar == null)
        {
            prefabAInstanciar = capsulaDefault;
        }

        // 3. Instanciar
        if (prefabAInstanciar != null)
        {
            GameObject nuevoObjeto = Instantiate(prefabAInstanciar, transform.position, Quaternion.identity, transform.parent);

            nuevoObjeto.transform.localScale = Vector3.one;
            nuevoObjeto.transform.localPosition = Vector3.zero;

            // --- PASO DE DATOS (EL RELEVO) ---
            // Buscamos el script VistaCriatura en el nuevo objeto (o sus hijos)
            VistaCriatura scriptVisual = nuevoObjeto.GetComponentInChildren<VistaCriatura>(true);

            if (scriptVisual != null)
            {
                // ¡AQUÍ ES DONDE PASAMOS LOS DATOS DEL CSV AL PREFAB!
                scriptVisual.Inicializar(_datosCompletos);

                // Le decimos que se ponga de pie (modo 3D)
                scriptVisual.Despertar();
            }
            else
            {
                Debug.LogError($"[ERROR] El objeto '{nuevoObjeto.name}' no tiene el script VistaCriatura. No se verán los textos.");
            }
            // ---------------------------------

            // Pintar (Aliado = Azul / Enemigo = Rojo)
            Material matFinal = _esAliado ? shaderAliado : shaderEnemigo;
            PintarRecursivo(nuevoObjeto, matFinal);

            // Destruimos la carta 2D vieja
            Destroy(gameObject);
        }
    }

    void PintarRecursivo(GameObject obj, Material mat)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            // Evitamos pintar los textos para que no se vean borrosos o coloreados
            if (r.GetComponent<TMP_Text>() == null)
            {
                r.material = mat;
            }
        }
    }
}