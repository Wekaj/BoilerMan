using Microsoft.Xna.Framework;
using System;

namespace LD42.Tools {
    public sealed class Furnace : ITool {
        private const float _closedTime = 3.5f, _openTime = 0.5f, _totalTime = _closedTime + _openTime;

        private float _timer;

        public Furnace(Rectangle region) {
            Region = region;
        }

        public Rectangle Region { get; }
        public bool IsOpen => _timer > _closedTime;

        public void Update(TimeSpan deltaTime) {
            _timer += (float)deltaTime.TotalSeconds;
            _timer %= _totalTime;
        }
    }
}
