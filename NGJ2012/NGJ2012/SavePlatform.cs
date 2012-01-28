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
        Vector2 offsetToWater;

        // Assets
        Texture2D platformTexture;

        public SavePlatform(Game game) : base(game)
        {
            parent = (Game1)game;
            screenRect = new Rectangle(0, 0, 640, 64);

            // Create physical objects
            offsetToWater = new Vector2(0, -1);
            platformBody = BodyFactory.CreateRectangle(parent.World, 10, 1, 1.0f, parent.WaterLayer.Position + offsetToWater);
            platformBody.BodyType = BodyType.Static;
            platformBody.Friction = float.MaxValue;
            platformBody.CollisionCategories = Game1.COLLISION_GROUP_STATIC_OBJECTS;
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
            // Update position of platform linked to water
            platformBody.SetTransform(parent.WaterLayer.Position + offsetToWater, 0);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            Vector2 pos = platformBody.GetWorldPoint(Vector2.Zero);
            Vector2 screenPos = Vector2.Transform(pos, camera);
            screenRect.X = (int)screenPos.X;
            screenRect.Y = (int)screenPos.Y;

            parent.SpriteBatch.Begin();
            parent.SpriteBatch.Draw(platformTexture, screenRect, Color.White);
            parent.SpriteBatch.End();
        }
    }
}
