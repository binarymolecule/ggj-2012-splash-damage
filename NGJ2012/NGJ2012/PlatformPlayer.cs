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
        private const float acceleration = 512.0f;
        private const float deacceleration = 256.0f;
        private const float maxRunSpeed = 8.0f;

        private const int JUMP_COOLDOWN_TIME = 500;  // wait 500 msec until next jump

        Game1 parent;

        World world;
        public Body playerCollider;
        public Vector2 cameraPosition = Vector2.Zero;
        
        private int numberOfLifes;

        public int NumberOfLifes
        {
            get { return numberOfLifes; }
        }
        private int jumpCooldownTime = 0;
        private float jumpForce = 0.5f;
        private const float BYTE_FORCE = 500.0f;
        private PowerUp currentlySelectedPowerUp;

        public PowerUp CurrentlySelectedPowerUp
        {
            get { return currentlySelectedPowerUp; }
            set { currentlySelectedPowerUp = value; }
        }

        float viewDirection;

        AnimatedSprite playerAnimation;
        int animID_Stand, animID_Walk, animID_Idle, animID_Jump, animID_Fall, animID_Hit;

        public PlatformPlayer(Game game, World world) : base(game)
        {
            this.world = world;
            parent = (Game1)game;

            playerCollider = BodyFactory.CreateCapsule(world, 1.0f, 0.2f, 0.001f);
            playerCollider.Position = new Vector2(2, -2);
            playerCollider.OnCollision += new OnCollisionEventHandler(PlayerCollidesWithWorld);
            //playerCollider.OnSeparation += new OnSeparationEventHandler(PlayerSeparatesFromWorld);
            playerCollider.Friction = 0.0f;
            playerCollider.Restitution = 0.0f;
            playerCollider.BodyType = BodyType.Dynamic;
            playerCollider.FixedRotation = true;
            playerCollider.Rotation = 0.0f;
            playerCollider.CollisionCategories = Game1.COLLISION_GROUP_DEFAULT;
            playerCollider.CollidesWith = Game1.COLLISION_GROUP_DEFAULT | Game1.COLLISION_GROUP_STATIC_OBJECTS | Game1.COLLISION_GROUP_TETRIS_BLOCKS;

            numberOfLifes = INITIAL_NUMBER_OF_LIFES;
        }

        /*
        List<Fixture> canJumpBecauseOf = new List<Fixture>();

        void PlayerSeparatesFromWorld(Fixture fixtureA, Fixture fixtureB)
        {
            canJumpBecauseOf.Remove(fixtureB);
        }
        */

        bool PlayerCollidesWithWorld(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if ((fixtureB.CollisionCategories & (Game1.COLLISION_GROUP_TETRIS_BLOCKS | Game1.COLLISION_GROUP_STATIC_OBJECTS)) == 0) return true;
            /*
            Vector2 normal;
            FixedArray2<Vector2> points;
            contact.GetWorldManifold(out normal, out points);
            Debug.Print("Point1:" + points[0].X + "," + points[0].Y);
            Debug.Print("Point2:" + points[1].X + "," + points[1].Y);

            if (normal.Y < 0 && playerCollider.LinearVelocity.Y > -0.01f)
            {
                canJumpBecauseOf.Add(fixtureB);
            }
            */
            return true;
        }
        
        protected bool canJump()
        {
            Debug.WriteLine("Velocity: X={0} , Y={1}", playerCollider.LinearVelocity.X, playerCollider.LinearVelocity.Y);

            return Math.Abs(playerCollider.LinearVelocity.Y) < 0.015f;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        TetrisPieceBatch drawer;
        protected override void LoadContent()
        {
            drawer = new TetrisPieceBatch(GraphicsDevice, Game.Content);

            // Create player animation
            string[] playerTextureNames = new string[] { "jumpAndRunPlayer" };
            playerAnimation = new AnimatedSprite(parent, "", playerTextureNames, new Vector2(36, 32));
            animID_Stand = playerAnimation.AddAnimation("stand", 0, 0, 125, true);
            animID_Walk = playerAnimation.AddAnimation("walk", 0, 0, 125, true);
            animID_Idle = playerAnimation.AddAnimation("idle", 0, 0, 125, true);
            animID_Jump = playerAnimation.AddAnimation("jump", 0, 0, 125, true);
            animID_Fall = playerAnimation.AddAnimation("fall", 0, 0, 125, true);
            animID_Hit = playerAnimation.AddAnimation("hit", 0, 0, 125, true);
            playerAnimation.SetAnimation(animID_Stand);
            
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
        
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Process user input
            int msec = gameTime.ElapsedGameTime.Milliseconds;

            KeyboardState state = Keyboard.GetState();
            float move = 0;            
            if (state.IsKeyDown(Keys.A)) move = -acceleration;
            if (state.IsKeyDown(Keys.D)) move = acceleration;

            currentRunSpeed *= Math.Max(0.0f, 1.0f - deacceleration*(float)gameTime.ElapsedGameTime.TotalSeconds);
            currentRunSpeed += move * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Math.Abs(currentRunSpeed) > maxRunSpeed)
                currentRunSpeed *= maxRunSpeed / Math.Abs(currentRunSpeed);

            bool isRunning = false;
            if (Math.Abs(currentRunSpeed) > 0.001f)
            {
                viewDirection = Math.Sign(currentRunSpeed);
                walkModifier = 1.0f;
                world.RayCast(new RayCastCallback(RayCastCallback), playerCollider.Position + viewDirection * new Vector2(0.2f, 0), playerCollider.Position + viewDirection * new Vector2(0.4f, 0));
                currentRunSpeed *= walkModifier;
                isRunning = true;
            }
            else
                currentRunSpeed = 0;

            playerCollider.LinearVelocity = new Vector2(currentRunSpeed, playerCollider.LinearVelocity.Y);

            // Switch to walking animation
            bool isFloating = !canJump();
            if (isFloating)
            {
                if (playerCollider.LinearVelocity.Y > 0)
                    playerAnimation.SetAnimation(animID_Fall);
            }
            else if (isRunning)
                playerAnimation.SetAnimation(animID_Walk);

            if (jumpCooldownTime > 0)
                jumpCooldownTime -= msec;
            if (state.IsKeyDown(Keys.W))
            {
                if (jumpCooldownTime <= 0 && canJump())
                {
                    jump();
                    jumpCooldownTime = JUMP_COOLDOWN_TIME; // wait some time until next jump
                } 
            }

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

            if (state.IsKeyDown(Keys.Enter) || state.IsKeyDown(Keys.E)) usePowerUp();

            if (state.IsKeyDown(Keys.F)) bite();

            // Update player animation
            playerAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);

            base.Update(gameTime);
        }

        private void bite()
        {
            world.RayCast(new RayCastCallback(biteRayCastCallback), playerCollider.Position + viewDirection * new Vector2(0.2f, 0), playerCollider.Position + viewDirection * new Vector2(0.6f, 0));
        }

        float biteRayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            if (fixture.CollisionCategories == Game1.COLLISION_GROUP_TETRIS_BLOCKS)
            {
                fixture.Body.ApplyForce(new Vector2(-this.viewDirection * BYTE_FORCE, -BYTE_FORCE));

                //Stop raytracing
                return 0;
            }
            else
            {
                //Continue raytracing:
                return -1;
            }
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
            playerAnimation.SetAnimation(animID_Jump);
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            // Draw animation
            Vector2 screenPos = Vector2.Transform(playerCollider.Position, camera);
            parent.SpriteBatch.Begin();
            playerAnimation.Draw(parent.SpriteBatch, screenPos, platformMode ? Game1.ScalePlatformSprites : Game1.ScaleTetrisSprites);
            parent.SpriteBatch.End();

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
