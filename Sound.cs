using System;
using UnityEngine;

namespace AudioManagement
{
    // This class represents a sound effect or a background theme that we can play through AudioManager
    [Serializable]
    public class Sound
    {
        public string name;
        [HideInInspector] public int baseIndex; // Index of this in the sound array or music array of AudioManager. Two sounds may have the same index if one is bg music and the other is a sound.
        // If there are multiple audiosources that can play the same sound, they will have the same index as well
    
        public AudioClip clip;
    
        [Range(0f, 1f)] public float volume = 1f;
        [Range(.1f, 3f)] public float pitch = 1f;
    
        /* The audioSources assigned to each object determine how many of these sounds can be simultaneously played at once. If we can be certain that a certain sound type will never be played overlappingly with
         * other sounds of the same type, we can set the sources to 1. An example of such sound is the player jump: There's only one player, and the player can't jump again before a single jump has been completed.
         * If there can be multiple sounds of the same type that do overlap, we need to reserve more sources for playing the sounds. An example of these types of sounds are enemy attack sounds, which there can be many
         * of if there are many enemies on the screen.
         * Of course, it's also possible to dynamically adjust this depending on the scenario, e.g. if there are 10 enemies on the screen, as opposed to just 1.
         */
        public int totalSources = 1;
        [HideInInspector] public int currentSourceIndex = 0;
        [HideInInspector] public AudioSource[] sources; 
    
        private Sound(string value) { Value = value; }
        public string Value { get; private set; }
    
        // List of all sounds as enum-like variables for easier searching. We also start these variables with capital letters to match the enum look.
        // NOTE: The names of these must match the names given to the sounds in the AudioManager object in Unity Editor
    
        //SOUNDS
        public static Sound PlayerJump { get { return new Sound("PlayerJump"); } }
        public static Sound PlayerShoot { get { return new Sound("PlayerShoot"); } }
        public static Sound PlayerHurt { get { return new Sound("PlayerHurt"); } }
            
        public static Sound HealthPickUp { get { return new Sound("HealthPickUp"); } }
        public static Sound EnemyHurt { get { return new Sound("EnemyHurt"); } }
        public static Sound EnemyAttack { get { return new Sound("EnemyAttack"); } }
    
        //BACKGROUND THEMES
        public static Sound LevelTheme { get { return new Sound("LevelTheme"); } }
    
    }
}
