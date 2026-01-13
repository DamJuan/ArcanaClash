using UnityEngine;

public class CartaSpawner : MonoBehaviour
{
    [Header("Configuración Única de esta Carta")]
    public DatosCarta datosDeEstaCarta;

    [Header("Referencias")]
    public GameObject capsulaVisual;
    public Transform puntoDeSpawn;

    private Material materialShader;

    void Awake()
    {
        if (capsulaVisual != null)
        {
            materialShader = capsulaVisual.GetComponent<Renderer>().material;
        }
    }

    public void Inicializar(DatosCarta datos)
    {
        datosDeEstaCarta = datos;
    }

    public void Despertar()
    {
        if (datosDeEstaCarta == null || datosDeEstaCarta.modeloPersonaje == null)
        {
            Debug.LogError("¡Esta carta no tiene datos o modelo asignado! Asegúrate de llamar a Inicializar().");
            return;
        }

        GameObject nuevoPersonaje = Instantiate(datosDeEstaCarta.modeloPersonaje, transform);

        if (puntoDeSpawn != null)
        {
            nuevoPersonaje.transform.position = puntoDeSpawn.position;
            nuevoPersonaje.transform.rotation = puntoDeSpawn.rotation;
        }
        else
        {
            nuevoPersonaje.transform.localPosition = Vector3.zero;
            nuevoPersonaje.transform.localRotation = Quaternion.identity;
        }

        AplicarShader(nuevoPersonaje);

        capsulaVisual.SetActive(false);
    }

    void AplicarShader(GameObject personaje)
    {
        Renderer[] renderers = personaje.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            Material[] nuevosMats = new Material[r.materials.Length];
            for (int i = 0; i < nuevosMats.Length; i++)
            {
                nuevosMats[i] = materialShader;
            }
            r.materials = nuevosMats;
        }
    }
}