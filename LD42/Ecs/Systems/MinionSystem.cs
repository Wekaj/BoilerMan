using Artemis;
using LD42.Ecs.Components;
using LD42.Items;
using LD42.Tools;
using Microsoft.Xna.Framework;
using System;

namespace LD42.Ecs.Systems {
    public sealed class MinionSystem : EntityUpdatingSystem {
        private static readonly Random _random = new Random();

        private readonly Furnace _furnace;
        private readonly SimpleTool _musicBox;

        public MinionSystem(Furnace furnace, SimpleTool musicBox) 
            : base(Aspect.All(typeof(PositionComponent), typeof(ForceComponent), typeof(ObjectComponent), typeof(MinionComponent))) {
            _furnace = furnace;
            _musicBox = musicBox;
        }

        public override void Process(Entity entity) {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            ForceComponent forceComponent = entity.GetComponent<ForceComponent>();
            ObjectComponent objectComponent = entity.GetComponent<ObjectComponent>();
            MinionComponent minionComponent = entity.GetComponent<MinionComponent>();

            float speed = _musicBox.IsActive ? 20f : 18f;

            if (_random.Next(1000) < 100) {
                minionComponent.Offset = new Vector2(64f * ((float)_random.NextDouble() * 2f - 1f), 64f * ((float)_random.NextDouble() * 2f - 1f));
                minionComponent.Age++;
            }

            if (objectComponent.IsHeld || positionComponent.Depth != 0f) {
                return;
            }

            if (minionComponent.TargetItem != null) {
                if (minionComponent.TargetItem.DeletingState) {
                    minionComponent.TargetItem = null;
                }
                else {
                    PositionComponent itemPositionComponent = minionComponent.TargetItem.GetComponent<PositionComponent>();
                    if (itemPositionComponent == null || itemPositionComponent.Depth < 0f || itemPositionComponent.Position.Y <= _furnace.Region.Bottom - 4f) {
                        ObjectComponent itemObjectComponent = minionComponent.TargetItem.GetComponent<ObjectComponent>();
                        if (itemObjectComponent != null) {
                            itemObjectComponent.IsMarked = false;
                        }

                        minionComponent.TargetItem = null;
                    }
                }
            }

            if (minionComponent.TargetItem == null) {
                float closestDistance = float.MaxValue;
                Entity closestObject = null;
                foreach (Entity item in EntityWorld.EntityManager.GetEntities(Aspect.All(typeof(ObjectComponent), typeof(PositionComponent)))) {
                    ObjectComponent itemObjectComponent = item.GetComponent<ObjectComponent>();
                    if (itemObjectComponent.Type != Item.Coal || itemObjectComponent.IsMarked) {
                        if (itemObjectComponent.Type != Item.Minion && minionComponent.Age > 1000 && !itemObjectComponent.IsMarked) {

                        }
                        else {
                            continue;
                        }
                    }

                    PositionComponent itemPositionComponent = item.GetComponent<PositionComponent>();
                    if (itemPositionComponent.Depth < 0f || itemPositionComponent.Position.Y <= _furnace.Region.Bottom) {
                        continue;
                    }

                    float distance = (itemPositionComponent.Position - positionComponent.Position).LengthSquared();
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestObject = item;
                    }
                }
                minionComponent.TargetItem = closestObject;

                if (closestObject != null) {
                    ObjectComponent itemObjectComponent = closestObject.GetComponent<ObjectComponent>();
                    itemObjectComponent.IsMarked = true;
                }
            }

            if (positionComponent.Position.Y <= _furnace.Region.Bottom + 40f && _furnace.Timer > Furnace.ClosedTime - 1f) {
                Vector2 target = _furnace.Region.Center.ToVector2() + new Vector2(0f, 192f) + minionComponent.Offset * 2f;

                forceComponent.Force += Vector2.Normalize(target - positionComponent.Position) * speed;
            }
            else if (minionComponent.TargetItem != null) {
                PositionComponent itemPositionComponent = minionComponent.TargetItem.GetComponent<PositionComponent>();

                Vector2 difference = itemPositionComponent.Position - positionComponent.Position;
                float distance = difference.Length();
                if (distance > 32f) {
                    Vector2 target = itemPositionComponent.Position + minionComponent.Offset * Math.Min((distance - 32f) / 100f, 1f);

                    forceComponent.Force += Vector2.Normalize(target - positionComponent.Position) * speed;
                }
                else {
                    float hDir = _furnace.Region.Left - positionComponent.Position.X;
                    if (positionComponent.Position.X > _furnace.Region.Center.X) {
                        hDir = _furnace.Region.Right - positionComponent.Position.X;
                    }

                    forceComponent.Force += new Vector2(hDir / 30f, -speed);

                    ForceComponent itemForceComponent = minionComponent.TargetItem.GetComponent<ForceComponent>();

                    itemForceComponent.Force += new Vector2(hDir / 30f, -speed);
                }
            }
            else {
                Vector2 target = _furnace.Region.Center.ToVector2() + new Vector2(0f, 160f) + minionComponent.Offset * 2f;

                forceComponent.Force += Vector2.Normalize(target - positionComponent.Position) * speed;
            }
        }
    }
}
