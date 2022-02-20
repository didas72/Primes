using System;
using System.Collections.Generic;

using DidasUtils.Numerics;

using Raylib_cs;

namespace Primes.UI.Render
{
    public class TextBox : IRenderable
    {
        public string Text { get; set; }
        public int FontSize { get; set; }
        public Vector2i Position { get; set; }
        public Vector2i Size { get; set; }
        public Color TextColor { get; set; }



        public TextBox(string text, Vector2i position, Vector2i size)
        {
            Text = text; FontSize = 20; Position = position; Size = size; TextColor = Color.WHITE;
        }
        public TextBox(string text, int fontSize, Vector2i position, Vector2i size, Color textColor)
        {
            Text = text; FontSize = fontSize; Position = position; Size = size; TextColor = textColor;
        }



        public void Render(Vector2i localOffset)
        {
            string lText = Text;
            while (Raylib.MeasureText(lText, FontSize) > Size.x) //trim to fit in given size
                lText = lText[0..^1];
            Raylib.DrawText(lText, Position.x + localOffset.x, Position.y + localOffset.y, FontSize, TextColor);
        }
    }
}
