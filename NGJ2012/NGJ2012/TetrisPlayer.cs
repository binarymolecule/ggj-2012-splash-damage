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
    public class TetrisPlayer : Microsoft.Xna.Framework.GameComponent
    {
        private World _world;
        private float gameBlockSize;
        private Vector2 cursorPosition;

        public TetrisPlayer(Game game, World world, float igameBlockSize)
            : base(game)
        {
            _world = world;
            gameBlockSize = igameBlockSize;
            cursorPosition = new Vector2(3, 3);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            _lineVertices = new VertexPositionColor[1024];

            // set up a new basic effect, and enable vertex colors.
            _basicEffect = new BasicEffect(Game.GraphicsDevice);
            _basicEffect.VertexColorEnabled = true;

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }


        private BasicEffect _basicEffect;
        private VertexPositionColor[] _lineVertices;
        private int _lineVertsCount;


        public void DrawBody(Body bod)
        {
            Matrix mat = Matrix.CreateRotationZ(bod.Rotation) * Matrix.CreateTranslation(new Vector3(bod.Position, 0.0f)) * Matrix.CreateScale(gameBlockSize);

            Game.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            //tell our basic effect to begin.
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height, 0, 0, 1);
            _basicEffect.View = mat;
            _basicEffect.CurrentTechnique.Passes[0].Apply();

            foreach (Fixture fix in bod.FixtureList)
            {
                DrawLineShape(fix.Shape, Color.Black);
            }

            Flush();
        }

        public void DrawLineShape(Shape shape, Color color)
        {
            if (shape.ShapeType == ShapeType.Polygon)
            {
                PolygonShape loop = (PolygonShape)shape;
                for (int i = 0; i < loop.Vertices.Count; ++i)
                {
                    if (_lineVertsCount >= _lineVertices.Length)
                        Flush();
                    _lineVertices[_lineVertsCount].Position = new Vector3(loop.Vertices[i], 0f);
                    _lineVertices[_lineVertsCount + 1].Position = new Vector3(loop.Vertices.NextVertex(i), 0f);
                    _lineVertices[_lineVertsCount].Color = _lineVertices[_lineVertsCount + 1].Color = color;
                    _lineVertsCount += 2;
                }
            }
        }



        private void Flush()
        {
            if (_lineVertsCount < 2) return;
            Game.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _lineVertices, 0, _lineVertsCount / 2);
            _lineVertsCount = 0;
        }
    }
}
