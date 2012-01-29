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
using FarseerPhysics.Dynamics.Contacts;


namespace NGJ2012
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TetrisPlayer : DrawableGameComponentExtended
    {
        private World _world;
        const float movementSpeed = 16.0f;

        List<bool[,]> tetrisShapes = new List<bool[,]>();
        List<Texture2D> tetrisTextures = new List<Texture2D>();
        List<int> tetrisProb = new List<int>();

        List<TetrisPiece> pieces = new List<TetrisPiece>();
        List<TetrisPiece> activePieces = new List<TetrisPiece>();
        private TetrisPiece currentPiece, currentCheat;
        float currentPieceMaxLen;
        private TetrisPiece nextPiece;
        int countdownToCheat = 5;

        internal TetrisPiece nextTetrixPiece { get { return nextPiece; } }
        FixedAngleJoint currentPieceRotation;
        OnCollisionEventHandler currentPieceCollide;
        OnSeparationEventHandler currentPieceSeparate;
        TetrisPieceBatch drawer;

        public float SPAWN_TIME = 0.5f;

        // Absolute position in world coordinate system where new pieces are spawned
        public GameViewport viewportToSpawnIn;

        public TetrisPlayer(Game game, World world)
            : base(game)
        {
            _world = world;

            tetrisShapes.Add(new bool[,] { { true, false }, { true, false }, { true, true } });
            tetrisShapes.Add(new bool[,] { { true, true, true }, { false, false, true } });
            tetrisShapes.Add(new bool[,] { { true, true }, { true, true } });
            tetrisShapes.Add(new bool[,] { { true, true, true }, { false, true, false } });
            tetrisShapes.Add(new bool[,] { { true }, { true }, { true }, { true } });
            tetrisShapes.Add(new bool[,] { { false, true, true }, { true, true, false } });
            tetrisShapes.Add(new bool[,] { { true, true, false }, { false, true, true } });
            tetrisShapes.Add(new bool[,] { { true } });
            tetrisShapes.Add(new bool[,] { { true }, { true } });
            tetrisShapes.Add(new bool[,] { { true }, { true }, { true } });

            tetrisProb.Add(1); // LL
            tetrisProb.Add(1); // LR
            tetrisProb.Add(3); // O
            tetrisProb.Add(2); // T
            tetrisProb.Add(3); // I
            tetrisProb.Add(1); // Z
            tetrisProb.Add(1); // MZ
            tetrisProb.Add(1); // I1
            tetrisProb.Add(1); // I2
            tetrisProb.Add(1); // I3

            Game1.Timers.Create(SPAWN_TIME, false, Spawn);
        }

        bool currentPieceCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            // ignore collisions with Cat30
            //            if ((fixtureA.CollisionCategories & Game1.COLLISION_GROUP_DEFAULT) != 0) return false;
            if ((fixtureB.CollisionCategories & Game1.COLLISION_GROUP_DEFAULT) != 0)
                return false;
            if ((fixtureB.CollisionCategories & Game1.COLLISION_GROUP_LEVEL_SEPARATOR) != 0) 
                return false;

            dropCurrentPiece();
            return true;
        }

        void currentPieceSeparation(Fixture fixtureA, Fixture fixtureB)
        {
        }

        private void Spawn(Utility.Timer timer)
        {
            if (nextPiece == null)
            {
                nextPiece = getRandomTetrisPiece();
            }

            currentPiece = nextPiece;
            currentPieceMaxLen = Math.Max(currentPiece.shape.GetLength(0), currentPiece.shape.GetLength(1));
            currentPiece.body.Position = getSpawnPosition();
            currentPieceCollide = new OnCollisionEventHandler(currentPieceCollision);
            currentPieceSeparate = new OnSeparationEventHandler(currentPieceSeparation);
            currentPiece.body.OnCollision += currentPieceCollide;
            currentPiece.body.OnSeparation += currentPieceSeparate;
            currentPieceRotation = JointFactory.CreateFixedAngleJoint(_world, currentPiece.body);
            pieces.Add(currentPiece);
            activePieces.Add(currentPiece);

            currentCheat = null;
            --countdownToCheat;
            if (countdownToCheat < 0)
            {
                currentCheat = new TetrisPiece(_world, tetrisTextures[2], tetrisShapes[2], currentPiece.body.WorldCenter + new Vector2(1, -2));
                currentCheat.body.FixedRotation = true;
                currentCheat.body.Rotation = (float)Math.PI / 2;
                currentCheat.body.OnCollision += currentPieceCollide;
                currentCheat.body.OnSeparation += currentPieceSeparate;
                JointFactory.CreateRevoluteJoint(_world, currentCheat.body, currentPiece.body, currentPiece.body.LocalCenter);
                pieces.Add(currentCheat);
                activePieces.Add(currentCheat);
                countdownToCheat = 5;
            }

            currentPiece.body.Enabled = true;
            if(currentCheat!=null) currentCheat.body.Enabled = true;

            nextPiece = getRandomTetrisPiece();

            Debug.Print("Spawn new tetris piece at: {0}, {1}", currentPiece.body.Position.X, currentPiece.body.Position.Y);
        }

        private Vector2 getSpawnPosition()
        {
            Vector2 p = viewportToSpawnIn.cameraPosition + new Vector2(viewportToSpawnIn.screenWidthInGAME / 3.0f, -viewportToSpawnIn.screenHeightInGAME / 2.0f + 0.0f);
            while (p.X < 0) p.X += Game1.worldWidthInBlocks;
            while (p.X > Game1.worldWidthInBlocks) p.X -= Game1.worldWidthInBlocks;
            return p;
        }

        Random rnd = new Random();
        private TetrisPiece getRandomTetrisPiece()
        {
            int probSum = 0;
            foreach (int c in tetrisProb)
                probSum += c;
            int shapeS = rnd.Next(probSum);
            int shape = tetrisProb.Count - 1;
            for (int i = 0; i < tetrisProb.Count; i++)
			{
                shapeS -= tetrisProb[i];
                if (shapeS < 0)
                {
                    shape = i;
                    break;
                }
            }

            return new TetrisPiece(_world, tetrisTextures[shape], tetrisShapes[shape], new Vector2(-100, -100));
        }

        private void dropCurrentPiece()
        {
            if (currentPiece != null)
            {
                if (isCurrentPieceBlocked())
                {
                    currentPiece.body.Position = getSpawnPosition();
                }
                else
                {
                    currentPiece.body.LinearVelocity = Vector2.Zero;
                    currentPiece.body.ResetDynamics();
                    currentPiece.body.OnCollision -= currentPieceCollide;
                    currentPiece.body.OnSeparation -= currentPieceSeparate;
                    if (currentCheat != null)
                    {
                        currentCheat.body.OnCollision -= currentPieceCollide;
                        currentCheat.body.OnSeparation -= currentPieceSeparate;
                    }

                    if (currentPieceRotation != null)
                    {
                        _world.RemoveJoint(currentPieceRotation);
                        currentPieceRotation = null;
                    }

                    currentPiece = null;
                    currentPieceCollide = null;
                    currentCheat = null;

                    Game1.Timers.Create(SPAWN_TIME, false, Spawn);
                }
            }
        }

        private bool isCurrentPieceBlocked()
        {
            bool blocked = false;
            ContactEdge contact = currentPiece.body.ContactList;
            while (contact != null)
            {
                if ((contact.Other.FixtureList[0].CollisionCategories & Game1.COLLISION_GROUP_LEVEL_SEPARATOR) != 0) blocked = true;
                contact = contact.Next;
            }
            if (currentCheat != null)
            {
                contact = currentCheat.body.ContactList;
                while (contact != null)
                {
                    if ((contact.Other.FixtureList[0].CollisionCategories & Game1.COLLISION_GROUP_LEVEL_SEPARATOR) != 0) blocked = true;
                    contact = contact.Next;
                }
            }
            return blocked;
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

            string[] shapeNames = new string[] { "LR", "LL", "O", "T", "I", "MZ", "Z", "I1", "I2", "I3" };
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
            GamePadState gstate = GamePad.GetState((Game as Game1).PlayerIdTetris);
            if (state.IsKeyDown(Keys.Left)) moveDir.X = -1;
            else if (state.IsKeyDown(Keys.Right)) moveDir.X = +1;
            else moveDir.X = 0;
            if (gstate.IsConnected) moveDir.X = gstate.ThumbSticks.Left.X;

            if (state.IsKeyDown(Keys.Down) || gstate.IsButtonDown(Buttons.A) || gstate.ThumbSticks.Left.Y < -0.5) moveDir.Y = 3;
            else moveDir.Y = 0.25f;

            if (state.IsKeyDown(Keys.M)) paused = true;

            if (currentPiece != null)
            {
                float spawnWidth = viewportToSpawnIn.screenWidthInGAME / 2.0f;
                float spawnL = viewportToSpawnIn.cameraPosition.X - spawnWidth + currentPieceMaxLen / 2.0f;
                float spawnR = viewportToSpawnIn.cameraPosition.X + spawnWidth - currentPieceMaxLen / 2.0f;
                Vector2 currentPieceCenter = currentPiece.body.GetWorldPoint(currentPiece.body.LocalCenter);
//                if (currentPieceCenter.X < spawnL && moveDir.X < 0) moveDir.X = 0;
//                if (spawnR < currentPieceCenter.X && moveDir.X > 0) moveDir.X = 0;
                currentPiece.body.LinearVelocity = moveDir * movementSpeed;

 //              if (currentPieceCenter.X < spawnL && currentPieceCenter.X > spawnL+Game1.worldWidthInBlocks/2) currentPiece.body.LinearVelocity = new Vector2(currentPiece.body.LinearVelocity.X + (spawnL - currentPieceCenter.X) * 10, currentPiece.body.LinearVelocity.Y);

                if (state.IsKeyDown(Keys.PageDown) || gstate.IsButtonDown(Buttons.X))
                {
                    if (!downDown)
                    {
                        currentPieceRotation.TargetAngle += (float)Math.PI / 2;
                        downDown = true;
                    }
                }
                else downDown = false;

                if (state.IsKeyDown(Keys.Up) || gstate.IsButtonDown(Buttons.B))
                {
                    if (!upDown)
                    {
                        currentPieceRotation.TargetAngle -= (float)Math.PI / 2;
                        upDown = true;
                    }
                }
                else upDown = false;

                if (currentPiece.body.Position.Y > 10) dropCurrentPiece();

                if (currentPiece.body.Position.X < 0) currentPiece.body.Position = new Vector2(currentPiece.body.Position.X + Game1.worldWidthInBlocks, currentPiece.body.Position.Y);
                if (currentPiece.body.Position.X > Game1.worldWidthInBlocks) currentPiece.body.Position = new Vector2(currentPiece.body.Position.X - Game1.worldWidthInBlocks, currentPiece.body.Position.Y);
            }

            List<TetrisPiece> deactivateUs = new List<TetrisPiece>();
            foreach (TetrisPiece cur in activePieces)
            {
                if (cur.body.Awake) cur.freezeCountdown = 10;
                else
                {
                    Vector2 center = cur.body.GetWorldPoint(cur.body.LocalCenter);
                    if (center.Y < (Game as Game1).WaterLayer.Position.Y)
                    {
                        cur.freezeCountdown -= gameTime.ElapsedGameTime.TotalSeconds;
                        if (cur.freezeCountdown < 0)
                        {
                            cur.body.BodyType = BodyType.Static;
                            deactivateUs.Add(cur);
                        }
                    }
                }
            }

            foreach (TetrisPiece cur in deactivateUs)
                activePieces.Remove(cur);

            base.Update(gameTime);
        }

        public override void DrawGameWorldOnce(Matrix camera, bool platformMode)
        {
            drawer.cameraMatrix = camera;
            foreach (TetrisPiece cur in pieces)
            {
                bool tintMe = (cur == currentPiece || cur == currentCheat);
                Color colr = Color.White;
                if(tintMe) {
                    if(isCurrentPieceBlocked()) colr = new Color(0.0f, 0.0f, 0.0f, 0.5f);
                    else if ((Game as Game1).PlayerIdTetris == PlayerIndex.One) colr = new Color(1.0f, 0.3f, 0.3f, 1.0f);
                    else colr = new Color(0.3f, 1.0f, 0.3f, 1.0f);
                }
                drawer.DrawTetrisPiece(cur, colr);
                //drawer.DrawBody(cur.body);
            }
        }



        internal void reactiveAllPieces()
        {
            foreach (TetrisPiece piece in pieces)
            {
                if (!activePieces.Contains(piece))
                {
                    piece.body.BodyType = BodyType.Dynamic;
                    activePieces.Add(piece);
                }
            }
        }
    }
}
