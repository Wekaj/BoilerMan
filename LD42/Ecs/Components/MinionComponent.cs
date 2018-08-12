using Artemis;
using Artemis.Interface;
using Microsoft.Xna.Framework;
using System;

namespace LD42.Ecs.Components {
    public sealed class MinionComponent : IComponent {
        private static readonly Random _random = new Random();

        public Entity TargetItem { get; set; }
        public Vector2 Offset { get; set; }
        public int Intelligence { get; set; } = _random.Next(100);
        public float Tendency { get; set; } = (float)_random.NextDouble();
    }
}
