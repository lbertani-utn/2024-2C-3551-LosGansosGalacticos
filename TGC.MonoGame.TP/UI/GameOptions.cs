using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP.UI
{
    class GameOptions
    {
        public bool GodMode { get; set; }
        public bool SoundEffects { get; set; }
        public bool Music { get; set; }
        public float Volume { get; set; }

        public GameOptions()
        {
            this.GodMode = false;
            this.SoundEffects = true;
            this.Music = true;
            this.Volume = 5;

        }

    }
}
