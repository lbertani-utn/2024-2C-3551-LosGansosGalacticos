using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Scenes.Battlefield;

namespace TGC.MonoGame.TP.Scenes
{
    class BattlefieldScene : Scene
    {
        // shaders
        private SkyBox SkyEffect;
        private Effect ShadowMapEffect;
        private Effect TerrainEffect;
        private Effect ObjectEffect;

        // cámara
        private static float CameraNearPlaneDistance = 1f;
        private static float CameraFarPlaneDistance = 2000f;
        private const float camX = 0.2f;
        private const float camY = -0.1f;

        // TODO crear clase para tanque jugador
        private Model Model { get; set; }
        private Tank tank;
        private Bullet[] Bullets;
        private const int bulletCount = 10;
        private Tank[] Enemies;
        private const int enemyCount = 5;

        // terreno
        private SimpleTerrain terrain;
        private float terrainSize;

        // shadowmap
        private const int ShadowmapSize = 2048;
        private float LightCameraFarPlaneDistance = 2000f;
        private float LightCameraNearPlaneDistance = 500f;
        private FullScreenQuad ScreenQuad;
        private RenderTarget2D ShadowMapRenderTarget;



        public BattlefieldScene(GraphicsDeviceManager graphics, ContentManager content) : base(graphics, content)
        {

        }

        public override void Initialize()
        {
            StaticObjects = new List<WorldEntity>();
            DynamicObjects = new List<WorldEntity>();

            // cámara principal - detrás del tanque
            MainCamera = new TargetCamera(graphics.GraphicsDevice.Viewport.AspectRatio, Vector3.One * 100f, Vector3.Zero);
            MainCamera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, graphics.GraphicsDevice.Viewport.AspectRatio, CameraNearPlaneDistance, CameraFarPlaneDistance);
            camera = MainCamera;

            // bounding frustum de la cámara que sigue al tanque
            Frustum = new BoundingFrustum(MainCamera.View * MainCamera.Projection);

            // cámara para debug - vista aérea
            DebugCamera = new StaticCamera(graphics.GraphicsDevice.Viewport.AspectRatio, Vector3.UnitY * 1000f, -Vector3.UnitY, Vector3.UnitZ);
            DebugCamera.RightDirection = Vector3.UnitX;
            DebugCamera.BuildView();

            // cámara en fuente de luz - sol
            LightPosition = new Vector3(-1000f, 550f, 600f); // posición de la luz para que tenga sentido con el skyboxc
            LightPosition *= 0.5f; // acerco un poco la luz para hacer debug
            LightCameraFarPlaneDistance = 1350; //Vector3.Distance(LightPosition, new Vector3(512, 0, -512));
            LightCameraNearPlaneDistance = 200; // Vector3.Distance(LightPosition, new Vector3(-512, 0, 512));
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
            ShadowMapRenderTarget = new RenderTarget2D(graphics.GraphicsDevice, ShadowmapSize, ShadowmapSize, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
            // Load the shadowmap effect
            ShadowMapEffect = content.Load<Effect>(ContentFolder.Effects + "ShadowMap");


            AmbientColor = Vector3.One;
            Vector3 DiffuseColor = Vector3.One;
            Vector3 SpecularColor = Vector3.One;

            ObjectEffect = content.Load<Effect>(ContentFolder.Effects + "BlinnPhong");
            //ObjectEffect.Parameters["lightPosition"].SetValue(LightPosition);
            ObjectEffect.Parameters["ambientColor"].SetValue(AmbientColor);
            ObjectEffect.Parameters["diffuseColor"].SetValue(DiffuseColor);
            ObjectEffect.Parameters["specularColor"].SetValue(SpecularColor);
            // TODO coeficientes que dependen del material
            ObjectEffect.Parameters["KAmbient"].SetValue(0.1f);
            ObjectEffect.Parameters["KDiffuse"].SetValue(0.6f);
            ObjectEffect.Parameters["KSpecular"].SetValue(0.2f);
            ObjectEffect.Parameters["shininess"].SetValue(16.0f);


            // Terreno
            Texture2D terrainHeightMap = content.Load<Texture2D>(ContentFolder.Textures + "Rolling Hills Height Map/Rolling Hills Height Map 256");
            Texture2D terrainBaseColor = content.Load<Texture2D>(ContentFolder.Textures + "Grass/Grass_005_BaseColor");
            Texture2D terrainNormalMap = content.Load<Texture2D>(ContentFolder.Textures + "Grass/Grass_005_Normal");
            terrainSize = 1024f;
            float heightScale = 0.4f;
            float terrainScale = terrainSize / terrainHeightMap.Width;
            TerrainEffect = content.Load<Effect>(ContentFolder.Effects + "BlinnPhongNormalMap");
            TerrainEffect.Parameters["lightPosition"].SetValue(LightPosition);
            terrain = new SimpleTerrain(graphics.GraphicsDevice, terrainHeightMap, terrainBaseColor, terrainNormalMap, TerrainEffect, terrainScale, heightScale);

            // TODO setear position.Y, pitch y roll del tanque en la posición inicial
            Tree.LoadContent(content, ObjectEffect);
            Rock.LoadContent(content, ObjectEffect);
            Bush.LoadContent(content, ObjectEffect);

            Model skyBox = content.Load<Model>(ContentFolder.Models + "geometries/cube");
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
            tank = new Tank(new Vector3(0f, terrain.Height(0f, 0f), 0f), new Vector3(0.1f, 0.1f, 0.1f), MathHelper.PiOver2, 0f, 0f);
            tank.Load(content, Model);
            Tank.DefaultEffect = ObjectEffect;

            // proyectiles
            Bullet.LoadContent(content, ObjectEffect);
            Bullets = new Bullet[bulletCount];
            for (int i = 0; i < bulletCount; i++)
            {
                Bullets[i] = new Bullet();
            }

            LoadGizmos();
            LoadSceneObjects();
        }

