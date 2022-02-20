using System;
using System.Collections.Generic;
using System.Numerics;

using DidasUtils.Numerics;

using Raylib_cs;

namespace Primes.UI.Render
{
    public class Holder : IRenderable, IUpdatable
    {
        public string Id { get; }
        public Vector2i Position { get; set; }
        public List<IRenderable> Children { get; }
        public bool Enabled { get; set; } = true;



        public Holder(Vector2i position)
        {
            Position = position;
            Children = new();
            Id = string.Empty;
        }
        public Holder(Vector2i position, string id)
        {
            Position = position;
            Children = new();
            Id = id;
        }



        public void Update(Vector2i localOffset)
        {
            if (!Enabled) return;

            foreach (IRenderable renderable in Children)
            {
                if (renderable is IUpdatable upt)
                    upt.Update(localOffset + Position);
            }
        }
        public void Render(Vector2i localOffset)
        {
            if (!Enabled) return;

            foreach(IRenderable renderable in Children)
            {
                renderable.Render(localOffset + Position);
            }
        }
        public void Add(IRenderable renderable) => Children.Add(renderable);
    }
}
