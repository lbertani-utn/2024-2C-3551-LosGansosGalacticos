namespace TGC.MonoGame.TP {
    public delegate void BindingAction(ref float rot, ref float dir, float time);
    
    public static class BindingLogic {
        public static void PositiveRotation(ref float rot, ref float dir, float time) {
            rot += time;
        }
    
        public static void NegativeRotation(ref float rot, ref float dir, float time) {
            rot -= time;
        }

        public static void PositiveDirection(ref float rot, ref float dir, float time) {
            dir += time;
        }
    
        public static void NegativeDirection(ref float rot, ref float dir, float time) {
            dir -= time;
        }
    }
}
