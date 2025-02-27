using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.UI;
using TGC.MonoGame.TP.Scenes.Entities; 
using TGC.MonoGame.TP.Scenes.Headquarters;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

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
        private const float titleScale = 5F;
        private const float menuScale = 2F;
        private MenuType menuType;
        private MenuType returnTo;
        private string[][] menuOptions;
        private int selectedOption = 0;
        private Color titleColor = Color.White;
        private Color colorSelectedOption = Color.Yellow;
        private Color colorSelectedOptionDark = Color.Olive;
        private Color colorNotSelectedOption = Color.White;
        private Color colorNotSelectedOptionDark = Color.Gray;
        private SoundEffect menuSoundEffect;

        public HeadquartersScene(GraphicsDeviceManager graphics, ContentManager content, GameOptions options) : base(graphics, content, options)
        {
        }

        public override void Initialize()
        {
            menuType = MenuType.Main;
            returnTo = MenuType.Main;
            menuOptions = new string[3][];
            menuOptions[(int) MenuType.Main] = new string[] { Message.Play, Message.Options, Message.Exit };
            menuOptions[(int) MenuType.Pause] = new string[] { Message.Resume, Message.Restart, Message.Options, Message.Exit };
            menuOptions[(int) MenuType.Options] = new string[] { Message.Volume, Message.Music, Message.Sound, Message.GodMode, Message.Return };

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

            // music & sounds
            BackgroundMusic = content.Load<Song>(ContentFolder.Music + "Evil March");
            menuSoundEffect = content.Load<SoundEffect>(ContentFolder.Sounds + "menu_select");

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
            // Escape sale del juego
            exitGame = input.Escape;

            changeScene = false;
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
                    PlaySoundEffect(menuSoundEffect);
                }
            }
            else if (input.IsKeyPressed(Keys.Up) || input.IsKeyPressed(Keys.W))
            {
                if (selectedOption > 0)
                {
                    selectedOption--;
                    PlaySoundEffect(menuSoundEffect);
                }
            }
            else if (input.IsKeyPressed(Keys.Left) || input.IsKeyPressed(Keys.A))
            {
                if(menuType== MenuType.Options)
                {
                    if (selectedOption == 0 && options.Volume > 0)
                    {
                        options.Volume -= 0.05f;
                        MediaPlayer.Volume = options.Volume;
                        PlaySoundEffect(menuSoundEffect);
                    }
                    else if (selectedOption == 1 && !options.Music)
                    {
                        options.Music = true;
                        PlaySoundEffect(menuSoundEffect);
                        PlaySceneMusic();
                    }
                    else if (selectedOption == 2 && !options.SoundEffects)
                    {
                        options.SoundEffects = true;
                        PlaySoundEffect(menuSoundEffect);
                    }
                    else if (selectedOption == 3 && !options.GodMode)
                    {
                        options.GodMode = true;
                        PlaySoundEffect(menuSoundEffect);
                    }
                }
            }
            else if (input.IsKeyPressed(Keys.Right) || input.IsKeyPressed(Keys.D))
            {
                if (menuType == MenuType.Options)
                {
                    if (selectedOption == 0 && options.Volume < 1)
                    {
                        options.Volume += 0.05f;
                        MediaPlayer.Volume = options.Volume;
                        PlaySoundEffect(menuSoundEffect);
                    }
                    else if (selectedOption == 1 && options.Music)
                    {
                        options.Music = false;
                        PlaySoundEffect(menuSoundEffect);
                        StopSceneMusic();
                    }
                    else if (selectedOption == 2 && options.SoundEffects)
                    {
                        options.SoundEffects = false;
                        PlaySoundEffect(menuSoundEffect);
                    }
                    else if (selectedOption == 3 && options.GodMode)
                    {
                        options.GodMode = false;
                        PlaySoundEffect(menuSoundEffect);
                    }
                }
            }
            else if (input.IsKeyPressed(Keys.Enter))
            {
                string option = menuOptions[(int)menuType][selectedOption];

                if (option == Message.Play || option == Message.Restart)
                {

                }
                if (option == Message.Resume)
                {

                }
                if (option == Message.Options)
                {
                    returnTo = menuType;
                    menuType = MenuType.Options;
                    selectedOption = 0;
                }
                if (option == Message.Return)
                {
                    menuType = returnTo;
                    selectedOption = 0;
                }
                if (option == Message.Exit)
                {
                    exitGame = true;
                }

            }

            MainCamera.Position = mainCameraPosition;
            MainCamera.BuildView();
            gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

        }

        public override void Draw(CameraType selectedCamera, bool debugBoundingBoxes, bool debugPositions, bool debugShadowMap)
        {
            SelectCamera(selectedCamera);


            #region Pass 1

            if (!debugShadowMap)
            {
                // Set the render target as our shadow map, we are drawing the depth into this texture
                graphics.GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
            }
            else
            {
                graphics.GraphicsDevice.SetRenderTarget(null);
            }

            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
            ObjectEffect.CurrentTechnique = ObjectEffect.Techniques["DepthPass"];
            foreach (WorldEntity e in StaticObjects)
            {
                e.DrawDepthPass(ObjectEffect, LightCamera);
            }
            #endregion

            if (debugShadowMap)
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

            DrawGizmos(debugBoundingBoxes, debugPositions);
            DrawUI();
        }

        protected override void DrawUI()
        {
            spriteBatch.Begin();
            DrawTitle(spriteBatch, spriteFont);
            DrawMenu(spriteBatch, spriteFont, menuOptions[(int)menuType], selectedOption);
            
            if (menuType == MenuType.Options)
            {
                // volumen
                int volumeLevel = (int) (options.Volume * 20f); 
                TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, Message.FullBar, TextHelper.SettingPosition(graphics.GraphicsDevice, spriteFont, menuScale, 0, 0, -0.5f), GetTextColor(0, selectedOption, false, true), menuScale);
                TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, new string(Message.Bar, volumeLevel) , TextHelper.SettingPosition(graphics.GraphicsDevice, spriteFont, menuScale, 0, 0, -0.5f), GetTextColor(0, selectedOption, true, true), menuScale);

                // música
                TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, Message.Yes, TextHelper.SettingPosition(graphics.GraphicsDevice, spriteFont, menuScale, 1, 0, 0f), GetTextColor(1, selectedOption, options.Music, true), menuScale);
                TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, Message.No, TextHelper.SettingPosition(graphics.GraphicsDevice, spriteFont, menuScale, 1, 1, 0f), GetTextColor(1, selectedOption, options.Music, false), menuScale);

                // efectos de sonido
                TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, Message.Yes, TextHelper.SettingPosition(graphics.GraphicsDevice, spriteFont, menuScale, 2, 0, 0f), GetTextColor(2, selectedOption, options.SoundEffects, true), menuScale);
                TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, Message.No, TextHelper.SettingPosition(graphics.GraphicsDevice, spriteFont, menuScale, 2, 1, 0f), GetTextColor(2, selectedOption, options.SoundEffects, false), menuScale);

                // invencible
                TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, Message.Yes, TextHelper.SettingPosition(graphics.GraphicsDevice, spriteFont, menuScale, 3, 0, 0f), GetTextColor(3, selectedOption, options.GodMode, true), menuScale);
                TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, Message.No, TextHelper.SettingPosition(graphics.GraphicsDevice, spriteFont, menuScale, 3, 1, 0f), GetTextColor(3, selectedOption, options.GodMode, false), menuScale);
            }
            spriteBatch.End();
        }

        private Color GetTextColor(int rowNumber, int rowSelected, bool value, bool expectedValue)
        {
            Color c = colorNotSelectedOptionDark;
            if (rowNumber != rowSelected)
            {
                c = value == expectedValue ? colorSelectedOptionDark : colorNotSelectedOptionDark;
            }
            else
            {
                c = value == expectedValue ? colorSelectedOption : colorNotSelectedOption; 
            }
           return c;
        }

        private void DrawTitle(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, Message.Title, TextHelper.TitlePosition(graphics.GraphicsDevice), titleColor, titleScale);
        }
        private void DrawMenu(SpriteBatch spriteBatch, SpriteFont spriteFont, string[] menu, int selected)
        {
            for (int opt = 0; opt < menu.Length; opt++)
            {
                Color color = opt != selected ? colorNotSelectedOption : colorSelectedOption;
                TextHelper.DrawStringWithShadow(spriteBatch, spriteFont, menu[opt], TextHelper.MenuPosition(graphics.GraphicsDevice, spriteFont, menuScale, opt), color, menuScale);
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
