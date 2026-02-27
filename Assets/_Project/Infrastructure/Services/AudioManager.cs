using UnityEngine;
using Monk.Configs;

namespace Monk.Infrastructure
{
    public class AudioManager : MonoBehaviour
    {
        private const string MusicVolumeKey = "monk.audio.musicVolume";
        private const string SfxVolumeKey = "monk.audio.sfxVolume";

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioConfig audioConfig;

        private static AudioManager instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (audioConfig != null)
            {
                float musicVol = PlayerPrefs.GetFloat(MusicVolumeKey, audioConfig.DefaultMusicVolume);
                float sfxVol = PlayerPrefs.GetFloat(SfxVolumeKey, audioConfig.DefaultSFXVolume);
                SetMusicVolume(musicVol);
                SetSFXVolume(sfxVol);

                if (audioConfig.MainThemeClip != null)
                {
                    PlayMusic(audioConfig.MainThemeClip);
                }
            }
        }

        public void PlayMusic(AudioClip clip)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            sfxSource.PlayOneShot(clip);
        }

        public void PlayHurt()
        {
            if (audioConfig != null && audioConfig.HurtClip != null)
                sfxSource.PlayOneShot(audioConfig.HurtClip);
        }

        public void PlayHitEnemy()
        {
            if (audioConfig != null && audioConfig.HitEnemyClip != null)
                sfxSource.PlayOneShot(audioConfig.HitEnemyClip);
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.volume = volume;
            PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        }

        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = volume;
            PlayerPrefs.SetFloat(SfxVolumeKey, volume);
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }
    }
}
