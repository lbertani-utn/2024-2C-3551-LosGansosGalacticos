using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Geometries;
using TGC.MonoGame.TP.Scenes.Entities;

namespace TGC.MonoGame.TP.Scenes
{
    abstract class Scene
    {
        protected GraphicsDeviceManager graphics;
        protected ContentManager content;
        protected ContentFolder contentFolder;
        protected Gizmos.Gizmos gizmos;

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


        public bool ChangeScene { get => changeScene; }
        protected bool changeScene = false;

        public Scene NextScene { get => nextScene; }
        protected Scene nextScene;



        public Scene(GraphicsDeviceManager graphics, ContentManager content)
        {
            this.graphics = graphics;
            this.content = content;
        }

        public abstract void Initialize();
        public abstract void LoadContent();

        protected void LoadGizmos()
        {
            gizmos = new Gizmos.Gizmos();
            gizmos.LoadContent(graphics.GraphicsDevice, new ContentManager(content.ServiceProvider, ContentFolder.Root));
            gizmos.Enabled = true;

        }

        protected abstract void LoadSceneObjects();
        public abstract void Update(float elapsedTime, UserInput input);
        public abstract void Draw(CameraType SelectedCamera, bool drawBoundingBoxes, bool drawPositions, bool drawShadowMap);

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
