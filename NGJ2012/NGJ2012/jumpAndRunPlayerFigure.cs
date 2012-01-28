using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Contacts;

namespace NGJ2012
{
    class jumpAndRunPlayerFigure : DrawableGameComponent
    {
        private const float ACCEL = 10.0f;
        private const float WIDTH = 10;
        private const float HEIGHT = 10;

        private Game1 game;
        private World world;
        private SpriteBatch spriteBatch;
        private Texture2D texture;

        private Body playerBody;


        public jumpAndRunPlayerFigure(Game1 game, World world, SpriteBatch spriteBatch)
            : base(game)
        {
            this.game = game;
            this.world = world;
            this.spriteBatch = spriteBatch;

            playerBody = BodyFactory.CreateCapsule(world, 1, 45, 10);
            playerBody.BodyType = BodyType.Kinematic;
            playerBody.Position = new Vector2(0.0f, 0.0f);

            playerBody.OnCollision += new OnCollisionEventHandler(player_OnCollision);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Draw(texture, playerBody.Position, null, Color.White, playerBody.Rotation, new Vector2(WIDTH, HEIGHT) / 2.0f, 1.0f, SpriteEffects.None, 0.0f);
            base.Draw(gameTime);
        }

        protected override void LoadContent()
        {
            this.texture = game.Content.Load<Texture2D>("jumpAndRunPlayer");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.W))
            {

            }
            if (keyboard.IsKeyDown(Keys.A))
            {

            }
            if (keyboard.IsKeyDown(Keys.S))
            {

            }
            if (keyboard.IsKeyDown(Keys.D))
            {

            }

            base.Update(gameTime);
        }

        bool player_OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            return true;
        }
    }
}
