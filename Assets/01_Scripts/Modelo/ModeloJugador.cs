using System.Collections.Generic;
using System.Linq; // Esto lo uso para barajar el mazo


// El trabajo  de esta clase es gestionar el estado de un jugador
// su mazo, su mano, su magia y su vida.
public class ModeloJugador
{
    public string Nombre { get; private set; }

    public int MagiaActual { get; private set; }
    public int MagiaMaxima { get; private set; }

    public int ZonasTerrestresRestantes { get; private set; }



    // Cartas que componen el mazo del jugador.
    public List<ModeloCarta> Mazo { get; private set; }

    // Las cartas que el jugador tiene en la mano.
    public List<ModeloCarta> Mano { get; private set; }

    // Las cartas que ya han sido usadas o destruidas.
    public List<ModeloCarta> Descarte { get; private set; }


    public ModeloJugador(string nombre)
    {
        this.Nombre = nombre;

        this.Mazo = new List<ModeloCarta>();
        this.Mano = new List<ModeloCarta>();
        this.Descarte = new List<ModeloCarta>();

        this.MagiaMaxima = 0; 
        this.MagiaActual = 0;
        this.ZonasTerrestresRestantes = 4;
    }


    public void PrepararMazo(List<ModeloCarta> mazoCargado)
    {
        this.Mazo = mazoCargado;

        // Uso System.Randos porque UnityEngine.Random no funciona fuera del entorno de Unity.
        var random = new System.Random();
        this.Mazo = this.Mazo.OrderBy(carta => random.Next()).ToList();
    }

    public void RobarCarta()
    {
        if (Mazo.Count > 0)
        {
            ModeloCarta cartaRobada = Mazo[0];
            Mazo.RemoveAt(0);
            Mano.Add(cartaRobada);
        }
        else
        {
            // TODO : Manejar el caso de mazo vacío (pérdida de vida, fatiga o cualquier cosa que se me ocurra)
        }
    }

    public void RestaurarMagia()
    {
        this.MagiaActual = this.MagiaMaxima;
    }

    public void GastarMagia(int coste)
    {
        this.MagiaActual -= coste;
    }
}