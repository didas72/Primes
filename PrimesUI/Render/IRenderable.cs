using System;
using System.Collections.Generic;

using DidasUtils.Numerics;

namespace Primes.UI.Render
{
    public interface IRenderable
    {
        Vector2i Position { get; set; }

        void Render(Vector2i localOffset);
    }
}
