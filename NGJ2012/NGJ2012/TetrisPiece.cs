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
    class TetrisPiece
    {
        public Body body;
        public List<Fixture> fixtures;
        public Texture2D texture;

        public TetrisPiece(World world, Texture2D texture, bool[,] shape, Vector2 position)
        {
            body = BodyFactory.CreateBody(world, position);

            fixtures = new List<Fixture>();
            for (int y = 0; y < shape.GetLength(0); y++)
            {
                for (int x = 0; x < shape.GetLength(1); x++)
                {
                    if (!shape[y, x]) continue;
                    Vertices v = new Vertices(new Vector2[] { new Vector2(x+0, y+0), new Vector2(x+1, y+0), new Vector2(x+1,y+1), new Vector2(x+0, y+1) });
                    fixtures.Add(FixtureFactory.AttachPolygon(v, 1.0f, body));
                }
            }

            body.BodyType = BodyType.Dynamic;
            body.Restitution = 0.1f;
            body.Friction = 1.0f;
            body.CollisionCategories = Game1.COLLISION_GROUP_TETRIS_BLOCKS;
            body.CollidesWith = Game1.COLLISION_GROUP_STATIC_OBJECTS | Game1.COLLISION_GROUP_TETRIS_BLOCKS | Game1.COLLISION_GROUP_DEFAULT | Game1.COLLISION_GROUP_LEVEL_SEPARATOR;

            this.texture = texture;
        }
    }
}

