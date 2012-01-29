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
using FarseerPhysics.Common;


namespace NGJ2012
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TetrisPieceBatch
    {
        public GraphicsDevice GraphicsDevice;
        public Matrix cameraMatrix;

        public TetrisPieceBatch(GraphicsDevice iGraphicsDevice, ContentManager Content)
        {
            GraphicsDevice = iGraphicsDevice;
            _lineVertices = new VertexPositionColor[1024];
            _quadVertices = new VertexPositionColorTexture[1024];

            // set up a new basic effect, and enable vertex colors.
            _basicEffect = new BasicEffect(GraphicsDevice);
            _basicEffect.VertexColorEnabled = true;

            _basicEffect.Projection =  Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);

            effect = Content.Load<Effect>("shapes/Shader");
        }

        private BasicEffect _basicEffect;
        private Effect effect;
        private VertexPositionColor[] _lineVertices;
        private int _lineVertsCount;
        private VertexPositionColorTexture[] _quadVertices;
        private int _quadVertsCount;

        public void DrawBody(Body bod)
        {
            Matrix mat = Matrix.CreateRotationZ(bod.Rotation) * Matrix.CreateTranslation(new Vector3(bod.Position, 0.0f));

            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            //tell our basic effect to begin.
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
            //_basicEffect.View = mat;
            _basicEffect.TextureEnabled = false;
            _basicEffect.CurrentTechnique.Passes[0].Apply();


            bool wrapL = bod.Position.X < Game1.worldDuplicateBorder;
            bool wrapR = bod.Position.X > Game1.worldWidthInBlocks - Game1.worldDuplicateBorder;

            for (int i = -1; i <= 1; i++)
            {
                if (i == -1 && !wrapL) continue;
                if (i == 1 && !wrapR) continue;

                _basicEffect.View = mat * Matrix.CreateTranslation(new Vector3(-i * Game1.worldWidthInBlocks, 0, 0)) * cameraMatrix;
                _basicEffect.CurrentTechnique.Passes[0].Apply();

                foreach (Fixture fix in bod.FixtureList)
                {
                    DrawLineShape(fix.Shape, Color.Black);
                }

                Flush();
            }
        }

        public void DrawBodyTextured(Body bod, Texture2D texture)
        {
            DrawBodyTextured(bod, texture, 1.0f / 4.0f);
        }
        public void DrawBodyTextured(Body bod, Texture2D texture, float textureScale)
        {
            Matrix mat = Matrix.CreateRotationZ(bod.Rotation) * Matrix.CreateTranslation(new Vector3(bod.Position, 0.0f));

            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1));
