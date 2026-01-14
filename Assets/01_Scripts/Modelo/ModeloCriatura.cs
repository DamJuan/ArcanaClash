using System.Collections.Generic;

public class ModeloCriatura : ModeloCarta
{
    public override TipoCarta Tipo => TipoCarta.Criatura;

    public int Ataque { get; private set; }
    public int VidaActual { get; private set; }
    public int VidaMaxima { get; private set; }
    public string NombreModelo3D { get; private set; }

    public bool PuedeAtacar { get; set; }

    // Constructor actualizado recibiendo el nombreModelo3D al final
    public ModeloCriatura(int id, string nombre, int coste, int ataque, int vida, string nombreModelo3D)
            : base(id, nombre, coste)
    {
        this.Ataque = ataque;
        this.VidaMaxima = vida;
        this.VidaActual = vida;

        // Asignamos el valor que viene del CSV a la propiedad de la clase
        this.NombreModelo3D = nombreModelo3D;

        this.PuedeAtacar = false;
    }

    public void RecibirDanio(int danio)
    {
        this.VidaActual -= danio;
        if (this.VidaActual <= 0)
        {
            // Lógica de muerte
        }
    }

    public override void EjecutarEfecto(List<object> objetivos)
    {
        // Lógica de efectos
    }
}