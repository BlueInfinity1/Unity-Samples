using System;
using UnityEngine;
using UnityEngine.Audio;

/* 
This singleton class pools AudioSources to efficiently manage overlapping playback of sounds.
Each sound needs a specific number of AudioSources assigned, depending on how many instances of that sound 
might play simultaneously. For example, if a sound can overlap up to 3 times, you should assign 3 AudioSources.
*/

namespace AudioManagement
{    
    public class AudioManager : MonoBehaviour
    {
        public Sound[] sounds;
        public Sound[] music;
    
        [SerializeField] AudioMixer mainAudioMixer;
        [SerializeField] bool playMusic; // Whether we should play background music or not when the level starts
    
        public static AudioManager instance;
    
        void Awake()
        {
            if (instance == null)
            {
                DontDestroyOnLoad(gameObject);
                instance = this;
    
                int index = 0;
    
                foreach (Sound s in sounds)
                {
                    InitializeAudioSourcesForSound(s, index);
                    index++;
                }
    
                index = 0;
    
                foreach (Sound s in music)
                {
                    // There shouldn't be a need to play background themes overlappingly from many audio sources,
                    // so we'll use sources[0] for all of these
                    InitializeAudioSourcesForSound(s, index);
                    index++;
                }
    
                if (playMusic)
                {
                    Play(snd: Sound.LevelTheme, isLoopingSound: true, isSoundEffect: false);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }
    
        /// <summary>
        /// Initializes audio sources for a given sound.
        /// </summary>
        /// <param name="s">The sound object to initialize.</param>
        /// <param name="soundIndex">The index of the sound.</param>
        void InitializeAudioSourcesForSound(Sound s, int soundIndex)
        {
            s.baseIndex = soundIndex;
            s.sources = new AudioSource[s.totalSources];
    
            for (int i = 0; i < s.totalSources; i++)
            {
                // Create multiple audio sources for the same sound if overlapping playback is needed
                s.sources[i] = gameObject.AddComponent<AudioSource>();
                s.sources[i].clip = s.clip;
                s.sources[i].volume = s.volume;
                s.sources[i].pitch = s.pitch;
                s.sources[i].playOnAwake = false;
            }
        }
    
        /// <summary>
        /// Plays a sound or music track.
        /// </summary>
        /// <param name="snd">The sound to play.</param>
        /// <param name="isLoopingSound">Whether the sound should loop.</param>
        /// <param name="isSoundEffect">True if the sound is an effect, false if it's music.</param>
        public void Play(Sound snd, bool isLoopingSound = false, bool isSoundEffect = true)
        {
            string name = snd.Value;
            Sound[] soundArray = isSoundEffect ? sounds : music;
            Sound s = Array.Find(soundArray, sound => (sound.name == name));
    
            if (s != null)
            {
                // Pick the correct audio source to play, and pick a source that's most likely free
                AudioSource source = s.sources[s.currentSourceIndex];
                s.currentSourceIndex = (s.currentSourceIndex + 1) % s.totalSources;
    
                if (source != null)
                {
                    source.loop = isLoopingSound;
                    source.Play();
                }
            }
            else
            {
                Debug.Log("Sound with name " + name + " could not be found");
            }
        }
    
        /// <summary>
        /// Sets the FX volume on the main audio mixer.
        /// Converts linear volume to a logarithmic scale.
        /// </summary>
        /// <param name="volume">Linear volume value (0 to 1).</param>
        public void SetFXVolume(float volume)
        {
            // Since we can't take the logarithm of 0, we need to change the value slightly when near 0
            float nonZeroVolume = Mathf.Max(volume, 0.0001f);
            mainAudioMixer.SetFloat("SoundVolume", Mathf.Log10(nonZeroVolume) * 20);
        }
    
        /// <summary>
        /// Sets the music volume on the main audio mixer.
        /// Converts linear volume to a logarithmic scale.
        /// </summary>
        /// <param name="volume">Linear volume value (0 to 1).</param>
        public void SetMusicVolume(float volume)
        {
            // Since we can't take the logarithm of 0, we need to change the value slightly when near 0
            float nonZeroVolume = Mathf.Max(volume, 0.0001f);
            mainAudioMixer.SetFloat("MusicVolume", Mathf.Log10(nonZeroVolume) * 20);
        }
    }
}
