using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TGC.MonoGame.TP.Collisions;
using TGC.MonoGame.TP.Materials;

namespace TGC.MonoGame.TP
{
    internal class Tank
    {
        public static Effect DefaultEffect;
        private static Texture[] Textures;
        private static Material[] Materials;
        OrientedBoundingBox[] BoundingVolumes;
        Vector3[] BoundingVolumeTraslation;
        Vector3[] BoundingVolumeScale;


        private const float PiOver3 = 1.04720f;
        private const float PiOver6 = 0.52356f;
        private const float PiOver12 = 0.26180f;


        public Vector3 Position;
        public Vector3 Scale;
        //public Quaternion Rotation;
        public float Pitch = 0f;
        public float Yaw = 0f;
        public float Roll = 0f;
        public Matrix World;

        private const float CannonCooldown = 1f;
        private float recharging = 0f;
        private Matrix ShootingPosition;
        private Matrix ShootingDirection;
        public WorldEntityStatus Status;

        public float Speed
        {
            get => Propulsion + Downhill;
        }
        public float Propulsion = 0f;
        public float Downhill = 0f;
        public const float SpeedIncrease = 0.25f;
        public const float SpeedLimit = 20f;
        public const float Friction = 0.05f;
        private const float FrontWheelRotation = 1.6f;

        private float _wheelRotation;
        public float WheelRotation
        {
            get => _wheelRotation;
            set => _wheelRotation = value;
        }

        private float _steerRotation;
        public float SteerRotation
        {
            get => _steerRotation;
            set => _steerRotation = MathHelper.Clamp(value, -MathHelper.PiOver4, MathHelper.PiOver4);
        }
        private float _turretRotation;
        public float TurretRotation
        {
            get => _turretRotation;
            set => _turretRotation = MathHelper.Clamp(value, -PiOver3, PiOver3);
        }
        private float _cannonRotation = -PiOver12;
        public float CannonRotation
        {
            get => _cannonRotation;
            set => _cannonRotation = MathHelper.Clamp(value, -PiOver6, PiOver6);
        }

        #region Fields
        // The XNA framework Model object that we are going to display.
        private Model tankModel;

        // Shortcut references to the bones that we are going to animate.
        // We could just look these up inside the Draw method, but it is more efficient to do the lookups while loading and cache the results.
        private ModelBone leftBackWheelBone;
        private ModelBone rightBackWheelBone;
        private ModelBone leftFrontWheelBone;
        private ModelBone rightFrontWheelBone;
        private ModelBone leftSteerBone;
        private ModelBone rightSteerBone;
        private ModelBone turretBone;
        private ModelBone cannonBone;
        private ModelBone hatchBone;

        // Store the original transform matrix for each animating bone.
        private Matrix leftBackWheelTransform;
        private Matrix rightBackWheelTransform;
        private Matrix leftFrontWheelTransform;
        private Matrix rightFrontWheelTransform;
        private Matrix leftSteerTransform;
        private Matrix rightSteerTransform;
        private Matrix turretTransform;
        private Matrix cannonTransform;
        private Matrix hatchTransform;

        // Array holding all the bone transform matrices for the entire model.
        // We could just allocate this locally inside the Draw method, but it is more efficient to reuse a single array, as this avoids creating unnecessary garbage.
        private Matrix[] boneTransforms;
        #endregion Fields

        public Tank(Vector3 position, Vector3 scale, float yaw, float pitch, float roll)
        {
            this.Position = position;
            this.Scale = scale;
            this.Yaw = yaw;
            this.Pitch = pitch;
            this.Roll = roll;
            this.World = Matrix.CreateScale(scale) * Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateTranslation(position);
            this.Status = WorldEntityStatus.Intact;
        }

        public void LoadBoundingVolumes()
        {
            BoundingVolumeTraslation = new Vector3[8];
            BoundingVolumeTraslation[0] = new Vector3(-0.00000f, 1.37576f, -0.93567f);
            BoundingVolumeTraslation[1] = new Vector3(-1.66787f, 2.18925f, -0.29799f);
            BoundingVolumeTraslation[2] = new Vector3(-1.97411f, 1.05415f, -2.45230f);
            BoundingVolumeTraslation[3] = new Vector3(-1.70793f, 0.73387f, 2.40269f);
            BoundingVolumeTraslation[4] = new Vector3(1.66787f, 2.18925f, -0.29799f);
            BoundingVolumeTraslation[5] = new Vector3(1.97411f, 1.05415f, -2.45230f);
            BoundingVolumeTraslation[6] = new Vector3(1.70793f, 0.73387f, 2.40269f);
            BoundingVolumeTraslation[7] = new Vector3(-0.00000f, 2.97638f, -0.35596f);

            BoundingVolumeScale = new Vector3[8];
            BoundingVolumeScale[0] = new Vector3(2.54742f, 1.00186f, 2.54742f);
            BoundingVolumeScale[1] = new Vector3(0.73622f, 0.73622f, 3.09886f);
            BoundingVolumeScale[2] = new Vector3(1.22763f, 1.05816f, 1.22763f);
            BoundingVolumeScale[3] = new Vector3(0.85413f, 0.84755f, 0.85413f);
            BoundingVolumeScale[4] = new Vector3(0.73622f, 0.73622f, 3.09886f);
            BoundingVolumeScale[5] = new Vector3(1.22763f, 1.05816f, 1.22763f);
            BoundingVolumeScale[6] = new Vector3(0.85413f, 0.84755f, 0.85413f);
            BoundingVolumeScale[7] = new Vector3(1.47766f, 0.65884f, 1.47766f);


            BoundingVolumes = new OrientedBoundingBox[8];
            for (int i = 0; i < 8; i++)
            {
                BoundingVolumes[i] = new OrientedBoundingBox(BoundingVolumeTraslation[i], BoundingVolumeScale[i]);
            }

        }

