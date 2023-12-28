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
        public string Id_Name { get; set; }
        public List<string> Lines { get; set; }
        public int FontSize { get; set; }
        public Vector2i Position { get; set; }
        public Vector2i Size { get; set; }
        public Color TextColor { get; set; }
        public Color ScrollBackColor { get; set; }
        public Color ScrollFrontColor { get; set; }
        public int Scroll { get; set; }


        public bool AllowSelect { get; set; }
        public int Selected { get; set; }
        public Color SelectColor { get; set; }
        public Color HighlightColor { get; set; }
        public EventHandler OnSelected { get; set; }

        public bool UseCustomFont { get; set; }
        public Font CustomFont { get; set; }

        private int highlit;



        public TextList(Vector2i position, Vector2i size)
        {
            Lines = new(); FontSize = 20; Position = position; Size = size; TextColor = Color.WHITE; ScrollBackColor = new(77, 77, 77, 255); ScrollFrontColor = new(122, 122, 122, 255); Scroll = 0;
            AllowSelect = false; SelectColor = new(0, 0, 0, 0); HighlightColor = new(0, 0, 0, 0); Selected = -1;
        }
        public TextList(List<string> lines, int fontSize, Vector2i position, Vector2i size, Color textColor, Color scrollBackColor, Color scrollFrontColor)
        {
            Lines = lines; FontSize = fontSize; Position = position; Size = size; TextColor = textColor; ScrollBackColor = scrollBackColor; ScrollFrontColor = scrollFrontColor; Scroll = 0;
            AllowSelect = false; SelectColor = new(0, 0, 0, 0); HighlightColor = new(0, 0, 0, 0); Selected = -1;
        }



        public void Update(Vector2i localOffset)
        {
            Vector2 mousePos = Raylib.GetMousePosition() - (Vector2)localOffset - (Vector2)Position;

            if (mousePos.X > 0 && mousePos.X < Size.x && mousePos.Y > 0 && mousePos.Y < Size.y)
            {
                int scrollMult = (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) ? 20 : 1) * (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL) ? 50 : 1);

                Scroll -= (int)Raylib.GetMouseWheelMove() * scrollMult;//negative bc mousewheel is inverted
                Scroll = Mathf.Clamp(Scroll, 0, Math.Max(Lines.Count - 1, 0));

                if (AllowSelect)
                {
                    highlit = (int)Math.Round(mousePos.Y / FontSize) + Scroll;

                    if (Raylib.IsMouseButtonPressed(0))
                    {
                        Selected = highlit;
                        if (Selected >= 0 && Selected < Lines.Count)
                            OnSelected?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }
        public void Render(Vector2i localOffset)
        {
            int yOff = 0;//store y offset so we can determine where to stop drawing

            for (int i = Scroll; i < Lines.Count; i++)
            {
                if (yOff + FontSize > Size.y) break;

                string lText = Lines[i];

                if (!UseCustomFont)
                {
                    while (Raylib.MeasureText(lText, FontSize) > Math.Max(Size.x - 4, 1)) //trim to fit in given size
                        lText = lText[0..^1];
                    Raylib.DrawText(lText, Position.x + localOffset.x, Position.y + localOffset.y + yOff, FontSize, AllowSelect && i == Selected ? SelectColor : AllowSelect && i == highlit ? HighlightColor : TextColor);
                    yOff += FontSize;
                }
                else
                {
                    while (Raylib.MeasureTextEx(CustomFont, lText, FontSize, 2f).X > Math.Max(Size.x - 4, 1)) //trim to fit in given size
                        lText = lText[0..^1];
                    Raylib.DrawTextEx(CustomFont, lText, new Vector2(Position.x + localOffset.x, Position.y + localOffset.y + yOff), FontSize, 2f, AllowSelect && i == Selected ? SelectColor : AllowSelect && i == highlit ? HighlightColor : TextColor);
                    yOff += FontSize;
                }
            }

            int scrollBY = (int)(((float)Scroll / Math.Max(Lines.Count, 1)) * Size.y);
            Raylib.DrawRectangle(Position.x + localOffset.x + Size.x - 4, Position.y + localOffset.y, 4, Size.y, ScrollBackColor);
            Raylib.DrawRectangle(Position.x + localOffset.x + Size.x - 4, Position.y + localOffset.y + scrollBY, 4, Size.y / Math.Max(Lines.Count, 2), ScrollFrontColor);
        }



        public static TextList CreateSelectable(Vector2i position, Vector2i size)
        {
            TextList list = new(position, size);
            list.AllowSelect = true;
            list.SelectColor = Color.BLUE;
            list.HighlightColor = Color.GRAY;

            return list;
        }
    }
}
