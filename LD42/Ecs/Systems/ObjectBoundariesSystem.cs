using Artemis;
using LD42.Ecs.Components;
using Microsoft.Xna.Framework;
using System;

namespace LD42.Ecs.Systems {
    public sealed class ObjectBoundariesSystem : EntityUpdatingSystem {
        private readonly Rectangle _world;
        private readonly Rectangle[] _walls;

        public ObjectBoundariesSystem(Rectangle world, params Rectangle[] walls) 
            : base(Aspect.All(typeof(ObjectComponent), typeof(PositionComponent), typeof(ForceComponent))) {
            _world = world;
            _walls = walls;
        }

        public override void Process(Entity entity) {
            ObjectComponent objectComponent = entity.GetComponent<ObjectComponent>();

            if (objectComponent.IsHeld) {
                return;
            }

            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            ForceComponent forceComponent = entity.GetComponent<ForceComponent>();

            Vector2 worldForce = Vector2.Zero;
            worldForce.X -= Math.Max(0f, positionComponent.Position.X + objectComponent.Radius - _world.Right);
            worldForce.X += Math.Max(0f, _world.Left - positionComponent.Position.X + objectComponent.Radius);
            worldForce.Y -= Math.Max(positionComponent.Position.Y + objectComponent.Radius - _world.Bottom, 0f);
            worldForce.Y += Math.Max(_world.Top - positionComponent.Position.Y + objectComponent.Radius, 0f);
            if (worldForce.X != 0f || worldForce.Y != 0f) {
                forceComponent.Force += Vector2.Normalize(worldForce) * 100f;
            }

            Vector2 wallForce = Vector2.Zero;
            foreach (Rectangle wall in _walls) {
                if (wall.Contains(positionComponent.Position - new Vector2(0f, objectComponent.Radius))
                    || wall.Contains(positionComponent.Position + new Vector2(0f, objectComponent.Radius))
                    || wall.Contains(positionComponent.Position - new Vector2(objectComponent.Radius, 0f))
                    || wall.Contains(positionComponent.Position + new Vector2(objectComponent.Radius, 0f))) {
                    wallForce += positionComponent.Position - wall.Center.ToVector2();
                }
            }
            if (wallForce.X != 0f || wallForce.Y != 0f) {
                forceComponent.Force += Vector2.Normalize(wallForce) * 80f;
            }
        }
    }
}
