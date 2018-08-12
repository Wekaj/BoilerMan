using Microsoft.Xna.Framework.Audio;
using System;

namespace LD42.Tools {
    public sealed class Skylight : ITool {
        private readonly SoundEffect _openSound, _closeSound;

        public Skylight(SoundEffect openSound, SoundEffect closeSound) {
            _openSound = openSound;
            _closeSound = closeSound;
        }

        public bool IsActive { get; private set; }

        public void Update(TimeSpan deltaTime, bool isActive) {
            bool wasActive = IsActive;

            IsActive = isActive;

            if (!wasActive && IsActive) {
                _openSound.Play();
            }
            else if (wasActive && !IsActive) {
                _closeSound.Play();
            }
        }
    }
}
