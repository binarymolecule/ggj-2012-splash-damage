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
    public class TetrisPieceBatch
    {
        public GraphicsDevice GraphicsDevice;
        public Matrix cameraMatrix;

        public TetrisPieceBatch(GraphicsDevice iGraphicsDevice)
        {
            GraphicsDevice = iGraphicsDevice;
            _lineVertices = new VertexPositionColor[1024];
            _quadVertices = new VertexPositionTexture[1024];

            // set up a new basic effect, and enable vertex colors.
            _basicEffect = new BasicEffect(GraphicsDevice);
            _basicEffect.VertexColorEnabled = true;

            _basicEffect.Projection =  Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
        }

        private BasicEffect _basicEffect;
        private VertexPositionColor[] _lineVertices;
        private int _lineVertsCount;
        private VertexPositionTexture[] _quadVertices;
        private int _quadVertsCount;

        public void DrawBody(Body bod)
        {
            Matrix mat = Matrix.CreateRotationZ(bod.Rotation) * Matrix.CreateTranslation(new Vector3(bod.Position, 0.0f)) * cameraMatrix;

            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            //tell our basic effect to begin.
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
            _basicEffect.View = mat;
            _basicEffect.CurrentTechnique.Passes[0].Apply();

            foreach (Fixture fix in bod.FixtureList)
            {
                DrawLineShape(fix.Shape, Color.Black);
            }

            Flush();
        }

        private void DrawLineShape(Shape shape, Color color)
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

        private void DrawQuadShape(Shape shape, Color color)
        {
            const float textureScale = 1.0f / 4.0f;
            if (shape.ShapeType == ShapeType.Polygon)
            {
                PolygonShape loop = (PolygonShape)shape;
                for (int i = 0; i < loop.Vertices.Count-1; ++i)
                {
                    if (_quadVertsCount + 3 >= _quadVertices.Length)
                        Flush();
                    _quadVertices[_quadVertsCount].TextureCoordinate = loop.Vertices[0] * textureScale;
                    _quadVertices[_quadVertsCount++].Position = new Vector3(loop.Vertices[0], 0f);
                    _quadVertices[_quadVertsCount].TextureCoordinate = loop.Vertices[i] * textureScale;
                    _quadVertices[_quadVertsCount++].Position = new Vector3(loop.Vertices[i], 0f);
                    _quadVertices[_quadVertsCount].TextureCoordinate = loop.Vertices[i + 1] * textureScale;
                    _quadVertices[_quadVertsCount++].Position = new Vector3(loop.Vertices[i + 1], 0f);
                }
            }
        }

        private void Flush()
        {
            if (_lineVertsCount >= 2)
            {
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _lineVertices, 0, _lineVertsCount / 2);
                _lineVertsCount = 0;
            }
            if (_quadVertsCount >= 2)
            {
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _quadVertices, 0, _quadVertsCount / 3);
                _quadVertsCount = 0;
            }
        }
    }
}