        protected override void LoadSceneObjects()
        {
            LoadTerrainObjects(0.1f, 0.95f);
            LoadTanks(0.7f, 0.9f);
        }

        private void LoadTerrainObjects(float innerMargin, float outerMargin)
        {

            Random rnd = new Random();
            int treeCount = 0;
            int bushCount = 0;
            int rockCount = 0;

            for (int i = 0; i < 200; i++)
            {
                // posición
                float x = (float)rnd.NextDouble() * terrainSize - terrainSize / 2;
                float z = (float)rnd.NextDouble() * terrainSize - terrainSize / 2;
                float y = terrain.Height(x, z);

                // escala
                float height = (float)rnd.NextDouble() * 0.4f + 0.8f;
                float width = (float)rnd.NextDouble() * 0.4f + 0.8f;

                // rotación
                float rot = (float)rnd.NextDouble() * MathHelper.TwoPi;
                float objType = (float)rnd.NextDouble();

                if (objType > 0.4f)
                {
                    Tree t = new Tree(new Vector3(x, y, z), new Vector3(width, height, width), rot);
                    StaticObjects.Add(t);
                    //terrain.spacialMap.Add(t);
                    treeCount += 1;
                }
                else if (objType > 0.2f)
                {
                    Bush b = new Bush(new Vector3(x, y, z), new Vector3(width, height, width), rot);
                    StaticObjects.Add(b);
                    //terrain.spacialMap.Add(b);
                    bushCount += 1;
                }
                else
                {
                    Rock r = new Rock(new Vector3(x, y, z), new Vector3(width, height, width), rot);
                    StaticObjects.Add(r);
                    //terrain.spacialMap.Add(r);
                    rockCount += 1;
                }

            }

            Debug.WriteLine("Trees: {0}", treeCount);
            Debug.WriteLine("Bushes: {0}", bushCount);
            Debug.WriteLine("Rocks: {0}", rockCount);
        }

