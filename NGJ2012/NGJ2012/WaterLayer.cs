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

namespace NGJ2012
{
    /// <summary>
    /// Implement animated water layer.
    /// </summary>
    public class WaterLayer : DrawableGameComponentExtended
    {
        Game1 parent;
        Rectangle screenRect;

        // Physical objects
        //Body waterBody;
        Vector2 pos;
        public Vector2 Position { get { return pos; } }
        public float Height { get { return pos.Y; } }
        public int resolution = 5;

        float riseSpeed = 0;
        int riseTime = 0;

        // Assets
        Texture2D waterTexture;
        private DynamicVertexBuffer vb;
        private VertexPositionColor[] array;
        private BasicEffect _basicEffect;
        private Effect effect;

        public WaterLayer(Game game) : base(game)
        {
            parent = (Game1)game;
            screenRect = new Rectangle(0, 0, parent.WorldWidthInBlocks * 96, parent.WorldHeightInBlocks * 96);

            // Create physical objects
            /*
            waterBody = BodyFactory.CreateRectangle(parent.World, parent.WorldWidthInBlocks, 1, 1.0f,
                                                    new Vector2(parent.WorldWidthInBlocks / 2.0f, parent.WorldHeightInBlocks));
            waterBody.BodyType = BodyType.Static;
            waterBody.Friction = float.MaxValue;
            */
            pos = new Vector2(0, -1);
        }

        protected override void LoadContent()
        {
            waterTexture = parent.Content.Load<Texture2D>("graphics/level/water");

            array = new VertexPositionColor[(parent.WorldWidthInBlocks * resolution) * 2 + 2];
            vb = new DynamicVertexBuffer(GraphicsDevice, typeof(VertexPositionColor), array.Length, BufferUsage.None);

            int i = 0;
            for (var x = 0; x <= parent.WorldWidthInBlocks; x++)
            {
                for (var substep = 0; substep < resolution && (x != parent.WorldWidthInBlocks || substep == 0); substep++)
                {
                    var rx = x + substep * (1 / resolution);
                    array[i++] = new VertexPositionColor(new Vector3(rx, 0, 0), Color.Blue);
                    array[i++] = new VertexPositionColor(new Vector3(rx, -1, 0), Color.Blue);
                }
            }

            vb.SetData(array);

            _basicEffect = new BasicEffect(GraphicsDevice);
            _basicEffect.VertexColorEnabled = true;

            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);

            effect = Game.Content.Load<Effect>("shapes/WaterShader");
        }

        protected override void UnloadContent()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public void StartRising(int msec)
        {
            // Start rising one block
            riseTime = msec;
            riseSpeed = 1.0f / (0.001f * msec);
        }

        public override void Update(GameTime gameTime)
        {
            // Move water layer upwards
            if (riseTime > 0)
            {
                int msec = gameTime.ElapsedGameTime.Milliseconds;
                pos.Y -= riseSpeed * (0.001f * msec);
                riseTime -= msec;
                if (riseTime <= 0)
                    riseTime = 0; // stop rising
            }

            var i = 0;
            for (var x = 0; x <= parent.WorldWidthInBlocks; x++)
            {
                for (var substep = 0; substep < resolution && (x != parent.WorldWidthInBlocks || substep == 0); substep++)
                {
                    var rx = x + substep * (1 / resolution);
                    array[i++] = new VertexPositionColor(new Vector3(rx, 2, 0), Color.Black);

                    float h = pos.Y;
                    //h += (float)Math.Sin(rx - gameTime.TotalGameTime.TotalSeconds * 2.1) * 0.1f - 0.1f;
                    //h += (float)Math.Sin(rx - gameTime.TotalGameTime.TotalSeconds * 1.2) * 0.2f - 0.2f;
                    h += (float)Perlin.noise(rx * 0.1f, gameTime.TotalGameTime.TotalSeconds * 0.1, 0);


                    array[i++] = new VertexPositionColor(new Vector3(rx, h, 0), Color.White);
                }
            }

            vb.SetData(array);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            effect.Parameters["Projection"].SetValue(Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1));
            effect.Parameters["View"].SetValue(camera);
            effect.Parameters["World"].SetValue(Matrix.Identity);
            //effect.Parameters["BasicTexture"].SetValue(texture);
            effect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.SetVertexBuffer(vb);
            GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, vb.VertexCount-2);



            //Vector2 screenPos = Vector2.Transform(pos, camera);
            //screenRect.X = (int)screenPos.X;
            //screenRect.Y = (int)screenPos.Y;
            
            //parent.SpriteBatch.Begin();
            //parent.SpriteBatch.Draw(waterTexture, screenRect, Color.White * 0.5f);
            //parent.SpriteBatch.End();
        }
    }
}
