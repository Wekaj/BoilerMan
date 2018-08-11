using Artemis;
using Artemis.System;
using System;

namespace LD42.Ecs.Systems {
    public abstract class EntityUpdatingSystem : EntityProcessingSystem {
        protected EntityUpdatingSystem(Aspect aspect) 
            : base(aspect) {
        }

        protected TimeSpan DeltaTime { get; private set; }

        protected override void Begin() {
            base.Begin();

            DeltaTime = TimeSpan.FromTicks(EntityWorld.Delta);
        }
    }
}
