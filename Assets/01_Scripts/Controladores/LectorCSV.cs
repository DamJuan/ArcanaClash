using UnityEngine;
using System.Collections.Generic;

public class LectorCSV : MonoBehaviour
{
    public TextAsset archivoCSV;

    public List<ModeloCriatura> CargarCartas()
    {
        List<ModeloCriatura> listaCartas = new List<ModeloCriatura>();

        if (archivoCSV == null)
        {
            Debug.LogError("¡No has asignado el archivo CSV al Lector!");
            return listaCartas;
        }

        string[] lineas = archivoCSV.text.Split('\n');


        for (int i = 1; i < lineas.Length; i++)
        {
            string linea = lineas[i].Trim();
            if (string.IsNullOrEmpty(linea)) continue;

            string[] datos = linea.Split(';');


            if (datos.Length < 8)
            {
                continue;
            }

            int id = int.Parse(datos[0]);
            string nombre = datos[1];
            int coste = int.Parse(datos[2]);
            int ataque = int.Parse(datos[3]);
            int vida = int.Parse(datos[4]);

            string descripcion = datos[5];
            string imagen = datos[6];

            string nombreModelo3D = datos[7].Trim();

            ModeloCriatura nuevaCarta = new ModeloCriatura(id, nombre, coste, ataque, vida, nombreModelo3D);

            listaCartas.Add(nuevaCarta);
        }

        return listaCartas;
    }
}