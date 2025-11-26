using UnityEngine;

public class GeneradorSuelo : MonoBehaviour
{
    public GameObject PrefabBaldosa;
    public int Ancho = 10;
    public int Largo = 10;
    public float TamanoBaldosa = 2f;

    [ContextMenu("Generar Suelo")] // Esto crea un botón en el menú del script
    void Generar()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Creamos el suelo nuevo
        for (int x = 0; x < Ancho; x++)
        {
            for (int z = 0; z < Largo; z++)
            {
                Vector3 pos = new Vector3(x * TamanoBaldosa, -1, z * TamanoBaldosa);
                GameObject baldosa = Instantiate(PrefabBaldosa, transform);
                baldosa.transform.localPosition = pos;
            }
        }
    }
}