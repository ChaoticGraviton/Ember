namespace Assets.Scripts
{
    using Assets.Scripts.Design;
    using Assets.Scripts.Design.Tools;
    using HarmonyLib; // Including the Harmony Library
    using ModApi.Scenes.Events;
    using System.Xml.Serialization;
    using UnityEngine;
    using System.Collections.Generic;

    public class Mod : ModApi.Mods.GameMod
    {
        private Mod() : base()
        {
        }

        public static bool isUserApproved;

        public static bool whiteListEnabled = false;

        private List<string> approvedUsers = new() { "ChaoticGraviton" };

        public string presetPath;
        public DesignerScript designer => (DesignerScript)Game.Instance.Designer;

        public static Mod Instance { get; } = GetModInstance<Mod>();

        protected override void OnModInitialized()
        {
            Game.Instance.SceneManager.SceneLoaded += OnSceneLoaded;
            EmberDesignerButton.Initialize();
            Harmony harmony = new Harmony("Ember");
            harmony.PatchAll();
            presetPath = Application.persistentDataPath + "/UserData/Ember/Presets/";
            System.IO.Directory.CreateDirectory(presetPath);

            if (whiteListEnabled)
            {
                if (string.IsNullOrEmpty(Game.Instance.Settings.UserName)) isUserApproved = false;
                else
                {
                    for (int item = 0; item < approvedUsers.Count; item++)
                    {
                        isUserApproved = approvedUsers[item] == Game.Instance.Settings.UserName;
                        if (isUserApproved)
                        {
                            Debug.Log("User Approved");
                            break;
                        }
                    }
                }
            }
            else isUserApproved = true;
        }

        public void OnSceneLoaded(object sender, SceneEventArgs e)
        {
            if (e.Scene == ModApi.Scenes.SceneNames.Designer)
            {
                ((MovePartTool)designer.MovePartTool).DragPartSelectionStarted += OnDragPartSelectionStarted;
                ((MovePartTool)designer.MovePartTool).DragPartSelectionEnded += OnDragPartSelectionEnded;
            }
        }

        private void OnDragPartSelectionStarted()
        {
            if (EmberDesignerButton.flyoutOpened) EmberDesignerButton.emberFlyout.flyout.IsHidden = true;
        }

        private void OnDragPartSelectionEnded()
        {
            if (EmberDesignerButton.flyoutOpened) EmberDesignerButton.emberFlyout.flyout.IsHidden = false;
        }
    }
    public class PlumeData
    {
        public string path;

        [XmlElement("PlumeMain")]
        public PlumeMain plumeMain;

        [XmlElement("PlumeDiamonds")]
        public PlumeDiamonds plumeDiamonds;

        [XmlElement("PlumeSoot")]
        public PlumeSoot plumeSoot;

        [XmlElement("EngineSmoke")]
        public EngineSmoke engineSmoke;

        [XmlAttribute]
        public string PresetName;

        [XmlAttribute]
        public string Author;
    }

    public class PlumeMain
    {
        [XmlAttribute]
        public string MainColor;
        [XmlAttribute]
        public string TipColor;
        [XmlAttribute]
        public string FlameColor;
        [XmlAttribute]
        public float GloabalIntensity;
        [XmlAttribute]
        public float TextureIntensity;
        [XmlAttribute]
        public float ExhaustScale;
        [XmlAttribute]
        public float ExhaustOffset;
        [XmlAttribute]
        public float RimShade;
        [XmlAttribute]
        public float DiscIntensity;
        [XmlAttribute]
        public float ExpansionRangeX;
        [XmlAttribute]
        public float ExpansionRangeY;
    }

    public class PlumeDiamonds
    {
        [XmlAttribute]
        public string DiamondColor;
        [XmlAttribute]
        public float DiamondOffset;
        [XmlAttribute]
        public float DiamondIntensity;
    }
    public class PlumeSoot
    {
        [XmlAttribute]
        public string SootColor;
        [XmlAttribute]
        public float SootIntensity;
        [XmlAttribute]
        public float SootLength;
    }
    public class EngineSmoke
    {
        [XmlAttribute]
        public bool HasSmoke;
        [XmlAttribute]
        public string SmokeColor;
        [XmlAttribute]
        public float SmokeSpeed;
    }
}