#if DEBUG
  #define DEBUG_COLLISION
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
        public TetrisPieceBatch TetrisBatch { get { return tetrisBatch; } }
        public PlatformPlayer platform;
        public PlatformPlayer PlatformPlayer { get { return platform; } }

        public PlayerIndex PlayerIdTetris = PlayerIndex.Two, PlayerIdPlatform = PlayerIndex.One;
        double playerSwitchProgress = -1;
        Texture2D playerSwitchTexture;
        public void SwitchPlayers()
        {
            PlayerIndex tmp = PlayerIdTetris;
            PlayerIdTetris = PlayerIdPlatform;
            PlayerIdPlatform = tmp;
            playerSwitchProgress = 1.0;
        }

        Body staticWorldGround;
        Body staticWorldL;
        Body staticWorldR;
        public const int worldWidthInBlocks = 30;
        public const int worldHeightInBlocks = 30;

        public const int worldDuplicateBorder = 5;

        public const Category COLLISION_GROUP_DEFAULT = Category.Cat1;
        public const Category COLLISION_GROUP_TETRIS_BLOCKS = Category.Cat2;
        public const Category COLLISION_GROUP_STATIC_OBJECTS = Category.Cat3;
        public const Category COLLISION_GROUP_LEVEL_SEPARATOR = Category.Cat4;

        public const int SCREEN_WIDTH = 1280, SCREEN_HEIGHT = 720;

        public readonly static Utility.TimerCollection Timers = new Utility.TimerCollection();

        // Public access to world
        public World World { get { return world; } }
        public int WorldWidthInBlocks { get { return worldWidthInBlocks; } }
        public int WorldHeightInBlocks { get { return worldHeightInBlocks; } }

        // Other level components
        public WaterLayer WaterLayer;
        public SavePlatform SavePlatform;
        public WaveLayer waveLayer;
        public GameOverLayer gameOverLayer;
        public TitleScreenLayer titleScreenLayer;
        private List<PowerUp> powerUps = new List<PowerUp>();

        // GUI components
        public GameStatusLayer StatusLayer { get; protected set; }
        public SpriteBatch SpriteBatchOnlyForGuiOverlay { get { return spriteBatch; } }

        public const float gameBlockSizePlatform = 48;
        public const float gameBlockSizeTetris = 48;
        Texture2D background;
        Texture2D cloud1;
        Texture2D cloud2;

        public float gameProgress = 0;
        //float tetrisProgressAdd = 10;
        float gameProgressSpeed = 3.5f;
        private GameViewport tetrisViewport;

        public GameViewport TetrisViewport { get { return tetrisViewport; } }
        //public GameViewport PlatformViewport { get { return platformViewport; } }

        //Power-Ups:
        private const float TIME_BETWEEN_POWERUPSPAWNS_SECS = 3.0f;
        private const int SPAWNHEIGHT_OF_PWUP_ABOVE_PLAYER = 2;
        private float elapsedTimeSinceLastPowerUp = 0.0f;

        public Vector2 manualPosition = Vector2.Zero;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = SCREEN_WIDTH;
            graphics.PreferredBackBufferHeight = SCREEN_HEIGHT;
            Content.RootDirectory = "Content";
            world = new World(new Vector2(0, 25));

            staticWorldGround = BodyFactory.CreateRectangle(world, worldWidthInBlocks, 1, 1.0f, new Vector2(worldWidthInBlocks / 2.0f, 0.5f));
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
            SavePlatform = new SavePlatform(this);
            Components.Add(SavePlatform);
            Components.Add(WaterLayer);

            waveLayer = new WaveLayer(this);
            Components.Add(waveLayer);

            titleScreenLayer = new TitleScreenLayer(this);
            gameOverLayer = new GameOverLayer(this);

            tetrisViewport = new GameViewport(this, gameBlockSizeTetris);
            tetrisViewport.resize(1280, 720);
            tetris.viewportToSpawnIn = tetrisViewport;

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
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            background = Content.Load<Texture2D>(@"graphics/level/Background");
            cloud1 = Content.Load<Texture2D>(@"graphics/level/cloud_01");
            cloud2 = Content.Load<Texture2D>(@"graphics/level/cloud_02");
            tetrisBatch = new TetrisPieceBatch(GraphicsDevice, Content);
            playerSwitchTexture = Content.Load<Texture2D>(@"graphics/gui/PlayerSwitch");

            // Load sound
            MusicManager.LoadMusic(Content, "background", "background");
            SoundManager.LoadSound(Content, "bell", "bell-01");
            SoundManager.LoadSound(Content, "collect_powerup", "powerup-01");
            SoundManager.LoadSound(Content, "splash", "splash-03");
            MusicManager.MaxVolume = 0.25f;
            SoundManager.SoundVolume = 1.0f;

            // Reset player state
            platform.ResetPlayer();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            MusicManager.Reset();
            SoundManager.Reset();
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

            // Start/update background music
            if (!MusicManager.IsPlaying)
            {
                MusicManager.FadeInMusic("background", true, 2.0f, 0.0f);
            }
            int msec = gameTime.ElapsedGameTime.Milliseconds;
            MusicManager.Update(msec);

            //Don't update the game while showing screens:
            if (gameOverLayer.IsActive || titleScreenLayer.IsActive)
            {
                base.Update(gameTime);
                return;
            }

            if (playerSwitchProgress > 0)
            {
                playerSwitchProgress -= gameTime.ElapsedGameTime.TotalSeconds * 0.4;
                return;
            }

            // Move camera manually
