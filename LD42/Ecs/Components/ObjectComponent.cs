using Artemis.Interface;
using LD42.Items;

namespace LD42.Ecs.Components {
    public sealed class ObjectComponent : IComponent {
        public ObjectComponent(Item type, float radius) {
            Type = type;
            Radius = radius;
        }

        public Item Type { get; set; }
        public float Radius { get; set; }
        public bool IsHeld { get; set; } = false;
        public Item SpawnerType { get; set; } = Item.None;
    }
}
