using System;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NGJ2012
{
    public class FadeHelper
    {
        /// <summary>
        /// Describe the current fade state.
        /// </summary>
        public enum FadeState
        {
            In, Out, FadeIn, FadeOut
        };

        FadeState fadeState;
        float fadeValue;
        int fadeTimer, maxFadeTime;
        int fadeIdleTime;

        public FadeState State { get { return fadeState; } }

        public float Value { get { return fadeValue; } }

        public bool IsFading { get { return (fadeState == FadeState.FadeIn) || (fadeState == FadeState.FadeOut); } }

        public FadeHelper()
        {
            Reset();
        }

        public void Reset()
        {
            fadeState = FadeState.Out;
            fadeValue = 0.0f;
            fadeTimer = maxFadeTime = 0;
            fadeIdleTime = 0;
        }

        public void Update(int msec)
        {
            // Update fading screen
            if (fadeState == FadeState.FadeIn)
            {
                fadeTimer -= msec;
                if (fadeTimer <= 0)
                {
                    fadeState = FadeState.In;
                    fadeTimer = maxFadeTime = 0;
                    fadeIdleTime = 0;
                    fadeValue = 1.0f;
                }
                else if (fadeTimer > maxFadeTime)
                    fadeValue = 0.0f;
                else
                    fadeValue = 1.0f - fadeTimer / (float)maxFadeTime;
            }
            else if (fadeState == FadeState.FadeOut)
            {
                fadeTimer -= msec;
                if (fadeTimer <= fadeIdleTime)
                {
                    fadeState = FadeState.Out;
                    fadeTimer = maxFadeTime = 0;
                    fadeIdleTime = 0;
                    fadeValue = 0.0f;
                }
                else if (fadeTimer < 0)
                    fadeValue = 0.0f;
                else
                    fadeValue = fadeTimer / (float)maxFadeTime;
            }
        }

        public void FadeIn(float time)
        {
            fadeState = FadeState.FadeIn;
            fadeTimer = maxFadeTime = (int)(1000.0f * time);
            fadeIdleTime = 0;
            fadeValue = 0.0f;
        }

        public void FadeOut(float time)
        {
            fadeState = FadeState.FadeOut;
            fadeTimer = maxFadeTime = (int)(1000.0f * time);
            fadeIdleTime = 0;
            fadeValue = 1.0f;
        }

        public void FadeIn(float time, float idleTime)
        {
            fadeState = FadeState.FadeIn;
            fadeTimer = maxFadeTime = (int)(1000.0f * time);
            fadeIdleTime = -(int)(1000.0f * idleTime);
            fadeTimer -= fadeIdleTime;
            fadeValue = 0.0f;
        }

        public void FadeOut(float time, float idleTime)
        {
            fadeState = FadeState.FadeOut;
            fadeTimer = maxFadeTime = (int)(1000.0f * time);
            fadeIdleTime = -(int)(1000.0f * idleTime);
            fadeValue = 1.0f;
        }
    }
}
