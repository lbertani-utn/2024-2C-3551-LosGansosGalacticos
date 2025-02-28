using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Scenes.Battlefield;
using TGC.MonoGame.TP.Scenes.Entities;
using TGC.MonoGame.TP.UI;

namespace TGC.MonoGame.TP.Scenes
{
    class BattlefieldScene : Scene
    {
        // shaders
        private SkyBox SkyEffect;
        private Effect ObjectEffect;

        // cámara
        private const float camX = 0.2f;
        private const float camY = -0.1f;

        // TODO crear clase para tanque jugador
        private Model Model { get; set; }
        private Tank player;
        private Tank[] tanks;
        private Bullet[] Bullets;
        private const int bulletCount = 20;
        private const int enemyCount = 5;

        // terreno
        private SimpleTerrain terrain;
        private float terrainSize;
        private float invisibleWall;
        Random rnd;

        public BattlefieldScene(GraphicsDeviceManager graphics, ContentManager content, GameOptions options) : base(graphics, content, options)
        {
        }

        public override void Initialize()
        {
            StaticObjects = new List<WorldEntity>();
            DynamicObjects = new List<WorldEntity>();
            tanks = new Tank[enemyCount + 1];
            rnd = new Random();

            // cámara principal - detrás del tanque
            MainCamera = new TargetCamera(graphics.GraphicsDevice.Viewport.AspectRatio, Vector3.One * 100f, Vector3.Zero);
            float CameraNearPlaneDistance = 1f;
            float CameraFarPlaneDistance = 2000f;
            MainCamera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, graphics.GraphicsDevice.Viewport.AspectRatio, CameraNearPlaneDistance, CameraFarPlaneDistance);
            camera = MainCamera;

            // bounding frustum de la cámara que sigue al tanque
            Frustum = new BoundingFrustum(MainCamera.View * MainCamera.Projection);

            // cámara para debug - vista aérea
            DebugCamera = new StaticCamera(graphics.GraphicsDevice.Viewport.AspectRatio, Vector3.UnitY * 1000f, -Vector3.UnitY, Vector3.UnitZ);
            DebugCamera.RightDirection = Vector3.UnitX;
            DebugCamera.BuildView();

            // cámara en fuente de luz - sol
            LightPosition = new Vector3(-1000f, 250f, 800f); // posición de la luz para que tenga sentido con el skybox
            float LightCameraNearPlaneDistance = 600f;
            float LightCameraFarPlaneDistance = 2025f;
            LightCamera = new TargetCamera(1f, LightPosition, Vector3.Zero);
            LightCamera.BuildProjection(1f, LightCameraNearPlaneDistance, LightCameraFarPlaneDistance, MathHelper.PiOver2);
            LightCamera.BuildView();

        }




