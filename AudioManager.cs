using System;
using UnityEngine;
using UnityEngine.Audio;

//A singleton class for playing audio
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    public Sound[] music;
	[SerializeField] AudioMixer mainAudioMixer;
	public static AudioManager instance;
	
	[SerializeField] private bool playMusic; //Whether we should play background music or not when the level starts

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

            foreach (Sound s in music) //There shouldn't be a need to play bg themes overlappingly from many audiosources, so we'll use sources[0] for all of these
            {
                InitializeAudioSourcesForSound(s, index);
                index++;
            }

            if (playMusic)            
                Play(snd: Sound.LevelTheme, isLoopingSound: true, isSoundEffect: false);
            
        }
        else if (instance != this)
            Destroy(gameObject);
    }

    void InitializeAudioSourcesForSound(Sound s, int soundIndex)
    {
        s.baseIndex = soundIndex;        
        s.sources = new AudioSource[s.totalSources];

        for (int i = 0; i < s.totalSources; i++) //create multiple audiosources for the same sound if we need to play the sound many times overlappingly
        {
            s.sources[i] = gameObject.AddComponent<AudioSource>();
            s.sources[i].clip = s.clip;
            s.sources[i].volume = s.volume;
            s.sources[i].pitch = s.pitch;
            s.sources[i].playOnAwake = false;
        }
    }

    public void Play(Sound snd, bool isLoopingSound = false, bool isSoundEffect = true)
    {
        string name = snd.Value;
        Sound[] soundArray = isSoundEffect ? sounds : music;
        Sound s = Array.Find(soundArray, sound => (sound.name == name));

        if (s != null)
        {
            AudioSource source = s.sources[s.currentSourceIndex];
            s.currentSourceIndex = (s.currentSourceIndex + 1) % s.totalSources; //pick the correct audiosource to play, and pick a source that's most likely going to be free

            if (source != null)
            {
                source.loop = isLoopingSound;
                source.Play();
            }
        }
        else
            Debug.Log("Sound with name " + name + " could not be found");
    }

    /// <summary>
    /// Sets the FX volume.
    /// </summary>
    /// <param name="volume">Volume.</param>
    public void SetFXVolume(float volume)
	{
		mainAudioMixer.SetFloat("FX", volume);
	}

	/// <summary>
	/// Sets the music volume.
	/// </summary>
	/// <param name="volume">Volume.</param>
	public void SetMusicVolume(float volume)
	{
		mainAudioMixer.SetFloat("Music", volume);
	}
}
