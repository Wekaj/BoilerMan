using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace LD42.Ecs.Components {
    public sealed class PositionComponent : IComponent {
        public PositionComponent() {
            Position = Vector2.Zero;
        }

        public PositionComponent(Vector2 position) {
            Position = position;
        }

        public PositionComponent(float x, float y) {
            Position = new Vector2(x, y);
        }

        public Vector2 Position { get; set; }
        public float Depth { get; set; } = 0f; 
    }
}