        /// <summary>
        ///     Loads the tank model.
        /// </summary>
        public void Load(ContentManager Content, Model model)
        {
            tankModel = model;
            LoadBoundingVolumes();
            LoadTextures(Content);
            LoadMaterials();

            // Look up shortcut references to the bones we are going to animate.
            leftBackWheelBone = tankModel.Bones["l_back_wheel_geo"];
            rightBackWheelBone = tankModel.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = tankModel.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = tankModel.Bones["r_front_wheel_geo"];
            leftSteerBone = tankModel.Bones["l_steer_geo"];
            rightSteerBone = tankModel.Bones["r_steer_geo"];
            turretBone = tankModel.Bones["turret_geo"];
            cannonBone = tankModel.Bones["canon_geo"];

            // Store the original transform matrix for each animating bone.
            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
            leftSteerTransform = leftSteerBone.Transform;
            rightSteerTransform = rightSteerBone.Transform;
            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;

            // Allocate the transform matrix array.
            boneTransforms = new Matrix[tankModel.Bones.Count];


        }

        public void LoadMaterials()
        {
            Materials = new Material[tankModel.Meshes.Count];
            Material brass = new Brass();
            Material chrome = new Chrome();
            Materials[0] = brass;
            Materials[1] = brass;
            Materials[2] = chrome;
            Materials[3] = brass;
            Materials[4] = chrome;
            Materials[5] = brass;
            Materials[6] = chrome;
            Materials[7] = brass;
            Materials[8] = chrome;
            Materials[9] = brass;
            Materials[10] = brass;
            Materials[11] = brass;
        }

        public void LoadTextures(ContentManager Content)
        {
            Texture engine = Content.Load<Texture2D>("Models/tank/engine_diff_tex");
            Texture turret = Content.Load<Texture2D>("Models/tank/turret_alt_diff_tex");
            Textures = new Texture[tankModel.Meshes.Count];
            Textures[0] = turret;
            Textures[1] = engine;
            Textures[2] = engine;
            Textures[3] = engine;
            Textures[4] = engine;
            Textures[5] = engine;
            Textures[6] = engine;
            Textures[7] = engine;
            Textures[8] = engine;
            Textures[9] = turret;
            Textures[10] = turret;
            Textures[11] = turret;
        }

