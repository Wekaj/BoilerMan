using Artemis.Interface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD42.Ecs.Components {
    public sealed class SpriteComponent : IComponent {
        public SpriteComponent(Texture2D texture) {
            Texture = texture;
            Origin = Vector2.Zero;
        }

        public SpriteComponent(Texture2D texture, Vector2 origin) {
            Texture = texture;
            Origin = origin;
        }

        public Texture2D Texture { get; set; }
        public Vector2 Origin { get; set; }
    }
}
