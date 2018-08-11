using Artemis;
using Artemis.System;
using LD42.Ecs.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace LD42.Ecs.Systems {
    public sealed class ObjectCollisionSystem : EntitySystem {
        private readonly List<Entity> _entities = new List<Entity>();

        public ObjectCollisionSystem() 
            : base(Aspect.All(typeof(ObjectComponent), typeof(PositionComponent), typeof(ForceComponent))) {
        }

        protected override void ProcessEntities(IDictionary<int, Entity> entities) {
            _entities.AddRange(entities.Values);

            for (int i = 0; i < _entities.Count; i++) {
                for (int j = i + 1; j < _entities.Count; j++) {
                    CheckCollision(_entities[i], _entities[j]);
                }
            }

            _entities.Clear();
        }

        private void CheckCollision(Entity entity1, Entity entity2) {
            ObjectComponent objectComponent1 = entity1.GetComponent<ObjectComponent>(),
                objectComponent2 = entity2.GetComponent<ObjectComponent>();

            if (objectComponent1.IsHeld || objectComponent2.IsHeld
                || !objectComponent1.IsSolid || !objectComponent2.IsSolid) {
                return;
            }

            PositionComponent positionComponent1 = entity1.GetComponent<PositionComponent>(),
                positionComponent2 = entity2.GetComponent<PositionComponent>();
            ForceComponent forceComponent1 = entity1.GetComponent<ForceComponent>(),
                forceComponent2 = entity2.GetComponent<ForceComponent>();

            Vector2 difference = positionComponent1.Position - positionComponent2.Position;

            float distance = difference.Length();
            if (distance == 0f || distance > objectComponent1.Radius + objectComponent2.Radius) {
                return;
            }

            float strength = 1000f / distance;
            Vector2 force = Vector2.Normalize(difference) * strength;

            forceComponent1.Force += force;
            forceComponent2.Force -= force;
        }
    }
}
