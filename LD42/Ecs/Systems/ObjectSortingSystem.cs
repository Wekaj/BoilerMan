using Artemis;
using Artemis.System;
using LD42.Ecs.Components;
using LD42.Graphics;
using LD42.Items;

namespace LD42.Ecs.Systems {
    public sealed class ObjectSortingSystem : EntityProcessingSystem {
        public ObjectSortingSystem() 
            : base(Aspect.All(typeof(ObjectComponent), typeof(SpriteComponent), typeof(PositionComponent))) {
        }

        public override void Process(Entity entity) {
            ObjectComponent objectComponent = entity.GetComponent<ObjectComponent>();
            SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();

            if (positionComponent.Depth < 0f) {
                spriteComponent.LayerDepth = Layers.BelowGround;
            }
            else if (positionComponent.Depth < 16f) {
                spriteComponent.LayerDepth = objectComponent.Type == Item.Minion ? Layers.Minion : Layers.OnGround;
            }
            else {
                spriteComponent.LayerDepth = Layers.Air;
            }
        }
    }
}
