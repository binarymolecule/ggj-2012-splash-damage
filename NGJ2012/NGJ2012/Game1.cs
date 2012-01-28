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

namespace NGJ2012
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        World world;
        TetrisPlayer tetris;
        TetrisPieceBatch tetrisBatch;
        PlatformPlayer platform;

        int worldWidthInBlocks = 24;
        int worldHeightInBlocks = 20;
        Body staticWorldGround;
        Body staticWorldL;
        Body staticWorldR;

        public const Category COLLISION_GROUP_DEFAULT = Category.Cat1;
        public const Category COLLISION_GROUP_TETRIS_BLOCKS = Category.Cat2;
        public const Category COLLISION_GROUP_STATIC_OBJECTS = Category.Cat3;

        // Player related components
        jumpAndRunPlayerFigure jumpAndRunPlayer;

        // Public access to world
        public World World { get { return world; } }
        public int WorldWidthInBlocks { get { return worldWidthInBlocks; } }
        public int WorldHeightInBlocks { get { return worldHeightInBlocks; } }

        // Other level components
        public WaterLayer WaterLayer;
        public SavePlatform SavePlatform;

        // GUI components
        public GameStatusLayer StatusLayer { get; protected set; }
        public SpriteBatch SpriteBatch { get { return spriteBatch; } }

        public Vector2 cameraPosition = Vector2.Zero;

        public const float gameBlockSizePlatform = 96.0f;
        public const float gameBlockSizeTetris = 32.0f;

        const int platformModeWidth = 1000;
        const int tetrisModeWidth = 1280 - platformModeWidth;

        RenderTarget2D platformModeLeft;
        RenderTarget2D platformModeRight;

        RenderTarget2D tetrisModeLeft;
        RenderTarget2D tetrisModeRight;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            world = new World(new Vector2(0, 25));

            staticWorldGround = BodyFactory.CreateRectangle(world, worldWidthInBlocks, 1, 1.0f, new Vector2(worldWidthInBlocks / 2.0f, worldHeightInBlocks));
            staticWorldL = BodyFactory.CreateRectangle(world, 1, worldHeightInBlocks, 1.0f, new Vector2(0, worldHeightInBlocks / 2.0f));
            staticWorldR = BodyFactory.CreateRectangle(world, 1, worldHeightInBlocks, 1.0f, new Vector2(worldWidthInBlocks, worldHeightInBlocks / 2.0f));
            staticWorldGround.BodyType = BodyType.Static;
            staticWorldL.BodyType = BodyType.Static;
            staticWorldR.BodyType = BodyType.Static;
            staticWorldGround.Friction = 100.0f;
            staticWorldL.Friction = 100.0f;
            staticWorldR.Friction = 100.0f;
            staticWorldGround.CollisionCategories = COLLISION_GROUP_STATIC_OBJECTS;
            staticWorldL.CollisionCategories = COLLISION_GROUP_STATIC_OBJECTS;
            staticWorldR.CollisionCategories = COLLISION_GROUP_STATIC_OBJECTS;

            tetris = new TetrisPlayer(this, world);
            Components.Add(tetris);

            jumpAndRunPlayer = new jumpAndRunPlayerFigure(this, world, this.spriteBatch);
            Components.Add(platform = new PlatformPlayer(this, world));
            this.Components.Add(jumpAndRunPlayer);

            // Create other level components
            WaterLayer = new WaterLayer(this);
            Components.Add(WaterLayer);
            //SavePlatform = new WaterLayer(this);
            //Components.Add(SavePlatform);

            // Add GUI components
            StatusLayer = new GameStatusLayer(this);
            //Components.Add(StatusLayer);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            platformModeLeft = new RenderTarget2D(GraphicsDevice, platformModeWidth, 720);
            platformModeRight = new RenderTarget2D(GraphicsDevice, platformModeWidth, 720);
            tetrisModeLeft = new RenderTarget2D(GraphicsDevice, tetrisModeWidth, 720);
            tetrisModeRight = new RenderTarget2D(GraphicsDevice, tetrisModeWidth, 720);

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);


            tetrisBatch = new TetrisPieceBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            cameraPosition = 0.9f * cameraPosition + 0.1f * platform.playerCollider.Position;
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(platformModeLeft);
            DrawGameWorldOnce(true);

            GraphicsDevice.SetRenderTarget(tetrisModeLeft);
            DrawGameWorldOnce(false);

            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin();
            spriteBatch.Draw(platformModeLeft, new Rectangle(0, 0, platformModeWidth, 720), Color.White);
            spriteBatch.Draw(tetrisModeLeft, new Rectangle(platformModeWidth, 0, tetrisModeWidth, 720), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawGameWorldOnce(bool platformMode)
        {
            Matrix camera = Matrix.CreateTranslation(-new Vector3(cameraPosition, 0.0f));
            camera *= Matrix.CreateScale(platformMode ? Game1.gameBlockSizePlatform : Game1.gameBlockSizeTetris);
            camera *= Matrix.CreateTranslation(new Vector3(platformMode ? platformModeWidth : tetrisModeWidth, 720, 0.0f) / 2.0f);

            GraphicsDevice.Clear(platformMode ? Color.CornflowerBlue : Color.Coral);
            tetrisBatch.cameraMatrix = camera;
            tetrisBatch.DrawBody(staticWorldGround);
            tetrisBatch.DrawBody(staticWorldL);
            tetrisBatch.DrawBody(staticWorldR);
            foreach (GameComponent c in Components)
            {
                if (c is DrawableGameComponentExtended)
                {
                    (c as DrawableGameComponentExtended).DrawGameWorldOnce(camera, platformMode);
                }
            }
        }
    }
}
