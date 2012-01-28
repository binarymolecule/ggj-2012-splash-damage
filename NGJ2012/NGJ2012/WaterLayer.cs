using System;
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
    /// Implement animated water layer.
    /// </summary>
    public class WaterLayer : DrawableGameComponentExtended
    {
        Game1 parent;
        Rectangle screenRect;

        // Physical objects
        //Body waterBody;
        Vector2 pos;
        public Vector2 Position { get { return pos; } }
        public float Height { get { return pos.Y; } }

        public float WaterSpeed = 0.1f; // rise speed in blocks per second

        // Assets
        Texture2D waterTexture;

        public WaterLayer(Game game) : base(game)
        {
            parent = (Game1)game;
            screenRect = new Rectangle(0, 0, parent.WorldWidthInBlocks * 96, parent.WorldHeightInBlocks * 96);

            // Create physical objects
            /*
            waterBody = BodyFactory.CreateRectangle(parent.World, parent.WorldWidthInBlocks, 1, 1.0f,
                                                    new Vector2(parent.WorldWidthInBlocks / 2.0f, parent.WorldHeightInBlocks));
            waterBody.BodyType = BodyType.Static;
            waterBody.Friction = float.MaxValue;
            */
            pos = new Vector2(0, 0);
        }

        protected override void LoadContent()
        {
            waterTexture = parent.Content.Load<Texture2D>("graphics/level/water");
        }

        protected override void UnloadContent()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            // Move water layer upwards
            pos.Y -= WaterSpeed * (0.001f * gameTime.ElapsedGameTime.Milliseconds);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            Vector2 screenPos = Vector2.Transform(pos, camera);
            screenRect.X = (int)screenPos.X;
            screenRect.Y = (int)screenPos.Y;
            
            parent.SpriteBatch.Begin();
            parent.SpriteBatch.Draw(waterTexture, screenRect, Color.White * 0.5f);
            parent.SpriteBatch.End();
        }
    }
}
