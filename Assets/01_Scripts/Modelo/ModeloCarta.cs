using System.Collections.Generic;

public abstract class ModeloCarta : ICartaJuego
{
    public int Id { get; protected set; }
    public string Nombre { get; protected set; }
    public int CosteMagia { get; protected set; }

    public abstract TipoCarta Tipo { get; }

    protected ModeloCarta(int id, string nombre, int coste)
    {
        this.Id = id;
        this.Nombre = nombre;
        this.CosteMagia = coste;
    }

    public abstract void EjecutarEfecto(List<object> objetivos);
}