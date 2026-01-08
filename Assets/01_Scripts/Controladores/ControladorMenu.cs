using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorMenu : MonoBehaviour
{
    [Header("UI General")]
    public GameObject panelOpciones;
    public Slider sliderVolumen;
    public Toggle togglePantalla;

    [Header("Fondos")]
    public GameObject[] listaDeFondos;

    void Start()
    {
        ElegirFondoAleatorio();

        float volumenGuardado = PlayerPrefs.GetFloat("volumenGuardado", 1f);
        AudioListener.volume = volumenGuardado;
        if (sliderVolumen != null)
            sliderVolumen.value = volumenGuardado;

        bool esPantallaCompleta = PlayerPrefs.GetInt("pantallaCompleta", 1) == 1;
        Screen.fullScreen = esPantallaCompleta;
        if (togglePantalla != null)
            togglePantalla.isOn = esPantallaCompleta;
    }


    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat("volumenGuardado", valor);
    }

    public void ActivarPantallaCompleta(bool esCompleta)
    {
        Screen.fullScreen = esCompleta;
        PlayerPrefs.SetInt("pantallaCompleta", esCompleta ? 1 : 0);
    }


    void ElegirFondoAleatorio()
    {
        if (listaDeFondos.Length == 0) return;

        foreach (GameObject fondo in listaDeFondos)
        {
            if (fondo != null) fondo.SetActive(false);
        }

        int indiceAleatorio = Random.Range(0, listaDeFondos.Length);

        if (listaDeFondos[indiceAleatorio] != null)
            listaDeFondos[indiceAleatorio].SetActive(true);
    }

    public void Jugar()
    {
        SceneManager.LoadScene("Game");
    }

    public void AbrirOpciones()
    {
        panelOpciones.SetActive(true);
    }

    public void CerrarOpciones()
    {
        panelOpciones.SetActive(false);
    }

    public void Salir()
    {
        Debug.Log("Saliendo...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}