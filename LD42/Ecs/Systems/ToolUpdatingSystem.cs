using Artemis;
using LD42.Ecs.Components;

namespace LD42.Ecs.Systems {
    public sealed class ToolUpdatingSystem : EntityUpdatingSystem {
        public ToolUpdatingSystem() 
            : base(Aspect.All(typeof(ToolComponent))) {
        }

        public override void Process(Entity entity) {
            ToolComponent toolComponent = entity.GetComponent<ToolComponent>();

            if (toolComponent.HoldingHand != null) {
                toolComponent.Tool.Update(DeltaTime);
            }
        }
    }
}
