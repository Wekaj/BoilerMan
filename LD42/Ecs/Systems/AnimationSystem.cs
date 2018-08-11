using Artemis;
using LD42.Ecs.Components;
using System;

namespace LD42.Ecs.Systems {
    public sealed class AnimationSystem : EntityUpdatingSystem {
        public AnimationSystem() 
            : base(Aspect.All(typeof(AnimationComponent), typeof(SpriteComponent))) {
        }

        public override void Process(Entity entity) {
            AnimationComponent animationComponent = entity.GetComponent<AnimationComponent>();
            SpriteComponent spriteComponent = entity.GetComponent<SpriteComponent>();
            
            if (animationComponent.Animation != null && animationComponent.Timer < animationComponent.Duration) {
                animationComponent.Timer += (float)DeltaTime.TotalSeconds;
                animationComponent.Timer = Math.Min(animationComponent.Timer, animationComponent.Duration);

                float p = animationComponent.Timer / animationComponent.Duration;
                spriteComponent.SourceRectangle = animationComponent.Animation.GetFrame(p);
            }
        }
    }
}
