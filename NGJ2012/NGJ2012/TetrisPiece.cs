using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class TetrisPiece
    {
        public double freezeCountdown = 1000;
        public Body body;
        public List<Fixture> fixtures;
        public Texture2D texture;
        public bool[,] shape;

        public TetrisPiece(World world, Texture2D texture, bool[,] shape, Vector2 position)
        {
            this.shape = shape;
            body = BodyFactory.CreateBody(world, position);

            fixtures = new List<Fixture>();
            for (int y = 0; y < shape.GetLength(0); y++)
            {
                for (int x = 0; x < shape.GetLength(1); x++)
                {
                    if (!shape[y, x]) continue;
                    float grow = -0.01f;
                    Vertices v = new Vertices(new Vector2[] { new Vector2(x + 0 + grow, y + 0 + grow), new Vector2(x + 1 - grow, y + 0 + grow), new Vector2(x + 1 - grow, y + 1 - grow), new Vector2(x + 0 + grow, y + 1 - grow) });
                    fixtures.Add(FixtureFactory.AttachPolygon(v, 10.0f, body));
                }
            }

            body.BodyType = BodyType.Dynamic;
            body.Restitution = 0.0f;
            body.Friction = 100.0f;
            body.CollisionCategories = Game1.COLLISION_GROUP_TETRIS_BLOCKS;
            body.CollidesWith = Game1.COLLISION_GROUP_STATIC_OBJECTS | Game1.COLLISION_GROUP_TETRIS_BLOCKS | Game1.COLLISION_GROUP_DEFAULT | Game1.COLLISION_GROUP_LEVEL_SEPARATOR;
            body.Enabled = false;
            body.LinearDamping = 2.0f;

            this.texture = texture;
        }

    }
}

