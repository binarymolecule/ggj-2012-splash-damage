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

        KeyboardState prevKeyboardState;
        GamePadState prevGamepadState;

        World world;
        TetrisPlayer tetris;

        public TetrisPlayer TetrisPlayer { get { return tetris; } }
        TetrisPieceBatch tetrisBatch;
        PlatformPlayer platform;
        public PlatformPlayer PlatformPlayer { get { return platform; } }

        Body staticWorldGround;
        Body staticWorldL;
        Body staticWorldR;
        public const int worldWidthInBlocks = 24;
        public const int worldHeightInBlocks = 20;

        public const float ScalePlatformSprites = 1.0f;
        public const float ScaleTetrisSprites = 0.25f;

        public const Category COLLISION_GROUP_DEFAULT = Category.Cat1;
        public const Category COLLISION_GROUP_TETRIS_BLOCKS = Category.Cat2;
        public const Category COLLISION_GROUP_STATIC_OBJECTS = Category.Cat3;
        public const Category COLLISION_GROUP_LEVEL_SEPARATOR = Category.Cat4;

        public readonly static Utility.TimerCollection Timers = new Utility.TimerCollection();

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

        public const float gameBlockSizePlatform = 64.0f;
        public const float gameBlockSizeTetris = 32.0f;

        public const int platformModeWidth = 850;
        public const int tetrisModeWidth = 1280 - platformModeWidth;

        RenderTarget2D platformModeLeft;
        RenderTarget2D platformModeRight;

        RenderTarget2D tetrisModeLeft;
        RenderTarget2D tetrisModeRight;

        public float gameProgress = 0;
        float gameProgressSpeed = 1;
        float tetrisProgressAdd = 10;
        private GameViewport tetrisViewport;
        private GameViewport platformViewport;

        //Power-Ups:
        private const float TIME_BETWEEN_POWERUPSPAWNS_SECS = 3.0f;
        private const int SPAWNHEIGHT_OF_PWUP_ABOVE_PLAYER = 2;
        private float elapsedTimeSinceLastPowerUp = 0.0f;

#if DEBUG
        public Vector2 manualPosition = Vector2.Zero;
        public TetrisPieceBatch DebugDrawer;
#endif

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
            world = new World(new Vector2(0, 25));

            staticWorldGround = BodyFactory.CreateRectangle(world, worldWidthInBlocks, 1, 1.0f, new Vector2(worldWidthInBlocks / 2.0f, 0));
            staticWorldL = BodyFactory.CreateRectangle(world, 4, worldHeightInBlocks, 1.0f, new Vector2(2.0f, -worldHeightInBlocks / 2.0f));
            staticWorldR = BodyFactory.CreateRectangle(world, 1, worldHeightInBlocks, 1.0f, new Vector2(worldWidthInBlocks, -worldHeightInBlocks / 2.0f));
            staticWorldGround.BodyType = BodyType.Static;
            staticWorldL.BodyType = BodyType.Static;
            staticWorldR.BodyType = BodyType.Static;
            staticWorldGround.Friction = 100.0f;
            staticWorldL.Friction = 100.0f;
            staticWorldR.Friction = 100.0f;
            staticWorldGround.CollisionCategories = COLLISION_GROUP_STATIC_OBJECTS;
            staticWorldL.CollisionCategories = COLLISION_GROUP_LEVEL_SEPARATOR;
            staticWorldR.CollisionCategories = COLLISION_GROUP_LEVEL_SEPARATOR;

            tetris = new TetrisPlayer(this, world);
            Components.Add(tetris);

            Components.Add(platform = new PlatformPlayer(this, world));

            // Create other level components
            WaterLayer = new WaterLayer(this);
            Components.Add(WaterLayer);
            SavePlatform = new SavePlatform(this);
            Components.Add(SavePlatform);

            tetrisViewport = new GameViewport(this, gameBlockSizeTetris)
            {
                platformMode = false
            };
            tetrisViewport.resize(tetrisModeWidth, 720);
            tetris.viewportToSpawnIn = tetrisViewport;

            platformViewport = new GameViewport(this, gameBlockSizePlatform);
            platformViewport.resize(platformModeWidth, 720);

            Components.Add(platformViewport);
            Components.Add(tetrisViewport);

            // Add GUI components
            StatusLayer = new GameStatusLayer(this);
            Components.Add(StatusLayer);
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
            Services.AddService(typeof(Utility.TimerCollection), Timers);

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

            tetrisBatch = new TetrisPieceBatch(GraphicsDevice, Content);

#if DEBUG
            DebugDrawer = new TetrisPieceBatch(GraphicsDevice, Content);
#endif
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
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamepadState = GamePad.GetState(PlayerIndex.One);
            if (gamepadState.Buttons.Back == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            // Move camera manually
#if DEBUG
            if (keyboardState.IsKeyDown(Keys.PageUp) && prevKeyboardState.IsKeyUp(Keys.PageUp))
                manualPosition.Y -= 1.0f;
            else if (keyboardState.IsKeyDown(Keys.PageDown) && prevKeyboardState.IsKeyUp(Keys.PageDown))
                manualPosition.Y += 1.0f;
#endif

            // update game progress
            gameProgress += gameProgressSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (gameProgress > Game1.worldWidthInBlocks) gameProgress -= Game1.worldWidthInBlocks;
            
            platformViewport.cameraPosition = new Vector2(gameProgress, platform.playerCollider.Position.Y);

            var camDiff = MathStuff.WorldDistance(platform.playerCollider.Position.X, platformViewport.cameraPosition.X, worldWidthInBlocks);
            if (camDiff > 0 && camDiff < 10)
            {
                platformViewport.cameraPosition.X -= MathHelper.Clamp(camDiff, 0, 3);
            }

            float tetrisPro = gameProgress + tetrisProgressAdd;
            if (tetrisPro > Game1.worldWidthInBlocks) tetrisPro -= Game1.worldWidthInBlocks;
            tetrisViewport.cameraPosition = new Vector2(tetrisPro, WaterLayer.Position.Y - 4);

            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            Timers.Update(gameTime);

            prevKeyboardState = keyboardState;
            prevGamepadState = gamepadState;

            addPowerupToWorld((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            platformViewport.Draw(gameTime);
            tetrisViewport.Draw(gameTime);

            spriteBatch.Begin();
            platformViewport.Compose(spriteBatch);
            tetrisViewport.Compose(spriteBatch, platformModeWidth);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawGameWorldOnce(Matrix camera, bool platformMode, int wrapAround)
        {
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

        private void addPowerupToWorld(float elapsedSeconds)
        {
            elapsedTimeSinceLastPowerUp += elapsedSeconds;

            if (elapsedTimeSinceLastPowerUp >= TIME_BETWEEN_POWERUPSPAWNS_SECS)
            {
                //Position the power up on the "screen next to the currenct visible area":
                int maxWidthInGame = (int)Math.Ceiling(Math.Max(this.tetrisViewport.screenWidthInGAME, this.platformViewport.screenWidthInGAME));
                int distanceToRightBorder = maxWidthInGame - (int)PlatformPlayer.cameraPosition.X % maxWidthInGame;
                int randomOffset = (new Random()).Next(0, maxWidthInGame);
                
                //Get a random power up:
                PowerUp p = PowerUp.getRandomPowerUp(this, world, platform.cameraPosition + new Vector2(distanceToRightBorder+randomOffset, -SPAWNHEIGHT_OF_PWUP_ABOVE_PLAYER));
                Components.Add(p);
                elapsedTimeSinceLastPowerUp = 0.0f;
            }    
        }
    }
}
