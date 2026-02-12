using System.Collections.Generic;
using System.Linq;

public class ModeloJugador
{
    public string Nombre { get; private set; }

    public int Vida { get; private set; }

    public int MagiaActual { get; private set; }
    public int MagiaMaxima { get; private set; }
    public int ZonasTerrestresRestantes { get; private set; }

    public int CartasJugadasEsteTurno { get; private set; }
    private const int MAX_JUGADAS_TURNO = 4;

    private const int MANA_MAXIMO = 10;

    public List<ModeloCriatura> Mazo { get; private set; }

    public List<ModeloCriatura> Mano { get; private set; }

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
        this.Mazo = new List<ModeloCriatura>();

        foreach (ModeloCriatura carta in cartasIniciales)
        {
            ModeloCriatura nuevaCarta = new ModeloCriatura(
                carta.Id,
                carta.Nombre,
                carta.CosteMagia,
                carta.Ataque,
                carta.VidaMaxima,
                carta.NombreModelo3D
            );
            this.Mazo.Add(nuevaCarta);
        }
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
    }

    public void Barajar()
    {
        System.Random random = new System.Random();
        this.Mazo = this.Mazo.OrderBy(carta => random.Next()).ToList();
    }

    public ModeloCriatura RobarCarta()
    {
        if (Mazo == null || Mazo.Count == 0)
        {
            return null;
        }

        if (Mano.Count >= MAX_CARTAS_MANO)
        {
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
        if (this.MagiaActual < 0)
        {
            this.MagiaActual = 0;
        }
    }

    public void SubirManaMaximo()
    {
        if (this.MagiaMaxima < MANA_MAXIMO)
        {
            this.MagiaMaxima++;
        }
    }

    public void RobarMana(int cantidad)
    {
        this.MagiaActual += cantidad;
        if (this.MagiaActual > this.MagiaMaxima)
        {
            this.MagiaActual = this.MagiaMaxima;
        }
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

    public void Curar(int cantidad)
    {
        Vida += cantidad;
        if (Vida > 20) Vida = 20;
    }

    public bool MazoVacio => Mazo == null || Mazo.Count == 0;

    public int CartasEnMazo => Mazo != null ? Mazo.Count : 0;
}