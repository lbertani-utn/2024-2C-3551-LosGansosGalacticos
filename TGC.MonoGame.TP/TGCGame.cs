using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Tank;

namespace TGC.MonoGame.TP
{
    /// <summary>
    ///     Esta es la clase principal del juego.
    ///     Inicialmente puede ser renombrado o copiado para hacer mas ejemplos chicos, en el caso de copiar para que se
    ///     ejecute el nuevo ejemplo deben cambiar la clase que ejecuta Program <see cref="Program.Main()" /> linea 10.
    /// </summary>
    public class TGCGame : Game
    {
        public const string ContentFolder = "Content";
        public const string ContentFolder3D = "Models/";
        public const string ContentFolderEffects = "Effects/";
        public const string ContentFolderMusic = "Music/";
        public const string ContentFolderSounds = "Sounds/";
        public const string ContentFolderSpriteFonts = "SpriteFonts/";
        public const string ContentFolderTextures = "Textures/";

        /// <summary>
        ///     Constructor del juego.
        /// </summary>
        public TGCGame()
        {
            // Maneja la configuracion y la administracion del dispositivo grafico.
            Graphics = new GraphicsDeviceManager(this);
            
            Graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width - 100;
            Graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height - 100;
            
            // Para que el juego sea pantalla completa se puede usar Graphics IsFullScreen.
            // Carpeta raiz donde va a estar toda la Media.
            Content.RootDirectory = "Content";
            // Hace que el mouse sea visible.
            IsMouseVisible = true;
        }

        private GraphicsDeviceManager Graphics { get; }
        private Point ScreenCenter;
        private SpriteBatch SpriteBatch { get; set; }
        private Gizmos.Gizmos Gizmos;
        private bool DrawBoundingBoxes = false;
        private bool DrawPositions = false;
        private Effect Effect { get; set; }
        private Random rnd = new Random();

        private TargetCamera Camera { get; set; }
        public static float CameraNearPlaneDistance { get; set; } = 1f;
        public static float CameraFarPlaneDistance { get; set; } = 2000f;
        private const float camX = 0.2f;
        private const float camY = -0.1f;


        // TODO crear clase para tanque jugador
        private Model Model { get; set; }
        private Steamroller Tank;
        private float Rotation;
        private Vector3 Position;
        private Matrix World { get; set; }
        private float Velocidad = 0f;
        private const float VelocidadIncremento = 0.5f;
        private const float VelocidadMaxima = 12;
        private const float Rozamiento = 0.05f;


        // terreno
        private Terrain terrain;

        private List<WorldEntity> Entities;

        // Mapeo de teclas
        //private Dictionary<Keys, BindingAction> KeyBindings;
        private KeyboardState keyboardState;
        private KeyboardState previousKeyboardState;
        private MouseState mouseState;
        private MouseState previousMouseState;

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo.
        ///     Escribir aqui el codigo de inicializacion: el procesamiento que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void Initialize()
        {
            // La logica de inicializacion que no depende del contenido se recomienda poner en este metodo.
            ScreenCenter = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            Mouse.SetPosition(ScreenCenter.X, ScreenCenter.Y);

            // deshabilito el backface culling
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            //KeyBindings = new() {
            //    {Keys.W, BindingLogic.PositiveDirection},
            //    {Keys.Up, BindingLogic.PositiveDirection},
            //    {Keys.A, BindingLogic.PositiveRotation},
            //    {Keys.Left, BindingLogic.PositiveRotation},
            //    {Keys.S, BindingLogic.NegativeDirection},
            //    {Keys.Down, BindingLogic.NegativeDirection},
            //    {Keys.D, BindingLogic.NegativeRotation},
            //    {Keys.Right, BindingLogic.NegativeRotation},
            //};

            Camera = new TargetCamera(GraphicsDevice.Viewport.AspectRatio, Vector3.One * 100f, Vector3.Zero)
            {
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, CameraNearPlaneDistance, CameraFarPlaneDistance)
            };

