using Artemis;
using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace LD42.Ecs.Components {
    public sealed class MinionComponent : IComponent {
        public Entity TargetItem { get; set; }
        public Vector2 Offset { get; set; }
        public int Age { get; set; }
    }
}