        public void Update(float elapsedTime)
        {
            recharging = MathHelper.Clamp(recharging - elapsedTime, 0f, CannonCooldown);

            // Transformaciones
            // Set the world matrix as the root transform of the model.
            tankModel.Root.Transform = World;

            // Calculate matrices based on the current animation position.
            Matrix leftBackWheelRotation = Matrix.CreateRotationX(WheelRotation);
            Matrix rightBackWheelRotation = Matrix.CreateRotationX(WheelRotation);
            Matrix leftFrontWheelRotation = Matrix.CreateRotationX(WheelRotation * FrontWheelRotation);
            Matrix rightFrontWheelRotation = Matrix.CreateRotationX(WheelRotation * FrontWheelRotation);
            Matrix steerRotation = Matrix.CreateRotationY(SteerRotation);
            Matrix turretRotation = Matrix.CreateRotationY(TurretRotation);
            Matrix cannonRotation = Matrix.CreateRotationX(CannonRotation);

            // Apply matrices to the relevant bones.
            leftBackWheelBone.Transform = leftBackWheelRotation * leftBackWheelTransform;
            rightBackWheelBone.Transform = rightBackWheelRotation * rightBackWheelTransform;
            leftFrontWheelBone.Transform = leftFrontWheelRotation * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = rightFrontWheelRotation * rightFrontWheelTransform;
            leftSteerBone.Transform = steerRotation * leftSteerTransform;
            rightSteerBone.Transform = steerRotation * rightSteerTransform;
            turretBone.Transform = turretRotation * turretTransform;
            cannonBone.Transform = cannonRotation * cannonTransform;

            // Look up combined bone matrices for the entire model.
            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Update bounding volumes
            Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(Yaw + MathHelper.Pi, Pitch, Roll);
            for (int i = 0; i < 8; i++)
            {
                Matrix worldMatrix = Matrix.CreateTranslation(BoundingVolumeTraslation[i]) * rotationMatrix * Matrix.CreateTranslation(Position);
                BoundingVolumes[i].Center = worldMatrix.Translation;
            }
            BoundingVolumes[0].Orientation = rotationMatrix;
            BoundingVolumes[1].Orientation = rotationMatrix;
            BoundingVolumes[2].Orientation = rightBackWheelRotation * rotationMatrix;
            BoundingVolumes[3].Orientation = rightFrontWheelRotation * steerRotation * rotationMatrix;
            BoundingVolumes[4].Orientation = rotationMatrix;
            BoundingVolumes[5].Orientation = leftBackWheelRotation * rotationMatrix;
            BoundingVolumes[6].Orientation = leftFrontWheelRotation * steerRotation * rotationMatrix;
            BoundingVolumes[7].Orientation = turretRotation * rotationMatrix;


            // Trasformaciones para proyectiles
            ShootingPosition = Matrix.CreateTranslation(0.00851f, 0.38970f, 1.45659f) * turretRotation * Matrix.CreateTranslation(BoundingVolumeTraslation[7]) * rotationMatrix * Matrix.CreateTranslation(Position);
            ShootingDirection = cannonRotation * turretRotation * rotationMatrix;

        }

        /// <summary>
        ///     Draws the tank model, using the current animation settings.
        /// </summary>
        public void Draw(Matrix view, Matrix projection)
        {
            // Draw the model
            for (int i = 0; i < tankModel.Meshes.Count; i++)
            {
                foreach (var part in tankModel.Meshes[i].MeshParts)
                {
                    part.Effect = DefaultEffect;
                }

                Matrix relativeTransform = boneTransforms[tankModel.Meshes[i].ParentBone.Index];
                DefaultEffect.Parameters["World"].SetValue(relativeTransform);
                DefaultEffect.Parameters["WorldViewProjection"].SetValue(relativeTransform * view * projection);
                DefaultEffect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(relativeTransform)));
                DefaultEffect.Parameters["baseTexture"].SetValue(Textures[i]);
                DefaultEffect.Parameters["ambientColor"].SetValue(Materials[i].AmbientColor);
                DefaultEffect.Parameters["diffuseColor"].SetValue(Materials[i].DiffuseColor);
                DefaultEffect.Parameters["specularColor"].SetValue(Materials[i].SpecularColor);
                DefaultEffect.Parameters["KAmbient"].SetValue(Materials[i].KAmbient);
                DefaultEffect.Parameters["KDiffuse"].SetValue(Materials[i].KDiffuse);
                DefaultEffect.Parameters["KSpecular"].SetValue(Materials[i].KSpecular);
                DefaultEffect.Parameters["shininess"].SetValue(Materials[i].Shininess);
                tankModel.Meshes[i].Draw();
            }
        }
        public void Draw(Matrix view, Matrix projection, Effect effect)
        {
            // Draw the model
            for (int i = 0; i < tankModel.Meshes.Count; i++)
            {
                foreach (var part in tankModel.Meshes[i].MeshParts)
                {
                    part.Effect = effect;
                }

                Matrix relativeTransform = boneTransforms[tankModel.Meshes[i].ParentBone.Index];
                effect.Parameters["WorldViewProjection"].SetValue(relativeTransform * view * projection);
                tankModel.Meshes[i].Draw();
            }
        }

        public void DrawBoundingBox(Gizmos.Gizmos gizmos)
        {
            foreach (OrientedBoundingBox obb in BoundingVolumes)
            {
                gizmos.DrawCube(Matrix.CreateScale(obb.Extents * 2) * obb.Orientation * Matrix.CreateTranslation(obb.Center), Color.Yellow);
            }
        }

        public bool Intersects(BoundingBox box)
        {
            for (int i = 0; i < 8; i++)
            {
                if (BoundingVolumes[i].Intersects(box))
                {
                    return true;
                }
            }
            return false;
        }

        public void Shoot(Bullet[] bullets, int bulletCount)
        {
            if (recharging == 0f)
            {
                // busco un proyectil libre
                Bullet b = null;
                for (int i = 0; i < bulletCount; i++)
                {
                    if (!bullets[i].Active)
                    {
                        b = bullets[i];
                        break;
                    }

                }
                // si encuentro uno, seteo valores
                if (b != null)
                {
                    b.ResetValues(ShootingPosition.Translation + ShootingDirection.Backward, ShootingDirection.Backward * 100);
                    recharging = CannonCooldown;
                }
            }
        }

    }
}
