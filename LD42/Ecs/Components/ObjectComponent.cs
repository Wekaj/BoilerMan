using Artemis.Interface;

namespace LD42.Ecs.Components {
    public sealed class ObjectComponent : IComponent {
        public ObjectComponent(float radius) {
            Radius = radius;
        }

        public float Radius { get; set; }
    }
}
