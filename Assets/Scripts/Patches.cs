using Assets.Scripts.Design;
using Assets.Scripts;
using ModApi.Ui;
using System.Collections.Generic;

namespace HarmonyLib
{
    [HarmonyPatch(typeof(DesignerScript))]
    public class DesignerScriptPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPostfix(DesignerScript __instance)
        {
            Traverse.Create(__instance).Field("_cycleFlyouts").GetValue<List<IFlyout>>().Add(Mod.Instance.EmberFlyout);
        }
    }
}