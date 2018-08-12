using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LD42.Screens {
    public sealed class MenuScreen : IScreen {
        private readonly LD42Game _game;

        private readonly SpriteBatch _spriteBatch;

        private readonly Texture2D _screen, _scoreTexture;
        private readonly SoundEffect _swishSound;

        private readonly int _score;

        private bool _hasBeenReleased = false;
        private bool _isPressed;

        public MenuScreen(LD42Game game, string extra = "", int score = 0) {
            _game = game;

            _score = score;

            _spriteBatch = new SpriteBatch(game.GraphicsDevice);

            _screen = game.Content.Load<Texture2D>("Textures/start_screen" + extra);
            _scoreTexture = game.Content.Load<Texture2D>("Textures/score");

            _swishSound = game.Content.Load<SoundEffect>("Sounds/swish");
        }

        public void Update(GameTime gameTime) {
            bool wasPressed = _isPressed;
            
            _isPressed = Mouse.GetState().LeftButton == ButtonState.Pressed;

            if (_hasBeenReleased && !_isPressed && wasPressed && _game.IsActive) {
                _game.Screen = new GameScreen(_game);

                _swishSound.Play();
            }

            if (!_isPressed) {
                _hasBeenReleased = true;
            }
        }

        public void Draw(GameTime gameTime) {
            _spriteBatch.Begin();

            _spriteBatch.Draw(_screen, Vector2.Zero, Color.White);

            for (int i = 0; i < _score; i++) {
                int x = i / 8;
                int y = i % 8;

                _spriteBatch.Draw(_scoreTexture, new Vector2(_game.GraphicsDevice.Viewport.Width - 32f - x * 20f, 86f + y * 20f), Color.White);
            }

            _spriteBatch.End();
        }
    }
}
