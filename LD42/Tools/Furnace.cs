using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

namespace LD42.Tools {
    public sealed class Furnace : ITool {
        public const float ClosedTime = 3.5f, OpenTime = 0.5f, TotalTime = ClosedTime + OpenTime;

        private readonly SoundEffect _gateOpenSound, _gateCloseSound;

        public Furnace(Rectangle region, SoundEffect gateOpenSound, SoundEffect gateCloseSound) {
            Region = region;

            _gateOpenSound = gateOpenSound;
            _gateCloseSound = gateCloseSound;
        }

        public bool IsActive { get; private set; }

        public Rectangle Region { get; }
        public bool IsOpen => Timer > ClosedTime;
        public float Timer { get; private set; }

        public void Update(TimeSpan deltaTime, bool isActive) {
            IsActive = isActive;

            bool wasOpen = IsOpen;

            if (isActive) {
                Timer += (float)deltaTime.TotalSeconds;
                Timer %= TotalTime;

                if (!wasOpen && IsOpen) {
                    _gateOpenSound.Play();
                }
                else if (wasOpen && !IsOpen) {
                    _gateCloseSound.Play();
                }
            }
        }
    }
}
