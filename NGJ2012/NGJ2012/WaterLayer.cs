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

        // Assets
        Texture2D waterTexture;

        public WaterLayer(Game game) : base(game)
        {
            parent = (Game1)game;
            screenRect = new Rectangle(0, 0, 1280, 128);

            // Create physical objects
            /*
            waterBody = BodyFactory.CreateRectangle(parent.World, parent.WorldWidthInBlocks, 1, 1.0f,
                                                    new Vector2(parent.WorldWidthInBlocks / 2.0f, parent.WorldHeightInBlocks));
            waterBody.BodyType = BodyType.Static;
            waterBody.Friction = float.MaxValue;
            */
            pos = new Vector2(0, parent.WorldHeightInBlocks);
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
