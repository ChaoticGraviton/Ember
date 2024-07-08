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
        private PlumePresetData.PresetDetails _details;
        private List<PlumePresetData.PlumeData> plumePresets;

        public LoadPresetUI(string path)
        {
            plumePresets = new List<PlumePresetData.PlumeData>();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PlumePresetData.PlumeData));
            path = (path.Remove(path.Length - 1));
            var presetFiles = Directory.EnumerateFiles(path, "*.xml", SearchOption.AllDirectories);
            foreach (string currentFile in presetFiles)
            {
                string fileName = currentFile.Substring(path.Length + 1);
                using (StringReader reader = new StringReader(File.ReadAllText(path + "/" + fileName)))
                {
                    PlumePresetData.PlumeData plumeData = xmlSerializer.Deserialize(reader) as PlumePresetData.PlumeData;
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
            _details = new PlumePresetData.PresetDetails(base.ListView.ListViewDetails);
            foreach (PlumePresetData.PlumeData plumeData in plumePresets)
            {
                ListViewItemScript listViewItemScript = ListView.CreateItem(plumeData.PresetName, plumeData.Author, plumeData);
            }

            yield return new WaitForEndOfFrame();
        }

        public override void UpdateDetails(ListViewItemScript item, Action completeCallback)
        {
            if (item != null) _details.UpdateDetails(item.ItemModel as PlumePresetData.PlumeData);
            completeCallback?.Invoke();
        }

        public override void OnPrimaryButtonClicked(ListViewItemScript selectedItem)
        {
            PlumePresetData.PlumeData plumeData = selectedItem.ItemModel as PlumePresetData.PlumeData;
            Mod.Instance.EmberFlyoutScript.ImportPreset(plumeData);
            Game.Instance.Designer.DesignerUi.ShowMessage("Imported preset from: '" + plumeData.PresetName + "'");
            ListView.Close();
        }

        public override void OnDeleteButtonClicked(ListViewItemScript selectedItem)
        {
            PlumePresetData.PlumeData plumeData = selectedItem.ItemModel as PlumePresetData.PlumeData;
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