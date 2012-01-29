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
    public class TitleScreenLayer : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private const float DURATION = 3.0f;

        Game1 game;
        GameComponentCollection originalComponents;
        private bool isActive = true;
        private float elapsedTimeSinceActive = 0.0f;
        private Texture2D texture;

        public bool IsActive
        {
            get { return isActive; }
        }

        public TitleScreenLayer(Game1 game)
            : base(game)
        {
            this.game = game;
            game.Components.Add(this);
        }

        protected override void LoadContent()
        {
            texture = game.Content.Load<Texture2D>("TitleScreen");
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState g1 = GamePad.GetState(PlayerIndex.One);
            GamePadState g2 = GamePad.GetState(PlayerIndex.Two);

            if (keyboardState.IsKeyDown(Keys.Enter) || 
                g1.IsButtonDown(Buttons.A) ||
                g2.IsButtonDown(Buttons.A)) 
                    this.onCloseTitleScreen();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            game.SpriteBatchOnlyForGuiOverlay.Begin();
            game.SpriteBatchOnlyForGuiOverlay.Draw(texture, new Vector2(0, 0), Color.White);
            game.SpriteBatchOnlyForGuiOverlay.End();
            base.Draw(gameTime);
        }

        private void onCloseTitleScreen()
        {
            game.Components.Remove(this);
            isActive = false;
        }
    }
}
