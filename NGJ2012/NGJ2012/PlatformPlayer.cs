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
        private const int INITIAL_NUMBER_OF_LIFES = 3;
        private const float acceleration = 128.0f;
        private const float maxRunSpeed = 8.0f;

        World world;
        public Body playerCollider;
        public Vector2 cameraPosition = Vector2.Zero;

        private int numberOfLifes;

        public int NumberOfLifes
        {
            get { return numberOfLifes; }
        }
        private float jumpForce = 0.5f;
        private PowerUp currentlySelectedPowerUp;

        public PowerUp CurrentlySelectedPowerUp
        {
            get { return currentlySelectedPowerUp; }
            set { currentlySelectedPowerUp = value; }
        }

        Texture2D playerTexture;

        public PlatformPlayer(Game game, World world) : base(game)
        {
            this.world = world;

            playerCollider = BodyFactory.CreateCapsule(world, 1.0f, 0.2f, 0.001f);
            playerCollider.Position = new Vector2(2, -2);
            playerCollider.OnCollision += new OnCollisionEventHandler(PlayerCollidesWithWorld);
            playerCollider.OnSeparation += new OnSeparationEventHandler(PlaterSeperatesFromWorld);
            playerCollider.Friction = 0.0f;
            playerCollider.Restitution = 0.0f;
            playerCollider.BodyType = BodyType.Dynamic;
            playerCollider.FixedRotation = true;
            playerCollider.Rotation = 0.0f;
            playerCollider.CollisionCategories = Game1.COLLISION_GROUP_DEFAULT;
            playerCollider.CollidesWith = Game1.COLLISION_GROUP_DEFAULT | Game1.COLLISION_GROUP_STATIC_OBJECTS | Game1.COLLISION_GROUP_TETRIS_BLOCKS;

            numberOfLifes = INITIAL_NUMBER_OF_LIFES;
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
            drawer = new TetrisPieceBatch(GraphicsDevice, Game.Content);
            playerTexture = Game.Content.Load<Texture2D>("jumpAndRunPlayer");
            base.LoadContent();
        }

        float currentRunSpeed;
        float walkModifier;

        float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            if (fixture.Body == playerCollider)
                return -1;
            if ((fixture.CollisionCategories & Game1.COLLISION_GROUP_LEVEL_SEPARATOR) != 0) 
                return 1;
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
                    jump();
                    pressedJump = true;
                }
            }
            else pressedJump = false;

            if (playerCollider.Position.X < 0)
            {
                playerCollider.Position = new Vector2(playerCollider.Position.X + (float)Game1.worldWidthInBlocks, playerCollider.Position.Y);
                cameraPosition.X += Game1.worldWidthInBlocks;
            }
            if (playerCollider.Position.X > Game1.worldWidthInBlocks)
            {
                playerCollider.Position = new Vector2(playerCollider.Position.X - (float)Game1.worldWidthInBlocks, playerCollider.Position.Y);
                cameraPosition.X -= Game1.worldWidthInBlocks;
            }
            cameraPosition = 0.9f * cameraPosition + 0.1f * playerCollider.Position;

            if (state.IsKeyDown(Keys.Enter)) usePowerUp();


            base.Update(gameTime);
        }

        private void usePowerUp()
        {
            if (this.currentlySelectedPowerUp != null)
            {
                this.currentlySelectedPowerUp.use();
            }
        }

        public void clearCurrentPowerUp() {
            this.currentlySelectedPowerUp = null;
        }

        public void increaseJumpPower(float inc) {
            this.jumpForce += inc;
        }

        public void jump()
        {
            playerCollider.ApplyForce(new Vector2(0, -jumpForce));
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            Vector2 screenPos = Vector2.Transform(playerCollider.Position, camera);
            Rectangle screenRect = new Rectangle((int)screenPos.X, (int)screenPos.Y, 64, 64);
#if DEBUG
            drawer.cameraMatrix = camera;
            drawer.DrawBody(playerCollider);
#endif
        }

        public void addPowerUp(PowerUp powerup)
        {
            this.currentlySelectedPowerUp = powerup;
        }

        internal void increaseLifes()
        {
            this.numberOfLifes++;
        }
    }
}
