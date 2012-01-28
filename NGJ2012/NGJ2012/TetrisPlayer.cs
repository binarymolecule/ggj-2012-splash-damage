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
using FarseerPhysics.Dynamics.Joints;


namespace NGJ2012
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TetrisPlayer : DrawableGameComponentExtended
    {
        private World _world;
        const float movementSpeed = 8.0f;

        List<bool[,]> tetrisShapes = new List<bool[,]>();
        List<Texture2D> tetrisTextures = new List<Texture2D>();

        List<TetrisPiece> pieces = new List<TetrisPiece>();
        private TetrisPiece currentPiece;
        FixedAngleJoint currentPieceRotation;
        OnCollisionEventHandler currentPieceCollide;
        TetrisPieceBatch drawer;

        public TetrisPlayer(Game game, World world)
            : base(game)
        {
            _world = world;

            tetrisShapes.Add(new bool[,] { { true, false }, { true, false }, { true, true } });
            tetrisShapes.Add(new bool[,] { { false, true }, { false, true }, { true, true } });
            tetrisShapes.Add(new bool[,] { { true, true }, { true, true } });
            tetrisShapes.Add(new bool[,] { { false, true, false }, { true, true, true } });
            tetrisShapes.Add(new bool[,] { { true }, { true }, { true }, { true } });
            tetrisShapes.Add(new bool[,] { { false, true, true }, { true, true, false } });
            tetrisShapes.Add(new bool[,] { { true, true, false }, { false, true, true } });
            currentPiece = null;
        }

        bool currentPieceCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            // ignore collisions with Cat30
            if ((fixtureA.CollisionCategories & Category.Cat1) != 0) return false;
            if ((fixtureB.CollisionCategories & Category.Cat1) != 0) return false;

            dropCurrentPiece();
            return true;
        }

        private void dropCurrentPiece()
        {
            currentPiece.body.LinearVelocity = Vector2.Zero;
            currentPiece.body.OnCollision -= currentPieceCollide;
            currentPieceCollide = null;
            currentPiece = null;
            _world.RemoveJoint(currentPieceRotation);
            currentPieceRotation = null;
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
            drawer = new TetrisPieceBatch(GraphicsDevice);

            base.LoadContent();
        }

        bool upDown = false;
        bool downDown = false;
        bool paused = false;
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            if (paused) return;

            if (currentPiece == null)
            {
                currentPiece = new TetrisPiece(_world, null, tetrisShapes[(new Random()).Next(tetrisShapes.Count)], new Vector2(2, 2));
                currentPieceCollide = new OnCollisionEventHandler(currentPieceCollision);
                currentPiece.body.OnCollision += currentPieceCollide;
                currentPieceRotation = JointFactory.CreateFixedAngleJoint(_world, currentPiece.body);
                pieces.Add(currentPiece);
            }

            // TODO: Add your update code here
            Vector2 moveDir = new Vector2();
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Left)) moveDir.X = -1;
            else if (state.IsKeyDown(Keys.Right)) moveDir.X = +1;
            else moveDir.X = 0;

            if (state.IsKeyDown(Keys.Space)) moveDir.Y = 3;
            else moveDir.Y = 0.25f;

            if (state.IsKeyDown(Keys.M)) paused = true;


            currentPiece.body.LinearVelocity = moveDir * movementSpeed;

            if (state.IsKeyDown(Keys.Down))
            {
                if (!downDown)
                {
                    currentPieceRotation.TargetAngle += (float)Math.PI / 2;
                    downDown = true;
                }
            }
            else downDown = false;

            if (state.IsKeyDown(Keys.Up))
            {
                if (!upDown)
                {
                    currentPieceRotation.TargetAngle -= (float)Math.PI / 2;
                    upDown = true;
                }
            }
            else upDown = false;


            base.Update(gameTime);
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            drawer.cameraMatrix = camera;
            foreach (TetrisPiece cur in pieces)
            {
                drawer.DrawBody(cur.body);
            }
        }

    
    }
}
