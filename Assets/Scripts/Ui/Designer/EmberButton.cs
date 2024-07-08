using System;
using System.Linq;
using System.Xml.Linq;
using Assets.Scripts.Design;
using ModApi.Ui;
using UnityEngine;
using HarmonyLib;
using UI.Xml;
using Assets.Scripts;
using Assets.Scripts.Craft.Parts.Modifiers.Propulsion;
using System.Collections.Generic;
using Assets.Scripts.Design.Staging;
using ModApi.Craft;
using static Assets.Scripts.Design.PreflightPanelScript;
using ModApi.Common.Events;

public static class EmberDesignerButton
{
    private static XmlElement emberButton;

    public static EmberFlyoutScriptOld emberFlyout;
    private static DesignerScript _designer => (DesignerScript)Game.Instance.Designer;
    public static bool FlyoutOpened => emberFlyout != null;

    public static List<RocketEngineScript> stageRocketEngines = new();

    public static List<JetEngineScript> stageJetEngines = new();

    internal static List<TabButton> tabButtonList = new();

    public static void Initialize()
    {
        return;
        var userInterface = Game.Instance.UserInterface;
        userInterface.AddBuildUserInterfaceXmlAction(
            UserInterfaceIds.Design.DesignerUi,
            OnBuildDesignerUI);
    }

    public static void UpdateAltitudeSliderDisplay()
    {
        if (FlyoutOpened) emberFlyout.UpdateAltitudeSliderDisplay();
    }

    public static void OnCorrectionAltitudeSliderChanged(float value)
    {
        if (FlyoutOpened) emberFlyout.OnCorrectionAltitudeSliderChanged(value);
    }

    public static void SymmetrySliceUpdated()
    {
        if (FlyoutOpened) emberFlyout.SymmetrySliceUpdated();
    }

    private const string _buttonId = "ember-flyout-button";

    private static void OnBuildDesignerUI(BuildUserInterfaceXmlRequest request)
    {
        /*
        _designer.DesignerUi.SelectedFlyoutChanged += SelectedFlyoutChanged;

        var nameSpace = XmlLayoutConstants.XmlNamespace;
        var viewButton = request.XmlDocument
            .Descendants(nameSpace + "Panel")
            .First(x => (string)x.Attribute("internalId") == "flyout-view");

        viewButton.Parent.Add(
    new XElement(
        nameSpace + "Panel",
        new XAttribute("id", _buttonId),
        new XAttribute("class", "toggle-button audio-btn-click"),
        new XAttribute("name", "ButtonPanel.EmberDesignerButton"),
        new XAttribute("tooltip", "Ember Plume Editor"),
        new XElement(
            nameSpace + "Image",
            new XAttribute("class", "toggle-button-icon"),
            new XAttribute("sprite", "Advanced Plume Editor/Sprites/EmberUIFlame"))));
        request.AddOnLayoutRebuiltAction(xmlLayoutController =>
        {
            emberButton = (XmlElement)xmlLayoutController.XmlLayout.GetElementById(_buttonId);
            emberButton.AddOnClickEvent(OnButtonClicked);
        });
        */
    }

    private static void SelectedFlyoutChanged(IFlyout flyout)
    {
        if (flyout != null)
        {
            if (FlyoutOpened && flyout.Title != "Ember") CloseFlyout(flyout.Title == "Part Properties"); // Handle plume change when going to part properties
            if (flyout.Title == "Ember" || flyout.Title == "Part Properties")
            {
                UpdateStageExhaustPreview(false);
            }
            if (Game.Instance.Designer.SelectedPart != null && flyout.Title == "Part Properties")
            {
                RocketEngineScript rocketEngine = Game.Instance.Designer.SelectedPart.GetModifier<RocketEngineScript>();
                if (rocketEngine != null)
                {
                    if (Game.Instance.Designer.SelectedPart.SymmetrySlice != null)
                    {
                        Symmetry.ExecuteOnSymmetricPartModifiers(rocketEngine.Data, true, delegate (RocketEngineData data)
                        {
                            data.Script.PreviewExhaust = false;
                        });
                        rocketEngine.PreviewExhaust = true;
                    }
                    rocketEngine.PreviewExhaust = true;
                }
            }
            if (flyout.Title != "Ember" && flyout.Title != "Part Properties") UpdateStageExhaustPreview(false);
        }
        else UpdateStageExhaustPreview(false);
    }

