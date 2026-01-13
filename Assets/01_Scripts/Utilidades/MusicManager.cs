using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Lista de Reproducción")]
    public List<AudioClip> playlist;
    public bool aleatorio = true;

    private AudioSource _audioSource;
    private int _ultimoIndice = -1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = false;
        _audioSource.playOnAwake = false;

        if (playlist.Count > 0)
        {
            PlayNextSong();
        }
    }

    void Update()
    {
     
        if (!_audioSource.isPlaying && playlist.Count > 0)
        {
            PlayNextSong();
        }
    }

    void PlayNextSong()
    {
        int index = 0;

        if (aleatorio && playlist.Count > 1)
        {
            do
            {
                index = Random.Range(0, playlist.Count);
            } while (index == _ultimoIndice);
        }
        else
        {
            index = (_ultimoIndice + 1) % playlist.Count;
        }

        _ultimoIndice = index;
        _audioSource.clip = playlist[index];
        _audioSource.Play();
    }

  
    public void SetMusicVolume(float volume)
    {
        _audioSource.volume = volume;
    }
}