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
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;


namespace NGJ2012
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TetrisPlayer : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private World _world;
        private float gameBlockSize;
        const float movementSpeed = 8.0f;

        List<bool[,]> tetrisShapes = new List<bool[,]>();
        List<Texture2D> tetrisTextures = new List<Texture2D>();

        List<TetrisPiece> pieces = new List<TetrisPiece>();
        private TetrisPiece currentPiece;
        TetrisPieceBatch drawer;

        public TetrisPlayer(Game game, World world, float igameBlockSize)
            : base(game)
        {
            _world = world;
            gameBlockSize = igameBlockSize;

            tetrisShapes.Add(new bool[,] { { true, false }, { true, false }, { true, true } });
            tetrisShapes.Add(new bool[,] { { true, true }, { true, true } });
            tetrisShapes.Add(new bool[,] { { false, true, false }, { true, true, true } });
            tetrisShapes.Add(new bool[,] { { true }, { true }, { true }, { true } });
            currentPiece = new TetrisPiece(world, null, tetrisShapes[0], new Vector2(2,2));
            pieces.Add(currentPiece);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            drawer = new TetrisPieceBatch(GraphicsDevice, Matrix.CreateScale(gameBlockSize));

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            Vector2 moveDir = new Vector2();
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Left)) moveDir.X = -1;
            else if (state.IsKeyDown(Keys.Right)) moveDir.X = +1;
            else moveDir.X = 0;
            if (state.IsKeyDown(Keys.Up)) moveDir.Y = -1;
            else if (state.IsKeyDown(Keys.Down)) moveDir.Y = +1;
            else moveDir.Y = 0;
            currentPiece.body.Position += (float)gameTime.ElapsedGameTime.TotalSeconds * moveDir * movementSpeed;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {

            foreach (TetrisPiece cur in pieces)
            {
                drawer.DrawBody(cur.body);
            }
        }

    
    }
}
