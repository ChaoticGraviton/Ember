using Assets.Scripts;
using Assets.Scripts.Menu.ListView;
using ModApi.Ui;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts
{
    public class LoadPresetUI : ListViewModel
    {
        public string Title { get; set; } = "Preset Selection";
        private PresetDetails _details;
        private List<PlumeData> plumePresets;

        public LoadPresetUI(string path)
        {
            plumePresets = new List<PlumeData>();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PlumeData));
            path = (path.Remove(path.Length - 1));
            var presetFiles = Directory.EnumerateFiles(path, "*.xml", SearchOption.AllDirectories);
            foreach (string currentFile in presetFiles)
            {
                string fileName = currentFile.Substring(path.Length + 1);
                using (StringReader reader = new StringReader(File.ReadAllText(path + "/" + fileName)))
                {
                    PlumeData plumeData = xmlSerializer.Deserialize(reader) as PlumeData;
                    plumeData.path = currentFile;
                    plumePresets.Add(plumeData);
                }
            }
        }

        public override void OnListViewInitialized(ListViewScript listView)
        {
            base.OnListViewInitialized(listView);
            listView.Title = Title;
            listView.CanDelete = true;
            listView.PrimaryButtonText = "IMPORT PRESET";
            listView.NoSelectionMessageText = "Select a preset from the left for more details";
            listView.DisplayType = ListViewScript.ListViewDisplayType.SmallDialog;

            NoItemsFoundMessage = ("No presets saved");
        }

        public override IEnumerator LoadItems()
        {
            _details = new PresetDetails(base.ListView.ListViewDetails);
            foreach (PlumeData plumeData in plumePresets)
            {
                ListViewItemScript listViewItemScript = ListView.CreateItem(plumeData.PresetName, plumeData.Author, plumeData);
            }

            yield return new WaitForEndOfFrame();
        }

        public override void UpdateDetails(ListViewItemScript item, Action completeCallback)
        {
            if (item != null) _details.UpdateDetails(item.ItemModel as PlumeData);
            completeCallback?.Invoke();
        }

        public override void OnPrimaryButtonClicked(ListViewItemScript selectedItem)
        {
            PlumeData plumeData = selectedItem.ItemModel as PlumeData;
            EmberDesignerButton.emberFlyout.ImportPreset(plumeData);
            Mod.Instance.designer.DesignerUi.ShowMessage("Imported preset from: '" + plumeData.PresetName + "'");
            ListView.Close();
        }

        public override void OnDeleteButtonClicked(ListViewItemScript selectedItem)
        {
            PlumeData plumeData = selectedItem.ItemModel as PlumeData;
            MessageDialogScript messageDialogScript = Game.Instance.UserInterface.CreateMessageDialog(MessageDialogType.OkayCancel);
            messageDialogScript.OkayButtonText = "DELETE";
            messageDialogScript.MessageText = string.Format("Confirm that you want to delete the prest '" + plumeData.PresetName + "'");
            messageDialogScript.UseDangerButtonStyle = true;
            messageDialogScript.OkayClicked += delegate (MessageDialogScript d)
            {
                d.Close();
                File.Delete(plumeData.path);
                ListView.DeleteItem(selectedItem);
                Items.Remove(selectedItem);
                ListView.SelectedItem = null;
            };
        }
    }
}

public class PresetDetails
{
    private DetailsTextScript _description;
    private DetailsWidgetGroup _plumeLabel;
    private DetailsWidgetGroup _shockLabel;
    private DetailsWidgetGroup _sootLabel;
    private DetailsWidgetGroup _smokeLabel;
    private DetailsPropertyScript _mainColor;
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

    public void UpdateDetails(PlumeData plumeData)
    {
        _description.Text = "Preset file location: " + plumeData.path;
        _mainColor.ValueText = plumeData.plumeMain.MainColor;
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
        _expansionRange.ValueText = plumeData.plumeMain.ExpansionRangeX.ToString() + ", " + plumeData.plumeMain.ExpansionRangeY.ToString();
    }
}