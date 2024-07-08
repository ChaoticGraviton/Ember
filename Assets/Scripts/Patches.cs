using Assets.Scripts.Design;
using Assets.Scripts;
using ModApi.Ui;
using System.Collections.Generic;
using Assets.Scripts.Craft.Parts.Modifiers.Propulsion;
using Assets.Scripts.Design.Staging;
using ModApi.Ui.Inspector;
using UnityEngine.UI;
using UnityEngine;

namespace HarmonyLib
{
    [HarmonyPatch(typeof(DesignerScript))]
    public class DesignerScriptPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPostfix(DesignerScript __instance) => __instance._cycleFlyouts.Add(Mod.Instance.EmberFlyout);
    }

    [HarmonyPatch(typeof(ExhaustSystemScript))]
    class AdjustableThrottle
    {
        public static Slider adjustableThrottleSlider;

        [HarmonyPatch("UpdateExhaust")]
        [HarmonyPrefix]
        static bool Prefix(ref float throttle)
        {
            if (Game.InDesignerScene && adjustableThrottleSlider != null)
                throttle = adjustableThrottleSlider.value;
            return true;
        }
    }

    [HarmonyPatch(typeof(StagingEditorPanelScript))]
    class StagingEditorPanelScriptPatch
    {
        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        public static void InitializePostfix(StagingEditorPanelScript __instance, DesignerUiScript designerUi) => new StagingEditorExhaustPreview(__instance);

        [HarmonyPatch("OnOpened")]
        [HarmonyPostfix]
        public static void OnOpenedPostfix(StagingEditorPanelScript __instance) => Mod.Instance.StagingEditorExhaustPreviewScript.OnOpened();

        [HarmonyPatch("OnClosed")]
        [HarmonyPostfix]
        public static void OnClosedPostfix(StagingEditorPanelScript __instance) => Mod.Instance.StagingEditorExhaustPreviewScript.OnClosed();
    }

    [HarmonyPatch(typeof(CraftPerformanceAnalysis))]
    public class CraftPerformanceAnalysisPatch
    {
        static public SliderModel altitudeSlider;
        private static void sliderChanged(ItemModel model, string name, bool finished) => Mod.Instance.EmberFlyoutScript.OnCorrectionAltitudeSliderChanged(altitudeSlider.Value);

        [HarmonyPatch("CreateEnvironmentGroup")]
        [HarmonyPostfix]
        public static void CreateEnvironmentGroupPostfix(CraftPerformanceAnalysis __instance)
        {
            altitudeSlider = __instance._altitudeSlider;
            altitudeSlider.ValueChangedByUserInput += sliderChanged;
        }

        [HarmonyPatch("SelectNextEnvironment")]
        [HarmonyPostfix]
        public static void SelectNextEnvironmentPostfix(CraftPerformanceAnalysis __instance) => Mod.Instance.EmberFlyoutScript.UpdateAltitudeSliderDisplay();

        [HarmonyPatch("OnStagingChanged")]
        [HarmonyPostfix]
        public static void OnStagingChangedPostfix(CraftPerformanceAnalysis __instance) => Mod.Instance.StagingEditorExhaustPreviewScript.RefreshExhaustPreview();

        [HarmonyPatch("AdvanceStage")]
        [HarmonyPostfix]
        public static void AdvanceStagePostfix(CraftPerformanceAnalysis __instance) => Mod.Instance.StagingEditorExhaustPreviewScript.RefreshExhaustPreview();
    }

    [HarmonyPatch(typeof(Symmetry), "SetSymmetryMode")]
    class SymmetrySliceUpdate
    {
        static void Postfix(Symmetry __instance) => Mod.Instance.EmberFlyoutScript.SymmetrySliceUpdated();
    }
}