using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TGC.MonoGame.TP.UI
{
    class GameOptions
    {
        public bool GodMode { get; set; }
        public bool SoundEffects { get; set; }
        public bool Music { get; set; }

        private float volume;
        public float Volume
        {
            get => volume;
            set => volume = MathHelper.Clamp(value, 0f, 1f);
        }

        public GameOptions()
        {
            this.GodMode = false;
            this.SoundEffects = true;
            this.Music = true;
            this.volume = 0.8f;

        }

    }
}
