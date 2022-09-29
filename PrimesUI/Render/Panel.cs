using System;
using System.Collections.Generic;
using System.Numerics;

using DidasUtils.Numerics;

using Raylib_cs;

namespace Primes.UI.Render
{
    public class Panel : IRenderable
    {
        public string Id_Name { get; set; }
        public Vector2i Position { get; set; }
        public Vector2i Size { get; set; }
        public Color Color { get; set; }



        public Panel(Vector2i position, Vector2i size, Color color)
        {
            Position = position;
            Size = size;
            Color = color;
        }



        public void Render(Vector2i localOffset)
        {
            Raylib.DrawRectangle(Position.x + localOffset.x, Position.y + localOffset.y, Size.x, Size.y, Color);
        }
    }
}
