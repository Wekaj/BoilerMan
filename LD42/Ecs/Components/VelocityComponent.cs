using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace LD42.Ecs.Components {
    public sealed class VelocityComponent : IComponent {
        public VelocityComponent() {
            Velocity = Vector2.Zero;
        }

        public VelocityComponent(float friction) {
            Velocity = Vector2.Zero;
            Friction = friction;
        }

        public VelocityComponent(Vector2 velocity) {
            Velocity = velocity;
        }

        public VelocityComponent(float x, float y) {
            Velocity = new Vector2(x, y);
        }

        public Vector2 Velocity { get; set; }
        public float Friction { get; set; } = 0f;
        public float MaxSpeed { get; set; } = float.MaxValue;
    }
}
