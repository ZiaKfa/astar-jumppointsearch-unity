using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]  AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    public AudioClip bgm;
    public AudioClip shoot;
    public AudioClip enemyHit;
    public AudioClip playerHit;
    public AudioClip heal;
    public AudioClip coin;
    public AudioClip select;
    public AudioClip bulletHit;
    public AudioClip valid;
    public AudioClip invalid;
    public static AudioManager instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        musicSource.clip = bgm;
        musicSource.loop = true;
        playBgm();
    }

    public void playSfx(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void stopBgm()
    {
        musicSource.Stop();
    }

    public void playBgm()
    {
        musicSource.Play();
    }

    public void setBgm(AudioClip bgmClip)
    {
        musicSource.clip = bgmClip;
    }
   
}