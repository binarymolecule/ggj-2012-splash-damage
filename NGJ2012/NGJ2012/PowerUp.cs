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
using FarseerPhysics.Dynamics.Contacts;

namespace NGJ2012
{
    public class PowerUp : DrawableGameComponentExtended
    {
        private const float JUMP_INCREASE = 0.3f;

        Game1 game;
        World world;

        private AnimatedSprite animation;

        // Public getter used for displaying items in GUI
        public Texture2D Texture { get { return animation.CurrentTexture; } }

        private Body collisionBody;
        private EPowerUpType powerUpType;
        private bool usageTimerRunning = false;
        private double remainingPowerUpTimeInSecs = 5;
        private const double START_BLINKING_FAST= 2;

        private const double BLINK_SLOW = 60.0f;
        private const double BLINK_FAST = 20.0f;

        public bool invisibleBecauseBlinking = false;

        private bool isUsedOnCollectingAndHasNoDuration;

        public enum EPowerUpType
        {
            MegaJump, ExtraLife, WaterProof
        }

        public PowerUp(Game1 game, World world, EPowerUpType powerUpType, Vector2 position)
            : base(game)
        {
            this.game = game;
            this.world = world;
            this.powerUpType = powerUpType;


            isUsedOnCollectingAndHasNoDuration = (powerUpType == EPowerUpType.ExtraLife);

            //Physics:
            collisionBody = BodyFactory.CreateCircle(world, 0.5f, 1.0f);
            collisionBody.Position = position;
            collisionBody.OnCollision += new OnCollisionEventHandler(onPlayerCollision);
            collisionBody.BodyType = BodyType.Kinematic;
            collisionBody.CollisionCategories = Category.Cat1;
            collisionBody.CollidesWith = Category.Cat1;
            collisionBody.IsSensor = true;
        }

        public static PowerUp getRandomPowerUp(Game1 game, World world, Vector2 position) 
        {
            Type type = typeof(EPowerUpType);
            String[] enumArray = (String[])Enum.GetNames(type);
            int random = (new Random()).Next(enumArray.Length);
            return new PowerUp(game, world, (EPowerUpType)Enum.Parse(type, enumArray[random]), position);
        }

        bool onPlayerCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            if (this.Visible)
            {
                if (this.isUsedOnCollectingAndHasNoDuration)
                {
                    this.use();
                }
                else
                {
                    game.PlatformPlayer.addPowerUp(this);
                }
                SoundManager.PlaySound("collect_powerup");

                this.Visible = false;
                delme();
            }

            return false;
        }

        bool deleted = false;
        public void delme()
        {
            if (deleted) return;
            deleted = true;
            world.RemoveBody(collisionBody);
        }

        protected override void LoadContent()
        {
            // Create animations for power ups
            List<String> animationNames = new List<String> { "collectables/powerjump/powerjump/powerjump_01_0001", "collectables/life_up/lifeup_01_0001", "PowerUp_Star", "collectables/water/water_01_0001" };
            animation = new AnimatedSprite(game, "", animationNames, new Vector2(20, 30));
            animation.AddAnimation("jump", 0, 1, 125, true);
            animation.AddAnimation("life", 1, 1, 125, true);
            animation.AddAnimation("star", 2, 1, 125, true);
            animation.AddAnimation("waterproof", 3, 1, 125, true);

            switch (this.powerUpType)
            {
                case EPowerUpType.MegaJump:
                    animation.SetAnimation("jump");
                    break;
                case EPowerUpType.ExtraLife:
                    animation.SetAnimation("life");
                    break;
                case EPowerUpType.WaterProof:
                    animation.SetAnimation("waterproof");
                    break;
                default:
                    animation.SetAnimation("star");
                    break;
            }
            
            
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (usageTimerRunning) {
                this.remainingPowerUpTimeInSecs -= gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (remainingPowerUpTimeInSecs <= 0) this.onPowerUpExhausted();
            else
            {
                if (remainingPowerUpTimeInSecs >= START_BLINKING_FAST) this.invisibleBecauseBlinking = (remainingPowerUpTimeInSecs * 100.0f) % BLINK_SLOW > BLINK_SLOW / 2;
                else this.invisibleBecauseBlinking = (remainingPowerUpTimeInSecs * 100.0f) % BLINK_FAST > BLINK_FAST / 2;
            }

            

            base.Update(gameTime);
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            if (this.Visible)
            {
                animation.Draw(game.TetrisBatch, collisionBody.WorldCenter, new Vector2(0.5f, 0.5f));

#if DEBUG_COLLISION
                this.game.TetrisBatch.DrawBody(collisionBody);
#endif
            }
        }

        public void use()
        {
            if(!usageTimerRunning)
            {
                if (!isUsedOnCollectingAndHasNoDuration) usageTimerRunning = true;

                switch (this.powerUpType)
                {
                    case EPowerUpType.MegaJump:
                        game.PlatformPlayer.increaseJumpPower(JUMP_INCREASE);
                        break;

                    case EPowerUpType.WaterProof:
                        game.PlatformPlayer.autoJump = true;
                        break;

                    case EPowerUpType.ExtraLife:
                        game.PlatformPlayer.increaseLifes();
                        break;
                }
            }
        }

        private void onPowerUpExhausted()
        {
            switch (this.powerUpType)
            {
                case EPowerUpType.MegaJump:
                    game.PlatformPlayer.resetJumpPower();
                    break;
                case EPowerUpType.WaterProof:
                    game.PlatformPlayer.autoJump = false;
                    break;
                case EPowerUpType.ExtraLife:
                    //Nothing to be done
                    break;
            }

            game.PlatformPlayer.clearCurrentPowerUp();
        }

        public bool UsageTimerRunning
        {
            get { return usageTimerRunning; }
        }

        public String getRemainingPowerUpTimeInSecsFixedPoint()
        {
            return string.Format("{0:f}", remainingPowerUpTimeInSecs);
        }

        public EPowerUpType PowerUpType
        {
            get { return powerUpType; }
        }


        public Vector2 Position { get { return collisionBody.Position; } }
    }
}
