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
    class PowerUp : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 game;
        World world;

        private Texture2D texture;
        private Body collisionBody;
        private EPowerUpType powerUpType;

        public enum EPowerUpType
        {
            MegaJump
        }

        public PowerUp(Game1 game, World world, EPowerUpType powerUpType)
            : base(game)
        {
            this.game = game;
            this.world = world;
            this.powerUpType = powerUpType;

            collisionBody = BodyFactory.CreateCapsule(world, 1.0f, 0.2f, 1.0f);
            collisionBody.Position = new Vector2(20, 20);
            collisionBody.OnCollision += new OnCollisionEventHandler(onPlayerCollision);
            collisionBody.Friction = 0.0f;
            collisionBody.Restitution = 0.0f;
            collisionBody.BodyType = BodyType.Static;
        }

        bool onPlayerCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            return true;
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

        public override void Draw(GameTime gameTime)
        {
            this.game.SpriteBatch.Begin();
            this.game.SpriteBatch.Draw(texture, collisionBody.Position, Color.White);
            this.game.SpriteBatch.End();

            base.Draw(gameTime);
        }

        
    }
}
