namespace Assets.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using ModApi;
    using ModApi.Craft.Propulsion;
    using ModApi.Settings.Core;
    using ModApi.Settings.Core.Events;
    using UnityEngine;

    public class ModSettings : SettingsCategory<ModSettings>
    {
        public ModSettings() : base("Ember")
        {
        }

        private static ModSettings _instance;

        private static XmlSerializer _xmlSerializer = new(typeof(PlumeData));

        private static FileStream _fileStream;

        private List<RocketEngineType> propulsionEngines = new();

        public static ModSettings Instance => _instance ??= Game.Instance.Settings.ModSettings.GetCategory<ModSettings>();
        public ButtonSetting GeneratePresets { get; private set; }

        public BoolSetting PreviewExhaustStaging { get; private set; }
        protected override void InitializeSettings()
        {
            GeneratePresets = CreateButton("Stock Plume Presets", "Generate");
            GeneratePresets.Changed += new EventHandler<SettingChangedEventArgs<int>>(OnGeneratePlumePresetClicked);

            PreviewExhaustStaging = CreateBool("Preview Staging Exhaust")
                .SetDescription("While in the staging editor, enables the exhaust of engines in the selected stage as set by the Design Info.")
                .SetDefault(true);
        }

        private void OnGeneratePlumePresetClicked(object sender, SettingChangedEventArgs<int> e)
        {
            propulsionEngines = (List<RocketEngineType>)Game.Instance.PropulsionData.RocketEngines;
            for (int engineIndex = 0; engineIndex < propulsionEngines.Count; engineIndex++)
            {
                List<FuelType> tempFuels = propulsionEngines[engineIndex].SupportedFuels;
                for (int i = 0; i < tempFuels.Count; i++) GeneratePlumePresets(tempFuels[i]);
            }
        }

        public static void GeneratePlumePresets(FuelType fuelType)
        {
            PlumeData plumeData = new()
            {
                PresetName = Utilities.ScrubFileName(fuelType.Name),
                Author = fuelType.Mod != null ? fuelType.Mod.ModInfo.Author : "Jundroo"
        };
            plumeData.plumeMain = new PlumeMain()
            {
                MainColor = ColorToHexAlpha(fuelType.ExhaustColor),
                TipColor = ColorToHexAlpha(fuelType.ExhaustColorTip),
                FlameColor = ColorToHexAlpha(fuelType.ExhaustColorFlame),
                GloabalIntensity = fuelType.GlobalIntensity,
                TextureIntensity = 1,
                ExhaustScale = 1,
                ExhaustOffset = 0,
                RimShade = fuelType.RimShade,
                DiscIntensity = 5,
                ExpansionRangeX = -1,
                ExpansionRangeY = -1
            };
            plumeData.plumeDiamonds = new PlumeDiamonds()
            {
                DiamondColor = ColorToHexAlpha(fuelType.ExhaustColorShock),
                DiamondOffset = -1,
                DiamondIntensity = fuelType.ShockIntensity
            };
            plumeData.plumeSoot = new PlumeSoot()
            {
                SootColor = ColorToHexAlpha(fuelType.ExhaustColorSoot),
                SootIntensity = 0,
                SootLength = 0
            };
            plumeData.engineSmoke = new EngineSmoke()
            {
                HasSmoke = true,
                SmokeColor = ColorToHexAlpha(fuelType.ExhaustColorSmoke),
                SmokeSpeed = 1,
            };
            _fileStream = new FileStream(Mod.Instance.presetPath + (Utilities.ScrubFileName(fuelType.Name) + ".xml"), FileMode.Create);
            _xmlSerializer.Serialize(_fileStream, plumeData);
            _fileStream.Close();
        }

        internal static string ColorToHexAlpha(Color32 color) => "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");
    }
}