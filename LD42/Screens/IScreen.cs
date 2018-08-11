using Microsoft.Xna.Framework;

namespace LD42.Screens {
    public interface IScreen {
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);
    }
}
