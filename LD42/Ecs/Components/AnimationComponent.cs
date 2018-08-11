﻿using Artemis.Interface;
using LD42.Graphics;

namespace LD42.Ecs.Components {
    public sealed class AnimationComponent : IComponent {
        public Animation Animation { get; set; } = null;
        public float Duration { get; set; }
        public float Timer { get; set; }

        public void Play(Animation animation, float duration) {
            Animation = animation;
            Duration = duration;
            Timer = 0f;
        }
    }
}
