using Artemis;
using Artemis.Manager;
using LD42.Ecs.Components;
using LD42.Ecs.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LD42.Screens {
    public sealed class GameScreen : IScreen {
        private readonly LD42Game _game;

        private readonly EntityWorld _entityWorld;
        private readonly Rectangle _ground, _box;

        private readonly SpriteBatch _spriteBatch;

        private Texture2D _groundTexture, _boxTexture, _coalTexture;

        public GameScreen(LD42Game game) {
            _game = game;

            _entityWorld = new EntityWorld();

            _ground = new Rectangle(0, 0, 448, 400);
            _ground.Offset((_game.GraphicsDevice.Viewport.Width - _ground.Width) / 2f, 
                (_game.GraphicsDevice.Viewport.Height - _ground.Height) / 2f + 36f);

            _box = new Rectangle(0, 0, 96, 160);
            _box.Offset((_game.GraphicsDevice.Viewport.Width - _box.Width) / 2f, 
                154f - _box.Height / 2f);

            _spriteBatch = new SpriteBatch(game.GraphicsDevice);

            CreateSystems();

            LoadContent(game.Content);

            Random random = new Random();
            for (int i = 0; i < 100; i++) {
                Entity coal = _entityWorld.CreateEntity();
                coal.AddComponent(new PositionComponent(100f + random.Next(600), 100f + random.Next(600)));
                coal.AddComponent(new VelocityComponent(1000f));
                coal.AddComponent(new ForceComponent(1f));
                coal.AddComponent(new ObjectComponent(7f));
                coal.AddComponent(new SpriteComponent(_coalTexture, _coalTexture.Bounds.Center.ToVector2()));
            }
        }

        private void CreateSystems() {
            _entityWorld.SystemManager.SetSystem(new ObjectCollisionSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new ObjectBoundariesSystem(_ground, _box), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new ForceSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new VelocitySystem(), GameLoopType.Update);

            _entityWorld.SystemManager.SetSystem(new SpriteDrawingSystem(_spriteBatch), GameLoopType.Draw);
        }

        private void LoadContent(ContentManager content) {
            _groundTexture = content.Load<Texture2D>("Textures/ground");
            _boxTexture = content.Load<Texture2D>("Textures/box");
            _coalTexture = content.Load<Texture2D>("Textures/coal");
        }

        public void Update(GameTime gameTime) {
            _entityWorld.Update(gameTime.ElapsedGameTime.Ticks);
        }

        public void Draw(GameTime gameTime) {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            Vector2 center = _game.GraphicsDevice.Viewport.Bounds.Center.ToVector2();

            _spriteBatch.Draw(_groundTexture, center + new Vector2(0f, 36f), origin: _groundTexture.Bounds.Center.ToVector2());

            _entityWorld.Draw();

            _spriteBatch.Draw(_boxTexture, new Vector2(center.X, 154f), origin: _boxTexture.Bounds.Center.ToVector2());

            _spriteBatch.End();
        }
    }
}
