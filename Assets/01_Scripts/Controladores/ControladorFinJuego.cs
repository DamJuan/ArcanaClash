using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorFinJuego : MonoBehaviour
{
    public GameObject panelVictoria;
    public GameObject panelDerrota;
    public GameObject botonReiniciar;

    public void MostrarVictoria()
    {
        panelVictoria.SetActive(true);
        if (botonReiniciar != null) botonReiniciar.SetActive(true);
        Time.timeScale = 0f;
    }

    public void MostrarDerrota()
    {
        panelDerrota.SetActive(true);
        if (botonReiniciar != null) botonReiniciar.SetActive(true);
        Time.timeScale = 0f;
    }

    public void VolverAJugar()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}