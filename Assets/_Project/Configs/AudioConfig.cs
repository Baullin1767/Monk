using UnityEngine;

namespace Monk.Configs
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Monk/Configs/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Volume Defaults")]
        [Range(0f, 1f)]
        [SerializeField] private float defaultMusicVolume = 0.7f;
        [Range(0f, 1f)]
        [SerializeField] private float defaultSFXVolume = 1f;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private AudioClip hurtClip;
        [SerializeField] private AudioClip coinClip;

        public float DefaultMusicVolume => defaultMusicVolume;
        public float DefaultSFXVolume => defaultSFXVolume;
        public AudioClip JumpClip => jumpClip;
        public AudioClip HurtClip => hurtClip;
        public AudioClip CoinClip => coinClip;
    }
}
