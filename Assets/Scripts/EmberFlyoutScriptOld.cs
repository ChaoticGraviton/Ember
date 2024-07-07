namespace Assets.Scripts
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Serialization;
    using Assets.Scripts.Craft.Parts.Modifiers.Propulsion;
    using Assets.Scripts.Design;
    using Assets.Scripts.Design.Staging;
    using Assets.Scripts.Ui;
    using HarmonyLib;
    using ModApi.Craft.Parts;
    using ModApi.Math;
    using ModApi.Ui;
    using ModApi.Ui.Inspector;
    using TMPro;
    using UI.Xml;
    using UnityEngine;
    using UnityEngine.UI;

    /* TODO //

    playtest?

    // TODO */

    public class EmberFlyoutScriptOld : MonoBehaviour
    {
        public FlyoutScript flyout = new();
        public static XmlLayout XmlLayout { get; private set; }

        public static RocketEngineScript selectedRocketEngine;
        public bool EngineSelected => selectedRocketEngine != null;

        public void OnFlyoutCloseButtonClicked() => EmberDesignerButton.CloseFlyout();

        public Slider throttleSlider = null;

        private TextMeshProUGUI throttleDisplayValue;

        public Slider emberAltitudeSlider;

        private bool smokeEnabled;

        private bool sootEnabled;

        private readonly List<string> textFieldList = new() { "_exhaustScale", "_exhaustOffset", "_exhaustRimShade", "_exhaustShockDirectionOffset", "_exhaustShockIntensity", "_exhaustSootIntensity", "_exhaustSootLength", "_smokeSpeedOverride", "_exhaustGlobalIntensity", "_exhaustTextureStrength", "_nozzleDiscStrength" };

        private readonly Dictionary<string, string> fieldUpdateReferences = new() { { "Base", "_exhaustColor" }, { "Tip", "_exhaustColorTip" }, { "Flame", "_exhaustColorFlame" }, { "Shock", "_exhaustColorShock" }, { "Soot", "_exhaustColorSoot" }, { "Smoke", "_smokeColor" } };

        private Dictionary<string, Color> colorReferences = new() { };

        private static CraftPerformanceAnalysis performanceAnalysis = Game.Instance.Designer.PerformanceAnalysis as CraftPerformanceAnalysis;

        private XmlSerializer _xmlSerializer;

        private FileStream _fileStream;

        private void SavePresetButtonClicked()
        {
            ModApi.Ui.InputDialogScript inputDialogScript = Game.Instance.UserInterface.CreateInputDialog();
            inputDialogScript.InputPlaceholderText = "Preset Name";
            inputDialogScript.MessageText = "Save Preset";
            inputDialogScript.OkayButtonText = "SAVE";
            inputDialogScript.CancelButtonText = "CANCEL";
            inputDialogScript.MaxLength = 255;
            inputDialogScript.InvalidCharacters.AddRange(Path.GetInvalidFileNameChars());
            inputDialogScript.OkayClicked += OnSavePlumePreset;
        }

        private void OnSavePlumePreset(ModApi.Ui.InputDialogScript inputDialog)
        {
            inputDialog.Close();
            PlumeData plumeData = new()
            {
                PresetName = inputDialog.InputText,
                Author = (!string.IsNullOrEmpty(Game.Instance.Settings.UserName)) ? Game.Instance.Settings.UserName : "Unknown"
            };
            plumeData.plumeMain = new PlumeMain()
            {
                MainColor = ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColor),
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
            plumeData.plumeDiamonds = new PlumeDiamonds()
            {
                DiamondColor = ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorShock),
                DiamondOffset = selectedRocketEngine.Data.ExhaustShockDirectionOffset,
                DiamondIntensity = selectedRocketEngine.Data.ExhaustShockIntensity == -1f ? selectedRocketEngine.FuelSource.FuelType.ShockIntensity : selectedRocketEngine.Data.ExhaustShockIntensity
            };
            plumeData.plumeSoot = new PlumeSoot()
            {
                SootColor = ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColorSoot),
                SootIntensity = selectedRocketEngine.Data.ExhaustSootIntensity,
                SootLength = selectedRocketEngine.Data.ExhaustSootLength
            };
            plumeData.engineSmoke = new EngineSmoke()
            {
                HasSmoke = selectedRocketEngine.Data.HasSmoke,
                SmokeColor = ColorToHexAlpha(selectedRocketEngine.Data.SmokeColor),
                SmokeSpeed = selectedRocketEngine.Data.SmokeSpeed,
            };
            var newPresetFilePath = inputDialog.InputText + ".xml";
            if (File.Exists(Mod.Instance.presetPath + newPresetFilePath))
            {
                ModApi.Ui.MessageDialogScript verifyOverwriteDialog = Game.Instance.UserInterface.CreateMessageDialog(MessageDialogType.OkayCancel);
                verifyOverwriteDialog.MessageText = "A preset file already exists with the name: '" + inputDialog.InputText + "'<br> Would you like to overwrite this preset?";
                verifyOverwriteDialog.CancelButtonText = "CANCEL";
                verifyOverwriteDialog.OkayButtonText = "OVERWRITE";
                verifyOverwriteDialog.UseDangerButtonStyle = true;
                verifyOverwriteDialog.OkayClicked += delegate (ModApi.Ui.MessageDialogScript dialog)
                {
                    dialog.Close();
                    SavePresetFile(newPresetFilePath, plumeData);
                };
            }
            else SavePresetFile(newPresetFilePath, plumeData);
        }

        private void SavePresetFile(string presetFilePath, PlumeData plumeData)
        {
            _fileStream = new FileStream(Mod.Instance.presetPath + presetFilePath, FileMode.Create);
            _xmlSerializer.Serialize(_fileStream, plumeData);
            _fileStream.Close();
            Game.Instance.Designer.DesignerUi.ShowMessage(File.Exists(Mod.Instance.presetPath + presetFilePath) ? "Plume preset saved at: " + Mod.Instance.presetPath + presetFilePath : "<color=#e7515a>Plume preset failed to save");
        }

        private void LoadPresetButtonClicked()
        {
            LoadPresetUI presetManager = new LoadPresetUI(Mod.Instance.presetPath);
            Game.Instance.UserInterface.CreateListView(presetManager);
        }

        public void ImportPreset(PlumeData plumeData)
        {
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColor").SetValue(plumeData.plumeMain.MainColor);
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

        public void ResetPlume()
        {
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColor").SetValue("Default");
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColorTip").SetValue("Default");
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColorFlame").SetValue("Default");
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustGlobalIntensity").SetValue(-1);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustTextureStrength").SetValue(1);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustScale").SetValue(1);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustOffset").SetValue(0);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustRimShade").SetValue(-1);
            Traverse.Create(selectedRocketEngine.Data).Field("_nozzleDiscStrength").SetValue(5);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustExpansionRange").SetValue(new Vector2(-1, -1));
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColorShock").SetValue("Default");
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustShockDirectionOffset").SetValue(0);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustShockIntensity").SetValue(-1);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustColorSoot").SetValue("Default");
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustSootIntensity").SetValue(0);
            Traverse.Create(selectedRocketEngine.Data).Field("_exhaustSootLength").SetValue(0);
            Traverse.Create(selectedRocketEngine.Data).Field("_hasSmoke").SetValue(true);
            Traverse.Create(selectedRocketEngine.Data).Field("_smokeColor").SetValue("Default");
            Traverse.Create(selectedRocketEngine.Data).Field("_smokeSpeedOverride").SetValue(1);
            UpdateFlyoutDisplayValues();
            UpdateSymmetricRocketEngines();
        }

        public void Open()
        {
            if (EngineSelected && Game.Instance.Designer.SelectedPart != null) SetSymmetricExhaust(true);
        }

        public void Close(bool exhaustState = false)
        {
            Game.Instance.Designer.SelectedPartChanged -= OnSelectedPartChanged;
            XmlLayout.Hide(() => Destroy(this.gameObject), true);
            if (EngineSelected) SetSymmetricExhaust(exhaustState);
        }

        public void SetSymmetricExhaust(bool enabled, bool isOldPart = false, RocketEngineScript engine = null)
        {
            if (isOldPart) selectedRocketEngine = engine;
            if (EngineSelected)
            {
                selectedRocketEngine.PreviewExhaust = enabled;
                Symmetry.ExecuteOnSymmetricPartModifiers(selectedRocketEngine.Data, true, delegate (RocketEngineData data)
            {
                data.Script.PreviewExhaust = enabled;
            });
            }
        }

        internal void OnLayoutRebuilt(IXmlLayoutController controller) // Initialize flyout
        {
            Game.Instance.Designer.SelectedPartChanged += OnSelectedPartChanged;
            XmlLayout = (XmlLayout)controller.XmlLayout;
            flyout.Initialize(XmlLayout.GetElementById("flyout-Ember"));
            flyout.Open();
            if (Mod.isUserApproved)
            {
                OnSelectedPartChanged(null, Game.Instance.Designer.SelectedPart);
                if (EngineSelected)
                {
                    UpdateFlyoutDisplayValues();
                }
                _xmlSerializer = new XmlSerializer(typeof(PlumeData));
                throttleSlider = XmlLayout.GetElementById<Slider>("preview-throttle");
                throttleSlider.SetValueWithoutNotify(1f);
                throttleDisplayValue = XmlLayout.GetElementById<TextMeshProUGUI>("preview-throttle-displayValue");
                OnThrottleSliderChanged(1f);
                AdjustableThrottle.adjustableThrottleSlider = throttleSlider;
                performanceAnalysis = Game.Instance.Designer.PerformanceAnalysis as CraftPerformanceAnalysis;
                emberAltitudeSlider = XmlLayout.GetElementById<Slider>("preview-altitude");
                emberAltitudeSlider.SetValueWithoutNotify(AltitudeSlider.altitudeSlider != null ? AltitudeSlider.altitudeSlider.Value : 1);
                XmlLayout.GetElementById("preview-altitude-displayValue").SetText(performanceAnalysis.GetAltitudeDisplayValue(performanceAnalysis.AtmosphereSample));
            }
            else
            {
                XmlLayout.GetElementById("notValidUser").SetActive(true);
                XmlLayout.GetElementById("notEngineSelected").SetActive(false);
                XmlLayout.GetElementById("engineSelected").SetActive(false);
                Game.Quit();
            }
        }

        private void OnSelectedPartChanged(IPartScript oldPart, IPartScript newPart)
        {
            if (newPart != null) // Hotkey to deselect part
            {
                if (EngineSelected)
                {
                    SetSymmetricExhaust(false);
                }
                selectedRocketEngine = null;
                if (newPart.GetModifier<RocketEngineScript>() != null)
                {
                    selectedRocketEngine = newPart.GetModifier<RocketEngineScript>();
                    SetSymmetricExhaust(true);
                }

                if (EngineSelected)
                {
                    SetFlyoutForEngineSelect(true);
                    UpdateFlyoutDisplayValues();
                }
                else SetFlyoutForEngineSelect(false);
            }
            else if (oldPart != null)
            {
                if (oldPart.GetModifier<RocketEngineScript>() != null)
                {
                    SetFlyoutForEngineSelect(false);
                    SetSymmetricExhaust(false, true, oldPart.GetModifier<RocketEngineScript>());
                }
            }
            else
            {
                SetSymmetricExhaust(false, true, selectedRocketEngine);
                SetFlyoutForEngineSelect(false);
            }
        }

        private void SetFlyoutForEngineSelect(bool engineSelect)
        {
            XmlLayout.GetElementById("engineSelected").SetActive(engineSelect);
            XmlLayout.GetElementById("notEngineSelected").SetActive(!engineSelect);
            XmlLayout.GetElementById("notValidUser").SetActive(!Mod.isUserApproved);
        }

        private void UpdateFlyoutDisplayValues()
        {
            if (EngineSelected)
            {
                // Plume Editing
                setColorDisplays("main-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColor);
                setColorDisplays("tip-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColorTip);
                setColorDisplays("flame-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColorFlame);
                XmlLayout.GetElementById("exhaust-global-intensity-input").SetValue(SetFormatForDefault(selectedRocketEngine.Data.ExhaustGlobalIntensity.ToString()));
                XmlLayout.GetElementById("exhaust-texture-strength-input").SetValue(selectedRocketEngine.Data.ExhaustTextureStrength.ToString());
                XmlLayout.GetElementById("exhaust-scale-input").SetValue(selectedRocketEngine.Data.ExhaustScale.ToString());
                XmlLayout.GetElementById("exhaust-offset-input").SetValue(selectedRocketEngine.Data.ExhaustOffset.ToString());
                XmlLayout.GetElementById("exhaust-rimshade-input").SetValue(SetFormatForDefault(selectedRocketEngine.Data.ExhaustRimShade.ToString()));
                XmlLayout.GetElementById("exhaust-disc-strength-input").SetValue(selectedRocketEngine.Data.NozzleDiscStrength.ToString());
                XmlLayout.GetElementById("exhaust-expansion-range-x-input").SetValue(selectedRocketEngine.Data.ExhaustExpansionRange[0].ToString());
                XmlLayout.GetElementById("exhaust-expansion-range-y-input").SetValue(selectedRocketEngine.Data.ExhaustExpansionRange[1].ToString());

                // Shock Editing
                setColorDisplays("diamond-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColorShock);
                XmlLayout.GetElementById("diamond-offset-input").SetValue(selectedRocketEngine.Data.ExhaustShockDirectionOffset.ToString());
                XmlLayout.GetElementById("diamond-intensity-input").SetValue(SetFormatForDefault(selectedRocketEngine.Data.ExhaustShockIntensity.ToString()));

                // Soot Editing
                sootEnabled = selectedRocketEngine.Data.ExhaustSootIntensity > 0;
                XmlLayout.GetElementById("soot-toggle").SetValue(sootEnabled.ToString());
                XmlLayout.GetElementById("sootColor-Button").SetActive(XmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
                XmlLayout.GetElementById("soot-intensity-inputfield").SetActive(XmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
                XmlLayout.GetElementById("soot-length-inputfield").SetActive(XmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
                XmlLayout.GetElementById("sootColor").SetActive(XmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
                setColorDisplays("soot-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColorSoot);
                XmlLayout.GetElementById("soot-intensity-input").SetValue(selectedRocketEngine.Data.ExhaustSootIntensity.ToString());
                XmlLayout.GetElementById("soot-length-input").SetValue(selectedRocketEngine.Data.ExhaustSootLength.ToString());

                // Smoke Editing
                smokeEnabled = selectedRocketEngine.Data.HasSmoke;
                XmlLayout.GetElementById("smoke-toggle").SetValue(smokeEnabled.ToString());
                XmlLayout.GetElementById("smoke-speed").SetActive(XmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
                XmlLayout.GetElementById("smokeColor-Button").SetActive(XmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
                XmlLayout.GetElementById("smokeColor").SetActive(XmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
                setColorDisplays("smoke-color", "color", "tooltip", selectedRocketEngine.Data.SmokeColor);
                XmlLayout.GetElementById("smoke-speed-input").SetValue(selectedRocketEngine.Data.SmokeSpeed.ToString());

                // Updates the engine exhaust
                selectedRocketEngine.Data.Script.InitializeExhaust();
            }
        }

        private string SetFormatForDefault(string propertyValue)
        {
            //if (propertyValue == "-1") propertyValue = "Default";
            return propertyValue;
        }

        private void UpdateSymmetricRocketEngines()
        {
            if (Game.Instance.Designer.SelectedPart.SymmetrySlice != null)
            {
                Symmetry.ExecuteOnSymmetricPartModifiers(selectedRocketEngine.Data, true, delegate (RocketEngineData data)
                {
                    Traverse.Create(data).Field("_exhaustColor").SetValue(ColorToHexAlpha(selectedRocketEngine.Data.ExhaustColor));
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

        // Custom color to hex since the ModAPI doesn't include alpha, for some reason. If alpha is added to the ModAPI, then it could be converted back.
        public static string ColorToHexAlpha(Color32 color) => "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2") + color.a.ToString("X2");

        private void setColorDisplays(string updateElement, string attibuteA, string attibuteB, Color color)
        {
            XmlLayout.GetElementById(updateElement).SetAndApplyAttribute(attibuteA, ColorToHexAlpha(color));
            XmlLayout.GetElementById(updateElement).SetAndApplyAttribute(attibuteB, ColorToHexAlpha(color));
        }

        private void UpdateColorRefValues()
        {
            colorReferences["Base"] = selectedRocketEngine.Data.ExhaustColor;
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
            XmlLayout.GetElementById("sootColor-Button").SetActive(XmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
            XmlLayout.GetElementById("soot-intensity-inputfield").SetActive(XmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
            XmlLayout.GetElementById("soot-length-inputfield").SetActive(XmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
            XmlLayout.GetElementById("sootColor").SetActive(XmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());

            if (EngineSelected)
            {
                if (XmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean())
                {
                    Traverse.Create(selectedRocketEngine.Data).Field("_exhaustSootIntensity").SetValue(1);
                    XmlLayout.GetElementById("soot-intensity-input").SetText("1");
                }
                else
                {
                    Traverse.Create(selectedRocketEngine.Data).Field("_exhaustSootIntensity").SetValue(0);
                    XmlLayout.GetElementById("soot-intensity-input").SetText("0");
                }
                UpdateSymmetricRocketEngines();
            }
        }

        private void OnSmokeToggleClicked()
        {
            // Disables the visibility of the elements in the UI
            XmlLayout.GetElementById("smoke-speed").SetActive(XmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
            XmlLayout.GetElementById("smokeColor-Button").SetActive(XmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
            XmlLayout.GetElementById("smokeColor").SetActive(XmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
            if (EngineSelected)
            {
                Traverse.Create(selectedRocketEngine.Data).Field("_hasSmoke").SetValue(XmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
                UpdateSymmetricRocketEngines();
            }
        }

        private void TextBoxEdited(string boxID)
        {
            if (boxID == "exhaust-expansion-range-x-input" || boxID == "exhaust-expansion-range-y-input")
            {
                Vector2 expasionRangeValue = selectedRocketEngine.Data.ExhaustExpansionRange;
                expasionRangeValue[XmlLayout.GetElementById(boxID).GetAttribute("internalID").ToInt()] = XmlLayout.GetElementById(boxID).GetValue().ToFloat();
                Traverse.Create(selectedRocketEngine.Data).Field("_exhaustExpansionRange").SetValue(expasionRangeValue);
            }
            else
            {
                Traverse.Create(selectedRocketEngine.Data).Field(textFieldList[XmlLayout.GetElementById(boxID).GetAttribute("internalID").ToInt()]).SetValue(XmlLayout.GetElementById(boxID).GetValue().ToFloat());
            }
            UpdateFlyoutDisplayValues();
            UpdateSymmetricRocketEngines();
        }

        private void OnThrottleSliderChanged(float value)
        {
            throttleDisplayValue.SetText(Units.GetPercentageString(value));
        }

        public void OnAltitudeSliderChanged(float value)
        {
            performanceAnalysis.SetAltitudePercentage(value);
            XmlLayout.GetElementById("preview-altitude-displayValue").SetText(performanceAnalysis.GetAltitudeDisplayValue(performanceAnalysis.AtmosphereSample));
        }

        public void OnCorrectionAltitudeSliderChanged(float value)
        {
            emberAltitudeSlider.SetValueWithoutNotify(value);
            XmlLayout.GetElementById("preview-altitude-displayValue").SetText(performanceAnalysis.GetAltitudeDisplayValue(performanceAnalysis.AtmosphereSample));
        }

        public void UpdateAltitudeSliderDisplay()
        {
            OnCorrectionAltitudeSliderChanged(emberAltitudeSlider.value);
        }

        public void SymmetrySliceUpdated()
        {
            SetSymmetricExhaust(true);
        }
    }

    [HarmonyPatch(typeof(ExhaustSystemScript), "UpdateExhaust")]
    class AdjustableThrottle
    {
        public static Slider adjustableThrottleSlider;

        static bool Prefix(ref float throttle)
        {
            if (Game.InDesignerScene) if (adjustableThrottleSlider != null) throttle = adjustableThrottleSlider.value;
            return true;
        }
    }

    [HarmonyPatch(typeof(CraftPerformanceAnalysis), "CreateEnvironmentGroup")]
    class AltitudeSlider
    {
        static public SliderModel altitudeSlider;
        static void Postfix(CraftPerformanceAnalysis __instance)
        {
            altitudeSlider = Traverse.Create(__instance).Field("_altitudeSlider").GetValue<SliderModel>();
            altitudeSlider.ValueChangedByUserInput += sliderChanged;
        }
        private static void sliderChanged(ItemModel model, string name, bool finished)
        {
            EmberDesignerButton.OnCorrectionAltitudeSliderChanged(altitudeSlider.Value);
        }
    }

    [HarmonyPatch(typeof(CraftPerformanceAnalysis), "SelectNextEnvironment")]
    class AltitudeSliderDisplayUpdate
    {
        static void Postfix(CraftPerformanceAnalysis __instance)
        {
            EmberDesignerButton.UpdateAltitudeSliderDisplay();
        }
    }

    [HarmonyPatch(typeof(Symmetry), "SetSymmetryMode")]
    class SymmetrySliceUpdate
    {
        static void Postfix(Symmetry __instance)
        {
            EmberDesignerButton.SymmetrySliceUpdated();
        }
    }

    [HarmonyPatch(typeof(StagingEditorPanelScript), "OnOpened")]
    class StageEditorOpen
    {
        // Called when Opening the UI
        static void Postfix(StagingEditorPanelScript __instance)
        {
            EmberDesignerButton.StageExhaustPreview();
        }
    }

    [HarmonyPatch(typeof(ActivationGroupsPanelScript), "OnOpened")]
    class AGEditorOpened
    {
        // Called when Opening the UI
        static void Postfix(ActivationGroupsPanelScript __instance)
        {
            EmberDesignerButton.UpdateStageExhaustPreview(false);
        }
    }

    [HarmonyPatch(typeof(ValidationPanelScript), "OnOpened")]
    class ValidationOpened
    {
        // Called when Opening the UI
        static void Postfix(ValidationPanelScript __instance)
        {
            EmberDesignerButton.UpdateStageExhaustPreview(false);
        }
    }

    [HarmonyPatch(typeof(CraftPerformanceAnalysis), "OnStagingChanged")]
    class StageUpdate
    {
        // Occurs when updating within the staging menu
        static void Postfix(CraftPerformanceAnalysis __instance)
        {
            EmberDesignerButton.StageExhaustPreview();
        }
    }

    [HarmonyPatch(typeof(CraftPerformanceAnalysis), "AdvanceStage")]
    class StageAdvanced
    {
        // Occurs when using the stages scroller on Design Info
        static void Postfix(CraftPerformanceAnalysis __instance)
        {
            EmberDesignerButton.StageExhaustPreview();
        }
    }
}