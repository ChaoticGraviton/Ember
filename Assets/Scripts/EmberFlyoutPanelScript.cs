using Assets.Scripts.Craft.Parts.Modifiers.Propulsion;
using HarmonyLib;
using ModApi.Craft.Parts;
using ModApi.Design;
using ModApi.Math;
using ModApi.Ui;
using Ookii.Dialogs;
using Rewired;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UI.Xml;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Assets.Scripts.Design
{
    public class EmberFlyoutPanelScript : DesignerFlyoutPanelScript
    {
        XmlElement _content;
        XmlElement _noEngine;
        private IDesigner _designer;
        public Slider throttleSlider;
        public Slider emberAltitudeSlider;
        private TextMeshProUGUI throttleDisplayValue;
        private TextMeshProUGUI altitudeDisplayValue;
        CraftPerformanceAnalysis performanceAnalizer;
        public static RocketEngineScript selectedRocketEngine;
        private XmlSerializer _xmlSerializer;
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

        private void OnFlyoutOpening(IFlyout flyout)
        {
            if (_designer.SelectedPart?.GetModifier<RocketEngineScript>() != null)
            {
                selectedRocketEngine = _designer.SelectedPart.GetModifier<RocketEngineScript>();
                SetSymmetricExhaust(true);
                SetActiveForSelectedEngine(true);
                UpdateFlyoutDisplayValues();
            }
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

        public void OnCorrectionAltitudeSliderChanged(float value)
        {
            emberAltitudeSlider.SetValueWithoutNotify(value);
            altitudeDisplayValue.SetText(performanceAnalizer.GetAltitudeDisplayValue(performanceAnalizer.AtmosphereSample));
        }

        public void UpdateAltitudeSliderDisplay() => OnCorrectionAltitudeSliderChanged(emberAltitudeSlider.value);

        public void SymmetrySliceUpdated() => SetSymmetricExhaust(true);
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
                setColorDisplays("main-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColor);
                setColorDisplays("expand-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColorExpanded);
                setColorDisplays("tip-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColorTip);
                setColorDisplays("flame-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColorFlame);
                xmlLayout.GetElementById("exhaust-global-intensity-input").SetValue(SetFormatForDefault(selectedRocketEngine.Data.ExhaustGlobalIntensity.ToString()));
                xmlLayout.GetElementById("exhaust-texture-strength-input").SetValue(selectedRocketEngine.Data.ExhaustTextureStrength.ToString());
                xmlLayout.GetElementById("exhaust-scale-input").SetValue(selectedRocketEngine.Data.ExhaustScale.ToString());
                xmlLayout.GetElementById("exhaust-offset-input").SetValue(selectedRocketEngine.Data.ExhaustOffset.ToString());
                xmlLayout.GetElementById("exhaust-rimshade-input").SetValue(SetFormatForDefault(selectedRocketEngine.Data.ExhaustRimShade.ToString()));
                xmlLayout.GetElementById("exhaust-disc-strength-input").SetValue(selectedRocketEngine.Data.NozzleDiscStrength.ToString());
                xmlLayout.GetElementById("exhaust-expansion-range-x-input").SetValue(selectedRocketEngine.Data.ExhaustExpansionRange[0].ToString());
                xmlLayout.GetElementById("exhaust-expansion-range-y-input").SetValue(selectedRocketEngine.Data.ExhaustExpansionRange[1].ToString());

                // Shock Editing
                setColorDisplays("diamond-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColorShock);
                xmlLayout.GetElementById("diamond-offset-input").SetValue(selectedRocketEngine.Data.ExhaustShockDirectionOffset.ToString());
                xmlLayout.GetElementById("diamond-intensity-input").SetValue(SetFormatForDefault(selectedRocketEngine.Data.ExhaustShockIntensity.ToString()));

                // Soot Editing
                sootEnabled = selectedRocketEngine.Data.ExhaustSootIntensity > 0;
                xmlLayout.GetElementById("soot-toggle").SetValue(sootEnabled.ToString());
                xmlLayout.GetElementById("sootColor-Button").SetActive(xmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
                xmlLayout.GetElementById("soot-intensity-inputfield").SetActive(xmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
                xmlLayout.GetElementById("soot-length-inputfield").SetActive(xmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
                xmlLayout.GetElementById("sootColor").SetActive(xmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
                setColorDisplays("soot-color", "color", "tooltip", selectedRocketEngine.Data.ExhaustColorSoot);
                xmlLayout.GetElementById("soot-intensity-input").SetValue(selectedRocketEngine.Data.ExhaustSootIntensity.ToString());
                xmlLayout.GetElementById("soot-length-input").SetValue(selectedRocketEngine.Data.ExhaustSootLength.ToString());

                // Smoke Editing
                smokeEnabled = selectedRocketEngine.Data.HasSmoke;
                xmlLayout.GetElementById("smoke-toggle").SetValue(smokeEnabled.ToString());
                xmlLayout.GetElementById("smoke-speed").SetActive(xmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
                xmlLayout.GetElementById("smokeColor-Button").SetActive(xmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
                xmlLayout.GetElementById("smokeColor").SetActive(xmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
                setColorDisplays("smoke-color", "color", "tooltip", selectedRocketEngine.Data.SmokeColor);
                xmlLayout.GetElementById("smoke-speed-input").SetValue(selectedRocketEngine.Data.SmokeSpeed.ToString());

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
        private void setColorDisplays(string updateElement, string attibuteA, string attibuteB, Color color)
        {
            xmlLayout.GetElementById(updateElement).SetAndApplyAttribute(attibuteA, ColorToHexAlpha(color));
            xmlLayout.GetElementById(updateElement).SetAndApplyAttribute(attibuteB, ColorToHexAlpha(color));
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
            xmlLayout.GetElementById("sootColor-Button").SetActive(xmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
            xmlLayout.GetElementById("soot-intensity-inputfield").SetActive(xmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
            xmlLayout.GetElementById("soot-length-inputfield").SetActive(xmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());
            xmlLayout.GetElementById("sootColor").SetActive(xmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean());

            if (hasEngineSelected)
            {
                if (xmlLayout.GetElementById("soot-toggle").GetValue().ToBoolean())
                {
                    Traverse.Create(selectedRocketEngine.Data).Field("_exhaustSootIntensity").SetValue(1);
                    xmlLayout.GetElementById("soot-intensity-input").SetText("1");
                }
                else
                {
                    Traverse.Create(selectedRocketEngine.Data).Field("_exhaustSootIntensity").SetValue(0);
                    xmlLayout.GetElementById("soot-intensity-input").SetText("0");
                }
                UpdateSymmetricRocketEngines();
            }
        }

        private void OnSmokeToggleClicked()
        {
            // Disables the visibility of the elements in the UI
            xmlLayout.GetElementById("smoke-speed").SetActive(xmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
            xmlLayout.GetElementById("smokeColor-Button").SetActive(xmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
            xmlLayout.GetElementById("smokeColor").SetActive(xmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
            if (hasEngineSelected)
            {
                Traverse.Create(selectedRocketEngine.Data).Field("_hasSmoke").SetValue(xmlLayout.GetElementById("smoke-toggle").GetValue().ToBoolean());
                UpdateSymmetricRocketEngines();
            }
        }

        private void TextBoxEdited(string boxID)
        {
            if (boxID.Contains("exhaust-expansion-range"))
            //if (boxID == "exhaust-expansion-range-x-input" || boxID == "exhaust-expansion-range-y-input")
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

        private void UpdateSymmetricRocketEngines()
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

        internal void ImportPreset(PlumePresetData.PlumeData plumeData)
        {
            throw new NotImplementedException();
        }
    }
}