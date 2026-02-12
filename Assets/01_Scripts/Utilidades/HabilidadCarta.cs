using System.Collections.Generic;
using UnityEngine;

public abstract class HabilidadCarta
{
    public string Nombre { get; protected set; }
    public string Descripcion { get; protected set; }

    public enum MomentoEjecucion
    {
        AlJugar,
        AlAtacar,
        AlRecibirDanio,
        AlMorir,
        InicioTurno,
        FinTurno
    }

    public MomentoEjecucion Momento { get; protected set; }

    public abstract void Ejecutar(ModeloCriatura criatura, ControladorPartida controlador);

    public virtual bool PuedeEjecutar(ModeloCriatura criatura)
    {
        return criatura != null && criatura.VidaActual > 0;
    }
}

[System.Serializable]
public class HabilidadCuracion : HabilidadCarta
{
    public int cantidadCura = 2;
    public HabilidadCuracion()
    {
        Nombre = "Toque Curativo";
        Descripcion = "Al inicio del turno, cura 2 de vida a aliados adyacentes";
        Momento = MomentoEjecucion.InicioTurno;
    }

    public override void Ejecutar(ModeloCriatura usuario, ControladorPartida controlador)
    {
        List<ModeloCasilla> adyacentes = controlador.ObtenerCasillasAdyacentes(controlador.ObtenerCasillaDeCriatura(usuario));

        foreach (ModeloCasilla casilla in adyacentes)
        {
            if (casilla.EstaOcupada && casilla.CriaturaEnCasilla.EsDelJugador == usuario.EsDelJugador)
            {
                casilla.CriaturaEnCasilla.Curar(cantidadCura);

                controlador.MostrarTextoCuracion(casilla, cantidadCura);

                InfoCasilla infoCasilla = controlador.ObtenerInfoCasillaPublica(casilla.CoordenadaX, casilla.CoordenadaY);
                if (infoCasilla != null)
                {
                    VistaCriatura vistaCurada = infoCasilla.GetComponentInChildren<VistaCriatura>();
                    if (vistaCurada != null)
                    {
                        vistaCurada.AnimacionCuracion(cantidadCura);
                        vistaCurada.ActualizarVisuales();
                    }
                }

                Debug.Log($"{usuario.Nombre} cura a {casilla.CriaturaEnCasilla.Nombre}");
            }
        }
    }
}

public class HabilidadVampiro : HabilidadCarta
{
    private float porcentajeRobo = 0.5f;

    public HabilidadVampiro()
    {
        Nombre = "Sed de Sangre";
        Descripcion = "Se cura el 50% del daño infligido";
        Momento = MomentoEjecucion.AlAtacar;
    }

    public override void Ejecutar(ModeloCriatura criatura, ControladorPartida controlador)
    {
        int curacion = Mathf.CeilToInt(criatura.Ataque * porcentajeRobo);
        criatura.Curar(curacion);

        ModeloCasilla casilla = controlador.ObtenerCasillaDeCriatura(criatura);
        if (casilla != null)
        {
            controlador.MostrarTextoCuracion(casilla, curacion);

            InfoCasilla infoCasilla = controlador.ObtenerInfoCasillaPublica(casilla.CoordenadaX, casilla.CoordenadaY);
            if (infoCasilla != null)
            {
                VistaCriatura vista = infoCasilla.GetComponentInChildren<VistaCriatura>();
                if (vista != null)
                {
                    vista.AnimacionCuracion(curacion);
                    vista.ActualizarVisuales();
                }
            }
        }
    }
}

public class HabilidadEscudo : HabilidadCarta
{
    private bool escudoActivo = true;

    public HabilidadEscudo()
    {
        Nombre = "Escudo Protector";
        Descripcion = "Bloquea el primer ataque recibido";
        Momento = MomentoEjecucion.AlRecibirDanio;
    }

