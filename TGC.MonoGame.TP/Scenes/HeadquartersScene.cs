using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.UI;
using TGC.MonoGame.TP.Scenes.Entities; 
using TGC.MonoGame.TP.Scenes.Headquarters;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace TGC.MonoGame.TP.Scenes
{
    class HeadquartersScene : Scene
    {
        private Effect ObjectEffect;

        // camera
        Vector3 mainCameraInitialPosition;
        Vector3 mainCameraPosition;
        private float idle = 0;

        // UI
        private const float TitleScale = 5F;
        private const float MenuScale = 2F;
        private MenuType menuType;
        private string[][] menuOptions;
        private int selectedOption = 0;


        public HeadquartersScene(GraphicsDeviceManager graphics, ContentManager content, GameOptions options) : base(graphics, content, options)
        {
        }

        public override void Initialize()
        {
            menuType = MenuType.Main;
            menuOptions = new string[3][];
            menuOptions[(int) MenuType.Main] = new string[] { Message.Play, Message.Options, Message.Exit };
            menuOptions[(int) MenuType.Pause] = new string[] { Message.Resume, Message.Restart, Message.Options, Message.Exit };
            menuOptions[(int) MenuType.Options] = new string[] { Message.Volume, Message.Sound, Message.Music, Message.GodMode, Message.Volver };

            StaticObjects = new List<WorldEntity>();

            // cámara principal - apuntando al centro de la messa
            Vector3 targetPosition = new Vector3(-1f, 0.8f, -1f);
            mainCameraInitialPosition = new Vector3(-2.50f, 1.20f, -1.95f);
            mainCameraPosition = mainCameraInitialPosition;
            MainCamera = new TargetCamera(graphics.GraphicsDevice.Viewport.AspectRatio, mainCameraPosition, targetPosition);
            MainCamera.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, graphics.GraphicsDevice.Viewport.AspectRatio, 0.001f, 25f);
            camera = MainCamera;

            // bounding frustum de la cámara que sigue al tanque
            Frustum = new BoundingFrustum(MainCamera.View * MainCamera.Projection);

            // cámara para debug - vista aérea
            DebugCamera = new StaticCamera(graphics.GraphicsDevice.Viewport.AspectRatio, Vector3.UnitY * 16f, -Vector3.UnitY, Vector3.UnitZ);
            DebugCamera.RightDirection = Vector3.UnitX;
            DebugCamera.BuildView();

            // cámara en fuente de luz - lámpara en el techo
            LightPosition = new Vector3(-5f, 2.8f, -5f);
            LightCamera = new TargetCamera(1f, LightPosition, Vector3.Zero);
            LightCamera.BuildProjection(1f, 1f, 15f, MathHelper.PiOver2);
            LightCamera.BuildView();

        }

        #region Load
        public override void LoadContent()
        {
            int ShadowmapSize = 2048;
            ShadowMapRenderTarget = new RenderTarget2D(graphics.GraphicsDevice, ShadowmapSize, ShadowmapSize, false, SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = content.Load<SpriteFont>("SpriteFonts/CascadiaCode/CascadiaCodePL");

            AmbientColor = new Vector3(1f, 0.482352941f, 0.098039216f);

            // ObjectEffect
            AmbientColor = Vector3.One;
            ObjectEffect = content.Load<Effect>(ContentFolder.Effects + "ObjectShader");
            LoadSceneParameters();


            Floor.LoadContent(content, ObjectEffect);
            Wall.LoadContent(content, ObjectEffect);
            Box.LoadContent(content, ObjectEffect);
            Table.LoadContent(content, ObjectEffect);
            Map.LoadContent(content, ObjectEffect);

            LoadGizmos();
            LoadSceneObjects();
        }

        public override void LoadSceneParameters()
        {
            ObjectEffect.Parameters["LightViewProjection"].SetValue(LightCamera.View * LightCamera.Projection);
            ObjectEffect.Parameters["lightPosition"].SetValue(LightPosition);
            ObjectEffect.Parameters["ambientColor"].SetValue(AmbientColor);
            ObjectEffect.Parameters["diffuseColor"].SetValue(Vector3.One);
            ObjectEffect.Parameters["specularColor"].SetValue(Vector3.One);
            ObjectEffect.Parameters["KAmbient"].SetValue(0.1f);
            ObjectEffect.Parameters["KDiffuse"].SetValue(0.6f);
            ObjectEffect.Parameters["KSpecular"].SetValue(0.1f);
            ObjectEffect.Parameters["shininess"].SetValue(16.0f);
            ObjectEffect.Parameters["eyePosition"].SetValue(MainCamera.Position);
            ObjectEffect.Parameters["Tiling"].SetValue(Vector2.One);
        }

        protected override void LoadSceneObjects()
        {
            // Vector3 position, Vector3 scale, float yaw
            StaticObjects.Add(new Wall(new Vector3(4.85f, 1.5f, 0f), new Vector3(0.3f, 4f, 10f), 0f));
            StaticObjects.Add(new Wall(new Vector3(-0.15f, 1.5f, 4.85f), new Vector3(9.7f, 4f, 0.3f), 0f));
            StaticObjects.Add(new Floor(new Vector3(0f, -0.01f, 0f), new Vector3(10f, 0.02f, 10f), 0f));
            StaticObjects.Add(new Box(new Vector3(4.1f, 0.6f, 4.1f), new Vector3(1.2f, 1.2f, 1.2f), 0f));
            StaticObjects.Add(new Map(new Vector3(-1f, 0.801f, -1f), new Vector3(1.8f, 0.002f, 1.8f), 0f));
            StaticObjects.Add(new Table(new Vector3(-1f, 0.75f, -1f), new Vector3(2f, 0.1f, 2f), 0f));
            StaticObjects.Add(new Table(new Vector3(-1.95f, 0.35f, -0.05f), new Vector3(0.1f, 0.7f, 0.1f), 0f));
            StaticObjects.Add(new Table(new Vector3(-1.95f, 0.35f, -1.95f), new Vector3(0.1f, 0.7f, 0.1f), 0f));
            StaticObjects.Add(new Table(new Vector3(-0.05f, 0.35f, -1.95f), new Vector3(0.1f, 0.7f, 0.1f), 0f));
            StaticObjects.Add(new Table(new Vector3(-0.05f, 0.35f, -0.05f), new Vector3(0.1f, 0.7f, 0.1f), 0f));
        }
        #endregion

        public override void Update(float elapsedTime, UserInput input)
        {
            changeScene = false;
            gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            if (input.IsKeyPressed(Keys.Z))
            {
                changeScene = true;
            }


            // camera position
            if (input.keyboardState.IsKeyDown(Keys.J))
            {
                mainCameraPosition -= Vector3.UnitX / 100;
                idle = 0;
            }
            else if (input.keyboardState.IsKeyDown(Keys.U))
            {
                mainCameraPosition += Vector3.UnitX / 100;
                idle = 0;
            }
            else if (input.keyboardState.IsKeyDown(Keys.K))
            {
                mainCameraPosition += Vector3.UnitZ / 100;
                idle = 0;
            }
            else if (input.keyboardState.IsKeyDown(Keys.H))
            {
                mainCameraPosition -= Vector3.UnitZ / 100;
                idle = 0;
            }
            else if (input.keyboardState.IsKeyDown(Keys.O))
            {
                mainCameraPosition += Vector3.UnitY / 100;
                idle = 0;
            }
            else if (input.keyboardState.IsKeyDown(Keys.L))
            {
                mainCameraPosition -= Vector3.UnitY / 100;
                idle = 0;
            }
            else
            {
                idle += elapsedTime;
                if (idle > 5)
                {
                    idle = 0;
                    mainCameraPosition = mainCameraInitialPosition;
                }
            }

            // menu
            if (input.IsKeyPressed(Keys.Down) || input.IsKeyPressed(Keys.S))
            {
                if (selectedOption < menuOptions[(int) menuType].Length - 1)
                {
                    selectedOption++;
                    // TODO MenuSoundEffectInstance.Play();
                }
            }
            else if (input.IsKeyPressed(Keys.Up) || input.IsKeyPressed(Keys.W))
            {
                if (selectedOption > 0)
                {
                    selectedOption--;
                    // TODO MenuSoundEffectInstance.Play();
                }
            }
            else if (input.IsKeyPressed(Keys.Enter))
            {

            }

            MainCamera.Position = mainCameraPosition;
            MainCamera.BuildView();
        }

        public override void Draw(CameraType SelectedCamera, bool drawBoundingBoxes, bool drawPositions, bool drawShadowMap)
        {
            SelectCamera(SelectedCamera);


            #region Pass 1

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (!drawShadowMap)
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
            foreach (WorldEntity e in StaticObjects)
            {
                e.DrawDepthPass(ObjectEffect, LightCamera);
            }
            #endregion

            if (drawShadowMap)
            {
                return;
            }

            #region Pass 2
            // Set the render target as null, we are drawing on the screen!
            graphics.GraphicsDevice.SetRenderTarget(null);
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
            ObjectEffect.Parameters["shadowMap"].SetValue(ShadowMapRenderTarget);
            ObjectEffect.CurrentTechnique = ObjectEffect.Techniques["DrawObject"];
            foreach (WorldEntity e in StaticObjects)
            {
                e.Draw(Camera.View, Camera.Projection, ObjectEffect);
            }
            #endregion

            DrawGizmos(drawBoundingBoxes, drawPositions);
            DrawUI();
        }

        protected override void DrawUI()
        {
            spriteBatch.Begin();
            DrawTitle(spriteBatch, spriteFont);
            DrawMenu(spriteBatch, spriteFont, menuOptions[(int)menuType], selectedOption);
            spriteBatch.End();
        }

        private void DrawTitle(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, Message.Title, TextHelper.TitlePosition(graphics.GraphicsDevice), Color.White, TitleScale);
        }
        private void DrawMenu(SpriteBatch spriteBatch, SpriteFont spriteFont, string[] menu, int selected)
        {
            for (int opt = 0; opt < menu.Length; opt++)
            {
                Color color = opt != selected ? Color.White : Color.Yellow;
                TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, menu[opt], TextHelper.MenuPosition(graphics.GraphicsDevice, spriteFont, menu[opt], MenuScale, opt), color, MenuScale);
            }
        }

        protected override void DrawGizmos(bool drawBoundingBoxes, bool drawPositions)
        {
            if (drawBoundingBoxes || drawPositions)
            {
                foreach (WorldEntity e in StaticObjects)
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

                if (drawBoundingBoxes)
                {
                    gizmos.DrawFrustum(MainCamera.View * MainCamera.Projection, Color.White);
                }

                gizmos.Draw();
            }

        }

        public override void Dispose()
        {
            ScreenQuad.Dispose();
            ShadowMapRenderTarget.Dispose();
        }

    }
}