        #region Load
        public override void LoadContent()
        {
            // Create a full screen quad to post-process
            ScreenQuad = new FullScreenQuad(graphics.GraphicsDevice);
            // Create a shadow map. It stores depth from the light position
            int ShadowmapSize = 2048;
            ShadowMapRenderTarget = new RenderTarget2D(graphics.GraphicsDevice, ShadowmapSize, ShadowmapSize, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

            // ObjectEffect
            AmbientColor = Vector3.One;
            ObjectEffect = content.Load<Effect>(ContentFolder.Effects + "ObjectShader");
            LoadSceneParameters();

            // Terreno
            Texture2D terrainHeightMap = content.Load<Texture2D>(ContentFolder.Textures + "Rolling Hills Height Map/Rolling Hills Height Map 256");
            Texture2D terrainBaseColor = content.Load<Texture2D>(ContentFolder.Textures + "Grass/Grass_005_BaseColor");
            Texture2D terrainNormalMap = content.Load<Texture2D>(ContentFolder.Textures + "Grass/Grass_005_Normal");
            terrainSize = 1024f;
            invisibleWall = terrainSize / 100 * 49;

            float heightScale = 0.4f;
            float terrainScale = terrainSize / terrainHeightMap.Width;
            terrain = new SimpleTerrain(graphics.GraphicsDevice, terrainHeightMap, terrainBaseColor, terrainNormalMap, ObjectEffect, terrainScale, heightScale);

            // TODO setear position.Y, pitch y roll del tanque en la posición inicial
            Tree.LoadContent(content, ObjectEffect);
            Rock.LoadContent(content, ObjectEffect);
            Bush.LoadContent(content, ObjectEffect);

            // skyBox
            Model skyBox = content.Load<Model>(ContentFolder.Models + "geometries/skyBox");
            TextureCube skyBoxTexture = content.Load<TextureCube>(ContentFolder.Textures + "skybox/day_skybox");
            Effect skyBoxEffect = content.Load<Effect>(ContentFolder.Effects + "Skybox");
            SkyEffect = new SkyBox(skyBox, skyBoxTexture, skyBoxEffect, 1200);

            // Cargo el tanque
            // TODO mover esto a su clase
            Stopwatch sw = Stopwatch.StartNew();
            Model = content.Load<Model>(ContentFolder.Models + "tank/tank");
            sw.Stop();
            Debug.WriteLine("Load model tank/tank: {0} milliseconds", sw.ElapsedMilliseconds);


            ApplyEffect(Model, ObjectEffect);
            player = new Tank(new Vector3(0f, terrain.Height(0f, 0f), 0f), new Vector3(0.1f, 0.1f, 0.1f), MathHelper.PiOver2, 0f, 0f);
            player.Load(content, Model);
            Tank.DefaultEffect = ObjectEffect;
            tanks[0] = player;

            // proyectiles
            Bullet.LoadContent(content, ObjectEffect);
            Bullets = new Bullet[bulletCount];
            for (int i = 0; i < bulletCount; i++)
            {
                Bullets[i] = new Bullet();
            }

            // music
            BackgroundMusic = content.Load<Song>(ContentFolder.Music + "High Tension");

            LoadGizmos();
            LoadSceneObjects();
        }
        protected override void LoadInitialState()
        {
        }

        public override void LoadSceneParameters()
        {
            // ObjectEffect
            ObjectEffect.Parameters["LightViewProjection"].SetValue(LightCamera.View * LightCamera.Projection);
            ObjectEffect.Parameters["lightPosition"].SetValue(LightPosition);
            ObjectEffect.Parameters["ambientColor"].SetValue(AmbientColor);
            ObjectEffect.Parameters["diffuseColor"].SetValue(Vector3.One * 0.7f);
            ObjectEffect.Parameters["specularColor"].SetValue(Vector3.One);
            ObjectEffect.Parameters["KAmbient"].SetValue(0.3f);
            ObjectEffect.Parameters["KDiffuse"].SetValue(0.4f);
            ObjectEffect.Parameters["KSpecular"].SetValue(0.1f);
            ObjectEffect.Parameters["shininess"].SetValue(16.0f);
            ObjectEffect.Parameters["eyePosition"].SetValue(MainCamera.Position);
            ObjectEffect.Parameters["Tiling"].SetValue(Vector2.One);
        }

        protected override void LoadSceneObjects()
        {
            LoadTerrainObjects(terrainSize, 0.1f, 0.9f);
            LoadTanks(terrainSize, 0.8f, 0.9f);
        }


        private Vector3 GetRandomTerrainPosition(float terrainSize, float minRadius, float maxRadius)
        {
            float angle = (float)(rnd.NextDouble() * Math.PI * 2);
            float radius = ((float)rnd.NextDouble() * (maxRadius - minRadius) + minRadius) * terrainSize / 2;

            float x = (float)Math.Sin(angle) * radius;
            float z = (float)Math.Cos(angle) * radius;
            float y = terrain.Height(x, z);

            return new Vector3(x, y, z);
        }

        private void LoadTerrainObjects(float terrainSize, float minRadius, float maxRadius)
        {
            int treeCount = 0;
            int bushCount = 0;
            int rockCount = 0;

            for (int i = 0; i < 200; i++)
            {
                // posición
                Vector3 pos = GetRandomTerrainPosition(terrainSize, minRadius, maxRadius);

                // escala
                float height = (float)rnd.NextDouble() * 0.4f + 0.8f;
                float width = (float)rnd.NextDouble() * 0.4f + 0.8f;

                // rotación
                float rot = (float)rnd.NextDouble() * MathHelper.TwoPi;
                float objType = (float)rnd.NextDouble();

                if (objType > 0.4f)
                {
                    Tree t = new Tree(pos, new Vector3(width, height, width), rot);
                    StaticObjects.Add(t);
                    //terrain.spacialMap.Add(t);
                    treeCount += 1;
                }
                else if (objType > 0.2f)
                {
                    Bush b = new Bush(pos, new Vector3(width, height, width), rot);
                    StaticObjects.Add(b);
                    //terrain.spacialMap.Add(b);
                    bushCount += 1;
                }
                else
                {
                    Rock r = new Rock(pos, new Vector3(width, height, width), rot);
                    StaticObjects.Add(r);
                    //terrain.spacialMap.Add(r);
                    rockCount += 1;
                }

            }

            Debug.WriteLine("Trees: {0}", treeCount);
            Debug.WriteLine("Bushes: {0}", bushCount);
            Debug.WriteLine("Rocks: {0}", rockCount);
        }

        private void LoadTanks(float terrainSize, float minRadius, float maxRadius)
        {
            for (int i = 1; i <= enemyCount; i++)
            {
                // posición
                Vector3 pos = GetRandomTerrainPosition(terrainSize, minRadius, maxRadius);

                // rotación
                float rot = (float)rnd.NextDouble() * MathHelper.TwoPi;
                Tank t = new Tank(pos, new Vector3(0.01f, 0.01f, 0.01f), rot, 0f, 0f);
                t.Load(content, Model);
                tanks[i] = t;
            }
        }

        private void ApplyEffect(Model model, Effect effect)
        {
            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            foreach (var mesh in model.Meshes)
            {
                // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = effect;
                }
            }
        }
        #endregion

