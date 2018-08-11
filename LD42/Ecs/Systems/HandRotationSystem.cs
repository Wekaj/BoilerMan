using Artemis;
using Artemis.System;
using LD42.Ecs.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LD42.Ecs.Systems {
    public sealed class HandRotationSystem : EntityProcessingSystem {
        private readonly Vector2 _origin;

        public HandRotationSystem(Vector2 origin) 
            : base(Aspect.All(typeof(HandComponent), typeof(SpriteComponent), typeof(PositionComponent))) {
            _origin = origin;
        }

        public override void Process(Entity entity) {
            SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();

            spriteComponent.Effects = positionComponent.Position.X < _origin.X ? SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally;
            spriteComponent.Rotation = (float)Math.Atan2(positionComponent.Position.Y - _origin.Y, positionComponent.Position.X - _origin.X);
        }
    }
}
