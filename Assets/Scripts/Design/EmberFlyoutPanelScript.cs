using Assets.Scripts.Craft.Parts.Modifiers.Propulsion;
using HarmonyLib;
using ModApi.Craft.Parts;
using ModApi.Design;
using ModApi.Math;
using ModApi.Ui;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TMPro;
using UI.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Design
{
    public class EmberFlyoutPanelScript : DesignerFlyoutPanelScript
    {
        private IDesigner _designer;
        XmlElement _content;
        XmlElement _noEngine;
        XmlElement _colorMain;
        XmlElement _colorExpanded;
        XmlElement _colorTip;
        XmlElement _colorFlame;
        XmlElement _colorDiamond;
        XmlElement _colorSoot;
        XmlElement _colorSmoke;
        XmlElement _inputGlobalIntensity;
        XmlElement _inputTextureStrength;
        XmlElement _inputScale;
        XmlElement _inputOffset;
        XmlElement _inputRimshade;
        XmlElement _inputDiscStrength;
        XmlElement _inputExpansionMin;
        XmlElement _inputExpansionMax;
        XmlElement _inputDiamondOffset;
        XmlElement _inputDiamondIntensity;
        XmlElement _toggleSoot;
        XmlElement _buttonSootColor;
        XmlElement _inputSootIntensity;
        XmlElement _inputSootLength;
        XmlElement _panelSootColor;
        XmlElement _toggleSmoke;
        XmlElement _inputSmokeSpeed;
        XmlElement _panelSmokeColor;
        XmlElement _buttonSmokeColor;
        public Slider throttleSlider;
        public Slider emberAltitudeSlider;
        private TextMeshProUGUI throttleDisplayValue;
        private TextMeshProUGUI altitudeDisplayValue;
        CraftPerformanceAnalysis performanceAnalizer;
        public static RocketEngineScript selectedRocketEngine;
        private XmlSerializer _xmlSerializer;
        private FileStream _fileStream;
        private readonly List<string> textFieldList = new() { "_exhaustScale", "_exhaustOffset", "_exhaustRimShade", "_exhaustShockDirectionOffset", "_exhaustShockIntensity", "_exhaustSootIntensity", "_exhaustSootLength", "_smokeSpeedOverride", "_exhaustGlobalIntensity", "_exhaustTextureStrength", "_nozzleDiscStrength" };
        private readonly Dictionary<string, string> fieldUpdateReferences = new() { { "Base", "_exhaustColor" }, { "Expanded", "_exhaustColorExpanded" }, { "Tip", "_exhaustColorTip" }, { "Flame", "_exhaustColorFlame" }, { "Shock", "_exhaustColorShock" }, { "Soot", "_exhaustColorSoot" }, { "Smoke", "_smokeColor" } };
        private Dictionary<string, Color> colorReferences = new() { };
        public bool hasEngineSelected => selectedRocketEngine != null;
        private bool smokeEnabled;
        private bool sootEnabled;

        public override void Initialize(DesignerUiScript designerUi)
        {
            Debug.Log("Ember Initalize");
            base.Initialize(designerUi);
            Mod.Instance.EmberFlyout = Flyout;
            Mod.Instance.EmberFlyoutScript = this;
            _designer = designerUi.Designer;
            _designer.SelectedPartChanged += OnSelectedPartChanged;
            _xmlSerializer = new XmlSerializer(typeof(PlumePresetData.PlumeData));
            Flyout.Opening += OnFlyoutOpening;
            Flyout.Closing += OnFlyoutClosing;
        }

        public override void LayoutRebuilt(ParseXmlResult parseResult)
        {
            Debug.Log("Ember Layout Rebuilt");
            base.LayoutRebuilt(parseResult);
            _content = xmlLayout.GetElementById("content-root");
            _noEngine = xmlLayout.GetElementById("no-engine-selected-text");
            _colorMain = xmlLayout.GetElementById("main-color");
            _colorExpanded = xmlLayout.GetElementById("expand-color");
            _colorTip = xmlLayout.GetElementById("tip-color");
            _colorFlame = xmlLayout.GetElementById("flame-color");
            _inputGlobalIntensity = xmlLayout.GetElementById("exhaust-global-intensity-input");
            _inputTextureStrength = xmlLayout.GetElementById("exhaust-texture-strength-input");
            _inputScale = xmlLayout.GetElementById("exhaust-scale-input");
            _inputOffset = xmlLayout.GetElementById("exhaust-offset-input");
            _inputRimshade = xmlLayout.GetElementById("exhaust-rimshade-input");
            _inputDiscStrength = xmlLayout.GetElementById("exhaust-disc-strength-input");
            _inputExpansionMin = xmlLayout.GetElementById("exhaust-expansion-range-x-input");
            _inputExpansionMax = xmlLayout.GetElementById("exhaust-expansion-range-y-input");
            _colorDiamond = xmlLayout.GetElementById("diamond-color");
            _inputDiamondOffset = xmlLayout.GetElementById("diamond-offset-input");
            _inputDiamondIntensity = xmlLayout.GetElementById("diamond-intensity-input");
            _toggleSoot = xmlLayout.GetElementById("soot-toggle");
            _buttonSootColor = xmlLayout.GetElementById("sootColor-Button");
            _colorSoot = xmlLayout.GetElementById("soot-color");
            _inputSootIntensity = xmlLayout.GetElementById("soot-intensity-input");
            _inputSootLength = xmlLayout.GetElementById("soot-length-input");
            _panelSootColor = xmlLayout.GetElementById("sootColor");
            _toggleSmoke = xmlLayout.GetElementById("smoke-toggle");
            _buttonSmokeColor = xmlLayout.GetElementById("smokeColor-Button");
            _colorSmoke = xmlLayout.GetElementById("smoke-color");
            _inputSmokeSpeed = xmlLayout.GetElementById("smoke-speed-input");
            _panelSmokeColor = xmlLayout.GetElementById("smokeColor");
            AdjustableThrottle.adjustableThrottleSlider = throttleSlider = xmlLayout.GetElementById<Slider>("preview-throttle");
            throttleDisplayValue = xmlLayout.GetElementById<TextMeshProUGUI>("preview-throttle-displayValue");
            emberAltitudeSlider = xmlLayout.GetElementById<Slider>("preview-altitude");
            altitudeDisplayValue = xmlLayout.GetElementById<TextMeshProUGUI>("preview-altitude-displayValue");
            performanceAnalizer = Game.Instance.Designer.PerformanceAnalysis as CraftPerformanceAnalysis;
            emberAltitudeSlider.SetValueWithoutNotify(CraftPerformanceAnalysisPatch.altitudeSlider != null ? CraftPerformanceAnalysisPatch.altitudeSlider.Value : 1);
            altitudeDisplayValue.SetText(performanceAnalizer.GetAltitudeDisplayValue(performanceAnalizer.AtmosphereSample));
            throttleSlider.SetValueWithoutNotify(1f);
            OnThrottleSliderChanged(1f);
            SetActiveForSelectedEngine(false);
        }

        private void LoadPresetButtonClicked()
        {
            LoadPresetUI presetManager = new LoadPresetUI(Mod.Instance.presetPath);
            Game.Instance.UserInterface.CreateListView(presetManager);
        }

        private void SavePresetButtonClicked()
        {
            InputDialogScript inputDialogScript = Game.Instance.UserInterface.CreateInputDialog();
            inputDialogScript.InputPlaceholderText = "Preset Name";
            inputDialogScript.MessageText = "Save Preset";
            inputDialogScript.OkayButtonText = "SAVE";
            inputDialogScript.CancelButtonText = "CANCEL";
            inputDialogScript.MaxLength = 255;
            inputDialogScript.InvalidCharacters.AddRange(Path.GetInvalidFileNameChars());
            inputDialogScript.OkayClicked += OnSavePlumePreset;
        }

        public void ResetPlumeButtonClicked()
        {
            if (Game.Instance.Designer.SelectedPart.SymmetrySlice != null)
            {
                Symmetry.ExecuteOnSymmetricPartModifiers(selectedRocketEngine.Data, true, delegate (RocketEngineData data)
                {
                    ResetPlume(data);
                });
            }
            else
            {
                ResetPlume(selectedRocketEngine.Data);
            }
            UpdateFlyoutDisplayValues();
        }

        private void OnFlyoutOpening(IFlyout flyout)
        {
            if (_designer.SelectedPart?.GetModifier<RocketEngineScript>() != null)
            {
                selectedRocketEngine = _designer.SelectedPart.GetModifier<RocketEngineScript>();
                SetSymmetricExhaust(true);
                SetActiveForSelectedEngine(true);
                UpdateFlyoutDisplayValues();
            }
            else
            {
                SetActiveForSelectedEngine(false);
            }
            UpdateAltitudeSliderDisplay();
        }

        private void OnFlyoutClosing(IFlyout flyout)
        {
            if (DesignerUi.SelectedFlyout?.Title == "Part Properties")
            {
                if (_designer.SelectedPart?.GetModifier<RocketEngineScript>() != null)
                {
                    SetSymmetricExhaust(false);
                    _designer.SelectedPart.GetModifier<RocketEngineScript>().PreviewExhaust = true;
                }
            }
            else
                SetSymmetricExhaust(false);
        }

        private void OnSelectedPartChanged(IPartScript oldPart, IPartScript newPart)
        {
            if (!Flyout.IsOpen)
                return;

            if (newPart != null) // Hotkey to deselect part
            {
                if (hasEngineSelected)
                {
                    SetSymmetricExhaust(false);
                }
                selectedRocketEngine = null;
                if (newPart.GetModifier<RocketEngineScript>() != null)
                {
                    selectedRocketEngine = newPart.GetModifier<RocketEngineScript>();
                    SetSymmetricExhaust(true);
                }

                if (hasEngineSelected)
                {
                    SetActiveForSelectedEngine(true);
                    UpdateFlyoutDisplayValues();
                }
                else SetActiveForSelectedEngine(false);
            }
            else if (oldPart != null)
            {
                if (oldPart.GetModifier<RocketEngineScript>() != null)
                {
                    SetActiveForSelectedEngine(false);
                    SetSymmetricExhaust(false, true, oldPart.GetModifier<RocketEngineScript>());
                }
            }
            else
            {
                SetSymmetricExhaust(false, true, selectedRocketEngine);
                SetActiveForSelectedEngine(false);
            }
        }

        private void OnThrottleSliderChanged(float value) => throttleDisplayValue.SetText(Units.GetPercentageString(value));

        public void OnAltitudeSliderChanged(float value)
        {
            performanceAnalizer.SetAltitudePercentage(value);
            altitudeDisplayValue.SetText(performanceAnalizer.GetAltitudeDisplayValue(performanceAnalizer.AtmosphereSample));
        }

        public void SetAltitudeSlider(float value)
        {
            emberAltitudeSlider.SetValueWithoutNotify(value);
            altitudeDisplayValue.SetText(performanceAnalizer.GetAltitudeDisplayValue(performanceAnalizer.AtmosphereSample));
        }

        public void UpdateAltitudeSliderDisplay() => SetAltitudeSlider(emberAltitudeSlider.value);

        public void SetSymmetricExhaust(bool enabled, bool isOldPart = false, RocketEngineScript engine = null)
        {
            if (isOldPart)
                selectedRocketEngine = engine;
            if (hasEngineSelected)
            {
                selectedRocketEngine.PreviewExhaust = enabled;
                Symmetry.ExecuteOnSymmetricPartModifiers(selectedRocketEngine.Data, true, delegate (RocketEngineData data)
                {
                    data.Script.PreviewExhaust = enabled;
                });
            }
        }

        private void SetActiveForSelectedEngine(bool engineSelected)
        {
            _content.SetActive(engineSelected);
            _noEngine.SetActive(!engineSelected);
        }

        private void UpdateFlyoutDisplayValues()
        {
            if (hasEngineSelected)
            {
                // Plume Editing
                setColorDisplays(_colorMain, "color", "tooltip", selectedRocketEngine.Data.ExhaustColor);
                setColorDisplays(_colorExpanded, "color", "tooltip", selectedRocketEngine.Data.ExhaustColorExpanded);
                setColorDisplays(_colorTip, "color", "tooltip", selectedRocketEngine.Data.ExhaustColorTip);
                setColorDisplays(_colorFlame, "color", "tooltip", selectedRocketEngine.Data.ExhaustColorFlame);
                _inputGlobalIntensity.SetValue(SetFormatForDefault(selectedRocketEngine.Data.ExhaustGlobalIntensity.ToString()));
                _inputTextureStrength.SetValue(selectedRocketEngine.Data.ExhaustTextureStrength.ToString());
                _inputScale.SetValue(selectedRocketEngine.Data.ExhaustScale.ToString());
                _inputOffset.SetValue(selectedRocketEngine.Data.ExhaustOffset.ToString());
                _inputRimshade.SetValue(SetFormatForDefault(selectedRocketEngine.Data.ExhaustRimShade.ToString()));
                _inputDiscStrength.SetValue(selectedRocketEngine.Data.NozzleDiscStrength.ToString());
                _inputExpansionMin.SetValue(selectedRocketEngine.Data.ExhaustExpansionRange[0].ToString());
                _inputExpansionMax.SetValue(selectedRocketEngine.Data.ExhaustExpansionRange[1].ToString());

                // Shock Editing
                setColorDisplays(_colorDiamond, "color", "tooltip", selectedRocketEngine.Data.ExhaustColorShock);
                _inputDiamondOffset.SetValue(selectedRocketEngine.Data.ExhaustShockDirectionOffset.ToString());
                _inputDiamondIntensity.SetValue(SetFormatForDefault(selectedRocketEngine.Data.ExhaustShockIntensity.ToString()));

                // Soot Editing
                sootEnabled = selectedRocketEngine.Data.ExhaustSootIntensity > 0;
                _toggleSoot.SetValue(sootEnabled.ToString());
                _buttonSootColor.SetActive(sootEnabled);
                _inputSootIntensity.parentElement.SetActive(sootEnabled);
                _inputSootLength.parentElement.SetActive(sootEnabled);
                _panelSootColor.SetActive(sootEnabled);
                setColorDisplays(_colorSoot, "color", "tooltip", selectedRocketEngine.Data.ExhaustColorSoot);
                _inputSootIntensity.SetValue(selectedRocketEngine.Data.ExhaustSootIntensity.ToString());
                _inputSootLength.SetValue(selectedRocketEngine.Data.ExhaustSootLength.ToString());

                // Smoke Editing
                smokeEnabled = selectedRocketEngine.Data.HasSmoke;
                _toggleSmoke.SetValue(smokeEnabled.ToString());
                _buttonSmokeColor.SetActive(smokeEnabled);
                _inputSmokeSpeed.parentElement.SetActive(smokeEnabled);
                _panelSmokeColor.SetActive(smokeEnabled);
                setColorDisplays(_colorSmoke, "color", "tooltip", selectedRocketEngine.Data.SmokeColor);
                _inputSmokeSpeed.SetValue(selectedRocketEngine.Data.SmokeSpeed.ToString());

                // Updates the engine exhaust
                selectedRocketEngine.Data.Script.InitializeExhaust();
            }
        }

        private string SetFormatForDefault(string propertyValue)
        {
            //if (propertyValue == "-1") propertyValue = "Default";
            return propertyValue;
        }

        // Custom color to hex since the ModAPI doesn't include alpha, for some reason. If alpha is added to the ModAPI, then it could be converted back.
        public static string ColorToHexAlpha(Color32 color) => "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");

        private void setColorDisplays(XmlElement element, string attibuteA, string attibuteB, Color color)
        {
            element.SetAndApplyAttribute(attibuteA, ColorToHexAlpha(color));
            element.SetAndApplyAttribute(attibuteB, ColorToHexAlpha(color));
        }

        private void UpdateColorRefValues()
        {
            colorReferences["Base"] = selectedRocketEngine.Data.ExhaustColor;
            colorReferences["Expanded"] = selectedRocketEngine.Data.ExhaustColorExpanded;
            colorReferences["Tip"] = selectedRocketEngine.Data.ExhaustColorTip;
            colorReferences["Flame"] = selectedRocketEngine.Data.ExhaustColorFlame;
            colorReferences["Shock"] = selectedRocketEngine.Data.ExhaustColorShock;
            colorReferences["Soot"] = selectedRocketEngine.Data.ExhaustColorSoot;
            colorReferences["Smoke"] = selectedRocketEngine.Data.SmokeColor;
        }

        private void OnExhaustColorChanged(string type)
        {
            UpdateColorRefValues();
            var colorType = colorReferences[type];
            var fieldUpdateType = fieldUpdateReferences[type];
            Game.Instance.UserInterface.CreateColorPicker(true, colorType, delegate (Color c)
            {
                // Occurs when selecting "Okay"
                UpdatePlumeColors(fieldUpdateType, ColorUtility.ToHtmlStringRGBA(c));
                UpdateFlyoutDisplayValues();
            }, delegate (Color c)
            {
                // Occurs when updating the color wheel
                UpdatePlumeColors(fieldUpdateType, ColorUtility.ToHtmlStringRGBA(c));
            }, false);
        }

        private void UpdatePlumeColors(string updateField, string color)
        {
            Traverse.Create(selectedRocketEngine.Data).Field(updateField).SetValue("#" + color);
            UpdateSymmetricRocketEngines();
            selectedRocketEngine.Data.Script.InitializeExhaust();
        }

        private void OnSootToggleClicked()
        {
            // Disables the visibility of the elements in the UI
            bool sootToggle = _toggleSoot.GetValue().ToBoolean();
            _buttonSootColor.SetActive(sootToggle);
            _inputSootIntensity.parentElement.SetActive(sootToggle);
            _inputSootLength.parentElement.SetActive(sootToggle);
            _panelSootColor.SetActive(sootToggle);

            if (hasEngineSelected)
            {
                Traverse.Create(selectedRocketEngine.Data).Field("_exhaustSootIntensity").SetValue(sootToggle ? 1 : 0);
                _inputSootIntensity.SetText(selectedRocketEngine.Data.ExhaustSootIntensity.ToString());
                UpdateSymmetricRocketEngines();
            }
        }

        private void OnSmokeToggleClicked()
        {
            // Disables the visibility of the elements in the UI
            bool smokeToggle = _toggleSmoke.GetValue().ToBoolean();
            _inputSmokeSpeed.parentElement.SetActive(smokeToggle);
            _buttonSmokeColor.SetActive(smokeToggle);
            _panelSmokeColor.SetActive(smokeToggle);

            if (hasEngineSelected)
            {
                Traverse.Create(selectedRocketEngine.Data._hasSmoke).SetValue(smokeToggle);
                UpdateSymmetricRocketEngines();
            }
        }

        private void TextBoxEdited(string boxID)
        {
            if (boxID.Contains("exhaust-expansion-range"))
            {
                Vector2 expasionRangeValue = selectedRocketEngine.Data.ExhaustExpansionRange;
                expasionRangeValue[xmlLayout.GetElementById(boxID).GetAttribute("internalID").ToInt()] = xmlLayout.GetElementById(boxID).GetValue().ToFloat();
                Traverse.Create(selectedRocketEngine.Data).Field("_exhaustExpansionRange").SetValue(expasionRangeValue);
            }
            else
            {
                Traverse.Create(selectedRocketEngine.Data).Field(textFieldList[xmlLayout.GetElementById(boxID).GetAttribute("internalID").ToInt()]).SetValue(xmlLayout.GetElementById(boxID).GetValue().ToFloat());
            }
            UpdateFlyoutDisplayValues();
            UpdateSymmetricRocketEngines();
        }

        private void UpdateSymmetricRocketEngines(bool settingDefault = false)
        {
            if (Game.Instance.Designer.SelectedPart.SymmetrySlice != null)
            {
                Symmetry.ExecuteOnSymmetricPartModifiers(selectedRocketEngine.Data, true, delegate (RocketEngineData data)
                {
                    Traverse.Create(data).Field("_exhaustColor").SetValue(ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColor));
                    Traverse.Create(data).Field("_exhaustColorExpanded").SetValue(ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorExpanded));
                    Traverse.Create(data).Field("_exhaustColorTip").SetValue(ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorTip));
                    Traverse.Create(data).Field("_exhaustColorFlame").SetValue(ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorFlame));
                    Traverse.Create(data).Field("_exhaustGlobalIntensity").SetValue(selectedRocketEngine.Data.ExhaustGlobalIntensity);
                    Traverse.Create(data).Field("_exhaustTextureStrength").SetValue(selectedRocketEngine.Data.ExhaustTextureStrength);
                    Traverse.Create(data).Field("_exhaustScale").SetValue(selectedRocketEngine.Data.ExhaustScale);
                    Traverse.Create(data).Field("_exhaustOffset").SetValue(selectedRocketEngine.Data.ExhaustOffset);
                    Traverse.Create(data).Field("_exhaustRimShade").SetValue(selectedRocketEngine.Data.ExhaustRimShade);
                    Traverse.Create(data).Field("_nozzleDiscStrength").SetValue(selectedRocketEngine.Data.NozzleDiscStrength);
                    Traverse.Create(data).Field("_exhaustExpansionRange").SetValue(selectedRocketEngine.Data.ExhaustExpansionRange);
                    Traverse.Create(data).Field("_exhaustColorShock").SetValue(ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorShock));
                    Traverse.Create(data).Field("_exhaustShockDirectionOffset").SetValue(selectedRocketEngine.Data.ExhaustShockDirectionOffset);
                    Traverse.Create(data).Field("_exhaustShockIntensity").SetValue(selectedRocketEngine.Data.ExhaustShockIntensity);
                    Traverse.Create(data).Field("_exhaustColorSoot").SetValue(ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorSoot));
                    Traverse.Create(data).Field("_exhaustSootIntensity").SetValue(selectedRocketEngine.Data.ExhaustSootIntensity);
                    Traverse.Create(data).Field("_exhaustSootLength").SetValue(selectedRocketEngine.Data.ExhaustSootLength);
                    Traverse.Create(data).Field("_hasSmoke").SetValue(selectedRocketEngine.Data.HasSmoke);
                    Traverse.Create(data).Field("_smokeColor").SetValue(ColorToHexAlpha(selectedRocketEngine.Data.SmokeColor));
                    Traverse.Create(data).Field("_smokeSpeedOverride").SetValue(selectedRocketEngine.Data.SmokeSpeed);
                    data.Script.InitializeExhaust();
                });
            }
            else selectedRocketEngine.Data.Script.InitializeExhaust();
        }

        private void OnSavePlumePreset(InputDialogScript inputDialog)
        {
            inputDialog.Close();
            PlumePresetData.PlumeData plumeData = new()
            {
                PresetName = inputDialog.InputText,
                Author = (!string.IsNullOrEmpty(Game.Instance.Settings.UserName)) ? Game.Instance.Settings.UserName : "Unknown"
            };
            plumeData.plumeMain = new PlumePresetData.PlumeMain()
            {
                MainColor = ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColor),
                ExpandedColor = ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorExpanded),
                TipColor = ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorTip),
                FlameColor = ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorFlame),
                GloabalIntensity = selectedRocketEngine.Data.ExhaustGlobalIntensity == -1f ? selectedRocketEngine.FuelSource.FuelType.GlobalIntensity : selectedRocketEngine.Data.ExhaustGlobalIntensity,
                TextureIntensity = selectedRocketEngine.Data.ExhaustTextureStrength,
                ExhaustScale = selectedRocketEngine.Data.ExhaustScale,
                ExhaustOffset = selectedRocketEngine.Data.ExhaustOffset,
                RimShade = selectedRocketEngine.Data.ExhaustRimShade == -1f ? selectedRocketEngine.FuelSource.FuelType.RimShade : selectedRocketEngine.Data.ExhaustRimShade,
                DiscIntensity = selectedRocketEngine.Data.NozzleDiscStrength,
                ExpansionRangeX = selectedRocketEngine.Data.ExhaustExpansionRange[0],
                ExpansionRangeY = selectedRocketEngine.Data.ExhaustExpansionRange[1]
            };
            plumeData.plumeDiamonds = new PlumePresetData.PlumeDiamonds()
            {
                DiamondColor = ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorShock),
                DiamondOffset = selectedRocketEngine.Data.ExhaustShockDirectionOffset,
                DiamondIntensity = selectedRocketEngine.Data.ExhaustShockIntensity == -1f ? selectedRocketEngine.FuelSource.FuelType.ShockIntensity : selectedRocketEngine.Data.ExhaustShockIntensity
            };
            plumeData.plumeSoot = new PlumePresetData.PlumeSoot()
            {
                SootColor = ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorSoot),
                SootIntensity = selectedRocketEngine.Data.ExhaustSootIntensity,
                SootLength = selectedRocketEngine.Data.ExhaustSootLength
            };
            plumeData.engineSmoke = new PlumePresetData.EngineSmoke()
            {
                HasSmoke = selectedRocketEngine.Data.HasSmoke,
                SmokeColor = ColorToHexAlpha(selectedRocketEngine.Data.SmokeColor),
                SmokeSpeed = selectedRocketEngine.Data.SmokeSpeed,
            };
            var newPresetFilePath = inputDialog.InputText + ".xml";
            if (File.Exists(Mod.Instance.presetPath + newPresetFilePath))
            {
                MessageDialogScript verifyOverwriteDialog = Game.Instance.UserInterface.CreateMessageDialog(MessageDialogType.OkayCancel);
                verifyOverwriteDialog.MessageText = "A preset file already exists with the name: '" + inputDialog.InputText + "'<br> Would you like to overwrite this preset?";
                verifyOverwriteDialog.CancelButtonText = "CANCEL";
                verifyOverwriteDialog.OkayButtonText = "OVERWRITE";
                verifyOverwriteDialog.UseDangerButtonStyle = true;
                verifyOverwriteDialog.OkayClicked += delegate (MessageDialogScript dialog)
                {
                    dialog.Close();
                    SavePresetFile(newPresetFilePath, plumeData);
                };
            }
            else SavePresetFile(newPresetFilePath, plumeData);
        }

        private void SavePresetFile(string presetFilePath, PlumePresetData.PlumeData plumeData)
        {
            _fileStream = new FileStream(Mod.Instance.presetPath + presetFilePath, FileMode.Create);
            _xmlSerializer.Serialize(_fileStream, plumeData);
            _fileStream.Close();
            Game.Instance.Designer.DesignerUi.ShowMessage(File.Exists(Mod.Instance.presetPath + presetFilePath) ? "Plume preset saved at: " + Mod.Instance.presetPath + presetFilePath : "<color=#e7515a>Plume preset failed to save");
        }

        public void ImportPreset(PlumePresetData.PlumeData plumeData)
        {
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColor").SetValue(plumeData.plumeMain.MainColor);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColorExpanded").SetValue(plumeData.plumeMain?.ExpandedColor ?? plumeData.plumeMain.MainColor);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColorTip").SetValue(plumeData.plumeMain.TipColor);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColorFlame").SetValue(plumeData.plumeMain.FlameColor);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustGlobalIntensity").SetValue(plumeData.plumeMain.GloabalIntensity);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustTextureStrength").SetValue(plumeData.plumeMain.TextureIntensity);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustScale").SetValue(plumeData.plumeMain.ExhaustScale);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustOffset").SetValue(plumeData.plumeMain.ExhaustOffset);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustRimShade").SetValue(plumeData.plumeMain.RimShade);
            Traverse.Create(selectedRocketEngine.Data).Field("_nozzleDiscStrength").SetValue(plumeData.plumeMain.DiscIntensity);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustExpansionRange").SetValue(new Vector2(plumeData.plumeMain.ExpansionRangeX, plumeData.plumeMain.ExpansionRangeY));
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColorShock").SetValue(plumeData.plumeDiamonds.DiamondColor);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustShockDirectionOffset").SetValue(plumeData.plumeDiamonds.DiamondOffset);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustShockIntensity").SetValue(plumeData.plumeDiamonds.DiamondIntensity);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColorSoot").SetValue(plumeData.plumeSoot.SootColor);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustSootIntensity").SetValue(plumeData.plumeSoot.SootIntensity);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustSootLength").SetValue(plumeData.plumeSoot.SootLength);
            Traverse.Create(selectedRocketEngine.Data).Field("_hasSmoke").SetValue(plumeData.engineSmoke.HasSmoke);
            Traverse.Create(selectedRocketEngine.Data).Field("_smokeColor").SetValue(plumeData.engineSmoke.SmokeColor);
            Traverse.Create(selectedRocketEngine.Data).Field("_smokeSpeedOverride").SetValue(plumeData.engineSmoke.SmokeSpeed);
            UpdateFlyoutDisplayValues();
            UpdateSymmetricRocketEngines();
        }

        public void ResetPlume(RocketEngineData data)
        {
            Traverse.Create(data).Field("_exhaustColor").SetValue("Default");
            Traverse.Create(data).Field("_exhaustColorExpanded").SetValue("Default");
            Traverse.Create(data).Field("_exhaustColorTip").SetValue("Default");
            Traverse.Create(data).Field("_exhaustColorFlame").SetValue("Default");
            Traverse.Create(data).Field("_exhaustGlobalIntensity").SetValue(-1);
            Traverse.Create(data).Field("_exhaustTextureStrength").SetValue(1);
            Traverse.Create(data).Field("_exhaustScale").SetValue(1);
            Traverse.Create(data).Field("_exhaustOffset").SetValue(0);
            Traverse.Create(data).Field("_exhaustRimShade").SetValue(-1);
            Traverse.Create(data).Field("_nozzleDiscStrength").SetValue(5);
            Traverse.Create(data).Field("_exhaustExpansionRange").SetValue(new Vector2(-1, -1));
            Traverse.Create(data).Field("_exhaustColorShock").SetValue("Default");
            Traverse.Create(data).Field("_exhaustShockDirectionOffset").SetValue(0);
            Traverse.Create(data).Field("_exhaustShockIntensity").SetValue(-1);
            Traverse.Create(data).Field("_exhaustColorSoot").SetValue("Default");
            Traverse.Create(data).Field("_exhaustSootIntensity").SetValue(0);
            Traverse.Create(data).Field("_exhaustSootLength").SetValue(0);
            Traverse.Create(data).Field("_hasSmoke").SetValue(true);
            Traverse.Create(data).Field("_smokeColor").SetValue("Default");
            Traverse.Create(data).Field("_smokeSpeedOverride").SetValue(1);
            data.Script.InitializeExhaust();
        }
    }
}