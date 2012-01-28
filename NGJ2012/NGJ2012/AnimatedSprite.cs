﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace NGJ2012
{
    class AnimatedSprite
    {
        struct Animation
        {
            public int StartFrame;
            public int NumOfFrames;
            public int MsPerFrame;
            public bool Loop;
        }

        Game1 game;
        List<Texture2D> textures;
        Dictionary<string, int> animationIDs;
        List<Animation> animations;
        Vector2 textureOrigin;
        Animation currentAnimation;
        Texture2D currentTexture;

        int currentMsec;
        int currentFrame;

        public Color Color;
        Rectangle screenRect;

        public AnimatedSprite(Game1 parentGame, String[] assetNames, Vector2 originInPixels)
        {
            game = parentGame;
            textures = new List<Texture2D>(assetNames.Length);
            animationIDs = new Dictionary<string, int>();
            animations = new List<Animation>();
            textureOrigin = originInPixels;

            // Load textures
            foreach (string assetName in assetNames)
                textures.Add(game.Content.Load<Texture2D>(assetName));

            Color = Color.White;
            screenRect = new Rectangle(0, 0, 0, 0);
        }

        public int AddAnimation(string name, int startFrame, int numOfFrames, int msPerFrame, bool loop)
        {
            int index = animations.Count;
            animationIDs.Add(name, index);
            animations.Add(new Animation { StartFrame = startFrame, NumOfFrames = numOfFrames, MsPerFrame = msPerFrame, Loop = loop });
            return index;
        }

        public void SetAnimation(string name)
        {
            int index;
            if (animationIDs.TryGetValue(name, out index))
                currentAnimation = animations[index];
        }

        public void SetAnimation(int index)
        {
            currentAnimation = animations[index];
            currentFrame = currentAnimation.StartFrame;
            currentMsec = 0;
            currentTexture = textures[currentFrame];
        }

        public void Update(int msec)
        {
            Debug.WriteLine("Current frame: {0}", currentFrame);

            currentMsec += msec;
            if (currentMsec >= currentAnimation.MsPerFrame)
            {
                currentMsec -= currentAnimation.MsPerFrame;
                if (currentFrame + 1 < currentAnimation.StartFrame + currentAnimation.NumOfFrames)
                {
                    currentFrame++;
                    currentTexture = textures[currentFrame];
                }
                else if (currentAnimation.Loop)
                {
                    currentFrame = currentAnimation.StartFrame;
                    currentTexture = textures[currentFrame];
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, float scale)
        {
            spriteBatch.Draw(currentTexture, position, null, Color, 0.0f, textureOrigin, scale, SpriteEffects.None, 1.0f);
        }
    }
}