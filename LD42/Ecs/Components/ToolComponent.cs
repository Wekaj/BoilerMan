using Artemis;
using Artemis.Interface;
using Microsoft.Xna.Framework;
using System;

namespace LD42.Ecs.Components {
    public sealed class ToolComponent : IComponent {
        public ToolComponent(float radius, Func<float, Vector2> positionFunction) {
            Radius = radius;
            PositionFunction = positionFunction;
        }

        public float Radius { get; set; }
        public Entity HoldingHand { get; set; } = null;
        public Func<float, Vector2> PositionFunction { get; set; }
    }
}
