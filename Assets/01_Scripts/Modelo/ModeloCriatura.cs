using System.Collections.Generic;

public class ModeloCriatura : ModeloCarta
{
    public override TipoCarta Tipo => TipoCarta.Criatura;

    public int Ataque { get; private set; }
    public int VidaActual { get; private set; }
    public int VidaMaxima { get; private set; }

    public ModeloCriatura(int id, string nombre, int coste, int ataque, int vida)
        : base(id, nombre, coste)
    {
        this.Ataque = ataque;
        this.VidaMaxima = vida;
        this.VidaActual = vida;
    }

    public void RecibirDanio(int danio)
    {
        this.VidaActual -= danio;
        if (this.VidaActual <= 0)
        {
        }
    }

    public override void EjecutarEfecto(List<object> objetivos)
    {
    }
}