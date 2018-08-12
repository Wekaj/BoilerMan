using Artemis;
using Artemis.System;
using LD42.Ecs.Components;
using LD42.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace LD42.Ecs.Systems {
    public sealed class ArmDrawingSystem : EntityProcessingSystem {
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _armTexture, _jointTexture;

        private readonly SortedSet<Arm> _arms = new SortedSet<Arm>(Comparer<Arm>.Create((a1, a2) => {
            int i = a1.Shoulder.Y.CompareTo(a2.Shoulder.Y);
            
            if (i == 0) {
                i = a1.Elbow.Y.CompareTo(a2.Elbow.Y);
            }

            return i == 0 ? 1 : i;
        }));

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
            float elbowHeight = Math.Min((positionComponent.Depth + shoulderHeight) / 2f + 10000f / distance, 100f);

            Vector2 hand = positionComponent.Position + Vector2.Normalize(handComponent.Shoulder - positionComponent.Position) * 18f;
            hand -= new Vector2(0f, 6f);

            _arms.Add(new Arm(handComponent.Shoulder, shoulderHeight,
                elbow, elbowHeight,
                hand, positionComponent.Depth));
        }

        protected override void End() {
            base.End();

            float i = 0f;
            foreach (Arm arm in _arms) {
                Vector2 p1 = arm.Shoulder - new Vector2(0f, arm.ShoulderDepth);
                Vector2 p2 = arm.Elbow - new Vector2(0f, arm.ElbowDepth);
                Vector2 p3 = arm.Hand - new Vector2(0f, arm.HandDepth);

                float i1 = 0.0002f, i2 = 0.0001f, i3 = 0f;

                if (p2.Y < p1.Y) {
                    i1 = 0f;
                    i2 = 0.0001f;
                    i3 = 0.0002f;
                }

                DrawSegment(p1, p2, i + i1);
                _spriteBatch.Draw(_jointTexture, p2, origin: new Vector2(10f), layerDepth: Layers.Arms + i + i2);
                DrawSegment(p2, p3, i + i3);

                i += 0.001f;
            }
            _arms.Clear();
        }

        private void DrawSegment(Vector2 p1, Vector2 p2, float i) {
            float angle = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            float length = (p2 - p1).Length();
            _spriteBatch.Draw(_armTexture, p1, rotation: angle, origin: new Vector2(8f), 
                scale: new Vector2(length / _armTexture.Width, 1f), layerDepth: Layers.Arms + i);
        }

        private sealed class Arm {
            public Arm(Vector2 shoulder, float shoulderDepth,
                Vector2 elbow, float elbowDepth,
                Vector2 hand, float handDepth) {
                Shoulder = shoulder;
                Elbow = elbow;
                Hand = hand;

                ShoulderDepth = shoulderDepth;
                ElbowDepth = elbowDepth;
                HandDepth = handDepth;
            }

            public Vector2 Shoulder { get; set; }
            public Vector2 Elbow { get; set; }
            public Vector2 Hand { get; set; }

            public float ShoulderDepth { get; set; }
            public float ElbowDepth { get; set; }
            public float HandDepth { get; set; }
        }
    }
}
