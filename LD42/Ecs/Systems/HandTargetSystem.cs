using Artemis;
using LD42.Ecs.Components;
using Microsoft.Xna.Framework;

namespace LD42.Ecs.Systems {
    public sealed class HandTargetSystem : EntityUpdatingSystem {
        public HandTargetSystem() 
            : base(Aspect.All(typeof(HandComponent), typeof(PositionComponent))) {
        }

        public override void Process(Entity entity) {
            HandComponent handComponent = entity.GetComponent<HandComponent>();
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();

            Vector2 target = handComponent.TargetPosition;
            if (handComponent.HeldTool != null) {
                ToolComponent toolComponent = handComponent.HeldTool.GetComponent<ToolComponent>();
                target += toolComponent.PositionFunction(handComponent.Timer);
            }
            
            Vector2 difference = target - positionComponent.Position;
            positionComponent.Position += difference * 10f * (float)DeltaTime.TotalSeconds;
        }
    }
}
