using Microsoft.Xna.Framework;

namespace LD42.Tools {
    public sealed class Furnace {
        public Furnace(Rectangle region) {
            Region = region;
        }

        public Rectangle Region { get; }
        public bool IsOpen { get; } = true;
    }
}
