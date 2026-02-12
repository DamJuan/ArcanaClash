using System.Collections.Generic;
using UnityEngine;

public class ModeloCriatura : ModeloCarta
{
    public override TipoCarta Tipo => TipoCarta.Criatura;

    public int Ataque { get; private set; }
    public int VidaActual { get; private set; }
    public int VidaMaxima { get; private set; }
    public string NombreModelo3D { get; private set; }

    public bool PuedeAtacar { get; set; }
    public bool EsDelJugador { get; set; }

    public List<HabilidadCarta> Habilidades { get; private set; }

    public HabilidadEscudo escudo;

    public ModeloCriatura(int id, string nombre, int coste, int ataque, int vida, string nombreModelo3D)
            : base(id, nombre, coste)
    {
        this.Ataque = ataque;
        this.VidaMaxima = vida;
        this.VidaActual = vida;
        this.NombreModelo3D = nombreModelo3D;
        this.PuedeAtacar = false;
        this.EsDelJugador = false;

        this.Habilidades = new List<HabilidadCarta>();

        AsignarHabilidadesPorNombre(nombre);
    }

    private void AsignarHabilidadesPorNombre(string nombre)
    {
        switch (nombre)
        {
            case "Curandera":
                Habilidades.Add(new HabilidadCuracion());
                break;

            case "Vampiro":
                Habilidades.Add(new HabilidadVampiro());
                break;

            case "Guardia":
                escudo = new HabilidadEscudo();
                Habilidades.Add(escudo);
                break;

            case "Hidra":
                Habilidades.Add(new HabilidadDivision());
                break;

            case "Espíritu":
            case "Espritu":
                Habilidades.Add(new HabilidadExplosion());
                break;

            case "Ladrón":
            case "Ladrn":
                Habilidades.Add(new HabilidadRobo());
                break;

            case "Cazador":
                Habilidades.Add(new HabilidadCazador());
                break;
        }
    }

    public void RecibirDanio(int danio)
    {
        if (escudo != null && escudo.EscudoActivo)
        {
            escudo.Ejecutar(this, null);
            return;
        }

        this.VidaActual -= danio;
        if (this.VidaActual < 0) this.VidaActual = 0;
    }

    public void Curar(int cantidad)
    {
        this.VidaActual += cantidad;
        if (this.VidaActual > this.VidaMaxima)
        {
            this.VidaActual = this.VidaMaxima;
        }
    }

    public void EjecutarHabilidades(HabilidadCarta.MomentoEjecucion momento, ControladorPartida controlador)
    {
        foreach (HabilidadCarta habilidad in Habilidades)
        {
            if (habilidad.Momento == momento && habilidad.PuedeEjecutar(this))
            {
                habilidad.Ejecutar(this, controlador);
            }
        }
    }

    public int ObtenerBonusAtaque(ModeloCriatura objetivo)
    {
        int bonus = 0;
        foreach (HabilidadCarta habilidad in Habilidades)
        {
            if (habilidad is HabilidadCazador cazador)
            {
                bonus += cazador.ObtenerBonusDanio(objetivo);
            }
        }
        return bonus;
    }

    public override void EjecutarEfecto(List<object> objetivos) { }

    public bool TieneHabilidad<T>() where T : HabilidadCarta
    {
        foreach (HabilidadCarta habilidad in Habilidades)
        {
            if (habilidad is T) return true;
        }
        return false;
    }
}