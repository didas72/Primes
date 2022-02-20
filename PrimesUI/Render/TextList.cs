using System;
using System.Collections.Generic;
using System.Numerics;

using DidasUtils;
using DidasUtils.Numerics;

using Raylib_cs;

namespace Primes.UI.Render
{
    public class TextList : IRenderable, IUpdatable
    {
        public List<string> Lines { get; set; }
        public int FontSize { get; set; }
        public Vector2i Position { get; set; }
        public Vector2i Size { get; set; }
        public Color TextColor { get; set; }
        public Color ScrollBackColor { get; set; }
        public Color ScrollFrontColor { get; set; }
        public int Scroll { get; set; }



        public TextList(Vector2i position, Vector2i size)
        {
            Lines = new(); FontSize = 20; Position = position; Size = size; TextColor = Color.WHITE; ScrollBackColor = new(77, 77, 77, 255); ScrollFrontColor = new(102, 102, 102, 255); Scroll = 0;
        }
        public TextList(List<string> lines, int fontSize, Vector2i position, Vector2i size, Color textColor, Color scrollBackColor, Color scrollFrontColor)
        {
            Lines = lines; FontSize = fontSize; Position = position; Size = size; TextColor = textColor; ScrollBackColor = scrollBackColor; ScrollFrontColor = scrollFrontColor; Scroll = 0;
        }



        public void Update(Vector2i localOffset)
        {
            Vector2 mousePos = Raylib.GetMousePosition() - localOffset.ToVector2();

            if (mousePos.X > Position.x && mousePos.X < Position.x + Size.x && mousePos.Y > Position.y && mousePos.Y < Position.y + Size.y)
                Scroll -= (int)Raylib.GetMouseWheelMove();//negative bc mousewheel is inverted
            
            Scroll = Mathf.Clamp(Scroll, 0, Math.Max(Lines.Count - 1, 0));  
        }

        public void Render(Vector2i localOffset)
        {
            int yOff = 0;//store y offset so we can determine where to stop drawing

            for (int i = Scroll; i < Lines.Count; i++)
            {
                if (yOff + FontSize > Size.y) break;

                string lText = Lines[i];
                while (Raylib.MeasureText(lText, FontSize) > Math.Max(Size.x - 4, 1)) //trim to fit in given size
                    lText = lText[0..^1];
                Raylib.DrawText(lText, Position.x + localOffset.x, Position.y + localOffset.y + yOff, FontSize, TextColor);
                yOff += FontSize;
            }

            int scrollBY = (int)(((float)Scroll / Math.Max(Lines.Count, 1)) * Size.y);
            Raylib.DrawRectangle(Position.x + localOffset.x + Size.x - 4, Position.y + localOffset.y, 4, Size.y, ScrollBackColor);
            Raylib.DrawRectangle(Position.x + localOffset.x + Size.x - 4, Position.y + localOffset.y + scrollBY, 4, Size.y / Math.Max(Lines.Count, 1), ScrollFrontColor);
        }
    }
}