        private void LoadTanks(float innerMargin, float outerMargin)
        {
            Random rnd = new Random();
            Enemies = new Tank[enemyCount];

            for (int i = 0; i < enemyCount; i++)
            {
                // posición
                float x = (float)rnd.NextDouble() * terrainSize - terrainSize / 2;
                float z = (float)rnd.NextDouble() * terrainSize - terrainSize / 2;
                float y = terrain.Height(x, z);
                // rotación
                float rot = (float)rnd.NextDouble() * MathHelper.TwoPi;
                Tank t = new Tank(new Vector3(x, y, z), new Vector3(0.01f, 0.01f, 0.01f), rot, 0f, 0f);
                t.Load(content, Model);
                Enemies[i] = t;
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
            changeScene = false;

            if (input.keyboardState.IsKeyDown(Keys.Z) && input.previousKeyboardState.IsKeyUp(Keys.Z))
            {
                changeScene = true;
            }

            // rozamiento
            if (tank.Propulsion > 0)
            {
                tank.Propulsion = MathHelper.Clamp(tank.Propulsion - Tank.Friction, 0, Tank.SpeedLimit);
            }
            else if (tank.Propulsion < 0)
            {
                tank.Propulsion = MathHelper.Clamp(tank.Propulsion + Tank.Friction, -Tank.SpeedLimit, 0);
            }

            // disparo
            if ((input.keyboardState.IsKeyDown(Keys.Space) && input.previousKeyboardState.IsKeyUp(Keys.Space)) || (input.mouseState.LeftButton == ButtonState.Pressed && input.previousMouseState.LeftButton == ButtonState.Released))
            {
                tank.Shoot(Bullets, Bullets.Length);
            }

            // dirección rotación
            if (input.keyboardState.IsKeyDown(Keys.Right) || input.keyboardState.IsKeyDown(Keys.D))
            {
                tank.Yaw -= elapsedTime;
                tank.SteerRotation -= elapsedTime;
                tank.Propulsion = MathHelper.Clamp(tank.Propulsion + Tank.SpeedIncrease * 0.5f, -Tank.SpeedLimit, Tank.SpeedLimit);

            }
            else if (input.keyboardState.IsKeyDown(Keys.Left) || input.keyboardState.IsKeyDown(Keys.A))
            {
                tank.Yaw += elapsedTime;
                tank.SteerRotation += elapsedTime;
                tank.Propulsion = MathHelper.Clamp(tank.Propulsion + Tank.SpeedIncrease * 0.5f, -Tank.SpeedLimit, Tank.SpeedLimit);
            }

            // avance/retroceso
            if (input.keyboardState.IsKeyDown(Keys.Up) || input.keyboardState.IsKeyDown(Keys.W))
            {
                tank.Propulsion = MathHelper.Clamp(tank.Propulsion + Tank.SpeedIncrease, -Tank.SpeedLimit, Tank.SpeedLimit);
            }
            else if (input.keyboardState.IsKeyDown(Keys.Down) || input.keyboardState.IsKeyDown(Keys.S))
            {
                tank.Propulsion = MathHelper.Clamp(tank.Propulsion - Tank.SpeedIncrease, -Tank.SpeedLimit, Tank.SpeedLimit);
            }

            if (tank.Speed > 0f && tank.SteerRotation != 0f)
            {
                float sign = tank.SteerRotation > 0 ? 1 : -1;
                tank.SteerRotation -= (elapsedTime * 0.5f * sign);
            }

            // torreta y cañon
            tank.TurretRotation += input.mouseDeltaX * elapsedTime * camX;
            tank.CannonRotation += input.mouseDeltaY * elapsedTime * camY;

            Matrix RotationMatrix = Matrix.CreateRotationY(tank.Yaw);
            Matrix CameraRotationMatrix = Matrix.CreateFromYawPitchRoll(tank.Yaw + tank.TurretRotation, -tank.CannonRotation, 0f);

            Vector3 movement = RotationMatrix.Forward * tank.Speed * elapsedTime;
            tank.WheelRotation += (tank.Speed * elapsedTime / 8f); // TODO revisar esta fórmula
            tank.Position = tank.Position + movement;
            tank.Position.Y = terrain.Height(tank.Position.X, tank.Position.Z);

            float distanceForward = 3.303362f;
            float distanceRight = 3.032239f;
            float clampPitch = elapsedTime / 4;
            float clampRoll = elapsedTime / 4;

            // pendiente hacia adelante/atrás 
            Vector3 positionForward = tank.Position + RotationMatrix.Forward * distanceForward;
            positionForward.Y = terrain.Height(positionForward.X, positionForward.Z);
            float currentPitch = (tank.Position.Y - positionForward.Y) / (tank.Position - positionForward).Length();
            float deltaPitch = currentPitch - tank.Pitch;
            tank.Pitch += MathHelper.Clamp(deltaPitch, -clampPitch, clampPitch);

            // velocidad en pendiente
            tank.Downhill = tank.Propulsion * (float)Math.Sin(currentPitch);

            // pendiente hacia los costados
            Vector3 positionRight = tank.Position + RotationMatrix.Right * distanceRight;
            positionRight.Y = terrain.Height(positionRight.X, positionRight.Z);
            float currentRoll = (tank.Position.Y - positionRight.Y) / (tank.Position - positionRight).Length();
            float deltaRoll = currentRoll - tank.Roll;
            tank.Roll += MathHelper.Clamp(deltaRoll, -clampRoll, clampRoll);


            tank.World = Matrix.CreateScale(0.01f) * Matrix.CreateFromYawPitchRoll(tank.Yaw + MathHelper.Pi, tank.Pitch, tank.Roll) * Matrix.CreateTranslation(tank.Position); // TODO definir escala tanque
            tank.Update(elapsedTime);

            MainCamera.TargetPosition = tank.Position + CameraRotationMatrix.Forward * 40; // TODO revisar posición objetivo 
            MainCamera.Position = tank.Position + CameraRotationMatrix.Backward * 20 + Vector3.UnitY * 12; // TODO revisar posición cámara
            MainCamera.BuildView();

            ObjectEffect.Parameters["eyePosition"].SetValue(MainCamera.Position);


            // Update the view projection matrix of the bounding frustum
            Frustum.Matrix = MainCamera.View * MainCamera.Projection;

            gizmos.UpdateViewProjection(Camera.View, Camera.Projection);


            //// colisiones entre tanque y objetos del escenario
            foreach (WorldEntity e in StaticObjects)
            {
                if (e.Status != WorldEntityStatus.Destroyed)
                {
                    if (tank.Intersects(e.GetHitBox()))
                    {
                        // TODO destruir objeto
                        e.Status = WorldEntityStatus.Destroyed;
                    }
                }
            }

            // tanques enemigos
            foreach (Tank t in Enemies)
            {
                if (t.Status != WorldEntityStatus.Destroyed)
                {
                    if (tank.Intersects(t))
                    {
                        // TODO destruir objeto
                        t.Status = WorldEntityStatus.Destroyed;
                    }
                    t.Update(elapsedTime);
                }
            }

            // proyectiles
            foreach (Bullet b in Bullets)
            {
                if (b.Active)
                {
                    b.Update(elapsedTime, terrain, StaticObjects, Enemies);
                }
            }

        }
        #endregion

        #region Draw
        public override void Draw(CameraType SelectedCamera, bool drawBoundingBoxes, bool drawPositions, bool drawShadowMap)
        {
            SelectCamera(SelectedCamera);

            if (drawShadowMap)
            {
                // ShadowMapRenderTarget
                DrawShadows(null);
                return;
            }

            SkyEffect.Draw(Camera.View, Camera.Projection, Camera.Position);
            terrain.Draw(Camera.View, Camera.Projection);
            tank.Draw(Camera.View, Camera.Projection);

            // objetos del escenario
            int drawWorldEntity = 0;
            foreach (WorldEntity e in StaticObjects)
            {
                if (e.Status != WorldEntityStatus.Destroyed && Frustum.Intersects(e.GetDrawBox()))
                {
                    //terrain.spacialMap.Update(e);
                    e.Draw(Camera.View, Camera.Projection, ObjectEffect);
                    drawWorldEntity += 1;
                }
            }
            Debug.WriteLine(drawWorldEntity);

            // tanques enemigos
            foreach (Tank t in Enemies)
            {
                if (t.Status != WorldEntityStatus.Destroyed && t.Intersects(Frustum))
                {
                    t.Draw(Camera.View, Camera.Projection);
                }
            }

            // proyectiles
            foreach (Bullet b in Bullets)
            {
                if (b.Active)
                {
                    b.Draw(Camera.View, Camera.Projection, ObjectEffect);
                }
            }

            DrawGizmos(drawBoundingBoxes, drawPositions);
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
                    foreach (Tank t in Enemies)
                    {
                        if (t.Status != WorldEntityStatus.Destroyed)
                        {
                            t.DrawBoundingBox(gizmos);
                        }
                    }
                    tank.DrawBoundingBox(gizmos);
                    gizmos.DrawFrustum(MainCamera.View * MainCamera.Projection, Color.White);
                }

                gizmos.Draw();
            }
        }
        private void DrawShadows(RenderTarget2D renderTarget)
        {
            // Pass 1

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            // Set the render target as our shadow map, we are drawing the depth into this texture
            graphics.GraphicsDevice.SetRenderTarget(renderTarget);
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            ShadowMapEffect.CurrentTechnique = ShadowMapEffect.Techniques["DepthPass"];

            tank.Draw(LightCamera.View, LightCamera.Projection, ShadowMapEffect);
            foreach (Bullet b in Bullets)
            {
                b.DrawShadowMap(LightCamera.View, LightCamera.Projection, ShadowMapEffect);
            }
            foreach (WorldEntity e in StaticObjects)
            {
                e.DrawShadowMap(LightCamera.View, LightCamera.Projection, ShadowMapEffect);
            }
            terrain.DrawShadowMap(LightCamera.View, LightCamera.Projection, ShadowMapEffect);
        }

        #endregion


        public override void Dispose()
        {
            ScreenQuad.Dispose();
            ShadowMapRenderTarget.Dispose();
        }

    }
}
