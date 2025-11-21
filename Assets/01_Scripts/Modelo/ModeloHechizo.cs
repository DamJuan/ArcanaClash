using System.Collections.Generic;

public class ModeloHechizo : ModeloCarta
{
    public override TipoCarta Tipo => TipoCarta.Hechizo;

    public ModeloHechizo(int id, string nombre, int coste)
        : base(id, nombre, coste)
    {
    }

    public override void EjecutarEfecto(List<object> objetivos)
    {
    }
}