﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;

namespace NGJ2012
{
    /// <summary>
    /// Implement save/target platform.
    /// </summary>
    public class SavePlatform : DrawableGameComponentExtended
    {
        Game1 parent;
        Rectangle screenRect;

        // Physical objects
        Body platformBody;
        Vector2 offsetToWater = new Vector2(2, 0);
        float riseSpeed = 0;
        int riseTime = 0;

        bool canBeTriggered = false; // determines if save platform can be triggered by player
        public void AllowTriggering() { canBeTriggered = true; }
        public Body Body { get { return platformBody; } }

        // Assets
        Texture2D platformTexture;
        TetrisPieceBatch drawer;

        public SavePlatform(Game game) : base(game)
        {
            parent = (Game1)game;
            screenRect = new Rectangle(0, 0, 640, 64);

            // Create physical objects
            platformBody = BodyFactory.CreateRectangle(parent.World, 4, 1, 1.0f, parent.WaterLayer.Position + offsetToWater);
            platformBody.BodyType = BodyType.Kinematic;
            platformBody.Friction = 100.0f;
            platformBody.CollisionCategories = Game1.COLLISION_GROUP_STATIC_OBJECTS;
            platformBody.CollidesWith = Game1.COLLISION_GROUP_TETRIS_BLOCKS | Game1.COLLISION_GROUP_DEFAULT;
        }

        protected override void LoadContent()
        {
            platformTexture = parent.Content.Load<Texture2D>("graphics/level/platform");
            drawer = new TetrisPieceBatch(GraphicsDevice, Game.Content);
        }

        protected override void UnloadContent()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void Trigger()
        {
            if (canBeTriggered)
            {
                canBeTriggered = false;
                StartRising(3000);
            }
        }

        protected void StartRising(int msec)
        {
            // Start rising 3 blocks
            riseTime = msec;
            riseSpeed = 3.0f / (0.001f * msec);
            parent.WaterLayer.StartRising(msec);
        }

        public override void Update(GameTime gameTime)
        {
            // Update position of platform
            if (riseTime > 0)
            {
                int msec = gameTime.ElapsedGameTime.Milliseconds;
                platformBody.LinearVelocity = new Vector2(0, -riseSpeed);
                riseTime -= msec;
                if (riseTime <= 0)
                {
                    riseTime = 0; // stop rising
                    platformBody.SetTransform(parent.WaterLayer.Position + offsetToWater, 0.0f);
                    platformBody.LinearVelocity = Vector2.Zero;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            /*
            Vector2 screenPos = Vector2.Transform(platformBody.Position, camera);
            screenRect.X = (int)screenPos.X;
            screenRect.Y = (int)screenPos.Y;
            parent.SpriteBatch.Begin();
            parent.SpriteBatch.Draw(platformTexture, screenRect, Color.White);
            parent.SpriteBatch.End();
            */

            drawer.cameraMatrix = camera;
            drawer.DrawBodyTextured(platformBody, platformTexture, 1.0f/8.0f);
        }
    }
}
