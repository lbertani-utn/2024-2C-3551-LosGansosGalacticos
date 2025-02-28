using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Scenes.Entities;
using TGC.MonoGame.TP.UI;

namespace TGC.MonoGame.TP.Scenes
{
    abstract class Scene
    {
        protected GraphicsDeviceManager graphics;
        protected ContentManager content;
        protected ContentFolder contentFolder;
        protected Gizmos.Gizmos gizmos;
        protected GameOptions options;

        // Camera
        protected Camera Camera { get => camera; }
        protected Camera camera;
        protected TargetCamera MainCamera;
        protected StaticCamera DebugCamera;
        protected TargetCamera LightCamera;
        protected BoundingFrustum Frustum;

        // Light
        protected Vector3 LightPosition;
        protected Vector3 AmbientColor;

        // Shadow
        protected RenderTarget2D ShadowMapRenderTarget;
        protected FullScreenQuad ScreenQuad;

        // World objects
        protected List<WorldEntity> StaticObjects;
        protected List<WorldEntity> DynamicObjects;

        // UI
        protected SpriteBatch spriteBatch;
        protected SpriteFont spriteFont;

        // audio
        public Song BackgroundMusic;

        // control de escenas
        public bool ExitGame { get => exitGame; }
        protected bool exitGame = false;
        public Scene NextScene { get => nextScene; }
        protected Scene nextScene;
        public bool ChangeScene { get => changeScene; }
        protected bool changeScene = false;
        public bool RestartScene { get => restartScene; }
        protected bool restartScene = false;
        public bool ChangeRestartScene { get => changeRestartScene; }
        protected bool changeRestartScene = false;

        public Scene(GraphicsDeviceManager graphics, ContentManager content, GameOptions options)
        {
            this.graphics = graphics;
            this.content = content;
            this.options = options;
        }

        public abstract void Initialize();
        public abstract void LoadContent();

        protected void LoadGizmos()
        {
            gizmos = new Gizmos.Gizmos();
            gizmos.LoadContent(graphics.GraphicsDevice, new ContentManager(content.ServiceProvider, ContentFolder.Root));
            gizmos.Enabled = true;

        }

        public abstract void LoadSceneParameters();
        protected abstract void LoadSceneObjects();

        protected abstract void LoadInitialState();
        protected void PlaySceneMusic()
        {
            if (options.Music)
            {
                MediaPlayer.Volume = options.Volume;
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(BackgroundMusic);
            }
        }
        protected void StopSceneMusic()
        {
            MediaPlayer.Stop();
        }

        protected void PlaySoundEffect(SoundEffect sound)
        {
            if (options.SoundEffects)
            {
                sound.Play();
            }
        }

        public void StartScene()
        {
            ClearFlags();
            LoadInitialState();
            LoadSceneParameters();
            PlaySceneMusic();
        }
        public void ContinueScene()
        {
            ClearFlags();
            LoadSceneParameters();
            PlaySceneMusic();
        }

        private void ClearFlags()
        {
            changeScene = false;
            restartScene = false;
            changeRestartScene = false;
        }

        public abstract void Update(float elapsedTime, UserInput input);
        public abstract void Draw(CameraType selectedCamera, bool debugBoundingBoxes, bool debugPositions, bool debugShadowMap);
        protected abstract void DrawUI();

        protected void RestoreDeviceState()
        {
            // SpriteBatch altera el estado de esto
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            graphics.GraphicsDevice.BlendState = BlendState.Opaque;
        }

        public void SelectCamera(CameraType SelectedCamera)
        {
            if (SelectedCamera == CameraType.Main)
            {
                camera = MainCamera;
            }
            else if(SelectedCamera == CameraType.Debug )
            {
                camera = DebugCamera;
            }
            else if(SelectedCamera == CameraType.Light)
            {
                camera = LightCamera;
            }
            else
            {
                camera = MainCamera;
            }
        }

        protected abstract void DrawGizmos(bool drawBoundingBoxes, bool drawPositions);
        public abstract void Dispose();
        
        public void SetNextScene(Scene nextScene)
        {
            this.nextScene = nextScene;
        }

    }
}
