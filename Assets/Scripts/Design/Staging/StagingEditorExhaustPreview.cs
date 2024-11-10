using Assets.Scripts.Craft.Parts.Modifiers.Propulsion;
using HarmonyLib;
using ModApi.Craft;
using ModApi.Craft.Parts;
using System.Collections.Generic;

namespace Assets.Scripts.Design.Staging
{
    public class StagingEditorExhaustPreview
    {
        StagingEditorPanelScript StagingEditorPanelScript { get; set; }
        public static List<RocketEngineScript> stageRocketEngines = new();
        public static List<JetEngineScript> stageJetEngines = new();

        public StagingEditorExhaustPreview(StagingEditorPanelScript stagingEditorScript)
        {
            StagingEditorPanelScript = stagingEditorScript;
            Mod.Instance.StagingEditorExhaustPreviewScript = this;
        }

        public void OnOpened()
        {
            Game.Instance.Designer.CraftStructureChanged += OnCraftStructureChanged;
            RefreshExhaustPreview();
        }

        public void OnClosed()
        {
            Game.Instance.Designer.CraftStructureChanged -= OnCraftStructureChanged;
            UpdateStageExhaustPreview(false);
        }

        private void OnCraftStructureChanged()
        {
            RefreshExhaustPreview();
        }

        internal void RefreshExhaustPreview()
        {
            if (ModSettings.Instance.PreviewExhaustStaging.Value)
            {
                UpdateStageExhaustPreview(false);
                if (StagingEditorPanelScript.IsOpen)
                {
                    UpdateStageEngineList();
                    UpdateStageExhaustPreview(true);
                }
            }
        }

        private void UpdateStageExhaustPreview(bool previewState)
        {
            foreach (RocketEngineScript engine in stageRocketEngines)
                engine.PreviewExhaust = previewState;
            foreach (JetEngineScript engine in stageJetEngines)
                engine.PreviewExhaust = previewState;
        }

        private void UpdateStageEngineList()
        {
            stageRocketEngines = new();
            stageJetEngines = new();
            int selectedStage = Traverse.Create(Game.Instance.Designer.PerformanceAnalysis).Field("_selectedStage").GetValue<int>();
            StagingData craftStages = new StageCalculator(Game.Instance.Designer.CraftScript.PrimaryCommandPod).GetStages();

            if (selectedStage >= 0)
                PopulateEngineLists(Game.Instance.Designer.PerformanceAnalysis.StageAnalysis.Stages[selectedStage].StageNumber - 1);
            else
                for (int stage = 0; stage < craftStages.Stages.Count; stage++)
                    PopulateEngineLists(stage);

            void PopulateEngineLists(int selectedStage)
            {
                foreach (PartData enginePart in craftStages.Stages[selectedStage].Parts)
                {
                    if (enginePart.PartScript.GetModifier<RocketEngineScript>() != null)
                        stageRocketEngines.Add(enginePart.PartScript.GetModifier<RocketEngineScript>());
                    else if (enginePart.PartScript.GetModifier<JetEngineScript>() != null)
                        stageJetEngines.Add(enginePart.PartScript.GetModifier<JetEngineScript>());
                }
            }
        }
    }
}