//            effect.Parameters["View"].SetValue(mat);
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["BasicTexture"].SetValue(texture);
//            effect.CurrentTechnique.Passes[0].Apply();


            bool wrapL = bod.Position.X < Game1.worldDuplicateBorder;
            bool wrapR = bod.Position.X > Game1.worldWidthInBlocks - Game1.worldDuplicateBorder;

            for (int i = -1; i <= 1; i++)
            {
                if (i == -1 && !wrapL) continue;
                if (i == 1 && !wrapR) continue;

                effect.Parameters["View"].SetValue(mat * Matrix.CreateTranslation(new Vector3(-i * Game1.worldWidthInBlocks, 0, 0)) * cameraMatrix);
                effect.CurrentTechnique.Passes[0].Apply();

                foreach (Fixture fix in bod.FixtureList)
                {
                    DrawQuadShape(fix.Shape, Color.White, textureScale);
                }

                Flush();
            }
        }

        public void DrawTetrisPiece(TetrisPiece piece)
        {
            Matrix mat = Matrix.CreateRotationZ(piece.body.Rotation) * Matrix.CreateTranslation(new Vector3(piece.body.Position, 0.0f));

            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1));
            //effect.Parameters["View"].SetValue(mat);
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["BasicTexture"].SetValue(piece.texture);
            effect.CurrentTechnique.Passes[0].Apply();

            bool wrapL = piece.body.Position.X < Game1.worldDuplicateBorder;
            bool wrapR = piece.body.Position.X > Game1.worldWidthInBlocks - Game1.worldDuplicateBorder;

            for (int i = -1; i <= 1; i++)
            {
                if (i == -1 && !wrapL) continue;
                if (i == 1 && !wrapR) continue;

                effect.Parameters["View"].SetValue(mat * Matrix.CreateTranslation(new Vector3(-i * Game1.worldWidthInBlocks, 0, 0)) * cameraMatrix);
                effect.CurrentTechnique.Passes[0].Apply();

                for (int y = 0; y < piece.shape.GetLength(0); y++)
                {
                    for (int x = 0; x < piece.shape.GetLength(1); x++)
                    {
                        if (!piece.shape[y, x]) continue;
                        Vertices v = new Vertices(new Vector2[] { new Vector2(x + 0, y + 0), new Vector2(x + 1, y + 0), new Vector2(x + 1, y + 1), new Vector2(x + 0, y + 1) });
                        DrawPolygon(Color.White, 1.0f / 4.0f, v);
                    }
                }

                Flush();
            }
        }

        public void DrawAlignedQuad(Vector2 center, Vector2 size, Texture2D texture)
        {
            Matrix mat = Matrix.CreateTranslation(new Vector3(center, 0.0f)) * cameraMatrix;

            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1));
            effect.Parameters["View"].SetValue(mat);
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["BasicTexture"].SetValue(texture);
            effect.CurrentTechnique.Passes[0].Apply();

            Vertices vertices = new Vertices(new Vector2[] { new Vector2(-size.X / 2, -size.Y / 2), new Vector2(size.X / 2, -size.Y / 2), new Vector2(size.X / 2, size.Y / 2), new Vector2(-size.X / 2, size.Y / 2) });
            _quadVertices[_quadVertsCount].Color = Color.White;
            _quadVertices[_quadVertsCount].TextureCoordinate = new Vector2(0, 0);
            _quadVertices[_quadVertsCount++].Position = new Vector3(-size.X / 2, -size.Y / 2, 0f);
            _quadVertices[_quadVertsCount].Color = Color.White;
            _quadVertices[_quadVertsCount].TextureCoordinate = new Vector2(1, 0);
            _quadVertices[_quadVertsCount++].Position = new Vector3(size.X / 2, -size.Y / 2, 0f);
            _quadVertices[_quadVertsCount].Color = Color.White;
            _quadVertices[_quadVertsCount].TextureCoordinate = new Vector2(1, 1);
            _quadVertices[_quadVertsCount++].Position = new Vector3(size.X / 2, size.Y / 2, 0f);

            _quadVertices[_quadVertsCount].Color = Color.White;
            _quadVertices[_quadVertsCount].TextureCoordinate = new Vector2(0, 0);
            _quadVertices[_quadVertsCount++].Position = new Vector3(-size.X / 2, -size.Y / 2, 0f);
            _quadVertices[_quadVertsCount].Color = Color.White;
            _quadVertices[_quadVertsCount].TextureCoordinate = new Vector2(1, 1);
            _quadVertices[_quadVertsCount++].Position = new Vector3(size.X / 2, size.Y / 2, 0f);
            _quadVertices[_quadVertsCount].Color = Color.White;
            _quadVertices[_quadVertsCount].TextureCoordinate = new Vector2(0, 1);
            _quadVertices[_quadVertsCount++].Position = new Vector3(-size.X / 2, size.Y / 2, 0f);

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

        private void DrawQuadShape(Shape shape, Color color, float textureScale)
        {
            if (shape.ShapeType == ShapeType.Polygon)
            {
                PolygonShape loop = (PolygonShape)shape;
                color = DrawPolygon(color, textureScale, loop.Vertices);
            }
        }

        private Color DrawPolygon(Color color, float textureScale, Vertices vertices)
        {
            for (int i = 0; i < vertices.Count - 1; ++i)
            {
                if (_quadVertsCount + 3 >= _quadVertices.Length)
                    Flush();
                _quadVertices[_quadVertsCount].Color = color;
                _quadVertices[_quadVertsCount].TextureCoordinate = vertices[0] * textureScale;
                _quadVertices[_quadVertsCount++].Position = new Vector3(vertices[0], 0f);
                _quadVertices[_quadVertsCount].Color = color;
                _quadVertices[_quadVertsCount].TextureCoordinate = vertices[i] * textureScale;
                _quadVertices[_quadVertsCount++].Position = new Vector3(vertices[i], 0f);
                _quadVertices[_quadVertsCount].Color = color;
                _quadVertices[_quadVertsCount].TextureCoordinate = vertices[i + 1] * textureScale;
                _quadVertices[_quadVertsCount++].Position = new Vector3(vertices[i + 1], 0f);
            }
            return color;
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