        #region Update
        public override void Update(float elapsedTime, UserInput input)
        {
            if (input.Escape || input.IsKeyPressed(Keys.Z))
            {
                changeScene = true;
            }

            // rozamiento
            if (player.Propulsion > 0)
            {
                player.Propulsion = MathHelper.Clamp(player.Propulsion - Tank.Friction, 0, Tank.SpeedLimit);
            }
            else if (player.Propulsion < 0)
            {
                player.Propulsion = MathHelper.Clamp(player.Propulsion + Tank.Friction, Tank.ReverseSpeedLimit, 0);
            }

            // disparo
            if (input.IsKeyPressed(Keys.Space) || input.IsLeftButtonPressed())
            {
                player.Shoot(Bullets, Bullets.Length);
            }

            // dirección rotación
            if (input.keyboardState.IsKeyDown(Keys.Right) || input.keyboardState.IsKeyDown(Keys.D))
            {
                player.Yaw -= elapsedTime / 5f ;
                player.SteerRotation -= elapsedTime;
                if (player.Propulsion < Tank.TurningSpeedLimit)
                {
                    player.Propulsion = MathHelper.Clamp(player.Propulsion + Tank.SpeedIncrease * 0.5f, Tank.ReverseSpeedLimit, Tank.TurningSpeedLimit);
                }
                else
                {
                    player.Propulsion = MathHelper.Clamp(player.Propulsion - Tank.SpeedIncrease * 0.7f, Tank.TurningSpeedLimit, Tank.SpeedLimit);
                }
            }
            else if (input.keyboardState.IsKeyDown(Keys.Left) || input.keyboardState.IsKeyDown(Keys.A))
            {
                player.Yaw += elapsedTime / 5f;
                player.SteerRotation += elapsedTime;
                if (player.Propulsion < Tank.TurningSpeedLimit)
                {
                    player.Propulsion = MathHelper.Clamp(player.Propulsion + Tank.SpeedIncrease * 0.5f, Tank.ReverseSpeedLimit, Tank.TurningSpeedLimit);
                }
                else
                {
                    player.Propulsion = MathHelper.Clamp(player.Propulsion - Tank.SpeedIncrease * 0.7f, Tank.TurningSpeedLimit, Tank.SpeedLimit);
                }
            }

            // avance/retroceso
            if (input.keyboardState.IsKeyDown(Keys.Up) || input.keyboardState.IsKeyDown(Keys.W))
            {
                player.Propulsion = MathHelper.Clamp(player.Propulsion + Tank.SpeedIncrease, Tank.ReverseSpeedLimit, Tank.SpeedLimit);
            }
            else if (input.keyboardState.IsKeyDown(Keys.Down) || input.keyboardState.IsKeyDown(Keys.S))
            {
                player.Propulsion = MathHelper.Clamp(player.Propulsion - Tank.SpeedIncrease, Tank.ReverseSpeedLimit, Tank.SpeedLimit);
            }

            if (player.Speed > 0f && player.SteerRotation != 0f)
            {
                float sign = player.SteerRotation > 0 ? 1 : -1;
                player.SteerRotation -= (elapsedTime * 0.5f * sign);
            }

            // torreta y cañon
            player.TurretRotation += input.mouseDeltaX * elapsedTime * camX;
            player.CannonRotation += input.mouseDeltaY * elapsedTime * camY;

            Matrix RotationMatrix = Matrix.CreateRotationY(player.Yaw);
            Matrix CameraRotationMatrix = Matrix.CreateFromYawPitchRoll(player.Yaw + player.TurretRotation, -player.CannonRotation, 0f);

            Vector3 movement = RotationMatrix.Forward * player.Speed * elapsedTime;
            player.WheelRotation += (player.Speed * elapsedTime / 8f); // TODO revisar esta fórmula
            player.Position = player.Position + movement;

            if (player.Position.X < -invisibleWall || player.Position.X > invisibleWall || player.Position.Z < -invisibleWall || player.Position.Z > invisibleWall)
            {
                // TODO reproducir sonido
                player.Position.X = MathHelper.Clamp(player.Position.X, -invisibleWall, invisibleWall);
                player.Position.Z = MathHelper.Clamp(player.Position.Z, -invisibleWall, invisibleWall);
            }
            player.Position.Y = terrain.Height(player.Position.X, player.Position.Z);

            float clampPitch = elapsedTime / Tank.pitchLimit;
            float clampRoll = elapsedTime / Tank.rollLimit;

            // pendiente hacia adelante/atrás 
            Vector3 positionForward = player.Position + RotationMatrix.Forward * Tank.distanceForward;
            positionForward.Y = terrain.Height(positionForward.X, positionForward.Z);
            float currentPitch = (player.Position.Y - positionForward.Y) / (player.Position - positionForward).Length();
            float deltaPitch = currentPitch - player.Pitch;
            player.Pitch += MathHelper.Clamp(deltaPitch, -clampPitch, clampPitch);

            // velocidad en pendiente
            player.Downhill = player.Propulsion * (float)Math.Sin(currentPitch);

            // pendiente hacia los costados
            Vector3 positionRight = player.Position + RotationMatrix.Right * Tank.distanceRight;
            positionRight.Y = terrain.Height(positionRight.X, positionRight.Z);
            float currentRoll = (player.Position.Y - positionRight.Y) / (player.Position - positionRight).Length();
            float deltaRoll = currentRoll - player.Roll;
            player.Roll += MathHelper.Clamp(deltaRoll, -clampRoll, clampRoll);


            player.World = Matrix.CreateScale(0.01f) * Matrix.CreateFromYawPitchRoll(player.Yaw + MathHelper.Pi, player.Pitch, player.Roll) * Matrix.CreateTranslation(player.Position); // TODO definir escala tanque
            player.Update(elapsedTime);

            MainCamera.TargetPosition = player.Position + CameraRotationMatrix.Forward * 40; // TODO revisar posición objetivo 
            MainCamera.Position = player.Position + CameraRotationMatrix.Backward * 20 + Vector3.UnitY * 12; // TODO revisar posición cámara
            MainCamera.BuildView();

            ObjectEffect.Parameters["eyePosition"].SetValue(MainCamera.Position);


            // Update the view projection matrix of the bounding frustum
            Frustum.Matrix = MainCamera.View * MainCamera.Projection;
            gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            // movimiento tanques enemigos
            for (int tank = 1; tank < tanks.Length; tank++)
            {
                if (tanks[tank].Status != WorldEntityStatus.Destroyed)
                {
                    tanks[tank].AttackPlayer(elapsedTime, player.Position, Bullets, bulletCount, terrain);
                }
            }

            // colisiones tanques
            for (int tank = 0; tank < tanks.Length; tank++)
            {
                if (tanks[tank].Status != WorldEntityStatus.Destroyed)
                {
                    /// colisiones entre tanques y objetos del escenario
                    foreach (WorldEntity e in StaticObjects)
                    {
                        if (e.Status != WorldEntityStatus.Destroyed && tanks[tank].Intersects(e.GetHitBox()))
                        {
                            // TODO destruir objeto
                            e.Status = WorldEntityStatus.Destroyed;
                        }
                    }
                    /// colisiones entre tanques y objetos del escenario
                    for (int tank2 = 0; tank2 < tanks.Length; tank2++)
                    {
                        if (tank != tank2 && tanks[tank2].Status != WorldEntityStatus.Destroyed)
                        {
                            // TODO daño
                        }
                    }
                    tanks[tank].Update(elapsedTime);
                }

                /// colisiones proyectiles
                foreach (Bullet b in Bullets)
                {
                    if (b.Active)
                    {
                        b.Update(elapsedTime, terrain, StaticObjects, tanks);
                    }
                }

            }


        }
        #endregion