    private static void OnButtonClicked()
    {
        if (FlyoutOpened)
        {
            CloseFlyout();
        }
        else OpenFlyout();
    }

    public static void OpenFlyout()
    {
        var ui = Game.Instance.UserInterface;
        emberFlyout = ui.BuildUserInterfaceFromResource<EmberFlyoutScriptOld>("Advanced Plume Editor/EmberFlyout", (script, controller) => script.OnLayoutRebuilt(controller));
        emberButton.AddClass("toggle-button-toggled");
        _designer.DesignerUi.SelectedFlyout = emberFlyout.flyout;
        emberFlyout.Open();
    }

    public static void CloseFlyout(bool partPropertiesFlyout = false)
    {
        emberFlyout.Close(partPropertiesFlyout);
        if (_designer.DesignerUi.SelectedFlyout == (IFlyout)emberFlyout.flyout)
        _designer.DesignerUi.SelectedFlyout = null;
        emberFlyout = null;
        emberButton.RemoveClass("toggle-button-toggled");
    }

    internal static void StageExhaustPreview()
    {
        if (ModSettings.Instance.PreviewExhaustStaging.Value)
        {
            IFlyout flyout = _designer.DesignerUi.SelectedFlyout;
            if (flyout != null)
            {
                if (flyout.Title != "Ember" && flyout.Title != "Part Properties")
                {
                    tabButtonList = new List<TabButton>((_designer.DesignerUi.Flyouts.Preflight.Transform).GetComponentInChildren<PreflightPanelScript>().TabButtons);
                    UpdateStageExhaustPreview(false);
                    if (tabButtonList[0].Panel.IsOpen)
                    {
                        UpdateStageEngineList();
                        UnityEventDispatcher.Instance.ExecuteYield<WaitForEndOfFrame>(() => UpdateStageExhaustPreview(true));
                    }
                }
            }
        }
    }

    public static void UpdateStageExhaustPreview(bool state)
    {
        for (int engine = 0; engine < stageRocketEngines.Count; engine++)
        {
            stageRocketEngines[engine].PreviewExhaust = state;
        }
        for (int engine = 0; engine < stageJetEngines.Count; engine++)
        {
            stageJetEngines[engine].PreviewExhaust = state;
        }
    }

    public static void UpdateStageEngineList()
    {
        stageRocketEngines = new();
        stageJetEngines = new();
        int selectedStage = Traverse.Create(Game.Instance.Designer.PerformanceAnalysis).Field("_selectedStage").GetValue<int>();
        StagingData craftStages = new StageCalculator(Game.Instance.Designer.CraftScript.PrimaryCommandPod).GetStages();

        if (selectedStage >= 0)
        {
            selectedStage = (Game.Instance.Designer.PerformanceAnalysis.StageAnalysis.Stages[selectedStage].StageNumber - 1);
            for (int i = 0; i < craftStages.Stages[selectedStage].Parts.Count; i++)
            {
                if (craftStages.Stages[selectedStage].Parts[i].PartScript.GetModifier<RocketEngineScript>() != null) stageRocketEngines.Add(craftStages.Stages[selectedStage].Parts[i].PartScript.GetModifier<RocketEngineScript>());
                if (craftStages.Stages[selectedStage].Parts[i].PartScript.GetModifier<JetEngineScript>() != null) stageJetEngines.Add(craftStages.Stages[selectedStage].Parts[i].PartScript.GetModifier<JetEngineScript>());
            }
        }
        else
        {
            for (int stage = 0; stage < craftStages.Stages.Count; stage++)
            {
                for (int i = 0; i < craftStages.Stages[stage].Parts.Count; i++)
                {
                    if (craftStages.Stages[stage].Parts[i].PartScript.GetModifier<RocketEngineScript>() != null) stageRocketEngines.Add(craftStages.Stages[stage].Parts[i].PartScript.GetModifier<RocketEngineScript>());
                    if (craftStages.Stages[stage].Parts[i].PartScript.GetModifier<JetEngineScript>() != null) stageJetEngines.Add(craftStages.Stages[stage].Parts[i].PartScript.GetModifier<JetEngineScript>());
                }
            }
        }
    }
}