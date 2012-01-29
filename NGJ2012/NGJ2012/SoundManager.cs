using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace BalloonFight
{
    /// <summary>
    /// Manager class used for loading and playing sound effects.
    /// </summary>
    public static class SoundManager
    {
        const int MAX_SOUND_ITEMS = 32;
        static List<SoundEffect> sounds = new List<SoundEffect>(MAX_SOUND_ITEMS);
        static Dictionary<string, int> soundCues = new Dictionary<string, int>(MAX_SOUND_ITEMS);
        static bool globalMode = true;
        static int numOfGlobals = 0;

        /// <summary>
        /// Get or set volume of sound effects.
        /// </summary>
        public static float SoundVolume
        {
            get { return SoundEffect.MasterVolume; }
            set { SoundEffect.MasterVolume = value; }
        }

        /// <summary>
        /// Finalize global resources and switch to local resource loading mode.
        /// </summary>
        public static void SwitchToLocalMode()
        {
            globalMode = false;
        }

        /// <summary>
        /// Add sound effect with the given cue to the content manager.
        /// </summary>
        public static void LoadSound(string cue)
        {
            if (cue != "" && !soundCues.ContainsKey(cue))
            {
                soundCues.Add(cue, sounds.Count);
                String fullAssetName = Path.Combine(@"sound", cue);
                if (globalMode)
                {
                    sounds.Add(MainGame.GlobalContent.Load<SoundEffect>(fullAssetName));
                    numOfGlobals++;
                }
                else
                    sounds.Add(MainGame.LocalContent.Load<SoundEffect>(fullAssetName));
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
        /// Reset sound manager and delete links to local resources.
        /// </summary>
        public static void ResetLocal()
        {
            List<string> localKeys = new List<string>(soundCues.Count - numOfGlobals);
            foreach (KeyValuePair<string, int> item in soundCues)
            {
                if (item.Value >= numOfGlobals)
                    localKeys.Add(item.Key);
            }
            foreach (string localKey in localKeys)
                soundCues.Remove(localKey);
            sounds.RemoveRange(numOfGlobals, sounds.Count - numOfGlobals);
        }

        /// <summary>
        /// Reset sound manager and delete all links to resources.
        /// </summary>
        public static void Reset()
        {
            sounds.Clear();
            soundCues.Clear();
            globalMode = true;
            numOfGlobals = 0;
        }
    }
}
