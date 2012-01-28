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

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            parent.SpriteBatch.Begin();
            //parent.SpriteBatch.DrawString(font, "Test", Vector2.Zero, Color.White);
            parent.SpriteBatch.Draw(tex, screenRectangle, Color.White);
            parent.SpriteBatch.Draw(tex, playerRectangle, Color.Red);
            parent.SpriteBatch.End();
        }
    }
}
