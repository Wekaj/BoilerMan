using Artemis;
using LD42.Ecs.Components;
using LD42.Graphics;

namespace LD42.Ecs.Systems {
    public sealed class HoldingSystem : EntityUpdatingSystem {
        public HoldingSystem() 
            : base(Aspect.All(typeof(HandComponent), typeof(PositionComponent))) {
        }

        public override void Process(Entity entity) {
            HandComponent handComponent = entity.GetComponent<HandComponent>();
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();

            if (handComponent.HeldItem != null) {
                ObjectComponent heldObjectComponent = handComponent.HeldItem.GetComponent<ObjectComponent>();
                if (heldObjectComponent != null) {
                    heldObjectComponent.IsHeld = true;

                    PositionComponent heldPositionComponent = handComponent.HeldItem.GetComponent<PositionComponent>();
                    heldPositionComponent.Position = positionComponent.Position;
                    heldPositionComponent.Depth = positionComponent.Depth;
                }
            }
            else if (handComponent.HeldTool != null) {
                handComponent.Timer += (float)DeltaTime.TotalSeconds;
            }
        }
    }
}
