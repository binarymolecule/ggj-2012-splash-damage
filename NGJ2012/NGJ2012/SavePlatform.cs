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
    /// Implement save/target platform.
    /// </summary>
    public class SavePlatform : DrawableGameComponentExtended
    {
        Game1 parent;
        Rectangle screenRect;

        // Physical objects
        Body platformBody;

        // Assets
        Texture2D platformTexture;

        public SavePlatform(Game game) : base(game)
        {
            parent = (Game1)game;
            screenRect = new Rectangle(0, 0, 0, 0);

            // Create physical objects
            platformBody = BodyFactory.CreateRectangle(parent.World, 10, 1, 1.0f, new Vector2(0, 1));
            platformBody.BodyType = BodyType.Static;
            platformBody.Friction = float.MaxValue;
            platformBody.CollisionCategories = Category.Cat3;
        }

        protected override void LoadContent()
        {
            platformTexture = parent.Content.Load<Texture2D>("graphics/level/platform");
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
            // TODO Compute screen rect from position!
            screenRect.X = (int)(platformBody.GetWorldPoint(Vector2.Zero).X * 64);
            screenRect.Y = (int)(platformBody.GetWorldPoint(Vector2.Zero).Y * 64);
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            parent.SpriteBatch.Begin();
            parent.SpriteBatch.Draw(platformTexture, screenRect, Color.White);
            parent.SpriteBatch.End();
        }
    }
}
