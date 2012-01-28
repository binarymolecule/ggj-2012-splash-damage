using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NGJ2012
{
    public class GameViewport : DrawableGameComponent
    {
        public Vector2 cameraPosition = new Vector2(0,0);
        public float cellSizeInPX = 32;
        int screenWidth = 1024;
        int screenHeight = 720;
        public float screenWidthInGAME, screenHeightInGAME;
        public Game1 game;
        public RenderTarget2D leftScreen;
        public RenderTarget2D rightScreen;
        private float splitLine;

        public GameViewport(Game1 game, float icellWidth)
            : base(game)
        {
            this.game = game;
            this.cellSizeInPX = icellWidth;
            this.Visible = false;
        }

        public void resize(int x, int y)
        {
            screenWidth = x;
            screenWidthInGAME = screenWidth / cellSizeInPX;
            screenHeight = y;
            screenHeightInGAME = screenHeight / cellSizeInPX;
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
            float gameWorldStartInPX = cellSizeInPX * 0;
            float gameWorldEndInPX = cellSizeInPX * Game1.worldWidthInBlocks;
            float cameraLeftInPX = cameraPosition.X * cellSizeInPX - screenWidth / 2.0f;
            float cameraRightInPX = cameraPosition.X * cellSizeInPX + screenWidth / 2.0f;

            if (cameraLeftInPX < gameWorldStartInPX)
            {
                splitLine = gameWorldStartInPX - cameraLeftInPX;
                game.GraphicsDevice.SetRenderTarget(leftScreen);
                DrawGameWorldOnce(-1);
                GraphicsDevice.SetRenderTarget(rightScreen);
                DrawGameWorldOnce(0);
            }
            else if (gameWorldEndInPX < cameraRightInPX)
            {
                splitLine = screenWidth - (cameraRightInPX - gameWorldEndInPX);
                GraphicsDevice.SetRenderTarget(leftScreen);
                DrawGameWorldOnce(0);
                GraphicsDevice.SetRenderTarget(rightScreen);
                DrawGameWorldOnce(1);
            }
            else
            {
                GraphicsDevice.SetRenderTarget(leftScreen);
                DrawGameWorldOnce(0);
            }

            game.GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawGameWorldOnce(int wrapAround)
        {
            Matrix camera = Matrix.CreateTranslation(-new Vector3(cameraPosition, 0.0f));
#if DEBUG
            camera *= Matrix.CreateTranslation(-new Vector3((Game as Game1).manualPosition, 0.0f));
#endif
            camera *= Matrix.CreateTranslation(new Vector3(wrapAround * Game1.worldWidthInBlocks, 0, 0));
            camera *= Matrix.CreateScale(Game1.gameBlockSizeTetris);
            camera *= Matrix.CreateTranslation(new Vector3(screenWidth, screenHeight, 0.0f) / 2.0f);
            game.DrawGameWorldOnce(camera, true, wrapAround);
        }

        public void Compose(SpriteBatch spriteBatch, int x = 0, int y = 0)
        {
            if (splitLine < 0)
            {
                spriteBatch.Draw(leftScreen, new Rectangle(x, 0, (int)screenWidth, (int)screenHeight), Color.White);
            }
            else
            {
                spriteBatch.Draw(leftScreen, new Rectangle(x, 0, (int)splitLine, screenHeight), new Rectangle(0, 0, (int)splitLine, screenHeight), Color.White);
                spriteBatch.Draw(rightScreen, new Rectangle(x + (int)splitLine, 0, (int)screenWidth - (int)splitLine, screenHeight), new Rectangle((int)splitLine, 0, (int)screenWidth - (int)splitLine, screenHeight), Color.White);
            }
        }
    }
}
