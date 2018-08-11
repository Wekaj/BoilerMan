using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace LD42.Ecs.Components {
    public sealed class ForceComponent : IComponent {
        public ForceComponent(float mass) {
            Mass = mass;
        }

        public float Mass { get; set; }
        public Vector2 Force { get; set; }
    }
}
