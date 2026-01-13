using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia;

    public AudioSource FuenteEfectos;

    public AudioClip SonidoJugarCarta;

    void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ReproducirSonido(AudioClip clip)
    {
        if (clip != null)
        {
            FuenteEfectos.PlayOneShot(clip);
        }
    }
}