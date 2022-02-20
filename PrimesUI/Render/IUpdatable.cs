using System;

using DidasUtils.Numerics;

namespace Primes.UI.Render
{
    public interface IUpdatable
    {
        void Update(Vector2i offset);
    }
}
