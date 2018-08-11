using Artemis;
using Artemis.Interface;
using LD42.Items;
using LD42.Tools;
using Microsoft.Xna.Framework;
using System;

namespace LD42.Ecs.Components {
    public sealed class ToolComponent : IComponent {
        public ToolComponent(ITool tool, float radius, Func<float, Vector2> positionFunction) {
            Tool = tool;
            Radius = radius;
            PositionFunction = positionFunction;
        }

        public ITool Tool { get; set; }
        public float Radius { get; set; }
        public Func<float, Vector2> PositionFunction { get; set; }
        public Entity HoldingHand { get; set; } = null;
    }
}
