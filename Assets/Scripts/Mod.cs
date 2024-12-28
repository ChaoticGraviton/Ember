using Assets.Scripts.Design;
using UnityEngine;
using ModApi.Ui;
using System.Xml.Linq;
using System.Linq;
using Assets.Scripts.Design.Staging;
using System.IO;
using System;

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
            try
            {
                base.OnModInitialized();
                HarmonyLoader.LoadHarmony();
                presetPath = Application.persistentDataPath + "/UserData/Ember/Presets/";
                Directory.CreateDirectory(presetPath);
                Game.Instance.UserInterface.AddBuildUserInterfaceXmlAction(UserInterfaceIds.Design.DesignerUi, OnBuildDesignUi);
            }
            catch (Exception e)
            {
                string s = $"Mod {Mod.ModInfo.Name} failed to initalize. Verify all depencencies installed and enabled";
                Game.Instance.UserInterface.CreateMessageDialog(s);
                Debug.LogException(e);
                throw new FileNotFoundException(s);
            }
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