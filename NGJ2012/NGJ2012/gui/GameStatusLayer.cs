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

        private const float TETRIS_SCALE = 0.125f;

        // Assets
        SpriteFont font;
        Texture2D tex;

        public GameStatusLayer(Game game) : base(game)
        {
            parent = (Game1)game;
            screenRectangle = new Rectangle(Game1.platformModeWidth - 8, 0, 16, 720);
            playerRectangle = new Rectangle(screenRectangle.X - 8, 0, 32, 32);

            float offset = 4.0f;
            textPositionP1 = new Vector2(offset, 2.0f);
            textPositionP2 = new Vector2(Game1.platformModeWidth + offset, 2.0f);
        }

        protected override void LoadContent()
        {
            font = parent.Content.Load<SpriteFont>("fonts/guifont");
            tex = parent.Content.Load<Texture2D>("graphics/gui/gui");
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
            parent.SpriteBatch.Begin();
            parent.SpriteBatch.Draw(tex, screenRectangle, Color.White);
            parent.SpriteBatch.Draw(tex, playerRectangle, Color.Red);

            //Texts p1:
            Vector2 widthHeight = font.MeasureString(p1Text);
            Vector2 posPowerupTxt = new Vector2(textPositionP1.X, textPositionP1.Y + widthHeight.Y);
            
            parent.SpriteBatch.DrawString(font, p1Text, textPositionP1, Color.White);
            
            widthHeight = font.MeasureString(p1Text);
            if (texturePowerUp != null)
            {
                parent.SpriteBatch.DrawString(font, textPowerup, posPowerupTxt, Color.White);
                parent.SpriteBatch.Draw(texturePowerUp, posPowerupTxt + new Vector2(font.MeasureString(textPowerup).X, 0.0f), Color.White);
            }

            //Texts p2:
            parent.SpriteBatch.DrawString(font, p2Text, textPositionP2, Color.White);
            if (parent.TetrisPlayer.nextTetrixPiece != null) parent.SpriteBatch.Draw(parent.TetrisPlayer.nextTetrixPiece.texture, textPositionP2 + new Vector2(font.MeasureString(p2Text).X, 0.0f), null, Color.White, 0.0f, new Vector2(), TETRIS_SCALE, SpriteEffects.None, 0.0f);

            parent.SpriteBatch.End();


        }
    }
}