            // Configuramos nuestras matrices de la escena.
            Position = new Vector3(0f, 2f, 0f); // TODO posición inicial tanque
            World = Matrix.CreateTranslation(Position);

            Entities = new List<WorldEntity>();
            Random rnd = new();

            base.Initialize();
        }

        /// <summary>
        ///     Se llama una sola vez, al principio cuando se ejecuta el ejemplo, despues de Initialize.
        ///     Escribir aqui el codigo de inicializacion: cargar modelos, texturas, estructuras de optimizacion, el procesamiento
        ///     que podemos pre calcular para nuestro juego.
        /// </summary>
        protected override void LoadContent()
        {
            // Aca es donde deberiamos cargar todos los contenido necesarios antes de iniciar el juego.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            
            Gizmos = new Gizmos.Gizmos();
            Gizmos.LoadContent(GraphicsDevice, new ContentManager(Content.ServiceProvider, ContentFolder));
            Gizmos.Enabled = true;

            // Cargo un efecto basico propio declarado en el Content pipeline.
            // En el juego no pueden usar BasicEffect de MG, deben usar siempre efectos propios.
            Effect = Content.Load<Effect>(ContentFolderEffects + "BasicShader");

            // Cargo el tanque
            // TODO mover esto a su clase
            Model = Content.Load<Model>(ContentFolder3D + "tank/tank");
            ApplyEffect(Model, Effect);
            Tank = new Steamroller();
            Tank.Load(Model);

            // Asigno el efecto que cargue a cada parte del mesh.
            // Un modelo puede tener mas de 1 mesh internamente.
            foreach (var mesh in Model.Meshes)
            {
                // Un mesh puede tener mas de 1 mesh part (cada 1 puede tener su propio efecto).
                foreach (var meshPart in mesh.MeshParts)
                {
                    meshPart.Effect = Effect;
                }
            }

            terrain = new(this, Effect, GraphicsDevice, (200.0f, 200.0f), 1f);

            Tree.LoadContent(Content, Effect);
            Rock.LoadContent(Content, Effect);
            Bush.LoadContent(Content, Effect);

            LoadSurfaceObjects();

            base.LoadContent();
            previousKeyboardState = Keyboard.GetState();
        }

