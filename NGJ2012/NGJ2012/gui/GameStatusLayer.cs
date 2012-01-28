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
        Vector2 pos;

        // Assets
        SpriteFont font;

        public GameStatusLayer(Game game) : base(game)
        {
            parent = (Game1)game;
            pos = Vector2.Zero;
        }

        protected override void LoadContent()
        {
            font = parent.Content.Load<SpriteFont>("fonts/guifont");
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
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            parent.SpriteBatch.Begin();
            parent.SpriteBatch.DrawString(font, "Test", pos, Color.White);
            parent.SpriteBatch.End();
        }
    }
}
