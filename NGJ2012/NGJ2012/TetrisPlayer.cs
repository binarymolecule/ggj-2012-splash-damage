using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
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
        float currentPieceMaxLen;
        FixedAngleJoint currentPieceRotation;
        OnCollisionEventHandler currentPieceCollide;
        TetrisPieceBatch drawer;

        // Absolute position in world coordinate system where new pieces are spawned
        public GameViewport viewportToSpawnIn;

        public TetrisPlayer(Game game, World world) : base(game)
        {
            _world = world;

            tetrisShapes.Add(new bool[,] { { true, false }, { true, false }, { true, true } });
            tetrisShapes.Add(new bool[,] { { false, true }, { false, true }, { true, true } });
            tetrisShapes.Add(new bool[,] { { true, true }, { true, true } });
            tetrisShapes.Add(new bool[,] { { false, true, false }, { true, true, true } });
            tetrisShapes.Add(new bool[,] { { true }, { true }, { true }, { true } });
            tetrisShapes.Add(new bool[,] { { false, true, true }, { true, true, false } });
            tetrisShapes.Add(new bool[,] { { true, true, false }, { false, true, true } });

            Game1.Timers.Create(1.0f, false, Spawn);
        }

        bool currentPieceCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            // ignore collisions with Cat30
            if ((fixtureA.CollisionCategories & Game1.COLLISION_GROUP_DEFAULT) != 0) return false;
            if ((fixtureB.CollisionCategories & Game1.COLLISION_GROUP_DEFAULT) != 0) return false;

            dropCurrentPiece();
            return true;
        }

        private void Spawn(Utility.Timer timer)
        {
            int shape = (new Random()).Next(tetrisShapes.Count);
            currentPieceMaxLen = Math.Max(tetrisShapes[shape].GetLength(0), tetrisShapes[shape].GetLength(1));
            currentPiece = new TetrisPiece(_world, tetrisTextures[shape], tetrisShapes[shape], viewportToSpawnIn.cameraPosition + new Vector2(0,-viewportToSpawnIn.screenHeightInGAME/2.0f + 1.0f));
            currentPieceCollide = new OnCollisionEventHandler(currentPieceCollision);
            currentPiece.body.OnCollision += currentPieceCollide;
            currentPieceRotation = JointFactory.CreateFixedAngleJoint(_world, currentPiece.body);
            pieces.Add(currentPiece);

            Debug.Print("Spawn new tetris piece at: {0}, {1}", currentPiece.body.Position.X, currentPiece.body.Position.Y);
        }

        private void dropCurrentPiece()
        {
            currentPiece.body.LinearVelocity = Vector2.Zero;
            currentPiece.body.OnCollision -= currentPieceCollide;
            currentPieceCollide = null;
            currentPiece = null;
            _world.RemoveJoint(currentPieceRotation);
            currentPieceRotation = null;

            Game1.Timers.Create(1.0f, false, Spawn);
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
            drawer = new TetrisPieceBatch(GraphicsDevice, Game.Content);

            string[] shapeNames = new string[] { "LR","LL","O","T","I","MZ","Z" };
            for (int i = 0; i < shapeNames.Length; i++)
            {
                string n = "shapes/" + shapeNames[i];
                tetrisTextures.Add(Game.Content.Load<Texture2D>(n));
            }

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

            // TODO: Add your update code here
            Vector2 moveDir = new Vector2();
            KeyboardState state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Left)) moveDir.X = -1;
            else if (state.IsKeyDown(Keys.Right)) moveDir.X = +1;
            else moveDir.X = 0;

            if (state.IsKeyDown(Keys.Space)) moveDir.Y = 3;
            else moveDir.Y = 0.25f;

            if (state.IsKeyDown(Keys.M)) paused = true;

            if (currentPiece != null)
            {
                float spawnWidth = viewportToSpawnIn.screenWidthInGAME / 2.0f;
                float spawnL = viewportToSpawnIn.cameraPosition.X - spawnWidth + currentPieceMaxLen / 2.0f;
                float spawnR = viewportToSpawnIn.cameraPosition.X + spawnWidth - currentPieceMaxLen / 2.0f;
                Vector2 currentPieceCenter = currentPiece.body.GetWorldPoint(currentPiece.body.LocalCenter);
                if (currentPieceCenter.X < spawnL && moveDir.X < 0) moveDir.X = 0;
                if (spawnR < currentPieceCenter.X && moveDir.X > 0) moveDir.X = 0;
                currentPiece.body.LinearVelocity = moveDir * movementSpeed;

                if (currentPieceCenter.X < spawnL) currentPiece.body.LinearVelocity = new Vector2(currentPiece.body.LinearVelocity.X + (spawnL-currentPieceCenter.X)*10,currentPiece.body.LinearVelocity.Y);

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

            }


            base.Update(gameTime);
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            drawer.cameraMatrix = camera;
            foreach (TetrisPiece cur in pieces)
            {
                drawer.DrawBodyTextured(cur.body, cur.texture);
            }
        }


    }
}