        /// <summary>
        ///     Se llama en cada frame.
        ///     Se debe escribir toda la logica de computo del modelo, asi como tambien verificar entradas del usuario y reacciones
        ///     ante ellas.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logica de actualizacion del juego.
            float elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);


            // Capturar Input teclado y mouse
            previousKeyboardState = keyboardState;
            previousMouseState = mouseState;
            keyboardState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            float mouseDeltaX = previousMouseState.Position.X - mouseState.Position.X;
            float mouseDeltaY = previousMouseState.Position.Y - mouseState.Position.Y;

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                // Salgo del juego.
                Exit();
            }

            // gizmos
            if (keyboardState.IsKeyDown(Keys.B) && previousKeyboardState.IsKeyUp(Keys.B))
            {
                DrawBoundingBoxes = !DrawBoundingBoxes;
            }
            if (keyboardState.IsKeyDown(Keys.P) && previousKeyboardState.IsKeyUp(Keys.P))
            {
                DrawPositions = !DrawPositions;
            }

            // rozamiento
            if (Velocidad > 0)
            {
                Velocidad = MathHelper.Clamp(Velocidad - Rozamiento, 0, VelocidadMaxima);
            }
            else if (Velocidad < 0)
            {
                Velocidad = MathHelper.Clamp(Velocidad + Rozamiento, -VelocidadMaxima, 0);
            }

            // dirección rotación
            if ((keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D)))
            {
                Rotation -= elapsedTime;
                Tank.SteerRotation -= elapsedTime;
            }
            else if ((keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A)))
            {
                Rotation += elapsedTime;
                Tank.SteerRotation += elapsedTime;
            }

            // avance/retroceso
            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))
            {
                Velocidad = MathHelper.Clamp(Velocidad + VelocidadIncremento, -VelocidadMaxima, VelocidadMaxima);
            }
            else if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
            {
                Velocidad = MathHelper.Clamp(Velocidad - VelocidadIncremento, -VelocidadMaxima, VelocidadMaxima);
            }

            // torreta y cañon
            Tank.TurretRotation += mouseDeltaX * elapsedTime * camX;
            Tank.CannonRotation += mouseDeltaY * elapsedTime * camY;

            Matrix RotationMatrix = Matrix.CreateRotationY(Rotation);
            Matrix CameraRotationMatrix = Matrix.CreateFromYawPitchRoll(Rotation + Tank.TurretRotation, -Tank.CannonRotation, 0f);

            Vector3 movement = RotationMatrix.Forward * Velocidad * elapsedTime;
            Position = Position + movement;
            Position.Y = Terrain.GetPositionHeight(Position.X, Position.Z);
            World = Matrix.CreateScale(0.01f) * Matrix.CreateRotationY(Rotation + MathHelper.Pi) * Matrix.CreateTranslation(Position); // TODO definir escala tanque

            Tank.WheelRotation += (Velocidad * elapsedTime / 8f); // TODO revisar esta fórmula

            Camera.TargetPosition = Position + CameraRotationMatrix.Forward * 40; // TODO revisar posición objetivo 
            Camera.Position = Position + CameraRotationMatrix.Backward * 20 + Vector3.UnitY * 12; // TODO revisar posición cámara
            Camera.BuildView();


            Gizmos.UpdateViewProjection(Camera.View, Camera.Projection);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Se llama cada vez que hay que refrescar la pantalla.
        ///     Escribir aqui el codigo referido al renderizado.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            // Aca deberiamos poner toda la logia de renderizado del juego.
            GraphicsDevice.Clear(new Color(23 / 255.0f, 171 / 255.0f, 237 / 255.0f));

            // terreno
            terrain.Draw(GraphicsDevice, Effect);

            foreach (WorldEntity e in Entities)
            {
                terrain.spacialMap.Update(e);
                e.Draw(Camera.View, Camera.Projection, Effect);
            }

            Tank.Draw(World, Camera.View, Camera.Projection, Effect);

            // gizmos
            if (DrawBoundingBoxes || DrawPositions)
            {
                if (DrawBoundingBoxes)
                {
                    foreach (WorldEntity e in Entities)
                    {
                        e.DrawBoundingBox(Gizmos);
                    }
                }
                if (DrawPositions)
                {
                    foreach (WorldEntity e in Entities)
                    {
                        e.DrawPosition(Gizmos);
                    }
                }
                Gizmos.Draw();
            }
        }

        /// <summary>
        ///     Libero los recursos que se cargaron en el juego.
        /// </summary>
        protected override void UnloadContent()
        {
            // Libero los recursos.
            Content.Unload();

            base.UnloadContent();
        }

        private void LoadSurfaceObjects()
        {
            for (int i = 0; i < 200; i++)
            {
                // posición
                float x = (float) rnd.NextDouble() * 200f - 100f;
                float z = (float) rnd.NextDouble() * 200f - 100f;
                float y = Terrain.GetPositionHeight(x,z);

                // escala
                float height = (float)rnd.NextDouble() * 0.4f + 0.8f;
                float width = (float)rnd.NextDouble() * 0.4f + 0.8f;

                // rotación
                float rot = (float)rnd.NextDouble() * MathHelper.TwoPi;

                if (rnd.NextDouble() > 0.4f) {
                    Tree t = new(new Vector3(x, y, z), new Vector3(width, height, width), rot);
                    Entities.Add(t);
                    terrain.spacialMap.Add(t);
                }
                else if (rnd.NextDouble() > 0.1f)
                {
                    Bush b = new(new Vector3(x, y, z), new Vector3(width, height, width), rot);
                    Entities.Add(b);
                    terrain.spacialMap.Add(b);
                }
                else {
                    Rock r = new(new Vector3(x, y, z), new Vector3(width, height, width), rot);
                    Entities.Add(r);
                    terrain.spacialMap.Add(r);
                }
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

    }
}
