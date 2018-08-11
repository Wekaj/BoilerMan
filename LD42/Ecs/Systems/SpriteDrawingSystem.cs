using Artemis;
using Artemis.System;
using LD42.Ecs.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD42.Ecs.Systems {
    public sealed class SpriteDrawingSystem : EntityProcessingSystem {
        private readonly SpriteBatch _spriteBatch;

        public SpriteDrawingSystem(SpriteBatch spriteBatch) 
            : base(Aspect.All(typeof(SpriteComponent), typeof(PositionComponent))) {
            _spriteBatch = spriteBatch;
        }

        public override void Process(Entity entity) {
            SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();

            Vector2 position = positionComponent.Position - new Vector2(0f, positionComponent.Depth);

            _spriteBatch.Draw(spriteComponent.Texture, 
                position, 
                origin: spriteComponent.Origin,
                rotation: spriteComponent.Rotation,
                effects: spriteComponent.Effects,
                layerDepth: spriteComponent.LayerDepth);
        }
    }
}
