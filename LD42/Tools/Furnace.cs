using Microsoft.Xna.Framework;
using System;

namespace LD42.Tools {
    public sealed class Furnace : ITool {
        public const float ClosedTime = 3.5f, OpenTime = 0.5f, TotalTime = ClosedTime + OpenTime;

        public Furnace(Rectangle region) {
            Region = region;
        }

        public bool IsActive { get; private set; }

        public Rectangle Region { get; }
        public bool IsOpen => Timer > ClosedTime;
        public float Timer { get; private set; }

        public void Update(TimeSpan deltaTime, bool isActive) {
            IsActive = isActive;

            if (isActive) {
                Timer += (float)deltaTime.TotalSeconds;
                Timer %= TotalTime;
            }
        }
    }
}
