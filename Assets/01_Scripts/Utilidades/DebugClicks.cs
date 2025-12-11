using UnityEngine;
using UnityEngine.InputSystem; // <--- NECESARIO PARA EL NUEVO SISTEMA

public class DebugClicks : MonoBehaviour
{
    void Update()
    {
        // Comprobamos si existe el ratón y si se ha pulsado el botón izquierdo
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Leemos la posición del ratón con el nuevo sistema
            Vector2 mousePos = Mouse.current.position.ReadValue();

            Ray ray = Camera.main.ScreenPointToRay(mousePos);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Debug.Log($" HE TOCADO: {hit.collider.gameObject.name} (Capa: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");

                var script = hit.collider.GetComponent<VistaCriatura>();
                if (script != null)
                {
                    Debug.Log($"   > Tiene VistaCriatura. ¿Activado?: {script.enabled}");
                }
                else
                {
                    Debug.Log("   >  NO tiene el script VistaCriatura en el objeto del collider.");
                    // Buscamos en padres o hijos por si acaso
                    if (hit.collider.GetComponentInParent<VistaCriatura>()) Debug.Log("   > (Pero su PADRE sí lo tiene).");
                    if (hit.collider.GetComponentInChildren<VistaCriatura>()) Debug.Log("   > (Pero su HIJO sí lo tiene).");
                }
            }
            else
            {
                Debug.Log(" El rayo no ha tocado NADA (Aire).");
            }
        }
    }
}