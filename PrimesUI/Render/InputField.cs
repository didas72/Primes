using System;
using System.Collections.Generic;
using System.Numerics;

using DidasUtils.Numerics;

using Raylib_cs;

namespace Primes.UI.Render
{
    internal class InputField : IRenderable, IUpdatable
    {
        public string Id_Name { get; set; }
        public string Text { get; set; }
        public int FontSize { get; set; }
        public Vector2i Position { get; set; }
        public Vector2i Size { get; set; }
        public Color SelectedColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color TextColor { get; set; }
        public bool IsSelected { get; set; }
        public EventHandler<string> TextChanged { get; set; }



        public InputField(Vector2i position, Vector2i size)
        {
            Text = string.Empty; FontSize = 20; Position = position; Size = size; SelectedColor = Color.DARKGRAY; BackgroundColor = Color.GRAY; TextColor = Color.WHITE;
        }
        public InputField(string text, int fontSize, Vector2i position, Vector2i size, Color selectedColor, Color backgroundColor, Color textColor)
        {
            Text = text; FontSize = fontSize; Position = position; Size = size; SelectedColor = selectedColor; BackgroundColor = backgroundColor; TextColor = textColor;
        }



        public void Update(Vector2i offset)
        {
            Vector2 mousePos = Raylib.GetMousePosition() - (Vector2)offset;

            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON))
            {
                if (mousePos.X > Position.x && mousePos.X < Position.x + Size.x && mousePos.Y > Position.y && mousePos.Y < Position.y + Size.y)
                    IsSelected = true;
                else
                    IsSelected = false;
            }

            if (IsSelected)
            {
                int key = Raylib.GetKeyPressed(); //Doesn't consider Shift+Key combos
                if (key == 0) return;

                //DidasUtils.Logging.Log.LogEvent(key.ToString(), "InputField");

                if (key >= 32 && key <= 126)
                    Text += (char)key;
                else if (key == 259 && Text.Length > 0)
                    Text = Text[0..^1];
                else if (key == 340) //shift will be colon temporarily
                    Text += ':';
            }
        }

        public void Render(Vector2i localOffset)
        {
            string lText = Text;
            while (Raylib.MeasureText(lText, FontSize) > Size.x - 2) //trim to fit in given size
                lText = lText[1..];
            Raylib.DrawRectangle(Position.x + localOffset.x, Position.y + localOffset.y, Size.x, Size.y, IsSelected ? SelectedColor : BackgroundColor);
            Raylib.DrawText(lText, Position.x + localOffset.x + 1, Position.y + localOffset.y + 1, FontSize, TextColor);
        }
    }
}
