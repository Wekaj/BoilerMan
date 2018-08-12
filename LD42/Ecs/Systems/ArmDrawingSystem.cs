using Artemis;
using Artemis.System;
using LD42.Ecs.Components;
using LD42.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LD42.Ecs.Systems {
    public sealed class ArmDrawingSystem : EntityProcessingSystem {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _armTexture, _jointTexture;

        public ArmDrawingSystem(SpriteBatch spriteBatch, Texture2D armTexture, Texture2D jointTexture) 
            : base(Aspect.All(typeof(HandComponent), typeof(PositionComponent))) {
            _spriteBatch = spriteBatch;
            _armTexture = armTexture;
            _jointTexture = jointTexture;
        }

        public override void Process(Entity entity) {
            HandComponent handComponent = entity.GetComponent<HandComponent>();
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();

            float shoulderHeight = 32f;

            Vector2 difference = positionComponent.Position - handComponent.Shoulder;
            float distance = difference.Length();

            Vector2 elbow = handComponent.Shoulder + difference / 2f;
            float elbowHeight = (positionComponent.Depth + shoulderHeight) / 2f + 10000f / distance;

            Vector2 hand = positionComponent.Position + Vector2.Normalize(handComponent.Shoulder - positionComponent.Position) * 18f;
            hand -= new Vector2(0f, 6f);

            Vector2 p1 = handComponent.Shoulder - new Vector2(0f, shoulderHeight);
            Vector2 p2 = elbow - new Vector2(0f, elbowHeight);
            Vector2 p3 = hand - new Vector2(0f, positionComponent.Depth);

            DrawSegment(p1, p2);
            _spriteBatch.Draw(_jointTexture, p2, origin: new Vector2(10f), layerDepth: Layers.Arms);
            DrawSegment(p2, p3);
        }

        private void DrawSegment(Vector2 p1, Vector2 p2) {
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length = (p2 - p1).Length();
            _spriteBatch.Draw(_armTexture, p1, rotation: angle, origin: new Vector2(8f), 
                scale: new Vector2(length / _armTexture.Width, 1f), layerDepth: Layers.Arms);
        }
    }
}
