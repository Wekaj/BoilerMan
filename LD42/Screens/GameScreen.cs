using Artemis;
using Artemis.Manager;
using LD42.Ecs.Components;
using LD42.Ecs.Systems;
using LD42.Graphics;
using LD42.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace LD42.Screens {
    public sealed class GameScreen : IScreen {
        private static readonly Random _random = new Random();

        private readonly LD42Game _game;

        private readonly EntityWorld _entityWorld;
        private readonly Rectangle _ground, _box;
        private readonly Furnace _furnace;

        private readonly SpriteBatch _spriteBatch;

        private Texture2D _groundTexture, _boxTexture, _coalTexture,
            _handOpenTexture, _handGrabTexture;

        private Entity _hand, _object;

        private float _coalTimer = 0f, _coalPeriod = 1f;

        public GameScreen(LD42Game game) {
            _game = game;

            _entityWorld = new EntityWorld();

            _ground = new Rectangle(0, 0, 448, 400);
            _ground.Offset((_game.GraphicsDevice.Viewport.Width - _ground.Width) / 2f, 
                (_game.GraphicsDevice.Viewport.Height - _ground.Height) / 2f + 36f);

            _box = new Rectangle(0, 0, 96, 160);
            _box.Offset((_game.GraphicsDevice.Viewport.Width - _box.Width) / 2f, 
                154f - _box.Height / 2f);
            
            _furnace = new Furnace(new Rectangle(_ground.Left, _ground.Top, 448, 128));

            _spriteBatch = new SpriteBatch(game.GraphicsDevice);

            CreateSystems();

            LoadContent(game.Content);

            CreateHand(_box.Center.ToVector2() + new Vector2(192f, 32f));
            CreateHand(_box.Center.ToVector2() + new Vector2(-192f, 32f));
            CreateHand(_box.Center.ToVector2() + new Vector2(128f, 128f));
            CreateHand(_box.Center.ToVector2() + new Vector2(-128f, 128f));

            Entity tool = _entityWorld.CreateEntity();
            tool.AddComponent(new PositionComponent(new Vector2(16f, 64f)));
            tool.AddComponent(new ToolComponent(16f, t => {
                float p = (t % 1f) / 1f;
                return new Vector2((float)Math.Cos(p * MathHelper.TwoPi) * 8f, (float)Math.Sin(p * MathHelper.TwoPi) * 32f);
            }));
        }

        private void CreateSystems() {
            _entityWorld.SystemManager.SetSystem(new HoldingSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new ObjectCollisionSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new ObjectBoundariesSystem(_ground, _box), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new ObjectGravitySystem(_furnace), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new ForceSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new VelocitySystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new HandTargetSystem(), GameLoopType.Update);

            _entityWorld.SystemManager.SetSystem(new HandRotationSystem(_box.Center.ToVector2()), GameLoopType.Draw);
            _entityWorld.SystemManager.SetSystem(new ObjectSortingSystem(), GameLoopType.Draw);
            _entityWorld.SystemManager.SetSystem(new SpriteDrawingSystem(_spriteBatch), GameLoopType.Draw);
        }

        private void LoadContent(ContentManager content) {
            _groundTexture = content.Load<Texture2D>("Textures/ground");
            _boxTexture = content.Load<Texture2D>("Textures/box");
            _coalTexture = content.Load<Texture2D>("Textures/coal");
            _handOpenTexture = content.Load<Texture2D>("Textures/hand_open");
            _handGrabTexture = content.Load<Texture2D>("Textures/hand_grab");
        }

        private void CreateHand(Vector2 position) {
            Entity hand = _entityWorld.CreateEntity();
            hand.AddComponent(new PositionComponent(position) { Depth = 50f });
            hand.AddComponent(new VelocityComponent(1000f));
            hand.AddComponent(new ForceComponent(2f));
            hand.AddComponent(new HandComponent(position));
            hand.AddComponent(new SpriteComponent(_handOpenTexture, _handOpenTexture.Bounds.Center.ToVector2()) {
                LayerDepth = Layers.Hands
            });
        }

        private void CreateCoal(Vector2 position) {
            Entity coal = _entityWorld.CreateEntity();
            coal.AddComponent(new PositionComponent(position));
            coal.AddComponent(new VelocityComponent(1000f));
            coal.AddComponent(new ForceComponent(1f));
            coal.AddComponent(new ObjectComponent(15f));
            coal.AddComponent(new SpriteComponent(_coalTexture, _coalTexture.Bounds.Center.ToVector2()) {
                Rotation = (float)_random.NextDouble() * MathHelper.TwoPi
            });
        }

        private void DropItem(Entity hand) {
            if (hand == null) {
                return;
            }

            HandComponent handComponent = hand.GetComponent<HandComponent>();

            if (handComponent.HeldItem != null) {
                ObjectComponent objectComponent = handComponent.HeldItem.GetComponent<ObjectComponent>();
                objectComponent.IsHeld = false;

                handComponent.HeldItem = null;

                SpriteComponent spriteComponent = hand.GetComponent<SpriteComponent>();
                spriteComponent.Texture = _handOpenTexture;
            }
        }

        private void GrabTool(Entity hand, Entity tool) {
            PositionComponent toolPositionComponent = tool.GetComponent<PositionComponent>();

            HandComponent handComponent = hand.GetComponent<HandComponent>();
            handComponent.TargetPosition = toolPositionComponent.Position;
            handComponent.HeldTool = tool;
            
            SpriteComponent spriteComponent = hand.GetComponent<SpriteComponent>();
            spriteComponent.Texture = _handGrabTexture;

            ToolComponent toolComponent = tool.GetComponent<ToolComponent>();
            toolComponent.HoldingHand = hand;
        }

        private void ReleaseTool(Entity tool) {
            ToolComponent toolComponent = tool.GetComponent<ToolComponent>();

            HandComponent handComponent = toolComponent.HoldingHand.GetComponent<HandComponent>();
            handComponent.HeldTool = null;

            SpriteComponent spriteComponent = toolComponent.HoldingHand.GetComponent<SpriteComponent>();
            spriteComponent.Texture = _handOpenTexture;

            _hand = toolComponent.HoldingHand;
            _object = null;
            toolComponent.HoldingHand = null;
        }

        public void Update(GameTime gameTime) {
            _entityWorld.Update(gameTime.ElapsedGameTime.Ticks);

            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed) {
                Vector2 mousePosition = mouseState.Position.ToVector2();

                if (_hand == null) {
                    float closestDistance = float.MaxValue;
                    Entity closestObject = null;
                    foreach (Entity obj in _entityWorld.EntityManager.GetEntities(Aspect.One(typeof(ObjectComponent), typeof(ToolComponent)))) {
                        float distance = (obj.GetComponent<PositionComponent>().Position - mousePosition).LengthSquared();
                        if (distance < closestDistance) {
                            closestDistance = distance;
                            closestObject = obj;
                        }
                    }

                    closestDistance = (float)Math.Sqrt(closestDistance);
                    _object = closestObject;

                    if (_object.HasComponent<ObjectComponent>()) {
                        ObjectComponent objectComponent = _object.GetComponent<ObjectComponent>();
                        if (closestObject != null && closestDistance < objectComponent.Radius + 4f) {
                            PositionComponent positionComponent = _object.GetComponent<PositionComponent>();

                            closestDistance = float.MaxValue;
                            Entity closestHand = null;
                            foreach (Entity hand in _entityWorld.EntityManager.GetEntities(Aspect.All(typeof(HandComponent)))) {
                                if (hand.GetComponent<HandComponent>().HeldTool != null) {
                                    continue;
                                }

                                float distance = (hand.GetComponent<PositionComponent>().Position - positionComponent.Position).LengthSquared();
                                if (distance < closestDistance) {
                                    closestDistance = distance;
                                    closestHand = hand;
                                }
                            }

                            _hand = closestHand;
                        }
                    }
                    else {
                        // Attach hand to tool. 
                        ToolComponent toolComponent = _object.GetComponent<ToolComponent>();

                        if (closestObject != null && closestDistance < toolComponent.Radius + 4f) {
                            if (toolComponent.HoldingHand != null) {
                                ReleaseTool(_object);
                            }
                            else {
                                PositionComponent positionComponent = _object.GetComponent<PositionComponent>();

                                closestDistance = float.MaxValue;
                                Entity closestHand = null;
                                foreach (Entity hand in _entityWorld.EntityManager.GetEntities(Aspect.All(typeof(HandComponent)))) {
                                    if (hand.GetComponent<HandComponent>().HeldTool != null) {
                                        continue;
                                    }

                                    float distance = (hand.GetComponent<PositionComponent>().Position - positionComponent.Position).LengthSquared();
                                    if (distance < closestDistance) {
                                        closestDistance = distance;
                                        closestHand = hand;
                                    }
                                }

                                _hand = closestHand;
                                if (closestHand != null) {
                                    GrabTool(_hand, closestObject);
                                }
                            }
                        }
                    }
                }

                if (_hand != null) {
                    HandComponent handComponent = _hand.GetComponent<HandComponent>();

                    if (handComponent.HeldTool == null) {
                        PositionComponent handPositionComponent = _hand.GetComponent<PositionComponent>();

                        if (handComponent.HeldItem == null && _object != null) {
                            handComponent.TargetPosition = _object.GetComponent<PositionComponent>().Position;

                            if (handPositionComponent.Depth > 1f) {
                                handPositionComponent.Depth -= 200f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                handPositionComponent.Depth = Math.Max(handPositionComponent.Depth, 1f);
                            }
                            else if ((handPositionComponent.Position - handComponent.TargetPosition).Length() < 5f) {
                                handComponent.HeldItem = _object;
                                _hand.GetComponent<SpriteComponent>().Texture = _handGrabTexture;
                                _object.GetComponent<ObjectComponent>().IsHeld = true;
                            }
                        }

                        if (handComponent.HeldItem != null) {
                            handComponent.TargetPosition = mousePosition;

                            if (handPositionComponent.Depth < 50f) {
                                handPositionComponent.Depth += 300f * (float)gameTime.ElapsedGameTime.TotalSeconds;
                                handPositionComponent.Depth = Math.Min(handPositionComponent.Depth, 50f);
                            }
                        }
                    }
                }
            }
            else {
                DropItem(_hand);

                _hand = null;
                _object = null;
            }

            _coalTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (_coalTimer > _coalPeriod) {
                _coalTimer -= _coalPeriod;

                CreateCoal(new Vector2(_ground.Left + (float)_random.NextDouble() * _ground.Width, _ground.Bottom + 8f + (float)_random.NextDouble() * 16f));
            }
        }

        public void Draw(GameTime gameTime) {
            _spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, samplerState: SamplerState.PointClamp);

            Vector2 center = _game.GraphicsDevice.Viewport.Bounds.Center.ToVector2();

            _spriteBatch.Draw(_groundTexture, center + new Vector2(0f, 36f), origin: _groundTexture.Bounds.Center.ToVector2(), layerDepth: Layers.Ground);

            _entityWorld.Draw();

            _spriteBatch.Draw(_boxTexture, new Vector2(center.X, 154f), origin: _boxTexture.Bounds.Center.ToVector2(), layerDepth: Layers.AboveGround);

            _spriteBatch.End();
        }
    }
}
