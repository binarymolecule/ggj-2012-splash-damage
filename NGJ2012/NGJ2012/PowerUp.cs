﻿using System;
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
        private double actionEnabledTimeSecs = 3;

        public EPowerUpType PowerUpType
        {
            get { return powerUpType; }
        }

        public enum EPowerUpType
        {
            MegaJump, ExtraLife
        }

        public PowerUp(Game1 game, World world, EPowerUpType powerUpType, Vector2 position)
            : base(game)
        {
            this.game = game;
            this.world = world;
            this.powerUpType = powerUpType;

            //Physics:
            collisionBody = BodyFactory.CreateCircle(world, 0.5f, 1.0f);
            collisionBody.Position = position;
            collisionBody.OnCollision += new OnCollisionEventHandler(onPlayerCollision);
            collisionBody.BodyType = BodyType.Kinematic;
            collisionBody.CollisionCategories = Category.Cat1;
            collisionBody.CollidesWith = Category.Cat1;
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
                switch (this.powerUpType)
                {
                    case EPowerUpType.MegaJump:
                        game.PlatformPlayer.addPowerUp(this);
                        break;
                    case EPowerUpType.ExtraLife:
                        this.use();
                        break;
                }

                this.Visible = false;
            }

            return false;
        }            

        protected override void LoadContent()
        {
            // Create animations for power ups
            string[] animationNames = new string[] { "PowerUp_Jump", "PowerUp_Life", "PowerUp_Star" };
            animation = new AnimatedSprite(game, "", animationNames, new Vector2(20, 30));
            animation.AddAnimation("jump", 0, 0, 125, true);
            animation.AddAnimation("life", 0, 0, 125, true);
            animation.AddAnimation("star", 0, 0, 125, true);

            switch (this.powerUpType)
            {
                case EPowerUpType.MegaJump:
                    animation.SetAnimation("jump");
                    break;
                case EPowerUpType.ExtraLife:
                    animation.SetAnimation("life");
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
                this.actionEnabledTimeSecs -= gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (actionEnabledTimeSecs <= 0) this.onPowerUpExhausted();

            base.Update(gameTime);
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            if (this.Visible)
            {
                this.game.SpriteBatch.Begin();
                animation.Draw(this.game.SpriteBatch, Vector2.Transform(collisionBody.Position, camera),
                               platformMode ? Game1.ScalePlatformSprites : Game1.ScaleTetrisSprites);
                this.game.SpriteBatch.End();
            }
        }

        public void use()
        {
            if(!usageTimerRunning)
            {
                usageTimerRunning = true;

                switch (this.powerUpType)
                {
                    case EPowerUpType.MegaJump:
                        game.PlatformPlayer.increaseJumpPower(JUMP_INCREASE);
                        break;

                    case EPowerUpType.ExtraLife:
                        game.PlatformPlayer.increaseLifes();
                        onPowerUpExhausted();
                        break;
                }
            }
        }

        private void onPowerUpExhausted()
        {
            switch (this.powerUpType)
            {
                case EPowerUpType.MegaJump:
                    game.PlatformPlayer.increaseJumpPower(-JUMP_INCREASE);
                    break;
                case EPowerUpType.ExtraLife:
                    //Nothing TODO.
                    break;
            }

            game.PlatformPlayer.clearCurrentPowerUp();
            game.Components.Remove(this);
        }

        
    }
}
