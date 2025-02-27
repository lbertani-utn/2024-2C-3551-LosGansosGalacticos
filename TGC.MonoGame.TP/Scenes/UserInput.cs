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
        public bool DebugShadowMap = false;
        public bool DebugBoundingBoxes = false;
        public bool DebugPositions = false;
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

            // guardo se si presionó Escape (según la escena saldrá o pausará el juego)
            Escape = IsKeyPressed(Keys.Escape);

            // gizmos y otras opciones para debug
            if (IsKeyPressed(Keys.M))
            {
                DebugShadowMap = !DebugShadowMap;
            }
            if (IsKeyPressed(Keys.B))
            {
                DebugBoundingBoxes = !DebugBoundingBoxes;
                DrawGizmos = DebugBoundingBoxes || DebugPositions;
            }
            if (IsKeyPressed(Keys.P))
            {
                DebugPositions = !DebugPositions;
                DrawGizmos = DebugBoundingBoxes || DebugPositions;
            }
            if (IsKeyPressed(Keys.C))
            {
                SelectedCamera = CameraRotation[(int) SelectedCamera];
            }


        }

        public bool IsKeyPressed(Keys key)
        {
            return keyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyUp(key);
        }
        public bool IsKeyReleased(Keys key)
        {
            return keyboardState.IsKeyUp(key) && previousKeyboardState.IsKeyDown(key);
        }
        public bool IsLeftButtonPressed()
        {
            return mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
        }
        public bool IsRightButtonReleased()
        {
            return mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
        }

    }
}
