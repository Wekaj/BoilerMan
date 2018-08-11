using System;

namespace LD42.Tools {
    public interface ITool {
        void Update(TimeSpan deltaTime, bool isActive);
    }
}
