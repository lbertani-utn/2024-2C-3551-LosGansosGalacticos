using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TGC.MonoGame.TP.UI
{
    class Message
    {
        // título
        public const String Title = "Tanques";

        // menú principal
        public const String Play = "Jugar";
        public const String Options = "Opciones";
        public const String Exit = "Salir";

        // menú pausa
        public const String Resume = "Continuar";
        public const String Restart = "Reiniciar";

        // opciones
        public const String GodMode = "Invencible";
        public const String Volume = "Volumen";
        public const String Sound = "Efectos de sonido";
        public const String Music = "Musica"; // Música con tilde da error: Text contains characters that cannot be resolved by this SpriteFont.
        public const String Return = "Volver";

        public const String Left = "<";
        public const String Right = ">";
        public const String Yes = "Si";
        public const String No = "No";

        public const char Bar = '|';
        public const String FullBar = "||||||||||||||||||||";

    }
}
