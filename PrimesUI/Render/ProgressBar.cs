using System;
using System.Collections.Generic;

using DidasUtils.Numerics;

using Raylib_cs;

namespace Primes.UI.Render
{
    public class ProgressBar : IRenderable
    {
        public Vector2i Position { get; set; }
        public Vector2i Size { get; set; }
        public Color BorderColor { get; set; }
        public Color EmptyColor { get; set; }
        public Color FillColor { get; set; }
        public float Value { get; set; }



        public ProgressBar(Vector2i position, Vector2i size)
        {
            Position = position; Size = size; BorderColor = Color.WHITE; EmptyColor = Color.GRAY; FillColor = Color.GREEN; Value = 0; 
        }
        public ProgressBar(Vector2i position, Vector2i size, Color borderColor, Color emptyColor, Color fillColor)
        {
            Position = position; Size = size; BorderColor = borderColor; EmptyColor = emptyColor; FillColor = fillColor; Value = 0;
        }



        public void Render(Vector2i localOffset)
        {
            Raylib.DrawRectangle(Position.x + localOffset.x, Position.y + localOffset.y, Size.x, Size.y, BorderColor);
            Raylib.DrawRectangle(Position.x + localOffset.x + 1, Position.y + localOffset.y + 1, Size.x - 2, Size.y - 2, EmptyColor);
            Raylib.DrawRectangle(Position.x + localOffset.x + 1, Position.y + localOffset.y + 1, (int)(Value * (Size.x - 2)), Size.y - 2, FillColor);
        }
    }
}
