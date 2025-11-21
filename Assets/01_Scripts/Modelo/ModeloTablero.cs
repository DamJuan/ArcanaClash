public class ModeloTablero
{
    private const int TAMANO_TABLERO = 4;

    public ModeloCasilla[,] Casillas { get; private set; }

    public ModeloTablero()
    {
        Casillas = new ModeloCasilla[TAMANO_TABLERO, TAMANO_TABLERO];
        InicializarTablero();
    }

    private void InicializarTablero()
    {
        for (int x = 0; x < TAMANO_TABLERO; x++)
        {
            for (int y = 0; y < TAMANO_TABLERO; y++)
            {
                Casillas[x, y] = new ModeloCasilla(x, y, TipoTerreno.Llanura);
            }
        }
    }

    public ModeloCasilla ObtenerCasilla(int x, int y)
    {
        if (x >= 0 && x < TAMANO_TABLERO && y >= 0 && y < TAMANO_TABLERO)
        {
            return Casillas[x, y];
        }

        return null;
    }

}