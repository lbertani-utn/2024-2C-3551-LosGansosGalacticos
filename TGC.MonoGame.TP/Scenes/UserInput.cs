using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Scenes
{
    class UserInput
    {
        public KeyboardState keyboardState;
        public KeyboardState previousKeyboardState;
        public MouseState mouseState;
        public MouseState previousMouseState;
        public float mouseDeltaX = 0f;
        public float mouseDeltaY = 0f;

        public bool Escape = false;
        public bool DrawShadowMap = false;
        public bool DrawBoundingBoxes = false;
        public bool DrawPositions = false;
        public bool DrawGizmos = false;
        public CameraType SelectedCamera = CameraType.Main;
        private CameraType[] CameraRotation;

        public void Initialize(Point screenCenter)
        {

            Mouse.SetPosition(screenCenter.X, screenCenter.Y);
            CameraRotation = new CameraType[3] { CameraType.Debug, CameraType.Light, CameraType.Main};
        }

        public void Update()
        {
            // Capturar Input teclado y mouse
            previousKeyboardState = keyboardState;
            previousMouseState = mouseState;
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            mouseDeltaX = previousMouseState.Position.X - mouseState.Position.X;
            mouseDeltaY = previousMouseState.Position.Y - mouseState.Position.Y;

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                // Salgo del juego.
                Escape = true;
            }

            // gizmos y otras opciones para debug
            if (keyboardState.IsKeyDown(Keys.M) && previousKeyboardState.IsKeyUp(Keys.M))
            {
                DrawShadowMap = !DrawShadowMap;
            }
            if (keyboardState.IsKeyDown(Keys.B) && previousKeyboardState.IsKeyUp(Keys.B))
            {
                DrawBoundingBoxes = !DrawBoundingBoxes;
                DrawGizmos = DrawBoundingBoxes || DrawPositions;
            }
            if (keyboardState.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P))
            {
                DrawPositions = !DrawPositions;
                DrawGizmos = DrawBoundingBoxes || DrawPositions;
            }
            if (keyboardState.IsKeyDown(Keys.C) && previousKeyboardState.IsKeyUp(Keys.C))
            {
                SelectedCamera = CameraRotation[(int) SelectedCamera];
            }


        }



    }
}
