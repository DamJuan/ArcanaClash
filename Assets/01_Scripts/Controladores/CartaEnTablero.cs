using UnityEngine;

public class CartaEnTablero : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Material shaderAliado;
    public Material shaderEnemigo;
    public GameObject capsulaDefault; // Arrastra una cápsula aquí por si falla la carga

    // Datos internos de esta carta específica
    private string _nombreModelo;
    private bool _esAliado;
    private int _turnosRestantes = 1; // Ajusta esto según tu juego

    // 1. LLAMA A ESTO AL INSTANCIAR LA CARTA (Desde tu MazoController o Mano)
    public void ConfigurarCarta(ModeloCriatura datos, bool esAliado)
    {
        this._nombreModelo = datos.NombreModelo3D;
        this._esAliado = esAliado;

        // Debug para verificar que llega el dato
        // Debug.Log($"Carta creada: {datos.nombre}. Modelo a cargar: {_nombreModelo}");
    }

    // 2. LLAMA A ESTO CUANDO PASE EL TURNO
    public void PasarTurno()
    {
        _turnosRestantes--;

        if (_turnosRestantes <= 0)
        {
            Despertar();
        }
    }

private void Despertar()
    {
        Debug.Log("--- INICIO PROCESO DESPERTAR ---");
        Debug.Log($"1. Nombre que llega del CSV: '{_nombreModelo}'");

        GameObject prefabAInstanciar = null;

        // Intentar cargar desde Resources
        if (!string.IsNullOrEmpty(_nombreModelo))
        {
            // Nota: Resources.Load NO necesita la extensión .prefab
            prefabAInstanciar = Resources.Load<GameObject>(_nombreModelo);
            
            if (prefabAInstanciar != null) 
                Debug.Log("2. ¡ÉXITO! Se encontró el modelo en Resources.");
            else 
                Debug.Log($"2. FALLO. Unity buscó '{_nombreModelo}' en Resources y no encontró nada.");
        }
        else
        {
            Debug.Log("2. FALLO CRÍTICO. El nombre del modelo está VACÍO o es NULO.");
        }

        // Si no existe, usar cápsula
        if (prefabAInstanciar == null)
        {
            Debug.LogWarning("3. Activando PLAN B: Usando Cápsula por defecto.");
            prefabAInstanciar = capsulaDefault;
        }

        // Crear el nuevo objeto
        GameObject criatura = Instantiate(prefabAInstanciar, transform.position, transform.rotation, transform.parent);

        // ... resto del código de pintar ...
        Material matFinal = _esAliado ? shaderAliado : shaderEnemigo;
        PintarRecursivo(criatura, matFinal);
        Destroy(gameObject);
    }

    void PintarRecursivo(GameObject obj, Material mat)
    {
        // Busca en MeshRenderer y SkinnedMeshRenderer (para animados)
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            r.material = mat;
        }
    }
}