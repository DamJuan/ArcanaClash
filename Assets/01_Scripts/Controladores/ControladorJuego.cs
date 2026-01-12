using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorJuego : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelPausa;
    public Slider sliderVolumen;
    public Toggle togglePantalla;

    private bool estaPausado = false;

    void Start()
    {
        float volumenGuardado = PlayerPrefs.GetFloat("volumenGuardado", 1f);
        AudioListener.volume = volumenGuardado;

        if (sliderVolumen != null)
            sliderVolumen.value = volumenGuardado;

        bool esPantallaCompleta = PlayerPrefs.GetInt("pantallaCompleta", 1) == 1;
        Screen.fullScreen = esPantallaCompleta;

        if (togglePantalla != null)
            togglePantalla.isOn = esPantallaCompleta;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (estaPausado) Reanudar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        panelPausa.SetActive(true);
        Time.timeScale = 0f;
        estaPausado = true;
    }

    public void Reanudar()
    {
        panelPausa.SetActive(false);
        Time.timeScale = 1f;
        estaPausado = false;
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
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
}