#if DEBUG
            if (keyboardState.IsKeyDown(Keys.PageUp) && prevKeyboardState.IsKeyUp(Keys.PageUp))
                manualPosition.Y -= 1.0f;
            else if (keyboardState.IsKeyDown(Keys.PageDown) && prevKeyboardState.IsKeyUp(Keys.PageDown))
                manualPosition.Y += 1.0f;
#endif
            // update game progress
            float sec = (float)gameTime.ElapsedGameTime.TotalSeconds;
            gameProgress += gameProgressSpeed * sec;
            if (gameProgress > Game1.worldWidthInBlocks)
            {
                gameProgress -= Game1.worldWidthInBlocks;
                SavePlatform.AllowTriggering();
                WaterLayer.StartRising(5000);
                SavePlatform.StartRising(5000);
            }

            tetrisViewport.cameraPosition = new Vector2(gameProgress, WaterLayer.Position.Y - 3);

            world.Step(sec);

            Timers.Update(gameTime);

            prevKeyboardState = keyboardState;
            prevGamepadState = gamepadState;

            //Powerup stuff:
            addPowerupToWorld(sec);
            checkForPassedPowerupsToRemove();

            base.Update(gameTime);
        }

        Vector2 cloudOffsets = Vector2.Zero;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            cloudOffsets.X += 0.5f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            cloudOffsets.Y += 1.3f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (cloudOffsets.X > Game1.worldWidthInBlocks) cloudOffsets.X -= Game1.worldWidthInBlocks;
            if (cloudOffsets.Y > Game1.worldWidthInBlocks) cloudOffsets.Y -= Game1.worldWidthInBlocks;

            tetrisViewport.Draw(gameTime);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            tetrisViewport.Compose(spriteBatch);

            if (playerSwitchProgress > 0)
                spriteBatch.Draw(playerSwitchTexture, new Rectangle(0, 0, 1280, 720), new Color(1, 1, 1, (float)playerSwitchProgress));


            spriteBatch.End();


            base.Draw(gameTime);
        }


        public void DrawGameWorldOnce(Matrix camera, bool platformMode, int wrapAround)
        {
            GraphicsDevice.Clear(platformMode ? Color.CornflowerBlue : Color.Coral);
            spriteBatch.Begin();
            Vector3 tl = camera.Translation;
            spriteBatch.Draw(background, new Rectangle(0, 0, 1280, 720), Color.White);
            spriteBatch.End();

            tetrisBatch.cameraMatrix = camera;
            //tetrisBatch.DrawBody(staticWorldGround);
            //tetrisBatch.DrawBody(staticWorldL);
            //tetrisBatch.DrawBody(staticWorldR);
            tetrisBatch.DrawAlignedQuad(new Vector2(worldWidthInBlocks / 2, -worldHeightInBlocks / 2), new Vector2(worldWidthInBlocks, worldHeightInBlocks), background);
            tetrisBatch.DrawAlignedQuad(tetrisViewport.cameraPosition + new Vector2(-cloudOffsets.X, 0), new Vector2(tetrisViewport.screenWidthInGAME, tetrisViewport.screenHeightInGAME), cloud1);
            tetrisBatch.DrawAlignedQuad(tetrisViewport.cameraPosition + new Vector2(-cloudOffsets.Y, 0), new Vector2(tetrisViewport.screenWidthInGAME, tetrisViewport.screenHeightInGAME), cloud2);


            foreach (GameComponent c in Components)
            {
                if (c is DrawableGameComponentExtended)
                {
                    (c as DrawableGameComponentExtended).DrawGameWorldOnce(camera, platformMode);
                }
            }
#if DEBUG_COLLISION
            foreach (Body cur in world.BodyList)
                tetrisBatch.DrawBody(cur);
#endif
        }

        private void addPowerupToWorld(float elapsedSeconds)
        {
            elapsedTimeSinceLastPowerUp += elapsedSeconds;

            if (elapsedTimeSinceLastPowerUp >= TIME_BETWEEN_POWERUPSPAWNS_SECS)
            {
                //Position the power up on the "screen next to the currenct visible area":
                int maxWidthInGame = (int)Math.Ceiling(Math.Max(this.tetrisViewport.screenWidthInGAME, this.tetrisViewport.screenWidthInGAME));
                int distanceToRightBorder = maxWidthInGame - ((int)PlatformPlayer.cameraPosition.X % maxWidthInGame);
                int randomOffset = (new Random()).Next(0, maxWidthInGame);

                Vector2 spawnPos = new Vector2();
                spawnPos.X = (platform.cameraPosition.X + distanceToRightBorder + randomOffset) % Game1.worldWidthInBlocks;
                spawnPos.Y = platform.cameraPosition.Y - SPAWNHEIGHT_OF_PWUP_ABOVE_PLAYER;

                //Get a random power up:
                PowerUp p = PowerUp.getRandomPowerUp(this, world, spawnPos);
                Components.Add(p);
                powerUps.Add(p);
                elapsedTimeSinceLastPowerUp = 0.0f;
            }
        }

        private void checkForPassedPowerupsToRemove()
        {
            List<PowerUp> toDel = new List<PowerUp>();
            foreach (PowerUp p in powerUps)
            {
                if (p != platform.CurrentlySelectedPowerUp && waveLayer.isCollidingWith(p.Position))
                {
                    toDel.Add(p);
                }
            }

            foreach (PowerUp p in toDel)
            {
                removePowerUp(p);
            }
        }

        public void removePowerUp(PowerUp p)
        {
            Components.Remove(p);
            powerUps.Remove(p);
            p.delme();
        }
    }
}
