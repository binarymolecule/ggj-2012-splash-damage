#if DEBUG
//  #define DEBUG_COLLISION
#endif

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
using Utility;


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
        private const float maxSpeed = 32.0f;
        private const float defaultJumpForce = 0.5f;
        private const float timeUntilCanDieAgainReset = 2.0f;

        Game1 parent;
        const float ScalePlayerSprite = 0.25f;

        World world;
        public Body playerCollider;
        public Vector2 cameraPosition;

        private int numberOfLifes;
        float timeUntilCanDieAgain = timeUntilCanDieAgainReset;

        public int NumberOfLifes
        {
            get { return numberOfLifes; }
        }
        private bool didFallSinceLastJump = true;
        private float jumpForce = defaultJumpForce;
        private const float BYTE_FORCE = 300.0f;
        private PowerUp currentlySelectedPowerUp;
        public float floodHeight = 0;
        public bool dead = false;

        public PowerUp CurrentlySelectedPowerUp
        {
            get { return currentlySelectedPowerUp; }
            set { currentlySelectedPowerUp = value; }
        }

        float viewDirection;

        AnimatedSprite playerAnimation;
        int animID_Stand, animID_Walk, animID_Jump, animID_PowerJump, animID_Bite, animID_Killed;

        public PlatformPlayer(Game game, World world)
            : base(game)
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

        public void ResetPlayer(Timer timer = null)
        {
            playerCollider.Enabled = true;
            playerCollider.Position = new Vector2(3, -2 + parent.WaterLayer.Height);
            playerCollider.ResetDynamics();
            playerAnimation.SetAnimation(animID_Stand);
            cameraPosition = Vector2.Zero;
            canJumpBecauseOf.Clear();
            parent.SavePlatform.DisableTriggering();
            dead = false;
            timeUntilCanDieAgain = timeUntilCanDieAgainReset;
            //resetJumpPower();
        }

        List<Fixture> canJumpBecauseOf = new List<Fixture>();

        void PlayerSeparatesFromWorld(Fixture fixtureA, Fixture fixtureB)
        {
            while (canJumpBecauseOf.Contains(fixtureB))
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
            Debug.Print("Fix:" + fixtureB.CollisionCategories);

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
            int frame_Stand = 0, frameNum_Stand = 21;
            for (int i = 0; i < frameNum_Stand; i++)
                playerTextureNames.Add(String.Format("idle_01/jump_idle_20frs_01_{0:0000}", i));
            int frame_Walk = frame_Stand + frameNum_Stand, frameNum_Walk = 20;
            for (int i = 0; i < frameNum_Walk; i++)
                playerTextureNames.Add(String.Format("run_01/run_color_01_{0:0000}", i + 9));
            int frame_Jump = frame_Walk + frameNum_Walk, frameNum_Jump = 31;
            for (int i = 0; i < frameNum_Jump; i++)
                playerTextureNames.Add(String.Format("jump_01/jump_color_30frs_01_{0:0000}", i));
            int frame_PowerJump = frame_Jump + frameNum_Jump, frameNum_PowerJump = 31;
            for (int i = 0; i < frameNum_PowerJump; i++)
                playerTextureNames.Add(String.Format("power_jump_01/power_jump_color_30frs_01_{0:0000}", i));
            int frame_Bite = frame_PowerJump + frameNum_PowerJump, frameNum_Bite = 31;
            for (int i = 0; i < frameNum_Bite; i++)
                playerTextureNames.Add(String.Format("bite_01/bite_color_30frs_01_{0:0000}", i));

            playerAnimation = new AnimatedSprite(parent, "char", playerTextureNames, new Vector2(256, 336));
            animID_Stand = playerAnimation.AddAnimation("stand", frame_Stand, frameNum_Stand, 50, true);
            animID_Walk = playerAnimation.AddAnimation("walk", frame_Walk, frameNum_Walk, 33, true);
            animID_Jump = playerAnimation.AddAnimation("jump", frame_Jump, frameNum_Jump, 33, false);
            animID_PowerJump = playerAnimation.AddAnimation("power_jump", frame_PowerJump, frameNum_PowerJump, 33, false);
            animID_Bite = playerAnimation.AddAnimation("bite", frame_Bite, frameNum_Bite, 33, false);
            animID_Killed = animID_PowerJump; // playerAnimation.AddAnimation("killed", 0, 0, 125, true);
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
            if ((fixture.CollisionCategories & (Game1.COLLISION_GROUP_TETRIS_BLOCKS | Game1.COLLISION_GROUP_STATIC_OBJECTS)) == 0)
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
            if (parent.gameOverLayer.IsActive)
                return;

            timeUntilCanDieAgain -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Process user input
            int msec = gameTime.ElapsedGameTime.Milliseconds;

            bool eatenByWave = (Game as Game1).waveLayer.isCollidingWith(playerCollider.Position);
            if (!dead && timeUntilCanDieAgain<0 && (this.playerCollider.Position.Y > parent.WaterLayer.Height + 1.0f || eatenByWave))
            {
                this.numberOfLifes--;

                if (this.numberOfLifes == 0)
                {
                    parent.gameOverLayer.onGameOver();
                }
                else
                {
                    dead = true;
                    playerCollider.Enabled = false;
                    Game1.Timers.Create(1.5f, false, ResetPlayer);
                    SoundManager.PlaySound("splash");
                    (Game as Game1).SwitchPlayers();
                }
            }

            if (dead)
            {
                floodHeight = MathHelper.Clamp(floodHeight - (float)gameTime.ElapsedGameTime.TotalSeconds * 2f, 0, 1);
            }
            else
            {
                floodHeight = MathHelper.Clamp(floodHeight + (float)gameTime.ElapsedGameTime.TotalSeconds, 0, 1);

                KeyboardState state = Keyboard.GetState();
                GamePadState gstate = GamePad.GetState((Game as Game1).PlayerIdPlatform);
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

                currentRunSpeed *= Math.Max(0.0f, 1.0f - deacceleration * (float)gameTime.ElapsedGameTime.TotalSeconds);
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

            if (playerCollider.LinearVelocity.LengthSquared() > maxSpeed * maxSpeed)
                playerCollider.LinearVelocity = playerCollider.LinearVelocity * (maxSpeed / playerCollider.LinearVelocity.Length());

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
            }

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

        public void clearCurrentPowerUp()
        {
            parent.removePowerUp(currentlySelectedPowerUp);
            this.currentlySelectedPowerUp = null;            
        }

        public void increaseJumpPower(float inc)
        {
            this.jumpForce += inc;
        }

        public void resetJumpPower()
        {
            this.jumpForce = defaultJumpForce;
        }

        public void jump()
        {
            playerCollider.ApplyForce(new Vector2(0, -jumpForce));
            if (jumpForce > defaultJumpForce)
                playerAnimation.SetAnimation(animID_PowerJump);
            else
                playerAnimation.SetAnimation(animID_Jump);

            // TODO Verify this works!
            canJumpBecauseOf.Clear();
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            // Draw animation
            playerAnimation.Draw(parent.TetrisBatch, playerCollider.Position, new Vector2(2, 2));

#if DEBUG_COLLISION
            parent.TetrisBatch.cameraMatrix = camera;
            parent.TetrisBatch.DrawBody(playerCollider);
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
