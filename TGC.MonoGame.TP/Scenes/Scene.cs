using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Scenes
{
    abstract class Scene
    {
        protected GraphicsDeviceManager graphics;
        protected ContentManager content;
        protected ContentFolder contentFolder;
        protected Gizmos.Gizmos gizmos;

        protected TargetCamera MainCamera;
        protected StaticCamera DebugCamera;
        protected TargetCamera LightCamera;
        protected BoundingFrustum Frustum;
        protected Camera _camera;
        public Camera Camera { get => _camera; }

        protected Vector3 LightPosition;
        protected Vector3 AmbientColor;

        protected List<WorldEntity> StaticObjects;
        protected List<WorldEntity> DynamicObjects;


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
                _camera = MainCamera;
            }
            else if(SelectedCamera == CameraType.Debug )
            {
                _camera = DebugCamera;
            }
            else if(SelectedCamera == CameraType.Light)
            {
                _camera = LightCamera;
            }
            else
            {
                _camera = MainCamera;
            }
        }

        protected abstract void DrawGizmos(bool drawBoundingBoxes, bool drawPositions);
        public abstract void Dispose();

    }
}
