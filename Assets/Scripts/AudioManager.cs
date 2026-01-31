using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Fontes de Áudio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clipes de Música")]
    public AudioClip backgroundMusic;

    [Header("Clipes de SFX")]
    public AudioClip pickupSound;
    public AudioClip dropSound;
    public AudioClip cartAddSound;
    public AudioClip cartRemoveSound;
    public AudioClip checkoutSound;
    public AudioClip buttonClickSound;

    [Header("Configurações")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    public static AudioManager Instance { get; private set; }

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
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            
            if (backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
            }
        }

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayPickup()
    {
        PlaySFX(pickupSound);
    }

    public void PlayDrop()
    {
        PlaySFX(dropSound);
    }

    public void PlayCartAdd()
    {
        PlaySFX(cartAddSound);
    }

    public void PlayCartRemove()
    {
        PlaySFX(cartRemoveSound);
    }

    public void PlayCheckout()
    {
        PlaySFX(checkoutSound);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    public void ToggleMusic(bool enabled)
    {
        if (musicSource != null)
        {
            if (enabled)
                musicSource.UnPause();
            else
                musicSource.Pause();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }
}
