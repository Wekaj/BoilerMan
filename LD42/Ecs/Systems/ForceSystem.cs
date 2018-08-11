using Artemis;
using LD42.Ecs.Components;
using Microsoft.Xna.Framework;

namespace LD42.Ecs.Systems {
    public sealed class ForceSystem : EntityUpdatingSystem {
        public ForceSystem() 
            : base(Aspect.All(typeof(ForceComponent), typeof(VelocityComponent))) {
        }

        public override void Process(Entity entity) {
            ForceComponent forceComponent = entity.GetComponent<ForceComponent>();
            VelocityComponent velocityComponent = entity.GetComponent<VelocityComponent>();

            velocityComponent.Velocity += forceComponent.Force / forceComponent.Mass;
            forceComponent.Force = Vector2.Zero;
        }
    }
}
