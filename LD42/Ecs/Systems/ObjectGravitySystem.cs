using Artemis;
using LD42.Ecs.Components;
using LD42.Tools;
using System;

namespace LD42.Ecs.Systems {
    public sealed class ObjectGravitySystem : EntityUpdatingSystem {
        private readonly Furnace _furnace;

        public ObjectGravitySystem(Furnace furnace) 
            : base(Aspect.All(typeof(ObjectComponent), typeof(PositionComponent))) {
            _furnace = furnace;
        }

        public override void Process(Entity entity) {
            ObjectComponent objectComponent = entity.GetComponent<ObjectComponent>();
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();

            float floor = 0f;
            if ((_furnace.IsOpen && _furnace.Region.Contains(positionComponent.Position))
                || positionComponent.Depth < 0f) {
                floor = -100f;
            }

            if (!objectComponent.IsHeld && positionComponent.Depth > floor) {
                positionComponent.Depth = Math.Max(positionComponent.Depth - 1000f * (float)DeltaTime.TotalSeconds, floor);
            }
        }
    }
}
