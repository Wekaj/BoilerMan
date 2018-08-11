using System;

namespace LD42.Tools {
    public sealed class SimpleTool : ITool {
        public bool IsActive { get; private set; }

        public void Update(TimeSpan deltaTime, bool isActive) {
            IsActive = isActive;
        }
    }
}