    public override void Ejecutar(ModeloCriatura criatura, ControladorPartida controlador)
    {
        if (escudoActivo)
        {
            escudoActivo = false;

            if (controlador != null)
            {
                ModeloCasilla casilla = controlador.ObtenerCasillaDeCriatura(criatura);
                if (casilla != null)
                {
                    InfoCasilla infoCasilla = controlador.ObtenerInfoCasillaPublica(casilla.CoordenadaX, casilla.CoordenadaY);
                    if (infoCasilla != null)
                    {
                        VistaCriatura vista = infoCasilla.GetComponentInChildren<VistaCriatura>();
                        if (vista != null)
                        {
                            vista.ActualizarIconoHabilidades();
                        }
                    }
                }
            }
        }
    }

    public override bool PuedeEjecutar(ModeloCriatura criatura)
    {
        return base.PuedeEjecutar(criatura) && escudoActivo;
    }

    public void ResetearEscudo()
    {
        escudoActivo = true;
    }

    public bool EscudoActivo => escudoActivo;
}

public class HabilidadDivision : HabilidadCarta
{
    public HabilidadDivision()
    {
        Nombre = "Regeneración Mítica";
        Descripcion = "Al morir, invoca dos hidras pequeñas";
        Momento = MomentoEjecucion.AlMorir;
    }

    public override void Ejecutar(ModeloCriatura criatura, ControladorPartida controlador)
    {
        ModeloCasilla casillaOriginal = controlador.ObtenerCasillaDeCriatura(criatura);
        if (casillaOriginal == null) return;

        List<ModeloCasilla> adyacentes = controlador.ObtenerCasillasAdyacentes(casillaOriginal);
        int hidrasCreadas = 0;

        foreach (ModeloCasilla casilla in adyacentes)
        {
            if (!casilla.EstaOcupada && hidrasCreadas < 2)
            {
                ModeloCriatura hidraChica = new ModeloCriatura(
                    id: 19,
                    nombre: "Hidra Pequeña",
                    coste: 3,
                    ataque: 3,
                    vida: 3,
                    nombreModelo3D: "PrefabHidra"
                );

                controlador.InvocarCriatura(hidraChica, casilla, criatura.EsDelJugador);
                hidrasCreadas++;
            }
        }
    }
}

public class HabilidadExplosion : HabilidadCarta
{
    private int danioExplosion = 2;

    public HabilidadExplosion()
    {
        Nombre = "Explosión Etérea";
        Descripcion = "Al morir, causa 2 de daño a criaturas adyacentes";
        Momento = MomentoEjecucion.AlMorir;
    }

    public override void Ejecutar(ModeloCriatura criatura, ControladorPartida controlador)
    {
        ModeloCasilla casillaOriginal = controlador.ObtenerCasillaDeCriatura(criatura);
        if (casillaOriginal == null) return;

        List<ModeloCasilla> adyacentes = controlador.ObtenerCasillasAdyacentes(casillaOriginal);

        foreach (ModeloCasilla casilla in adyacentes)
        {
            if (casilla.EstaOcupada)
            {
                casilla.CriaturaEnCasilla.RecibirDanio(danioExplosion);
                controlador.MostrarTextoDanio(casilla, danioExplosion);
            }
        }
    }
}

public class HabilidadRobo : HabilidadCarta
{
    private int manaRobado = 1;

    public HabilidadRobo()
    {
        Nombre = "Pillaje";
        Descripcion = "Al atacar, roba 1 de maná al enemigo";
        Momento = MomentoEjecucion.AlAtacar;
    }

    public override void Ejecutar(ModeloCriatura criatura, ControladorPartida controlador)
    {
        if (criatura.EsDelJugador)
        {
            controlador.RobarMana(true, manaRobado);
        }
        else
        {
            controlador.RobarMana(false, manaRobado);
        }
    }
}

public class HabilidadCazador : HabilidadCarta
{
    private int bonusDanio = 2;

    public HabilidadCazador()
    {
        Nombre = "Cazador de Bestias";
        Descripcion = "+2 de ataque contra criaturas de coste bajo";
        Momento = MomentoEjecucion.AlAtacar;
    }

    public override void Ejecutar(ModeloCriatura criatura, ControladorPartida controlador)
    {

    }

    public int ObtenerBonusDanio(ModeloCriatura objetivo)
    {
        if (objetivo.CosteMagia <= 3)
        {
            return bonusDanio;
        }
        return 0;
    }
}