using System;
using System.Collections.Generic;

public interface ICartaJuego
{
    // Defino que toda carta debe tener un coste de magia y un nombre.
    // Es de solo lectura ({ get; }) porque el coste se asigna al crear la carta, no cambia.
    int CosteMagia { get; }
    String Nombre { get; }

    // Este es el método principal. Define la acción que la carta hace al jugarse.
    void EjecutarEfecto(List<object> objetivos);

}
