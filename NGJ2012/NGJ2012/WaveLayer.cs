using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace NGJ2012
{
    public class WaveLayer : DrawableGameComponentExtended
    {
        private Game1 game;
        private Texture2D texture;
        private Vector2 position = new Vector2(0, 0);

        private const float WAVE_SPEED = 10.0f;

        public WaveLayer(Game1 game)
            : base(game)
        {
            this.game = game;
        }

        protected override void LoadContent()
        {
            this.texture = game.Content.Load<Texture2D>("Wave");
            base.LoadContent();
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            this.position.X = game.gameProgress - game.TetrisViewport.screenWidthInGAME / 2;
            this.position.X = (this.position.X + Game1.worldWidthInBlocks) % Game1.worldWidthInBlocks;
            this.position.Y = game.WaterLayer.Height - texture.Height / Game1.gameBlockSizePlatform;
            base.Update(gameTime);
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            game.SpriteBatch.Draw(texture, Vector2.Transform(position, camera), Color.White);
            game.SpriteBatch.End();
        }

        public bool isCollidingWith(Vector2 objPos)
        {
            return (this.position.X <= objPos.X && objPos.X <= this.position.X + texture.Width / Game1.gameBlockSizePlatform);
        }
    }
}
