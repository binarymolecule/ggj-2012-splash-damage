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
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics.Joints;


namespace NGJ2012
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class PlatformPlayer : Microsoft.Xna.Framework.DrawableGameComponent
    {
        World world;
        Body playerCollider;

        public PlatformPlayer(Game game, World world)
            : base(game)
        {
            this.world = world;
            playerCollider = BodyFactory.CreateCapsule(world, 1.0f, 0.2f, 1.0f);
            playerCollider.Position = new Vector2(13, 2);
            playerCollider.OnCollision += new OnCollisionEventHandler(PlayerCollidesWithWorld);
            playerCollider.OnSeparation += new OnSeparationEventHandler(PlaterSeperatesFromWorld);
            playerCollider.Friction = 0.0f;
            playerCollider.Restitution = 0.0f;
            playerCollider.BodyType = BodyType.Dynamic;
            JointFactory.CreateFixedAngleJoint(world, playerCollider);
        }

        bool wallL, wallR;
        void PlaterSeperatesFromWorld(Fixture fixtureA, Fixture fixtureB)
        {
            
        }
        bool PlayerCollidesWithWorld(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            return true;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        TetrisPieceBatch drawer;
        protected override void LoadContent()
        {
            drawer = new TetrisPieceBatch(GraphicsDevice, Matrix.CreateScale(32.0f));
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            KeyboardState state = Keyboard.GetState();
            Vector2 move = Vector2.Zero;
            if (state.IsKeyDown(Keys.A)) move.X -= 1;
            if (state.IsKeyDown(Keys.D)) move.X += 1;
            if (state.IsKeyDown(Keys.W)) move.Y -= 1;

            playerCollider.LinearVelocity = new Vector2(move.X * 8.0f, playerCollider.LinearVelocity.Y + move.Y);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            drawer.DrawBody(playerCollider);
            base.Draw(gameTime);
        }
    }
}
