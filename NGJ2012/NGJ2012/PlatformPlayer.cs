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


namespace NGJ2012
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class PlatformPlayer : DrawableGameComponentExtended
    {
        World world;
        public Body playerCollider;

        /**
         * Maps the powerup type to the number of collected powerups of this type.
         */
        Dictionary<PowerUp.EPowerUpType, int> collectedPowerUps;


        public PlatformPlayer(Game game, World world)
            : base(game)
        {
            this.world = world;
            playerCollider = BodyFactory.CreateCapsule(world, 1.0f, 0.2f, 0.001f);
            playerCollider.Position = new Vector2(13, 2);
            playerCollider.OnCollision += new OnCollisionEventHandler(PlayerCollidesWithWorld);
            playerCollider.OnSeparation += new OnSeparationEventHandler(PlaterSeperatesFromWorld);
            playerCollider.Friction = 0.0f;
            playerCollider.Restitution = 0.0f;
            playerCollider.BodyType = BodyType.Dynamic;
            playerCollider.FixedRotation = true;
            playerCollider.Rotation = 0.0f;
            playerCollider.CollisionCategories = Game1.COLLISION_GROUP_DEFAULT;
            playerCollider.CollidesWith = Game1.COLLISION_GROUP_DEFAULT | Game1.COLLISION_GROUP_STATIC_OBJECTS | Game1.COLLISION_GROUP_TETRIS_BLOCKS;
        }

        List<Fixture> canJumpBecauseOf = new List<Fixture>();

        void PlaterSeperatesFromWorld(Fixture fixtureA, Fixture fixtureB)
        {
            canJumpBecauseOf.Remove(fixtureB);
        }
        bool PlayerCollidesWithWorld(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            Vector2 normal;
            FixedArray2<Vector2> points;
            contact.GetWorldManifold(out normal, out points);
            Debug.Print("Point1:" + points[0].X + "," + points[0].Y);
            Debug.Print("Point2:" + points[1].X + "," + points[1].Y);

            if (normal.Y < 0 && playerCollider.LinearVelocity.Y > -0.01f)
            {
                canJumpBecauseOf.Add(fixtureB);
            }

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
            drawer = new TetrisPieceBatch(GraphicsDevice);
            base.LoadContent();
        }

        float currentRunSpeed;
        float maxRunSpeed = 8.0f;
        float walkModifier;

        float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            if (fixture.Body == playerCollider)
                return -1;
            walkModifier = fraction;
            return 0;
        }

        bool pressedJump = false;
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            float acceleration = 128.0f;
            KeyboardState state = Keyboard.GetState();
            float move = 0;
            if (state.IsKeyDown(Keys.A)) move = -acceleration;
            if (state.IsKeyDown(Keys.D)) move = acceleration;

            currentRunSpeed *= (float)Math.Pow(0.001, gameTime.ElapsedGameTime.TotalSeconds);
            currentRunSpeed += move * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Math.Abs(currentRunSpeed) > maxRunSpeed) currentRunSpeed *= maxRunSpeed / Math.Abs(currentRunSpeed);

            if(Math.Abs(currentRunSpeed) > 0.001f)
            {
                float dir = Math.Sign(currentRunSpeed);
                walkModifier = 1.0f;
                world.RayCast(new RayCastCallback(RayCastCallback), playerCollider.Position + dir* new Vector2(0.2f, 0), playerCollider.Position + dir * new Vector2(0.4f, 0));
                currentRunSpeed *= walkModifier;
            }
            playerCollider.LinearVelocity = new Vector2(currentRunSpeed,playerCollider.LinearVelocity.Y);

            if (state.IsKeyDown(Keys.W))
            {
                if (canJumpBecauseOf.Count > 0 && !pressedJump)
                {
                    playerCollider.ApplyForce(new Vector2(0, -0.5f));
                    pressedJump = true;
                }
            }
            else pressedJump = false;


            base.Update(gameTime);
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            drawer.cameraMatrix = camera;
            drawer.DrawBody(playerCollider);
        }

        public void addPowerUp(PowerUp.EPowerUpType type)
        {
            int amount;
            if (this.collectedPowerUps.TryGetValue(type, out amount))
            {
                this.collectedPowerUps.Add(type, amount + 1);
            } else {
                this.collectedPowerUps.Add(type, 1);
            }
        }
    }
}
