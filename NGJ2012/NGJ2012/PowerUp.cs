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
using System.Diagnostics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;

namespace NGJ2012
{
    public class PowerUp : DrawableGameComponentExtended
    {
        Game1 game;
        World world;

        private Texture2D texture;
        private Body collisionBody;
        private EPowerUpType powerUpType;

        public enum EPowerUpType
        {
            MegaJump, ExtraLive
        }

        public PowerUp(Game1 game, World world, EPowerUpType powerUpType, Vector2 position)
            : base(game)
        {
            this.game = game;
            this.world = world;
            this.powerUpType = powerUpType;

            collisionBody = BodyFactory.CreateCircle(world, 0.5f, 1.0f);
            collisionBody.Position = position;
            collisionBody.OnCollision += new OnCollisionEventHandler(onPlayerCollision);
            collisionBody.BodyType = BodyType.Kinematic;
            collisionBody.CollisionCategories = Category.Cat1;
            collisionBody.CollidesWith = Category.Cat1;
        }

        bool onPlayerCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            switch (this.powerUpType)
            {
                case EPowerUpType.MegaJump:
                    break;
                case EPowerUpType.ExtraLive:
                    break;
            }

            game.Components.Remove(this);

            return false;
        }            

        protected override void LoadContent()
        {
            this.texture = game.Content.Load<Texture2D>("Star");
            
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            this.game.SpriteBatch.Begin();
            this.game.SpriteBatch.Draw(texture, Vector2.Transform(collisionBody.Position, camera), Color.White);
            this.game.SpriteBatch.End();
        }

        
    }
}
