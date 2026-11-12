using Assets.Scripts.Craft.Parts.Modifiers.Propulsion;
using HarmonyLib;
using ModApi.Craft.Parts;
using System;
using System.Net.Sockets;
using UnityEngine;

namespace Assets.Scripts.Design
{
    public class ExhaustSetting
    {
        public IPartScript Part { get; }
        public RocketEngineScript Rocket { get; }
        public JetEngineScript Jet { get; }
        public bool IsRocket => Rocket != null;
        public bool IsJet => Jet != null;
        public PartModifierData Data => IsRocket ? Rocket.Data : IsJet ? Jet.Data : null;

        private readonly Traverse _data;

        public ExhaustSetting(IPartScript part)
        {
            Part = part;
            Rocket = part.GetModifier<RocketEngineScript>();
            Jet = part.GetModifier<JetEngineScript>();

            if (IsRocket) _data = Traverse.Create(Rocket.Data);
            else if (IsJet) _data = Traverse.Create(Jet.Data);
        }

        public IFuelSource FuelSource
        {
            get
            {
                if (IsRocket) return Rocket.FuelSource;
                if (IsJet) return Jet.FuelSource;
                return null;
            }
        }

        public Color ExhaustColor
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustColor;
                else if (IsRocket) return Rocket.Data.ExhaustColor;
                return Color.magenta;
            }
        }

        public Color ExhaustColorExpanded
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustColorExpanded;
                else if (IsRocket) return Rocket.Data.ExhaustColorExpanded;
                return Color.magenta;
            }
        }
        public Color ExhaustColorTip
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustColorTip;
                else if (IsRocket) return Rocket.Data.ExhaustColorTip;
                return Color.magenta;
            }
        }
        public Color ExhaustColorFlame
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustColorFlame;
                else if (IsRocket) return Rocket.Data.ExhaustColorFlame;
                return Color.magenta;
            }
        }
        public Color ExhaustColorShock
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustColorShock;
                else if (IsRocket) return Rocket.Data.ExhaustColorShock;
                return Color.magenta;
            }
        }
        public Color ExhaustColorSoot
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustColorSoot;
                else if (IsRocket) return Rocket.Data.ExhaustColorSoot;
                return Color.magenta;
            }
        }

        public Color SmokeColor
        {
            get
            {
                if (IsJet) return Jet.Data.TryGetSmokeColor(out Color color) ? color : FuelSource.FuelType.ExhaustColorSmoke;
                else if (IsRocket) return Rocket.Data.SmokeColor;
                return Color.magenta;
            }
        }

        public float ExhaustGlobalIntensity
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustGlobalIntensity;
                else if (IsRocket) return Rocket.Data.ExhaustGlobalIntensity;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.GlobalIntensity, value);
                else if (IsRocket) Set(ExhaustFields.GlobalIntensity, value);
            }
        }
        public Vector2 ExhaustExpansionRange
        {
            get
            {
                if (IsJet) return new(-1, -1); // _jetEngineScript.Data.ExhaustExpansionRange; // TOOD: Investigate why this field doesn't show up in the data, but is in the xml overrides
                else if (IsRocket) return Rocket.Data.ExhaustExpansionRange;
                return new(-1, -1);
            }
            set
            {
                if (IsJet) Set(ExhaustFields.ExpansionRange, value);
                else if (IsRocket) Set(ExhaustFields.ExpansionRange, value);
            }

        }
        public float ExhaustScale
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustScale;
                else if (IsRocket) return Rocket.Data.ExhaustScale;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.Scale, value);
                else if (IsRocket) Set(ExhaustFields.Scale, value);
            }
        }
        public float ExhaustOffset
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustOffset;
                else if (IsRocket) return Rocket.Data.ExhaustOffset;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.Offset, value);
                else if (IsRocket) Set(ExhaustFields.Offset, value);
            }
        }
        public float ExhaustShockIntensity
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustShockIntensity;
                else if (IsRocket) return Rocket.Data.ExhaustShockIntensity;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.ShockIntensity, value);
                else if (IsRocket) Set(ExhaustFields.ShockIntensity, value);
            }
        }
        public float ExhaustShockDirectionOffset
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustShockDirectionOffset;
                else if (IsRocket) return Rocket.Data.ExhaustShockDirectionOffset;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.ShockDirectionOffset, value);
                else if (IsRocket) Set(ExhaustFields.ShockDirectionOffset, value);
            }
        }
        public float ExhaustRimShade
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustRimShade;
                else if (IsRocket) return Rocket.Data.ExhaustRimShade;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.RimShade, value);
                else if (IsRocket) Set(ExhaustFields.RimShade, value);
            }
        }
        public float ExhaustSootIntensity
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustSootIntensity;
                else if (IsRocket) return Rocket.Data.ExhaustSootIntensity;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.SootIntensity, value);
                else if (IsRocket) Set(ExhaustFields.SootIntensity, value);
            }
        }
        public float ExhaustSootLength
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustSootLength;
                else if (IsRocket) return Rocket.Data.ExhaustSootLength;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.SootLength, value);
                else if (IsRocket) Set(ExhaustFields.SootLength, value);
            }
        }
        public float ExhaustTextureStrength
        {
            get
            {
                if (IsJet) return Jet.Data.ExhaustTextureStrength;
                else if (IsRocket) return Rocket.Data.ExhaustTextureStrength;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.TextureStrength, value);
                else if (IsRocket) Set(ExhaustFields.TextureStrength, value);
            }
        }

        // Jet engine Has, Rocket Engine Doesn't
        public float NozzleDiscStrength
        {
            get
            {
                if (IsJet) return Jet.Data.NozzleDiscStrength;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.NozzleDiscStrength, value);
            }
        }
        public bool HasSmoke
        {
            get
            {
                if (IsJet) return Jet.Data.HasSmoke;
                else if (IsRocket) return Rocket.Data.HasSmoke;
                return true;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.HasSmoke, value);
                else if (IsRocket) Set(ExhaustFields.HasSmoke, value);
            }
        }
        public float SmokeSpeed
        {
            get
            {
                if (IsJet) return Jet.Data.SmokeSpeed;
                else if (IsRocket) return Rocket.Data.SmokeSpeed;
                return -1f;
            }
            set
            {
                if (IsJet) Set(ExhaustFields.SmokeSpeed, value);
                else if (IsRocket) Set(ExhaustFields.SmokeSpeed, value);
            }
        }
        public float SmokeOffset
        {
            get
            {
                if (IsRocket) return Rocket.Data.SmokeOffset;
                return -1;
            }
            set
            {
                if (IsRocket) Set(ExhaustFields.SmokeOffset, value);
            }
        }

        public void setExhaustColor(string color) => Set(ExhaustFields.Color, color);
        public void setExhaustExpandedColor(string color) => Set(ExhaustFields.ColorExpanded, color);
        public void setExhaustTipColor(string color) => Set(ExhaustFields.ColorTip, color);
        public void setExhaustFlameColor(string color) => Set(ExhaustFields.ColorFlame, color);
        public void setExhaustShockColor(string color) => Set(ExhaustFields.ColorShock, color);
        public void setExhaustSootColor(string color) => Set(ExhaustFields.ColorSoot, color);
        public void setExhaustSmokeColor(string color) => Set(ExhaustFields.ColorSmoke, color);
        public T Get<T>(string name, T fallback = default)
        {
            var field = _data.Field(name);

            if (!field.FieldExists())
                return fallback;

            return field.GetValue<T>();
        }

        public void Set<T>(string field, T value)
        {
            var f = _data.Field(field);

            if (f.FieldExists()) f.SetValue(value);
        }

        public void SetExhaustPreview(bool state)
        {
            if (IsRocket) Rocket.PreviewExhaust = state;
            else if (IsJet) Jet.PreviewExhaust = state;
        }

        public void InitializeExhaust()
        {
            if (IsRocket) Rocket.InitializeExhaust();
            else if (IsJet) Traverse.Create(Jet).Method("ApplyExhaustFuelSettings").GetValue(); // TODO: Find equivalent method.
        }

        public static void ExecuteOnSymmetricEngines(ExhaustSetting engine, bool includeSourceModifier, Action<ExhaustSetting> action)
        {
            Symmetry.ExecuteOnSymmetricPartModifiers(
                engine.Data,
                includeSourceModifier,
                modifier => action(new ExhaustSetting(modifier.Part.PartScript)));
        }
    }
}
