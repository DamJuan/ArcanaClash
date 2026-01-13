using UnityEngine;

[CreateAssetMenu(fileName = "NuevaCarta", menuName = "Cartas/DatosDeCarta")]
public class DatosCarta : ScriptableObject
{
    public string nombreCarta;
    [Tooltip("Arrastra aquí el Prefab del personaje de Mixamo")]
    public GameObject modeloPersonaje;
}