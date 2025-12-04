using System.Collections.Generic;
using System.Linq; // Esto lo uso para barajar el mazo


// El trabajo  de esta clase es gestionar el estado de un jugador
// su mazo, su mano, su magia y su vida.
public class ModeloJugador
{

     public string Nombre { get; private set; }

    public int Vida { get; private set; }

    public int MagiaActual { get; private set; }
    public int MagiaMaxima { get; private set; }
    public int ZonasTerrestresRestantes { get; private set; }

    public int CartasJugadasEsteTurno { get; private set; }
    private const int MAX_JUGADAS_TURNO = 4;



    // Cartas que componen el mazo del jugador.
    public List<ModeloCriatura> Mazo { get; private set; }

    // Las cartas que el jugador tiene en la mano.
    public List<ModeloCriatura> Mano { get; private set; }

    // Las cartas que ya han sido usadas o destruidas.
    public List<ModeloCriatura> Descarte { get; private set; }

    private const int MAX_CARTAS_MANO = 5;


    public ModeloJugador(string nombre)
    {
        this.Nombre = nombre;

        this.Mazo = new List<ModeloCriatura>();
        this.Mano = new List<ModeloCriatura>();
        this.Descarte = new List<ModeloCriatura>();

        this.MagiaMaxima = 2; 
        this.MagiaActual = 0;
        this.ZonasTerrestresRestantes = 4;
        CartasJugadasEsteTurno = 0;

        this.Vida = 20;
    }

    public void CargarMazo(List<ModeloCriatura> cartasIniciales)
    {
        this.Mazo = new List<ModeloCriatura>(cartasIniciales);
    }
    public bool PuedeJugarCarta()
    {
        return CartasJugadasEsteTurno < MAX_JUGADAS_TURNO;
    }

    public void RegistrarJugada()
    {
        CartasJugadasEsteTurno++;
    }

    public void ReiniciarTurno()
    {
        CartasJugadasEsteTurno = 0;
        // Aquí también recargo maná en el futuro
    }

    public void Barajar()
    {
        // Uso System.Randos porque UnityEngine.Random no funciona fuera del entorno de Unity.
        System.Random random = new System.Random();
        this.Mazo = this.Mazo.OrderBy(carta => random.Next()).ToList();
    }

    public ModeloCriatura RobarCarta()
    {
        // Si el mazo está vacío, no hacemos nada
        if (Mazo.Count == 0)
        {
            return null;
        }

        if (Mano.Count >= MAX_CARTAS_MANO)
        {
            // Si la mano está llena se descarta automaticamente la carta robada
            ModeloCriatura cartaQuemada = Mazo[0];
            Mazo.RemoveAt(0);
            Descarte.Add(cartaQuemada);

            return null;
        }

        ModeloCriatura cartaRobada = Mazo[0];
        Mazo.RemoveAt(0);
        Mano.Add(cartaRobada);

        return cartaRobada;
    }

    public void RestaurarMagia()
    {
        this.MagiaActual = this.MagiaMaxima;
    }

    public void GastarMagia(int coste)
    {
        this.MagiaActual -= coste;
    }

    public void SubirManaMaximo()
    {
        this.MagiaMaxima++;
    }

    public void EliminarCartaDeMano(ModeloCriatura carta)
    {
        if (Mano.Contains(carta))
        {
            Mano.Remove(carta);
        }
    }

    public void RecibirDanio(int cantidad)
    {
        Vida -= cantidad;
        if (Vida < 0) Vida = 0;
    }
}