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
        private Vector2 inGamePosition = new Vector2(0, 0);
        private Vector2 inGamePositionDubl = new Vector2(0, 0);

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
            this.inGamePosition.X = game.gameProgress - game.TetrisViewport.screenWidthInGAME / 2;
            this.inGamePosition.X = (this.inGamePosition.X + Game1.worldWidthInBlocks) % Game1.worldWidthInBlocks;
            this.inGamePosition.Y = game.WaterLayer.Height - texture.Height / Game1.gameBlockSizePlatform;

            inGamePositionDubl.X = inGamePosition.X - Game1.worldWidthInBlocks;
            inGamePositionDubl.Y = inGamePosition.Y;

            base.Update(gameTime);
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            game.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            game.SpriteBatch.Draw(texture, Vector2.Transform(inGamePosition, camera), Color.White);
            game.SpriteBatch.Draw(texture, Vector2.Transform(inGamePositionDubl, camera), Color.White); //Dublicate to avoid clipping on world wrap
            game.SpriteBatch.End();
        }

        public bool isCollidingWith(Vector2 objPos)
        {
            return (this.inGamePosition.X <= objPos.X && objPos.X <= this.inGamePosition.X + texture.Width / Game1.gameBlockSizePlatform);
        }
    }
}
