using Artemis;
using Artemis.Interface;
using Microsoft.Xna.Framework;

namespace LD42.Ecs.Components {
    public sealed class HandComponent : IComponent {
        public HandComponent(Vector2 targetPosition) {
            TargetPosition = targetPosition;
        }

        public Vector2 TargetPosition { get; set; }
        public Entity HeldItem { get; set; } = null;
        public Entity HeldTool { get; set; } = null;
        public float Timer { get; set; } = 0f;
    }
}
