using HarmonyLib;

namespace Assets.Scripts
{
    public class HarmonyLoader
    {
        internal static void LoadHarmony() => new Harmony("com.chaoticGraviton.ember").PatchAll();
    }
}