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

namespace NGJ2012
{
    /// <summary>
    /// Implements layer for GUI and status information elements.
    /// </summary>
    public class GameStatusLayer : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Game1 parent;
        Rectangle screenRectangle, playerRectangle;

        private String p1Text;
        private String p2Text;
        private String textPowerup;
        private Texture2D texturePowerUp;

        private Vector2 positionPowerUp;
        private Vector2 textPositionP1;
        private Vector2 textPositionP2;

        int uiBaseline = 20;

        private const float TETRIS_SCALE = 0.125f;

        // Assets
        SpriteFont font;
        Texture2D tex;
        private Texture2D uiSprites;

        public GameStatusLayer(Game game) : base(game)
        {
            parent = (Game1)game;
            screenRectangle = new Rectangle(Game1.SCREEN_WIDTH - 32, 0, 16, 720);
            playerRectangle = new Rectangle(screenRectangle.X - 8, 0, 32, 32);

            float offset = 4.0f;
            textPositionP1 = new Vector2(offset, 0);
            textPositionP2 = new Vector2(1000 + offset, 0);
        }

        protected override void LoadContent()
        {
            font = parent.Content.Load<SpriteFont>("fonts/guifont");
            tex = parent.Content.Load<Texture2D>("graphics/gui/gui");
            uiSprites = parent.Content.Load<Texture2D>(@"graphics/sprites");
        }


        public void DrawUiSprite(int index, int x, int y, int cellX = 0, int cellY = 0)
        {
            int itemsPerRow = 8;
            int cellSize = uiSprites.Width / itemsPerRow;
            int row = index / itemsPerRow;
            int col = index % itemsPerRow;

            var srcRect = new Rectangle(col * cellSize, row * cellSize, cellSize, cellSize);
            var destRect = new Rectangle(x + cellX * cellSize, y + cellY * cellSize, cellSize, cellSize);

            parent.SpriteBatchOnlyForGuiOverlay.Draw(uiSprites, destRect, srcRect, Color.White);
        }

        protected override void UnloadContent()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (!parent.GameIsRunning) return;

            playerRectangle.Y = (int)(700 + (parent.PlatformPlayer.cameraPosition.Y / parent.WorldHeightInBlocks) * 680);

            p1Text = "#Lifes = " + parent.PlatformPlayer.NumberOfLifes;
            textPowerup = "Power-Up = ";

            PowerUp p = parent.PlatformPlayer.CurrentlySelectedPowerUp;
            texturePowerUp = (p != null) ? parent.PlatformPlayer.CurrentlySelectedPowerUp.Texture : null;

            if (p != null && p.UsageTimerRunning)
            {
                textPowerup += "\n" + p.getRemainingPowerUpTimeInSecsFixedPoint() + " sec";
            }

            p2Text = "Next tetris: ";

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!parent.GameIsRunning) return;

            parent.SpriteBatchOnlyForGuiOverlay.Begin();
            parent.SpriteBatchOnlyForGuiOverlay.Draw(tex, screenRectangle, Color.White);
            parent.SpriteBatchOnlyForGuiOverlay.Draw(tex, playerRectangle, Color.Red);

            if (parent.TetrisPlayer.nextTetrixPiece != null)
            {
                var posX = textPositionP2.X;
                DrawUiSprite(46, (int)posX, uiBaseline, 0, 0);
                DrawUiSprite(46 + 1, (int)posX, uiBaseline, 1, 0);

                parent.SpriteBatchOnlyForGuiOverlay.Draw(parent.TetrisPlayer.nextTetrixPiece.texture, textPositionP2 + new Vector2(64, 0), null, Color.White, 0.0f, new Vector2(), TETRIS_SCALE, SpriteEffects.None, 0.0f);
            }

            DrawLives();
            parent.SpriteBatchOnlyForGuiOverlay.End();

        }

        private void DrawLives()
        {

            // Life display
            {
                int lifeUiX = 200;


                DrawUiSprite(0, lifeUiX, uiBaseline);

                var lives = parent.platform.NumberOfLifes.ToString();
                var i = 1;

                foreach (char c in lives.ToCharArray())
                {
                    if (c == '0')
                    {
                        DrawUiSprite(17, lifeUiX, uiBaseline, i++);
                    }
                    else
                    {
                        DrawUiSprite(8 + (c - '1'), lifeUiX, uiBaseline, i++);
                    }

                }



                if (parent.PlatformPlayer.CurrentlySelectedPowerUp != null && !parent.PlatformPlayer.CurrentlySelectedPowerUp.invisibleBecauseBlinking)
                {
                    DrawUiSprite(62, lifeUiX, uiBaseline, 8);
                    DrawUiSprite(63, lifeUiX, uiBaseline, 9);

                    switch (parent.PlatformPlayer.CurrentlySelectedPowerUp.PowerUpType)
                    {
                        case PowerUp.EPowerUpType.MegaJump:
                            DrawUiSprite(4, lifeUiX, uiBaseline, 10);
                            break;
                        case PowerUp.EPowerUpType.WaterProof:
                            DrawUiSprite(5, lifeUiX, uiBaseline, 10);
                            break;
                    }
                }
            }
        }
    }
}
