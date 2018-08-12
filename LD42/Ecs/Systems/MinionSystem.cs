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

        private Vector2 GetNowhereTarget(Vector2 offset, bool left) {
            if (left) {
                return _furnace.Region.Location.ToVector2() + new Vector2(100f, _furnace.Region.Height + 160f) + offset * 2.5f;
            }
            else {
                return _furnace.Region.Location.ToVector2() + new Vector2(_furnace.Region.Width - 100f, _furnace.Region.Height + 160f) + offset * 2.5f;
            }
        }

        public override void Process(Entity entity) {
            PositionComponent positionComponent = entity.GetComponent<PositionComponent>();
            ForceComponent forceComponent = entity.GetComponent<ForceComponent>();
            ObjectComponent objectComponent = entity.GetComponent<ObjectComponent>();
            MinionComponent minionComponent = entity.GetComponent<MinionComponent>();

            float speed = _musicBox.IsActive ? 20f : 18f;

            if (_random.Next(1000) < 100) {
                minionComponent.Offset = new Vector2(64f * ((float)_random.NextDouble() * 2f - 1f), 64f * ((float)_random.NextDouble() * 2f - 1f));
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
                    if (itemObjectComponent.IsMarked || !WantsItem(itemObjectComponent.Type, minionComponent.Intelligence)) {
                        continue;
                    }

                    PositionComponent itemPositionComponent = item.GetComponent<PositionComponent>();
                    float checkDistance = 0f;
                    if (minionComponent.Intelligence < 30) {
                        checkDistance = 20f;
                    }
                    if (itemPositionComponent.Depth < 0f || itemPositionComponent.Position.Y <= _furnace.Region.Bottom - checkDistance) {
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

            float safetyDistance = 20f;
            if (minionComponent.Intelligence > 90) {
                safetyDistance = 40f;
            }
            else if (minionComponent.Intelligence > 80) {
                safetyDistance = 30f;
            }

            if (minionComponent.Intelligence > 50 && (positionComponent.Position.Y <= _furnace.Region.Bottom + safetyDistance && _furnace.Timer > Furnace.ClosedTime - 1f)) {
                Vector2 target = GetNowhereTarget(minionComponent.Offset, minionComponent.Leftie);

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
                    float xTarget = _furnace.Region.Left + 100f * minionComponent.Tendency;
                    if (positionComponent.Position.X > _furnace.Region.Center.X) {
                        xTarget = _furnace.Region.Right - 100f * minionComponent.Tendency;
                    }
                    float hDir = xTarget - positionComponent.Position.X;

                    forceComponent.Force += new Vector2(hDir / 30f, -speed);

                    ForceComponent itemForceComponent = minionComponent.TargetItem.GetComponent<ForceComponent>();

                    itemForceComponent.Force += new Vector2(hDir / 30f, -speed);
                }
            }
            else {
                Vector2 target = GetNowhereTarget(minionComponent.Offset, minionComponent.Leftie);

                forceComponent.Force += Vector2.Normalize(target - positionComponent.Position) * speed;
            }
        }

        private bool WantsItem(Item item, int intelligence) {
            if (item == Item.Coal) {
                return true;
            }

            if (intelligence > 90 && item == Item.SoulPlant) {
                return true;
            }

            if (intelligence > 50 && item == Item.MadPlant) {
                return true;
            }

            if (intelligence < 10 && item != Item.Minion && item != Item.None) {
                return true;
            }

            return false;
        }
    }
}
