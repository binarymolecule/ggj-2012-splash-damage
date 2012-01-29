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

        Game1 parent;
        const float ScalePlayerSprite = 0.25f;

        World world;
        public Body playerCollider;
        public Vector2 cameraPosition;
        
        private int numberOfLifes;

        public int NumberOfLifes
        {
            get { return numberOfLifes; }
        }
        private bool didFallSinceLastJump = true;
        private float jumpForce = 0.5f;
        private const float BYTE_FORCE = 300.0f;
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
            playerCollider.OnCollision += new OnCollisionEventHandler(PlayerCollidesWithWorld);
            playerCollider.OnSeparation += new OnSeparationEventHandler(PlayerSeparatesFromWorld);
            playerCollider.Friction = 0.0f;
            playerCollider.Restitution = 0.0f;
            playerCollider.BodyType = BodyType.Dynamic;
            playerCollider.FixedRotation = true;
            playerCollider.Rotation = 0.0f;
            playerCollider.CollisionCategories = Game1.COLLISION_GROUP_DEFAULT;
            playerCollider.CollidesWith = Game1.COLLISION_GROUP_DEFAULT | Game1.COLLISION_GROUP_STATIC_OBJECTS | Game1.COLLISION_GROUP_TETRIS_BLOCKS;

            numberOfLifes = INITIAL_NUMBER_OF_LIFES;
        }

        public void ResetPlayer()
        {
            playerCollider.Position = new Vector2(3, -2 + parent.WaterLayer.Height);
            playerAnimation.SetAnimation(animID_Stand);
            cameraPosition = Vector2.Zero;
            canJumpBecauseOf.Clear();
            parent.SavePlatform.DisableTriggering();
        }

        List<Fixture> canJumpBecauseOf = new List<Fixture>();

        void PlayerSeparatesFromWorld(Fixture fixtureA, Fixture fixtureB)
        {
            while(canJumpBecauseOf.Contains(fixtureB))
               canJumpBecauseOf.Remove(fixtureB);
        }
        bool PlayerCollidesWithWorld(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            // Try to trigger save platform if player collides with it
            if (fixtureB.CollisionCategories == Game1.COLLISION_GROUP_STATIC_OBJECTS &&
                fixtureB.Body == parent.SavePlatform.Body)
            {
                parent.SavePlatform.Trigger();
            }
            if ((fixtureB.CollisionCategories & (Game1.COLLISION_GROUP_TETRIS_BLOCKS | Game1.COLLISION_GROUP_STATIC_OBJECTS)) == 0) return true;

            Vector2 normal;
            FixedArray2<Vector2> points;
            contact.GetWorldManifold(out normal, out points);
            Debug.Print("Point1:" + points[0].X + "," + points[0].Y);
            Debug.Print("Point2:" + points[1].X + "," + points[1].Y);

            if (normal.Y < 0)
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
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create player animation
            List<String> playerTextureNames = new List<String>();
            for (int i = 0; i < 9; i++)
                playerTextureNames.Add(String.Format("run_start/run_start_1_{0:0000}", i));
            for (int i = 9; i < 29; i++)
                playerTextureNames.Add(String.Format("run_loop_02/run_loop_02_{0:0000}", i));
            
            playerAnimation = new AnimatedSprite(parent, "char", playerTextureNames, new Vector2(256, 336));
            animID_Stand = playerAnimation.AddAnimation("stand", 0, 0, 125, true);
            //animID_Walk = playerAnimation.AddAnimation("walk", 0, 29, 50, 9);
            animID_Walk = playerAnimation.AddAnimation("walk", 9, 20, 40, true);
            animID_Idle = playerAnimation.AddAnimation("idle", 0, 3, 125, true);
            animID_Jump = playerAnimation.AddAnimation("jump", 0, 9, 40, false);
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
        bool canJump = false;
        float RayCastCallbackJump(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            if (fixture.Body == playerCollider)
                return -1;
            if ((fixture.CollisionCategories & (Game1.COLLISION_GROUP_TETRIS_BLOCKS|Game1.COLLISION_GROUP_STATIC_OBJECTS)) == 0)
                return 1;
            if (normal.Y > 0.01) 
                return 1;
            canJump = true;
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

            if (this.playerCollider.Position.Y > parent.WaterLayer.Height || this.playerCollider.Position.X < parent.waveLayer.getRightBorder())
            {
                this.numberOfLifes--;

                if (this.numberOfLifes == 0)
                {
                    //TODO: Gameover
                }
                else
                {
                    ResetPlayer();
                    //TODO Switch players:
                }
            }

            KeyboardState state = Keyboard.GetState();
            GamePadState gstate = GamePad.GetState(PlayerIndex.One);
            float move = 0;
            if (state.IsKeyDown(Keys.A))
            {
                move = -acceleration;
                playerAnimation.Flipped = true;
            }
            if (state.IsKeyDown(Keys.D))
            {
                move = acceleration;
                playerAnimation.Flipped = false;
            }
            if (gstate.IsConnected)
            {
                move = gstate.ThumbSticks.Left.X * Math.Abs(gstate.ThumbSticks.Left.X) * acceleration;
                playerAnimation.Flipped = move < 0;
            }

            currentRunSpeed *= Math.Max(0.0f, 1.0f - deacceleration*(float)gameTime.ElapsedGameTime.TotalSeconds);
            currentRunSpeed += move * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Math.Abs(currentRunSpeed) > maxRunSpeed)
                currentRunSpeed *= maxRunSpeed / Math.Abs(currentRunSpeed);

            bool isRunning = false;
            if (Math.Abs(currentRunSpeed) > 0.001f)
            {
                viewDirection = Math.Sign(currentRunSpeed);
                isRunning = true;
            }
            else
                currentRunSpeed = 0;

            float runSpeedScaleDueToVertical = 1.0f;// (float)Math.Sqrt(Math.Max(0, maxRunSpeed * maxRunSpeed - playerCollider.LinearVelocity.Y * playerCollider.LinearVelocity.Y);
            playerCollider.LinearVelocity = new Vector2(currentRunSpeed * runSpeedScaleDueToVertical, playerCollider.LinearVelocity.Y);

            if (playerCollider.LinearVelocity.Y > 0)
                didFallSinceLastJump = true;

            canJump = canJumpBecauseOf.Count > 0;
            if (canJump && didFallSinceLastJump)
            {
                if (isRunning)
                    playerAnimation.SetAnimation(animID_Walk);
                else
                    playerAnimation.SetAnimation(animID_Stand);
            }

            if (state.IsKeyDown(Keys.W) || gstate.IsButtonDown(Buttons.A))
            {
                if (didFallSinceLastJump)
                {
                    //canJump = false;
                    //world.RayCast(new RayCastCallback(RayCastCallbackJump), playerCollider.Position, playerCollider.Position + new Vector2(0, 1.0f));
                    canJump = canJumpBecauseOf.Count > 0 || Math.Abs(playerCollider.LinearVelocity.Y) < 0.01;
                    if (canJump)
                    {
                        jump();
                        didFallSinceLastJump = false;
                    }
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

            cameraPosition = 0.5f * cameraPosition + 0.5f * playerCollider.Position;

            if (state.IsKeyDown(Keys.Enter) || state.IsKeyDown(Keys.E) || gstate.IsButtonDown(Buttons.B)) usePowerUp();

            if (state.IsKeyDown(Keys.F) || gstate.IsButtonDown(Buttons.X)) bite();

            // Update player animation
            playerAnimation.Update(gameTime.ElapsedGameTime.Milliseconds);

            base.Update(gameTime);
        }

        private void bite()
        {
            //Check for objects slightly above or below to get also objects that are not directly on the height of your head:
            world.RayCast(new RayCastCallback(biteRayCastCallback), playerCollider.Position, playerCollider.Position + new Vector2(0.6f * viewDirection, -0.5f));
            world.RayCast(new RayCastCallback(biteRayCastCallback), playerCollider.Position, playerCollider.Position + new Vector2(0.6f * viewDirection, 0.0f));
            world.RayCast(new RayCastCallback(biteRayCastCallback), playerCollider.Position, playerCollider.Position + new Vector2(0.6f * viewDirection, +0.5f));
        }

        float biteRayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction)
        {
            if (fixture.CollisionCategories == Game1.COLLISION_GROUP_TETRIS_BLOCKS)
            {
                parent.TetrisPlayer.reactiveAllPieces();
                fixture.Body.ApplyForce(new Vector2(-this.viewDirection * BYTE_FORCE, -BYTE_FORCE));
              
                //Stop raytracing
                return 0;
            } else {
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
            parent.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            playerAnimation.Draw(parent.SpriteBatch, screenPos, 0.25f);
            parent.SpriteBatch.End();

#if DEBUG
            parent.DebugDrawer.cameraMatrix = camera;
            parent.DebugDrawer.DrawBody(playerCollider);
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
