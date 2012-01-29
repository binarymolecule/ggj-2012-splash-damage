using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace NGJ2012
{
    /// <summary>
    /// Manager class used for loading and playing sound effects.
    /// </summary>
    public static class SoundManager
    {
        static List<SoundEffect> sounds = new List<SoundEffect>();
        static Dictionary<string, int> soundCues = new Dictionary<string, int>();

        /// <summary>
        /// Get or set volume of sound effects.
        /// </summary>
        public static float SoundVolume
        {
            get { return SoundEffect.MasterVolume; }
            set { SoundEffect.MasterVolume = value; }
        }

        /// <summary>
        /// Add sound effect with the given cue to the content manager.
        /// </summary>
        public static void LoadSound(ContentManager content, string cue, string asset)
        {
            if (cue != "" && !soundCues.ContainsKey(cue))
            {
                soundCues.Add(cue, sounds.Count);
                String fullAssetName = Path.Combine(@"sound", asset);
                sounds.Add(content.Load<SoundEffect>(fullAssetName));
            }
        }

        /// <summary>
        /// Play sound effect with the given cue.
        /// </summary>
        public static void PlaySound(string cue)
        {
            int index;
            if (soundCues.TryGetValue(cue, out index))
                sounds[index].Play();
        }

        /// <summary>
        /// Play sound effect with the given index.
        /// </summary>
        public static void PlaySound(int index)
        {
            sounds[index].Play();
        }

        /// <summary>
        /// Reset sound manager and delete all links to resources.
        /// </summary>
        public static void Reset()
        {
            sounds.Clear();
            soundCues.Clear();
        }
    }
}
