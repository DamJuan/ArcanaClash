using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia;

    public AudioSource FuenteMusica;
    public AudioSource FuenteEfectos;

    public AudioClip MusicaFondo;
    public AudioClip SonidoJugarCarta;

    void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (MusicaFondo != null)
        {
            FuenteMusica.clip = MusicaFondo;
            FuenteMusica.loop = true;
            FuenteMusica.Play();
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