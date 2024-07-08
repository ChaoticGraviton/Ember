using Assets.Scripts.Design;
using System.Xml.Serialization;
using UnityEngine;
using ModApi.Ui;
using System.Xml.Linq;
using System.Linq;
using HarmonyLib;
using Assets.Scripts.Design.Staging;

namespace Assets.Scripts
{
    public class Mod : ModApi.Mods.GameMod
    {
        private Mod() : base() { }
        public IFlyout EmberFlyout;
        public bool EmberActive => EmberFlyout.IsOpen;
        public EmberFlyoutPanelScript EmberFlyoutScript;
        public StagingEditorExhaustPreview StagingEditorExhaustPreviewScript;
        public string presetPath;
        public static Mod Instance { get; } = GetModInstance<Mod>();

        protected override void OnModInitialized()
        {
            new Harmony("Ember").PatchAll();
            presetPath = Application.persistentDataPath + "/UserData/Ember/Presets/";
            System.IO.Directory.CreateDirectory(presetPath);
            Game.Instance.UserInterface.AddBuildUserInterfaceXmlAction(UserInterfaceIds.Design.DesignerUi, OnBuildDesignUi);
        }

        private void OnBuildDesignUi(BuildUserInterfaceXmlRequest request)
        {
            XNamespace ns = XmlLayoutConstants.XmlNamespace;
            XElement mainPanel = request.XmlDocument
                .Descendants(ns + "Panel")
                .First(x => (string)x.Attribute("id") == "main-panel");

            mainPanel.Add(
                new XElement(ns + "Panel",
                    new XAttribute("id", "flyout-ember"),
                    new XAttribute("class", "panel flyout"),
                    new XAttribute("width", "280"),
                    new XAttribute("active", "false"),
                        new XElement(ns + "Panel",
                            new XAttribute("class", "flyout-header"),
                                new XElement(ns + "TextMeshPro",
                                    new XAttribute("text", "Ember")),
                                new XElement(ns + "Image",
                                    new XAttribute("class", "flyout-close-button audio-btn-back"))),
                        new XElement(ns + "Panel",
                            new XAttribute("class", "flyout-content no-image"),
                                new XElement(ns + "ChildXmlLayout",
                                    new XAttribute("viewPath", "Advanced Plume Editor/Xml/Design/EmberPanel"),
                                    new XAttribute("controller", "Assets.Scripts.Design.EmberFlyoutPanelScript")))));

            XElement flyoutMenu = request.XmlDocument
                .Descendants(ns + "Panel")
                .First(x => (string)x.Attribute("internalId") == "flyout-menu");

            flyoutMenu.Parent.Add(
                new XElement(ns + "Panel",
                    new XAttribute("internalId", "flyout-ember"),
                    new XAttribute("class", "toggle-button toggle-flyout audio-btn-click"),
                    new XAttribute("name", "ButtonPanel.Ember"),
                    new XAttribute("tooltip", "Ember"),
                    new XAttribute("OnClick", "OnToggleFlyoutButtonClicked(this);"),
                        new XElement(ns + "Image",
                            new XAttribute("class", "toggle-button-icon"),
                            new XAttribute("sprite", "Advanced Plume Editor/Sprites/EmberUIFlame"))));
        }
    }
}