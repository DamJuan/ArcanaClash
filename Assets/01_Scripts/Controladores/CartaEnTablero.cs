using UnityEngine;
using System.Collections; // Necesario para las Corrutinas
using TMPro;

public class CartaEnTablero : MonoBehaviour
{
    [Header("Ajustes del Personaje")]
    public float alturaDeseada = 1.0f;
    public Vector3 rotacionEnemigo = new Vector3(0, 180, 0);

    [Header("Configuración Visual")]
    public GameObject capsulaDefault;
    public Material shaderAliado;
    public Material shaderEnemigo;

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
        string nombreModelo = _datosCompletos.NombreModelo3D;
        GameObject prefabPersonaje = Resources.Load<GameObject>(nombreModelo) ?? capsulaDefault;

        if (prefabPersonaje != null)
        {
            // 1. Instanciamos el personaje
            GameObject personaje = Instantiate(prefabPersonaje, transform.parent);

            // 2. Posición y Escala (Altura 1)
            personaje.transform.localPosition = new Vector3(0, alturaDeseada, 0);
            personaje.transform.localScale = Vector3.one;

            // 3. LA CLAVE: Desactivar Root Motion y forzar rotación antes de que el Animator actúe
            Animator anim = personaje.GetComponent<Animator>();
            if (anim != null)
            {
                anim.applyRootMotion = false;
                // Opcional: anim.enabled = false; // Solo si el giro sigue fallando
            }

            // 4. Aplicar rotación inmediata
            if (!_esAliado)
            {
                personaje.transform.localRotation = Quaternion.Euler(rotacionEnemigo);
            }
            else
            {
                personaje.transform.localRotation = Quaternion.identity;
            }

            // 5. Configurar VistaCriatura y materiales
            VistaCriatura vista = personaje.GetComponent<VistaCriatura>();
            if (vista != null)
            {
                vista.Inicializar(_datosCompletos);
                vista.Despertar();
            }

            Material matFinal = _esAliado ? shaderAliado : shaderEnemigo;
            PintarRecursivo(personaje, matFinal);

            // 6. Lanzar un refuerzo por si el Animator intenta resetearlo en el siguiente frame
            StartCoroutine(AsegurarGiroFinal(personaje));

            Destroy(gameObject);
        }
    }

    private IEnumerator AsegurarGiroFinal(GameObject obj)
    {
        // Forzamos la rotación durante los primeros 3 frames para ganarle a cualquier animación
        for (int i = 0; i < 3; i++)
        {
            if (obj == null) yield break;

            if (!_esAliado)
                obj.transform.localRotation = Quaternion.Euler(rotacionEnemigo);
            else
                obj.transform.localRotation = Quaternion.identity;

            yield return null;
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