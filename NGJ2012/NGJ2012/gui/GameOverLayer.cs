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
    public class GameOverLayer : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private const float DURATION = 3.0f;

        Game1 game;
        GameComponentCollection originalComponents;
        private bool isActive = false;
        private float elapsedTimeSinceActive = 0.0f;
        private Texture2D texture;

        public bool IsActive
        {
            get { return isActive; }
        }

        public GameOverLayer(Game1 game)
            : base(game)
        {
            this.game = game;
        }

        protected override void LoadContent()
        {
            texture = game.Content.Load<Texture2D>("GameOver");
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            float secs = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (isActive) elapsedTimeSinceActive += secs;

            if (elapsedTimeSinceActive >= DURATION) onGameOverEnd();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            game.SpriteBatchOnlyForGuiOverlay.Begin();
            game.SpriteBatchOnlyForGuiOverlay.Draw(texture, new Vector2(0,0), Color.White);
            game.SpriteBatchOnlyForGuiOverlay.End();
            base.Draw(gameTime);
        }

        public void onGameOver()
        {
            isActive = true;
            //originalComponents = game.Components;
            //game.Components.Clear();
            game.Components.Add(this);
            MusicManager.FadeOutMusic(2.0f);
        }

        private void onGameOverEnd()
        {
            game.Components.Remove(this);
            isActive = false;
        }
    }
}
