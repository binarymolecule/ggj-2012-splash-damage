using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NGJ2012
{
    class GameViewport : DrawableGameComponent
    {
        public Vector2 cameraPosition = new Vector2(0,0);
        public float scale = 1;
        public float cellWidth = 32;
        public float horizontalCells = 24;
        public float verticalCells = 20;
        public float screenWidth = 1024;
        public float screenHeight = 720;
        public Game1 game;
        public RenderTarget2D leftScreen;
        public RenderTarget2D rightScreen;
        public bool platformMode = true;
        private float splitLine;

        public GameViewport(Game1 game)
            : base(game)
        {
            this.game = game;
            this.Visible = false;
        }

        protected override void  LoadContent()
        {
            base.LoadContent();

            leftScreen = new RenderTarget2D(GraphicsDevice, (int)screenWidth, (int)screenHeight);
            rightScreen = new RenderTarget2D(GraphicsDevice, (int)screenWidth, (int)screenHeight);
        }

        public override void Draw(GameTime gameTime)
        {
            splitLine = -1;
            float gameWorldStartInPX = cellWidth * 0;
            float gameWorldEndInPX = cellWidth * horizontalCells;
            float cameraLeftInPX = cameraPosition.X * cellWidth * horizontalCells * scale - screenWidth / 2.0f;
            float cameraRightInPX = cameraPosition.X * cellWidth * horizontalCells * scale + screenWidth / 2.0f;

            if (cameraLeftInPX < gameWorldStartInPX)
            {
                splitLine = gameWorldStartInPX - cameraLeftInPX;
                game.GraphicsDevice.SetRenderTarget(leftScreen);
                game.DrawGameWorldOnce(platformMode, -1);
                game.GraphicsDevice.SetRenderTarget(rightScreen);
                game.DrawGameWorldOnce(platformMode, 0);
            }
            else if (gameWorldEndInPX < cameraRightInPX)
            {
                splitLine = screenWidth - (cameraRightInPX - gameWorldEndInPX);
                game.GraphicsDevice.SetRenderTarget(leftScreen);
                game.DrawGameWorldOnce(platformMode, 0);
                game.GraphicsDevice.SetRenderTarget(rightScreen);
                game.DrawGameWorldOnce(platformMode, 1);
            }
            else
            {
                game.GraphicsDevice.SetRenderTarget(leftScreen);
                game.DrawGameWorldOnce(platformMode, 0);
            }

            game.GraphicsDevice.SetRenderTarget(null);
        }

        public void Compose(SpriteBatch spriteBatch, int x = 0, int y = 0)
        {
            if (splitLine < 0)
            {
                spriteBatch.Draw(leftScreen, new Rectangle(x, 0, (int)screenWidth, (int)screenHeight), Color.White);
            }
            else
            {
                spriteBatch.Draw(leftScreen, new Rectangle(x, 0, (int)splitLine, 720), new Rectangle(0, 0, (int)splitLine, 720), Color.White);
                spriteBatch.Draw(rightScreen, new Rectangle(x + (int)splitLine, 0, (int)screenWidth - (int)splitLine, 720), new Rectangle((int)splitLine, 0, (int)screenWidth - (int)splitLine, 720), Color.White);
            }
        }
    }
}
