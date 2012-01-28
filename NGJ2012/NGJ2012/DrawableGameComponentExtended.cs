using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NGJ2012
{
    public abstract class DrawableGameComponentExtended : DrawableGameComponent
    {
        public DrawableGameComponentExtended(Game game) : base(game) 
        { 
        }
        public abstract void DrawGameWorldOnce(Matrix camera, bool platformMode);
    }
}
