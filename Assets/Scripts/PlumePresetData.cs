using Assets.Scripts.Menu.ListView;
using System.Xml.Serialization;
using UnityEngine;

public class PlumePresetData
{
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
        public string ExpandedColor;
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

    public class PresetDetails
    {
        private DetailsTextScript _description;
        private DetailsWidgetGroup _plumeLabel;
        private DetailsWidgetGroup _shockLabel;
        private DetailsWidgetGroup _sootLabel;
        private DetailsWidgetGroup _smokeLabel;
        private DetailsPropertyScript _mainColor;
        private DetailsPropertyScript _expandedColor;
        private DetailsPropertyScript _tipColor;
        private DetailsPropertyScript _flameColor;
        private DetailsPropertyScript _exhaustScale;
        private DetailsPropertyScript _exhaustOffset;
        private DetailsPropertyScript _rimShade;
        private DetailsPropertyScript _diamondColor;
        private DetailsPropertyScript _diamondOffset;
        private DetailsPropertyScript _diamondIntensity;
        private DetailsPropertyScript _sootColor;
        private DetailsPropertyScript _sootIntensity;
        private DetailsPropertyScript _sootLength;
        private DetailsPropertyScript _hasSmoke;
        private DetailsPropertyScript _smokeColor;
        private DetailsPropertyScript _smokeSpeed;
        private DetailsPropertyScript _globalIntenisty;
        private DetailsPropertyScript _textureIntensity;
        private DetailsPropertyScript _discIntensity;
        private DetailsPropertyScript _expansionRange;

        public PresetDetails(ListViewDetailsScript listViewDetails)
        {
            _description = listViewDetails.Widgets.AddText("PLACEHOLDER");
            listViewDetails.Widgets.AddSpacer();
            _plumeLabel = listViewDetails.Widgets.AddGroup();
            _plumeLabel.AddHeader("Main Plume");
            _mainColor = listViewDetails.Widgets.AddProperty("Main Color");
            _expandedColor = listViewDetails.Widgets.AddProperty("Expanded Color");
            _tipColor = listViewDetails.Widgets.AddProperty("Tip Color");
            _flameColor = listViewDetails.Widgets.AddProperty("Flame Color");
            _globalIntenisty = listViewDetails.Widgets.AddProperty("Intensity");
            _textureIntensity = listViewDetails.Widgets.AddProperty("Texture Intensity");
            _exhaustScale = listViewDetails.Widgets.AddProperty("Exhaust Scale");
            _exhaustOffset = listViewDetails.Widgets.AddProperty("Exhaust Offset");
            _rimShade = listViewDetails.Widgets.AddProperty("RimShade");
            _discIntensity = listViewDetails.Widgets.AddProperty("Disc Intensity");
            _expansionRange = listViewDetails.Widgets.AddProperty("Expansion Range");
            listViewDetails.Widgets.AddSpacer();
            _shockLabel = listViewDetails.Widgets.AddGroup();
            _shockLabel.AddHeader("Shock Diamonds");
            _diamondColor = listViewDetails.Widgets.AddProperty("Diamond Color");
            _diamondOffset = listViewDetails.Widgets.AddProperty("Diamond Offset");
            _diamondIntensity = listViewDetails.Widgets.AddProperty("Diamond Intensity");
            listViewDetails.Widgets.AddSpacer();
            _sootLabel = listViewDetails.Widgets.AddGroup();
            _sootLabel.AddHeader("Soot");
            _sootColor = listViewDetails.Widgets.AddProperty("Soot Color");
            _sootIntensity = listViewDetails.Widgets.AddProperty("Soot Intensity");
            _sootLength = listViewDetails.Widgets.AddProperty("Soot Length");
            listViewDetails.Widgets.AddSpacer();
            _smokeLabel = listViewDetails.Widgets.AddGroup();
            _smokeLabel.AddHeader("Smoke");
            _hasSmoke = listViewDetails.Widgets.AddProperty("Has Smoke");
            _smokeColor = listViewDetails.Widgets.AddProperty("Smoke Color");
            _smokeSpeed = listViewDetails.Widgets.AddProperty("Smoke Speed");
        }

        public void UpdateDetails(PlumePresetData.PlumeData plumeData)
        {
            _description.Text = $"Preset file location: {plumeData.path}";
            _mainColor.ValueText = plumeData.plumeMain.MainColor;
            _expandedColor.ValueText = plumeData.plumeMain.ExpandedColor ??= plumeData.plumeMain.MainColor;
            _tipColor.ValueText = plumeData.plumeMain.TipColor;
            _flameColor.ValueText = plumeData.plumeMain.FlameColor;
            _exhaustScale.ValueText = plumeData.plumeMain.ExhaustScale.ToString();
            _exhaustOffset.ValueText = plumeData.plumeMain.ExhaustOffset.ToString();
            _rimShade.ValueText = plumeData.plumeMain.RimShade.ToString();
            _diamondColor.ValueText = plumeData.plumeDiamonds.DiamondColor;
            _diamondOffset.ValueText = plumeData.plumeDiamonds.DiamondOffset.ToString();
            _diamondIntensity.ValueText = plumeData.plumeDiamonds.DiamondIntensity.ToString();
            _sootColor.ValueText = plumeData.plumeSoot.SootColor;
            _sootIntensity.ValueText = plumeData.plumeSoot.SootIntensity.ToString();
            _sootLength.ValueText = plumeData.plumeSoot.SootLength.ToString();
            _hasSmoke.ValueText = plumeData.engineSmoke.HasSmoke.ToString();
            _smokeColor.ValueText = plumeData.engineSmoke.SmokeColor.ToString();
            _smokeSpeed.ValueText = plumeData.engineSmoke.SmokeSpeed.ToString();
            _globalIntenisty.ValueText = plumeData.plumeMain.GloabalIntensity.ToString();
            _textureIntensity.ValueText = plumeData.plumeMain.TextureIntensity.ToString();
            _discIntensity.ValueText = plumeData.plumeMain.DiscIntensity.ToString();
            _expansionRange.ValueText = new Vector2(plumeData.plumeMain.ExpansionRangeX, plumeData.plumeMain.ExpansionRangeY).ToString();
        }
    }
}