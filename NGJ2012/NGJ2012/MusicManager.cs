using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace NGJ2012
{
    /// <summary>
    /// Manager class used for loading and playing background music.
    /// </summary>
    public static class MusicManager
    {
        static List<Song> music = new List<Song>();
        static Dictionary<string, int> musicCues = new Dictionary<string, int>();
        static FadeHelper fader = new FadeHelper();
        static int currentMusicIndex = -1;
        public static float MaxVolume = 1.0f;
        public static bool IsPlaying { get { return currentMusicIndex >= 0; } }

        /// <summary>
        /// Get or set volume of background music.
        /// </summary>
        public static float MusicVolume
        {
            get { return MediaPlayer.Volume; }
            set { MediaPlayer.Volume = value * MaxVolume; }
        }

        /// <summary>
        /// Determines if background music is set currently.
        /// </summary>
        //public static bool HasCurrentMusic { get { return (currentMusicIndex != -1); } }

        /// <summary>
        /// Determines if music with given cue has been loaded.
        /// </summary>
        public static bool HasMusic(string cue) { return musicCues.ContainsKey(cue); }

        /// <summary>
        /// Return asset name of background music with given cue.
        /// </summary>
        /// <returns></returns>
        public static string GetMusicAssetName(string cue)
        {
            int index;
            if (musicCues.TryGetValue(cue, out index))
                return music[index].Name;
            return "";
        }

        /// <summary>
        /// Add background music with the given cue to the content manager.
        /// </summary>
        public static void LoadMusic(ContentManager content, string cue, string asset)
        {
            if (cue != "" && !musicCues.ContainsKey(cue))
            {
                String fullAssetName = Path.Combine(@"sound", asset);
                musicCues.Add(cue, music.Count);
                music.Add(content.Load<Song>(fullAssetName));
            }
        }

        /// <summary>
        /// Play background music with the given cue immediately.
        /// </summary>
        public static void PlayMusic(string cue, bool loop)
        {
            int index;
            if (musicCues.TryGetValue(cue, out index))
                PlayMusic(index, loop);
        }

        /// <summary>
        /// Play background music with the given index immediately.
        /// </summary>
        public static void PlayMusic(int index, bool loop)
        {
            if (MediaPlayer.State != MediaState.Stopped)
                MediaPlayer.Stop();
            MediaPlayer.IsRepeating = loop;
            MediaPlayer.Play(music[index]);
            currentMusicIndex = index;
            fader.Reset();
        }

        /// <summary>
        /// Fade in background music with the given cue.
        /// </summary>
        public static void FadeInMusic(string cue, bool loop, float fadeTime, float idleTime)
        {
            int index;
            if (musicCues.TryGetValue(cue, out index))
                FadeInMusic(index, loop, fadeTime, idleTime);
        }

        /// <summary>
        /// Fade in background music with the given index.
        /// </summary>
        public static void FadeInMusic(int index, bool loop, float fadeTime, float idleTime)
        {
            if (MediaPlayer.State != MediaState.Stopped)
                MediaPlayer.Stop();
            MediaPlayer.IsRepeating = loop;
            fader.FadeIn(fadeTime, idleTime);
            MusicVolume = fader.Value;
            currentMusicIndex = index;
        }

        /// <summary>
        /// Fade in current background music.
        /// </summary>
        public static void FadeInMusic(float fadeTime, float idleTime)
        {
            if (currentMusicIndex != -1)
            {
                fader.FadeIn(fadeTime, idleTime);
                MusicVolume = fader.Value;
            }
        }

        /// <summary>
        /// Stop background music.
        /// </summary>
        public static void StopMusic()
        {
            if (MediaPlayer.State != MediaState.Stopped)
                MediaPlayer.Stop();
            currentMusicIndex = -1;
            fader.Reset();
        }

        /// <summary>
        /// Fade out background music.
        /// </summary>
        public static void FadeOutMusic(float fadeTime)
        {
            if (currentMusicIndex != -1)
            {
                if (fader.State == FadeHelper.FadeState.FadeIn)
                {
                    float sec = MathHelper.Min(0.9f, 1.0f - fader.Value) * fadeTime;
                    fader.FadeOut(fadeTime);
                    fader.Update((int)(1000.0f * sec));
                }
                else
                    fader.FadeOut(fadeTime);
                MusicVolume = fader.Value;
            }
        }

        /// <summary>
        /// Pause background music.
        /// </summary>
        public static void PauseMusic()
        {
            if (MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Pause();
        }

        /// <summary>
        /// Resume background music.
        /// </summary>
        public static void ResumeMusic()
        {
            if (MediaPlayer.State == MediaState.Paused)
                MediaPlayer.Resume();
        }

        /// <summary>
        /// Reset music manager and delete all links to resources.
        /// </summary>
        public static void Reset()
        {
            StopMusic();
            music.Clear();
            musicCues.Clear();
        }

        /// <summary>
        /// Update audio engine to keep in sync with game.
        /// </summary>
        /// <param name="msec">Elapsed time in msec.</param>
        public static void Update(int msec)
        {
            if (fader.IsFading)
            {
                fader.Update(msec);
                MusicVolume = fader.Value;
                //if (fader.State == FadeHelper.FadeState.Out)
                //    StopMusic();
                //else 


                // dont mark stopped so we wont re-start due to IsPlaying = false



                if (fader.Value > 0 && MediaPlayer.State == MediaState.Stopped)
                    MediaPlayer.Play(music[currentMusicIndex]);
            }
        }
    }
}
