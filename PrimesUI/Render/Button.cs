using System;
using System.Collections.Generic;
using System.Numerics;

using DidasUtils.Numerics;

using Raylib_cs;

namespace Primes.UI.Render
{
    public class Button : IRenderable, IUpdatable
    {
        public string Id_Name { get; set; }
        public string Text { get; set; }
        public int FontSize { get; set; }
        public Vector2i Position { get; set; }
        public Vector2i Size { get; set; }
        public Color TextColor { get; set; }
        public Color NormalColor { get; set; }
        public Color SelectedColor { get; set; }


        public EventHandler OnPressed { get; set; }
        public bool IsSelected { get; private set; } = false;



        public Button(string text, Vector2i position, Vector2i size)
        {
            Text = text; FontSize = 20; Position = position; Size = size; TextColor = Color.WHITE; NormalColor = Color.GRAY; SelectedColor = Color.BLUE; 
        }
        public Button(string text, int fontSize,Vector2i position, Vector2i size, Color textColor, Color normalColor, Color selectedColor)
        {
            Text = text; FontSize = fontSize; Position = position; Size = size; TextColor = textColor; NormalColor = normalColor; SelectedColor = selectedColor;
        }



        public void Update(Vector2i localOffset)
        {
            Vector2 mousePos = Raylib.GetMousePosition() - (Vector2)localOffset;

            if (mousePos.X > Position.x && mousePos.X < Position.x + Size.x && mousePos.Y > Position.y && mousePos.Y < Position.y + Size.y)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    OnPressed?.Invoke(this, EventArgs.Empty);
                }

                if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    IsSelected = true;
                }
                else
                    IsSelected = false;
            }
            else
                IsSelected = false;
        }
        public void Render(Vector2i localOffset)
        {
            string lText = Text;
            while (Raylib.MeasureText(lText, FontSize) > Size.x - 2) //trim to fit in given size
                lText = lText[0..^1];
            Raylib.DrawRectangle(Position.x + localOffset.x, Position.y + localOffset.y, Size.x, Size.y, IsSelected ? SelectedColor : NormalColor);
            Raylib.DrawText(lText, Position.x + localOffset.x + 1, Position.y + localOffset.y + 1, FontSize, TextColor);
        }
    }
}
