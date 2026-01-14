using UnityEngine;
using System.Collections.Generic;

public class LectorCSV : MonoBehaviour
{
    public TextAsset archivoCSV;

    // Método que devuelve una lista con todas las cartas leídas
    public List<ModeloCriatura> CargarCartas()
    {
        List<ModeloCriatura> listaCartas = new List<ModeloCriatura>();

        if (archivoCSV == null)
        {
            Debug.LogError("¡No has asignado el archivo CSV al Lector!");
            return listaCartas;
        }

        string[] lineas = archivoCSV.text.Split('\n');

        // Empezamos en i = 1 para saltar la cabecera (id;nombre;...)
        for (int i = 1; i < lineas.Length; i++)
        {
            string linea = lineas[i].Trim();
            if (string.IsNullOrEmpty(linea)) continue;

            string[] datos = linea.Split(';');

            if (datos[1] == "Rey") // Solo chivarse con el Rey para no llenar la consola
            {
                Debug.Log($"[LECTOR] Leyendo línea del REY. Columnas detectadas: {datos.Length}");
                if (datos.Length > 7)
                {
                    Debug.Log($"   -> Dato en columna 7 (Modelo): '{datos[7]}'");
                }
                else
                {
                    Debug.LogError("   ->  ¡FALTAN COLUMNAS! El array no llega a la posición 7.");
                }
            }

            // PROTECCIÓN: Verificamos que la línea tenga las 8 columnas necesarias
            // (0:id, 1:nombre, 2:coste, 3:ataque, 4:vida, 5:desc, 6:img, 7:modelo3d)
            if (datos.Length < 8)
            {
                // Si falta el dato del modelo 3D, saltamos esta línea para evitar errores
                continue;
            }

            int id = int.Parse(datos[0]);
            string nombre = datos[1];
            int coste = int.Parse(datos[2]);
            int ataque = int.Parse(datos[3]);
            int vida = int.Parse(datos[4]);

            // Leemos descripción e imagen aunque no las pasemos al constructor base por ahora
            string descripcion = datos[5];
            string imagen = datos[6];

            // --- NUEVO: Leemos el nombre del modelo 3D (Columna 7) ---
            string nombreModelo3D = datos[7].Trim();

            // Pasamos 'nombreModelo3D' al final del constructor actualizado
            ModeloCriatura nuevaCarta = new ModeloCriatura(id, nombre, coste, ataque, vida, nombreModelo3D);

            listaCartas.Add(nuevaCarta);
        }

        return listaCartas;
    }
}