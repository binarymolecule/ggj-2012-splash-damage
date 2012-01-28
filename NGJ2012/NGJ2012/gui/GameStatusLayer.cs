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
        private String textPowerup;
        private Texture2D texturePowerUp;
        private Vector2 textPosition = new Vector2(20.0f, 20.0f);

        // Assets
        SpriteFont font;
        Texture2D tex;

        public GameStatusLayer(Game game) : base(game)
        {
            parent = (Game1)game;
            screenRectangle = new Rectangle(Game1.platformModeWidth - 8, 0, 16, 720);
            playerRectangle = new Rectangle(screenRectangle.X - 8, 0, 32, 32);
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

            p1Text = "#Lifes = " + parent.PlatformPlayer.NumberOfLifes + "\n cameraPosition Player = " + parent.PlatformPlayer.cameraPosition;
            textPowerup = "Power-Up = ";
            texturePowerUp = (parent.PlatformPlayer.CurrentlySelectedPowerUp != null) ? parent.PlatformPlayer.CurrentlySelectedPowerUp.Texture : null;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            parent.SpriteBatch.Begin();
            parent.SpriteBatch.Draw(tex, screenRectangle, Color.White);
            parent.SpriteBatch.Draw(tex, playerRectangle, Color.Red);

            //Texts:
            Vector2 widthHeight = font.MeasureString(p1Text);
            Vector2 pos2 = new Vector2(textPosition.X, textPosition.Y + widthHeight.Y);

            parent.SpriteBatch.DrawString(font, p1Text, textPosition, Color.White);
            parent.SpriteBatch.DrawString(font, textPowerup, pos2, Color.White);

            widthHeight = font.MeasureString(p1Text);
            if (texturePowerUp != null) parent.SpriteBatch.Draw(texturePowerUp, new Vector2(pos2.X + widthHeight.X, textPosition.Y + widthHeight.Y), Color.White);

            parent.SpriteBatch.End();


        }
    }
}
