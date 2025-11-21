using System;

// Su trabajo es guardar el ESTADO de una casilla: qué terreno es y qué criatura tiene.
public class ModeloCasilla
{

    public int CoordenadaX { get; private set; }
    public int CoordenadaY { get; private set; }

    public TipoTerreno TipoActual { get; private set; }
    public bool EstaOcupada { get; private set; }
    public ModeloCriatura CriaturaEnCasilla { get; private set; }


    // Cuando llame a 'CambiarTerreno', este evento se dispara y la Vista
    // se actualiza sola para cambiar el material o el color.
    public Action<TipoTerreno> EventoTerrenoCambiado;
    public ModeloCasilla(int x, int y, TipoTerreno tipoInicial)
    {
        this.CoordenadaX = x;
        this.CoordenadaY = y;
        this.TipoActual = tipoInicial;
        this.EstaOcupada = false;
        this.CriaturaEnCasilla = null;
    }

    // Este método lo llamará una carta especial o un hechizo.
    public void CambiarTerreno(TipoTerreno nuevoTipo)
    {
        this.TipoActual = nuevoTipo;

        EventoTerrenoCambiado?.Invoke(nuevoTipo);
    }

    public void AsignarCriatura(ModeloCriatura criatura)
    {
        this.CriaturaEnCasilla = criatura;
        this.EstaOcupada = true;
    }
    public void LimpiarCasilla()
    {
        this.CriaturaEnCasilla = null;
        this.EstaOcupada = false;
    }
}