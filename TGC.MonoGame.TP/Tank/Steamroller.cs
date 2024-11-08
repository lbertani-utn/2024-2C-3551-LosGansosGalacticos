using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TGC.MonoGame.TP.Tank
{
    internal class Steamroller : Tank
    {

        protected Vector3[] DiffuseColors;
        private const float PiOver6 =  0.52356f;
        private const float PiOver12 = 0.26180f;

        public Vector3 position { get; set; }
        public Vector3 scale { get; set; }
        public Quaternion rotation { get; set; }

        private const float FrontWheelRotation = 1.6f;
        private float cannonCooldown = 0f;

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
            set => _turretRotation = MathHelper.Clamp(value, -MathHelper.PiOver4, MathHelper.PiOver4);
        }
        private float _cannonRotation = -PiOver12;
        public float CannonRotation
        {
            get => _cannonRotation;
            set => _cannonRotation = MathHelper.Clamp(value, -PiOver6, 0f);
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

        /// <summary>
        ///     Loads the tank model.
        /// </summary>
        public void Load(Model model)
        {
            tankModel = model;

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

            // default colors
            Vector3 hullColor = new Vector3(0.3f, 0.3f, 0.3f);
            Vector3 engineColor = new Vector3(0.4f, 0.4f, 0.4f);
            Vector3 steerColor = new Vector3(0.2f, 0.2f, 0.2f);
            Vector3 wheelColor = new Vector3(0.1f, 0.1f, 0.1f);
            Vector3 turretColor = new Vector3(0.2f, 0.2f, 0.2f);
            Vector3 cannonColor = new Vector3(0.3f, 0.3f, 0.3f);
            Vector3 hatchColor = new Vector3(0.3f, 0.3f, 0.3f);
            DiffuseColors = new Vector3[tankModel.Meshes.Count];
            DiffuseColors[0] = hullColor;
            DiffuseColors[1] = engineColor;
            DiffuseColors[2] = wheelColor;
            DiffuseColors[3] = steerColor;
            DiffuseColors[4] = wheelColor;
            DiffuseColors[5] = engineColor;
            DiffuseColors[6] = wheelColor;
            DiffuseColors[7] = steerColor;
            DiffuseColors[8] = wheelColor;
            DiffuseColors[9] = turretColor;
            DiffuseColors[10] = cannonColor;
            DiffuseColors[11] = hatchColor;
        }

        public void Update(float elapsedTime)
        {
        }

        /// <summary>
        ///     Draws the tank model, using the current animation settings.
        /// </summary>
        public void Draw(Matrix world, Matrix view, Matrix projection, Effect effect)
        {
            // Set the world matrix as the root transform of the model.
            tankModel.Root.Transform = world;

            // Calculate matrices based on the current animation position.
            Matrix leftBackWheelRotation = Matrix.CreateRotationX(WheelRotation);
            Matrix rightBackWheelRotation = Matrix.CreateRotationX(WheelRotation);
            Matrix leftFrontWheelRotation = Matrix.CreateRotationX(WheelRotation) * FrontWheelRotation;
            Matrix rightFrontWheelRotation = Matrix.CreateRotationX(WheelRotation) * FrontWheelRotation;
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

            // Draw the model
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);

            foreach (var mesh in tankModel.Meshes)
            {
                effect.Parameters["World"].SetValue(boneTransforms[mesh.ParentBone.Index]);
                effect.Parameters["DiffuseColor"].SetValue(DiffuseColors[mesh.ParentBone.Index]);
                mesh.Draw();
            }

        }


    }



}