        #region Draw
        public override void Draw(CameraType selectedCamera, bool debugBoundingBoxes, bool debugPositions, bool debugShadowMap)
        {
            SelectCamera(selectedCamera);


            #region Pass 1
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (!debugShadowMap)
            { 
                // Set the render target as our shadow map, we are drawing the depth into this texture
                graphics.GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
            }
            else
            {
                graphics.GraphicsDevice.SetRenderTarget(null);
            }

            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
            ObjectEffect.CurrentTechnique = ObjectEffect.Techniques["DepthPass"];

            terrain.Draw(LightCamera.View, LightCamera.Projection);
            foreach (WorldEntity e in StaticObjects)
            {
                if (e.Status != WorldEntityStatus.Destroyed)
                {
                    e.DrawDepthPass(ObjectEffect, LightCamera);
                }
            }
            
            // objetos que se mueven
            // integrados en la colección de objetos dinámicos
            foreach (WorldEntity e in DynamicObjects)
            {
                if (e.Status != WorldEntityStatus.Destroyed)
                {
                    e.DrawDepthPass(ObjectEffect, LightCamera);
                }
            }
            // todavía no integrados en la colección de objetos dinámicos
            player.Draw(LightCamera.View, LightCamera.Projection);
            foreach (Tank t in tanks)
            {
                if (t.Status != WorldEntityStatus.Destroyed)
                {
                    t.Draw(LightCamera.View, LightCamera.Projection);
                }
            }
            foreach (Bullet b in Bullets)
            {
                if (b.Active)
                {
                    b.Draw(LightCamera.View, LightCamera.Projection, ObjectEffect);
                }
            }
            #endregion

            if (debugShadowMap)
            {
                return;
            }

            #region Pass 2

            // Set the render target as null, we are drawing on the screen!
            graphics.GraphicsDevice.SetRenderTarget(null);
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);
            ObjectEffect.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);

            // skybox no se ve afectado por las sombras
            SkyEffect.Draw(Camera.View, Camera.Projection, Camera.Position);

            ObjectEffect.CurrentTechnique = ObjectEffect.Techniques["DrawNormalMap"];
            terrain.Draw(Camera.View, Camera.Projection);


            ObjectEffect.CurrentTechnique = ObjectEffect.Techniques["DrawObject"];
            player.Draw(Camera.View, Camera.Projection);


            foreach (WorldEntity e in StaticObjects)
            {
                if (e.Status != WorldEntityStatus.Destroyed && Frustum.Intersects(e.GetDrawBox()))
                {
                    e.DrawShadowed(Camera.View, Camera.Projection, ObjectEffect);
                }
            }
            foreach (WorldEntity e in DynamicObjects)
            {
                if (e.Status != WorldEntityStatus.Destroyed && Frustum.Intersects(e.GetDrawBox()))
                {
                    e.DrawShadowed(Camera.View, Camera.Projection, ObjectEffect);
                }
            }

            foreach (Tank t in tanks)
            {
                if (t.Status != WorldEntityStatus.Destroyed && t.Intersects(Frustum))
                {
                    t.Draw(Camera.View, Camera.Projection);
                }
            }
            foreach (Bullet b in Bullets)
            {
                if (b.Active)
                {
                    b.Draw(Camera.View, Camera.Projection, ObjectEffect);
                }
            }
            #endregion

            DrawGizmos(debugBoundingBoxes, debugPositions);
            DrawUI();
        }

        protected override void DrawUI()
        {
        }

        protected override void DrawGizmos(bool drawBoundingBoxes, bool drawPositions)
        {
            // gizmos
            if (drawBoundingBoxes || drawPositions)
            {
                foreach (WorldEntity e in StaticObjects)
                {
                    if (e.Status != WorldEntityStatus.Destroyed && Frustum.Intersects(e.GetDrawBox()))
                    {
                        if (drawBoundingBoxes)
                        {
                            e.DrawBoundingBox(gizmos);
                        }
                        if (drawPositions)
                        {
                            e.DrawPosition(gizmos);
                        }
                    }
                }

                if (drawBoundingBoxes)
                {
                    foreach (Bullet b in Bullets)
                    {
                        b.DrawBoundingBox(gizmos);
                    }
                    foreach (Tank t in tanks)
                    {
                        if (t.Status != WorldEntityStatus.Destroyed)
                        {
                            t.DrawBoundingBox(gizmos);
                        }
                    }
                    player.DrawBoundingBox(gizmos);
                    gizmos.DrawFrustum(MainCamera.View * MainCamera.Projection, Color.White);
                }

                gizmos.Draw();
            }
        }


        #endregion


        public override void Dispose()
        {
            ScreenQuad.Dispose();
            ShadowMapRenderTarget.Dispose();
        }

    }
}
