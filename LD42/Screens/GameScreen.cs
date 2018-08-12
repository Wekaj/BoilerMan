using Artemis;
using Artemis.Manager;
using LD42.Ecs.Components;
using LD42.Ecs.Systems;
using LD42.Graphics;
using LD42.Items;
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
        private readonly SimpleTool _musicBox, _bellows, _skylight;

        private readonly SpriteBatch _spriteBatch;

        private Texture2D _groundTexture, _gateTexture, _boxTexture, _coalTexture,
            _handOpenTexture, _handGrabTexture, _pixelTexture, _blueSeedTexture,
            _blueSaplingTexture, _bluePlantTexture, _minionTexture, _coalLargeTexture,
            _armTexture, _jointTexture, _madPlantTexture, _redSeedTexture,
            _redSaplingTexture, _redPlantTexture, _greenSeedTexture, _greenSaplingTexture,
            _greenPlantTexture;

        private Entity _hand, _object;

        private float _coalTimer = 0f, _coalPeriod = 3f;

        private float _flamePower = 0f, _flameShift = 0f;

        private const float _furnaceAnimationDuration = 0.075f;
        private float _furnaceAnimation = 0f;

        private int _incomingMinions;
        private float _minionTimer;

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
            _musicBox = new SimpleTool();
            _bellows = new SimpleTool();
            _skylight = new SimpleTool();

            _spriteBatch = new SpriteBatch(game.GraphicsDevice);

            LoadContent(game.Content);

            CreateSystems();

            CreateHand(_box.Center.ToVector2() + new Vector2(192f, 32f), _box.Center.ToVector2());
            CreateHand(_box.Center.ToVector2() + new Vector2(-192f, 32f), _box.Center.ToVector2());
            CreateHand(_box.Center.ToVector2() + new Vector2(128f, 128f), _box.Center.ToVector2());
            CreateHand(_box.Center.ToVector2() + new Vector2(-128f, 128f), _box.Center.ToVector2());

            Entity tool = _entityWorld.CreateEntity();
            tool.AddComponent(new PositionComponent(new Vector2(16f, 64f)));
            tool.AddComponent(new ToolComponent(_furnace, 16f, t => {
                float p = (t % 1f) / 1f;
                return new Vector2((float)Math.Cos(p * MathHelper.TwoPi) * 8f, (float)Math.Sin(p * MathHelper.TwoPi) * 32f);
            }));

            Entity seedBox = _entityWorld.CreateEntity();
            seedBox.AddComponent(new PositionComponent(new Vector2(_game.GraphicsDevice.Viewport.Width - 16f, _game.GraphicsDevice.Viewport.Height - 16f)));
            seedBox.AddComponent(new ObjectComponent(Item.None, 16f) {
                SpawnerType = Item.SoulSeed
            });

            Entity redSeedBox = _entityWorld.CreateEntity();
            redSeedBox.AddComponent(new PositionComponent(new Vector2(_game.GraphicsDevice.Viewport.Width - 16f, _game.GraphicsDevice.Viewport.Height - 48f)));
            redSeedBox.AddComponent(new ObjectComponent(Item.None, 16f) {
                SpawnerType = Item.RedSeed
            });

            Entity greenSeedBox = _entityWorld.CreateEntity();
            greenSeedBox.AddComponent(new PositionComponent(new Vector2(_game.GraphicsDevice.Viewport.Width - 16f, _game.GraphicsDevice.Viewport.Height - 80f)));
            greenSeedBox.AddComponent(new ObjectComponent(Item.None, 16f) {
                SpawnerType = Item.GreenSeed
            });

            Entity musicBox = _entityWorld.CreateEntity();
            musicBox.AddComponent(new PositionComponent(new Vector2(16f, _game.GraphicsDevice.Viewport.Height - 32f)));
            musicBox.AddComponent(new ToolComponent(_musicBox, 16f, t => {
                float p = (t % 2f) / 2f;
                return new Vector2((float)Math.Cos(p * MathHelper.TwoPi) * 16f, (float)Math.Sin(p * MathHelper.TwoPi) * 16f);
            }));

            Entity bellows = _entityWorld.CreateEntity();
            bellows.AddComponent(new PositionComponent(new Vector2(_game.GraphicsDevice.Viewport.Width - 16f, 64f)));
            bellows.AddComponent(new ToolComponent(_bellows, 16f, t => {
                float p = (t % 1f) / 1f;
                return new Vector2((float)Math.Cos(p * MathHelper.TwoPi) * 8f, (float)Math.Sin(p * MathHelper.TwoPi) * 32f);
            }));

            Entity skylight = _entityWorld.CreateEntity();
            skylight.AddComponent(new PositionComponent(new Vector2(_game.GraphicsDevice.Viewport.Width - 16f, 192f)));
            skylight.AddComponent(new ToolComponent(_skylight, 16f, t => {
                return Vector2.Zero;
            }));
        }

        private void CreateSystems() {
            _entityWorld.SystemManager.SetSystem(new ToolUpdatingSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new HoldingSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new MinionSystem(_furnace, _musicBox), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new ObjectCollisionSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new ObjectBoundariesSystem(_ground, _box), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new ObjectGravitySystem(_furnace), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new ForceSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new VelocitySystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new HandTargetSystem(), GameLoopType.Update);
            _entityWorld.SystemManager.SetSystem(new AnimationSystem(), GameLoopType.Update);

            _entityWorld.SystemManager.SetSystem(new HandRotationSystem(_box.Center.ToVector2()), GameLoopType.Draw);
            _entityWorld.SystemManager.SetSystem(new ObjectSortingSystem(), GameLoopType.Draw);
            _entityWorld.SystemManager.SetSystem(new SpriteDrawingSystem(_spriteBatch), GameLoopType.Draw);
            _entityWorld.SystemManager.SetSystem(new ArmDrawingSystem(_spriteBatch, _armTexture, _jointTexture), GameLoopType.Draw);
        }

        private void LoadContent(ContentManager content) {
            _groundTexture = content.Load<Texture2D>("Textures/ground");
            _gateTexture = content.Load<Texture2D>("Textures/gate");
            _boxTexture = content.Load<Texture2D>("Textures/box");
            _coalTexture = content.Load<Texture2D>("Textures/coal");
            _handOpenTexture = content.Load<Texture2D>("Textures/hand_open");
            _handGrabTexture = content.Load<Texture2D>("Textures/hand_grab");
            _pixelTexture = content.Load<Texture2D>("Textures/pixel");
            _blueSeedTexture = content.Load<Texture2D>("Textures/blue_seed");
            _blueSaplingTexture = content.Load<Texture2D>("Textures/blue_sapling");
            _bluePlantTexture = content.Load<Texture2D>("Textures/blue_plant");
            _redSeedTexture = content.Load<Texture2D>("Textures/red_seed");
            _redSaplingTexture = content.Load<Texture2D>("Textures/red_sapling");
            _redPlantTexture = content.Load<Texture2D>("Textures/red_plant");
            _greenSeedTexture = content.Load<Texture2D>("Textures/green_seed");
            _greenSaplingTexture = content.Load<Texture2D>("Textures/green_sapling");
            _greenPlantTexture = content.Load<Texture2D>("Textures/green_plant");
            _minionTexture = content.Load<Texture2D>("Textures/minion");
            _coalLargeTexture = content.Load<Texture2D>("Textures/coal_large");
            _armTexture = content.Load<Texture2D>("Textures/arm");
            _jointTexture = content.Load<Texture2D>("Textures/joint");
            _madPlantTexture = content.Load<Texture2D>("Textures/mad_plant");
        }

        private void CreateHand(Vector2 position, Vector2 shoulder) {
            Entity hand = _entityWorld.CreateEntity();
            hand.AddComponent(new PositionComponent(position) { Depth = 50f });
            hand.AddComponent(new VelocityComponent(1000f));
            hand.AddComponent(new ForceComponent(2f));
            hand.AddComponent(new HandComponent(shoulder, position, 50f));
            hand.AddComponent(new SpriteComponent(_handOpenTexture, _handOpenTexture.Bounds.Center.ToVector2()) {
                LayerDepth = Layers.Hands
            });
        }

        private Entity Create(Item item, Vector2 position) {
            switch (item) {
                case Item.Coal: {
                    return CreateCoal(position);
                }
                case Item.SoulSeed: {
                    return CreateSoulSeed(position);
                }
                case Item.SoulSapling: {
                    return CreateSoulSapling(position);
                }
                case Item.SoulPlant: {
                    return CreateSoulPlant(position);
                }
                case Item.Minion: {
                    return CreateMinion(position);
                }
                case Item.MadPlant:  {
                    return CreateMadPlant(position);
                }
                case Item.RedSeed: {
                    return CreateRedSeed(position);
                }
                case Item.RedSapling: {
                    return CreateRedSapling(position);
                }
                case Item.RedPlant: {
                    return CreateRedPlant(position);
                }
                case Item.GreenSeed: {
                    return CreateGreenSeed(position);
                }
                case Item.GreenSapling: {
                    return CreateGreenSapling(position);
                }
                case Item.GreenPlant: {
                    return CreateGreenPlant(position);
                }
                default: {
                    return null;
                }
            }
        }

        private Entity CreateItem(Vector2 position, Item type, float radius, Texture2D texture, float rotation = 0f) {
            Entity item = _entityWorld.CreateEntity();
            item.AddComponent(new PositionComponent(position));
            item.AddComponent(new VelocityComponent(1000f) { MaxSpeed = 300f });
            item.AddComponent(new ForceComponent(1f));
            item.AddComponent(new ObjectComponent(type, radius));
            item.AddComponent(new SpriteComponent(texture, texture.Bounds.Center.ToVector2()) { Rotation = rotation });
            return item;
        }

        private Entity CreateCoal(Vector2 position) {
            int r = _random.Next(100);
            if (r < 20) {
                Entity coal = CreateItem(position, Item.Coal, 23f, _coalLargeTexture, (float)_random.NextDouble() * MathHelper.TwoPi);
                coal.GetComponent<ForceComponent>().Mass = 2f;
                return coal;
            }
            return CreateItem(position, Item.Coal, 15f, _coalTexture, (float)_random.NextDouble() * MathHelper.TwoPi);
        }

        private Entity CreateSoulSeed(Vector2 position) {
            return CreateItem(position, Item.SoulSeed, 4f, _blueSeedTexture);
        }

        private Entity CreateSoulSapling(Vector2 position) {
            Entity sapling = CreateItem(position, Item.SoulSapling, 12f, _blueSaplingTexture);
            sapling.AddComponent(new AnimationComponent());
            sapling.GetComponent<AnimationComponent>().Play(new Animation(32, 32).AddFrame(0, 0).AddFrame(1, 0).AddFrame(2, 0).AddFrame(3, 0), 0.25f);

            sapling.GetComponent<SpriteComponent>().SourceRectangle = new Rectangle(0, 0, 32, 32);
            sapling.GetComponent<SpriteComponent>().Origin = new Vector2(16f);

            sapling.GetComponent<ObjectComponent>().TransformType = Item.SoulPlant;
            sapling.GetComponent<ObjectComponent>().TransformTimer = 13f;
            return sapling;
        }

        private Entity CreateSoulPlant(Vector2 position) {
            Entity plant = CreateItem(position, Item.SoulPlant, 20f, _bluePlantTexture);
            plant.AddComponent(new AnimationComponent());
            plant.GetComponent<AnimationComponent>().Play(new Animation(48, 48).AddFrame(0, 0).AddFrame(1, 0).AddFrame(2, 0), 0.2f);

            plant.GetComponent<SpriteComponent>().SourceRectangle = new Rectangle(0, 0, 48, 48);
            plant.GetComponent<SpriteComponent>().Origin = new Vector2(24f);
            return plant;
        }

        private Entity CreateRedSeed(Vector2 position) {
            return CreateItem(position, Item.RedSeed, 4f, _redSeedTexture);
        }

        private Entity CreateRedSapling(Vector2 position) {
            Entity sapling = CreateItem(position, Item.RedSapling, 12f, _redSaplingTexture);
            sapling.AddComponent(new AnimationComponent());
            sapling.GetComponent<AnimationComponent>().Play(new Animation(32, 32).AddFrame(0, 0).AddFrame(1, 0).AddFrame(2, 0).AddFrame(3, 0), 0.25f);

            sapling.GetComponent<SpriteComponent>().SourceRectangle = new Rectangle(0, 0, 32, 32);
            sapling.GetComponent<SpriteComponent>().Origin = new Vector2(16f);

            sapling.GetComponent<ObjectComponent>().TransformType = Item.RedPlant;
            sapling.GetComponent<ObjectComponent>().TransformTimer = 21f;
            return sapling;
        }

        private Entity CreateRedPlant(Vector2 position) {
            Entity plant = CreateItem(position, Item.RedPlant, 20f, _redPlantTexture);
            plant.AddComponent(new AnimationComponent());
            plant.GetComponent<AnimationComponent>().Play(new Animation(48, 48).AddFrame(0, 0).AddFrame(1, 0).AddFrame(2, 0), 0.2f);

            plant.GetComponent<SpriteComponent>().SourceRectangle = new Rectangle(0, 0, 48, 48);
            plant.GetComponent<SpriteComponent>().Origin = new Vector2(24f);
            return plant;
        }

        private Entity CreateGreenSeed(Vector2 position) {
            return CreateItem(position, Item.GreenSeed, 4f, _greenSeedTexture);
        }

        private Entity CreateGreenSapling(Vector2 position) {
            Entity sapling = CreateItem(position, Item.GreenSapling, 12f, _greenSaplingTexture);
            sapling.AddComponent(new AnimationComponent());
            sapling.GetComponent<AnimationComponent>().Play(new Animation(32, 32).AddFrame(0, 0).AddFrame(1, 0).AddFrame(2, 0).AddFrame(3, 0), 0.25f);

            sapling.GetComponent<SpriteComponent>().SourceRectangle = new Rectangle(0, 0, 32, 32);
            sapling.GetComponent<SpriteComponent>().Origin = new Vector2(16f);

            sapling.GetComponent<ObjectComponent>().TransformType = Item.GreenPlant;
            sapling.GetComponent<ObjectComponent>().TransformTimer = 26f;
            return sapling;
        }

        private Entity CreateGreenPlant(Vector2 position) {
            Entity plant = CreateItem(position, Item.GreenPlant, 20f, _greenPlantTexture);
            plant.AddComponent(new AnimationComponent());
            plant.GetComponent<AnimationComponent>().Play(new Animation(48, 48).AddFrame(0, 0).AddFrame(1, 0).AddFrame(2, 0), 0.2f);

            plant.GetComponent<SpriteComponent>().SourceRectangle = new Rectangle(0, 0, 48, 48);
            plant.GetComponent<SpriteComponent>().Origin = new Vector2(24f);
            return plant;
        }

        private Entity CreateMinion(Vector2 position) {
            Entity minion = CreateItem(position, Item.Minion, 13f, _minionTexture);
            minion.AddComponent(new MinionComponent());

            minion.GetComponent<ObjectComponent>().IsSolid = false;

            minion.GetComponent<SpriteComponent>().SourceRectangle = new Rectangle(0, 0, 32, 32);
            minion.GetComponent<SpriteComponent>().Origin = new Vector2(16f);

            minion.AddComponent(new AnimationComponent());
            minion.GetComponent<AnimationComponent>().Play(new Animation(32, 32).AddFrame(0, 0).AddFrame(1, 0).AddFrame(2, 0), 0.2f, true);
            return minion;
        }

        private Entity CreateMadPlant(Vector2 position) {
            Entity plant = CreateItem(position, Item.MadPlant, 17f, _madPlantTexture);
            plant.AddComponent(new AnimationComponent());
            plant.GetComponent<AnimationComponent>().Play(new Animation(48, 48).AddFrame(0, 0).AddFrame(1, 0).AddFrame(2, 0), 0.2f);

            plant.GetComponent<SpriteComponent>().SourceRectangle = new Rectangle(0, 0, 48, 48);
            plant.GetComponent<SpriteComponent>().Origin = new Vector2(24f);

            plant.GetComponent<ObjectComponent>().SpreadType = Item.MadPlant;
            plant.GetComponent<ObjectComponent>().SpreadTimer = 3f;
            return plant;
        }

        private void GrabItem(Entity hand, Entity item) {
            ObjectComponent objectComponent = item.GetComponent<ObjectComponent>();
            PositionComponent positionComponent = item.GetComponent<PositionComponent>();

            if (objectComponent.SpawnerType != Item.None) {
                Entity newItem = Create(objectComponent.SpawnerType, positionComponent.Position);

                GrabItem(hand, newItem);
            }
            else {
                _hand = hand;
                _object = item;

                objectComponent.IsHeld = true;

                HandComponent handComponent = hand.GetComponent<HandComponent>();
                handComponent.HeldItem = _object;

                SpriteComponent spriteComponent = hand.GetComponent<SpriteComponent>();
                spriteComponent.Texture = _handGrabTexture;
            }
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
            ToolComponent toolComponent = tool.GetComponent<ToolComponent>();
            PositionComponent toolPositionComponent = tool.GetComponent<PositionComponent>();
            
            toolComponent.HoldingHand = hand;
                
            HandComponent handComponent = hand.GetComponent<HandComponent>();
            handComponent.TargetPosition = toolPositionComponent.Position;
            handComponent.TargetDepth = toolPositionComponent.Depth;
            handComponent.HeldTool = tool;

            SpriteComponent spriteComponent = hand.GetComponent<SpriteComponent>();
            spriteComponent.Texture = _handGrabTexture;
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

        private Vector2 GetRandomEntrance() {
            return new Vector2(_ground.Left + (float)_random.NextDouble() * _ground.Width, _ground.Bottom + 8f + (float)_random.NextDouble() * 16f);
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
                        PositionComponent positionComponent = obj.GetComponent<PositionComponent>();
                        if (positionComponent.Depth < 0f) {
                            continue;
                        }

                        float distance = (positionComponent.Position - mousePosition).LengthSquared();
                        if (distance < closestDistance) {
                            closestDistance = distance;
                            closestObject = obj;
                        }
                    }

                    closestDistance = (float)Math.Sqrt(closestDistance);
                    _object = closestObject;

                    if (_object.HasComponent<ObjectComponent>()) {
                        ObjectComponent objectComponent = _object.GetComponent<ObjectComponent>();
                        if (closestObject != null && closestDistance < objectComponent.Radius + 10f) {
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

                        if (closestObject != null && closestDistance < toolComponent.Radius + 10f) {
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
                            PositionComponent positionComponent = _object.GetComponent<PositionComponent>();
                            if (positionComponent != null) {
                                handComponent.TargetPosition = positionComponent.Position;
                                handComponent.TargetDepth = 1f;

                                if (handPositionComponent.Depth <= handComponent.TargetDepth
                                    && (handPositionComponent.Position - handComponent.TargetPosition).Length() < 10f) {
                                    GrabItem(_hand, _object);
                                }
                            }
                        }

                        if (handComponent.HeldItem != null) {
                            handComponent.TargetPosition = mousePosition;
                            handComponent.TargetDepth = 50f;
                        }
                    }
                }
            }
            else {
                DropItem(_hand);

                _hand = null;
                _object = null;
            }

            if (_coalPeriod > 0.65f) {
                _coalPeriod -= (float)gameTime.ElapsedGameTime.TotalSeconds * 0.02f;
                _coalPeriod = Math.Max(_coalPeriod, 0.65f);
            }

            _coalTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (_coalTimer > _coalPeriod) {
                _coalTimer -= _coalPeriod;

                CreateCoal(GetRandomEntrance());
            }

            if (_minionTimer > 0f) {
                _minionTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (_incomingMinions > 0) {
                Vector2 position = new Vector2(_ground.Left - 32f, _furnace.Region.Bottom + 4f + (float)_random.NextDouble() * 64f);
                if (_random.Next(2) == 0) {
                    position.X = _ground.Right + 32f;
                }

                Entity minion = CreateMinion(position);

                _incomingMinions--;
                _minionTimer = 0.1f + (float)_random.NextDouble() * 0.4f;
            }

            foreach (Entity entity in _entityWorld.EntityManager.GetEntities(Aspect.All(typeof(ObjectComponent)))) {
                PositionComponent positionComponent = entity.GetComponent<PositionComponent>();

                if (positionComponent.Depth < -50f) {
                    ObjectComponent objectComponent = entity.GetComponent<ObjectComponent>();

                    entity.Delete();

                    switch (objectComponent.Type) {
                        case Item.Coal: {
                            _flameShift += 0.11f;
                            break;
                        }
                        case Item.SoulPlant: {
                            _incomingMinions++;
                            _minionTimer = 0.1f + (float)_random.NextDouble() * 0.4f;
                            break;
                        }
                    }
                }
                else {
                    ObjectComponent objectComponent = entity.GetComponent<ObjectComponent>();

                    if (objectComponent.TransformType != Item.None && !objectComponent.IsHeld) {
                        bool growing = false;

                        if (objectComponent.TransformType == Item.SoulPlant || objectComponent.TransformType == Item.GreenPlant) {
                            growing = _skylight.IsActive;
                        }

                        if (objectComponent.TransformType == Item.RedPlant) {
                            growing = _musicBox.IsActive;
                        }

                        if (growing) {
                            objectComponent.TransformTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }

                        if (objectComponent.TransformTimer <= 0f) {
                            entity.Delete();

                            Item item = objectComponent.TransformType;
                            if (objectComponent.Type == Item.SoulSapling) {
                                int count = 0;
                                foreach (Entity other in _entityWorld.EntityManager.GetEntities(Aspect.All(typeof(ObjectComponent), typeof(PositionComponent)))) {
                                    Vector2 position = other.GetComponent<PositionComponent>().Position;
                                    if ((position - positionComponent.Position).LengthSquared() < 64f * 64f) {
                                        count++;
                                    }
                                }

                                if (count > 10) {
                                    item = Item.MadPlant;
                                }
                                else if (count > 5) {
                                    if (_random.Next(3) == 0) {
                                        item = Item.MadPlant;
                                    }
                                } 
                                else {
                                    if (_random.Next(7) == 0) {
                                        item = Item.MadPlant;
                                    }
                                }
                            }
                            Create(item, positionComponent.Position);
                        }
                    }

                    if (objectComponent.SpreadType != Item.None && !objectComponent.IsHeld) {
                        objectComponent.SpreadTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (objectComponent.SpreadTimer <= 0f) {
                            objectComponent.SpreadTimer = 3f;
                            Create(objectComponent.SpreadType, positionComponent.Position + new Vector2((float)_random.NextDouble(), (float)_random.NextDouble()) * 10f);
                        }
                    }

                    if (objectComponent.Type == Item.SoulSeed) {
                        if (!objectComponent.IsHeld && positionComponent.Depth == 0f) {
                            entity.Delete();
                            CreateSoulSapling(positionComponent.Position);
                        }
                    }
                    else if (objectComponent.Type == Item.RedSeed) {
                        if (!objectComponent.IsHeld && positionComponent.Depth == 0f) {
                            entity.Delete();
                            CreateRedSapling(positionComponent.Position);
                        }
                    }
                    else if (objectComponent.Type == Item.GreenSeed) {
                        if (!objectComponent.IsHeld && positionComponent.Depth == 0f) {
                            entity.Delete();
                            CreateGreenSapling(positionComponent.Position);
                        }
                    }
                }
            }

            float shift = 0.4f;
            if (_bellows.IsActive) {
                shift = 0.2f;
            }
            _flameShift -= (float)gameTime.ElapsedGameTime.TotalSeconds * shift;
            _flamePower += _flameShift * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_flamePower < 0f) {
                _flamePower = 0f;
                _flameShift = 0f;
            }

            if (_furnace.IsOpen) {
                _furnaceAnimation += (float)gameTime.ElapsedGameTime.TotalSeconds;
                _furnaceAnimation = Math.Min(_furnaceAnimation, _furnaceAnimationDuration);
            }
            else {
                _furnaceAnimation -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _furnaceAnimation = Math.Max(_furnaceAnimation, 0f);
            }
        }

        public void Draw(GameTime gameTime) {
            _spriteBatch.Begin(sortMode: SpriteSortMode.FrontToBack, samplerState: SamplerState.PointClamp);

            Vector2 center = _game.GraphicsDevice.Viewport.Bounds.Center.ToVector2();
            
            if (_furnace.IsOpen) {
                int i = 0;
            }

            Rectangle sourceRectangle = new Rectangle(0, 0, _gateTexture.Width / 4, _gateTexture.Height);
            sourceRectangle.Offset(_gateTexture.Width / 4f * (int)(3.99f * _furnaceAnimation / _furnaceAnimationDuration), 0f);

            _spriteBatch.Draw(_gateTexture, _ground.Location.ToVector2(), null, sourceRectangle, layerDepth: Layers.Ground);
            _spriteBatch.Draw(_gateTexture, _ground.Location.ToVector2() + new Vector2(_ground.Width - _gateTexture.Width / 4f, 0f), null, sourceRectangle, layerDepth: Layers.Ground);

            _spriteBatch.Draw(_groundTexture, center + new Vector2(0f, 36f), origin: _groundTexture.Bounds.Center.ToVector2(), layerDepth: Layers.Ground);

            _entityWorld.Draw();

            _spriteBatch.Draw(_boxTexture, new Vector2(center.X, 154f), origin: _boxTexture.Bounds.Center.ToVector2(), layerDepth: Layers.AboveGround);

            _spriteBatch.Draw(_pixelTexture, new Vector2(128f, 16f), color: Color.OrangeRed, scale: new Vector2(128f * _flamePower / 100f, 16f));

            _spriteBatch.End();
        }
    }
}
