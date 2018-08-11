using Artemis;
using LD42.Ecs.Components;

namespace LD42.Ecs.Systems {
    public sealed class VelocitySystem : EntityUpdatingSystem {
        public VelocitySystem() 
            : base(Aspect.All(typeof(VelocityComponent), typeof(PositionComponent))) {
        }

        public override void Process(Entity entity) {
            VelocityComponent velocityComponent = entity.GetComponent<VelocityComponent>();
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();

            positionComponent.Position += velocityComponent.Velocity * (float)DeltaTime.TotalSeconds;

            float speed = velocityComponent.Velocity.Length();
            if (speed > 0f) {
                float ratio = (speed - velocityComponent.Friction * (float)DeltaTime.TotalSeconds) / speed;
                if (ratio < 0f) {
                    ratio = 0f;
                }
                velocityComponent.Velocity = velocityComponent.Velocity * ratio;
            }
        }
    }
}
