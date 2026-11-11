using Assets.Scripts.Craft.Parts;
using Assets.Scripts.Craft.Parts.Modifiers.Propulsion;
using HarmonyLib;
using Jundroo.Services.Ads;
using ModApi.Craft.Parts;
using UnityEngine;

namespace Assets.Scripts.Design
{
    internal class ExhaustSystemVisuals
    {
        private RocketEngineScript _rocketEngineScript = null;
        private JetEngineScript _jetEngineScript = null;

        public ExhaustSystemVisuals(RocketEngineScript rocketEngine)
        {
            this._rocketEngineScript = rocketEngine;
            this._jetEngineScript = null;
        }

        public ExhaustSystemVisuals(JetEngineScript jetEngineScript)
        {
            this._jetEngineScript = jetEngineScript;
            this._rocketEngineScript = null;
        }

        public ExhaustSystemVisuals(IPartScript partScript)
        {
            _jetEngineScript = partScript.GetModifier<JetEngineScript>();
            _rocketEngineScript = partScript.GetModifier<RocketEngineScript>();
        }

        public Color ExhaustColor
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustColor;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustColor;
                return Color.magenta;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustColor", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustColor", value);
            }
        }

        public Color ExhaustColorExpanded
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustColorExpanded;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustColorExpanded;
                return Color.magenta;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustColorExpanded", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustColorExpanded", value);
            }
        }
        public Color ExhaustColorTip
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustColorTip;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustColorTip;
                return Color.magenta;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustColorTip", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustColorTip", value);
            }
        }
        public Color ExhaustColorShock
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustColorShock;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustColorShock;
                return Color.magenta;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustColorShock", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustColorShock", value);
            }
        }
        public Color ExhaustColorFlame
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustColorFlame;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustColorFlame;
                return Color.magenta;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustColorFlame", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustColorFlame", value);
            }
        }
        public Color ExhaustColorSoot
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustColorSoot;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustColorSoot;
                return Color.magenta;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustColorSoot", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustColorSoot", value);
            }
        }
        public Color SmokeColor
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.TryGetSmokeColor(out Color color) ? color : Color.magenta;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.SmokeColor;
                return Color.magenta;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_smokeColor", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_smokeColor", value);
            }
        }
        public Vector2? ExhaustExpansionRange
        {
            get
            {
                if (_jetEngineScript != null) return new(-1, -1); // _jetEngineScript.Data.ExhaustExpansionRange; // TOOD: Investigate why this field doesn't show up in the data, but is in the xml overrides
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustExpansionRange;
                return null;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_smokeColor", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_smokeColor", value);
            }
        }
        public float ExhaustGlobalIntensity
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustGlobalIntensity;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustGlobalIntensity;
                return -1f;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustGlobalIntensity", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustGlobalIntensity", value);
            }
        }
        public float ExhaustScale
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustScale;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustScale;
                return -1f;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustScale", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustScale", value);
            }
        }
        public float ExhaustOffset
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustOffset;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustOffset;
                return -1f;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustOffset", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustOffset", value);
            }
        }
        public float ExhaustShockIntensity
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustShockIntensity;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustShockIntensity;
                return -1f;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustShockIntensity", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustShockIntensity", value);
            }
        }
        public float ExhaustShockDirectionOffset
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustShockDirectionOffset;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustShockDirectionOffset;
                return -1f;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustShockDirectionOffset", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustShockDirectionOffset", value);
            }
        }
        public float ExhaustRimShade
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustRimShade;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustRimShade;
                return -1f;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustRimShade", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustRimShade", value);
            }
        }
        public float ExhaustSootIntensity
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustSootIntensity;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustSootIntensity;
                return -1f;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustSootIntensity", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustSootIntensity", value);
            }
        }
        public float ExhaustSootLength
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustSootLength;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustSootLength;
                return -1f;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustSootLength", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustSootLength", value);
            }
        }
        public float ExhaustTextureStrength
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.ExhaustTextureStrength;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.ExhaustTextureStrength;
                return -1f;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustTextureStrength", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_exhaustTextureStrength", value);
            }
        }

        // Jet engine Has, Rocket Engine Doesn't
        public float? NozzleDiscStrength
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.NozzleDiscStrength;
                return null;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_exhaustGlobalIntensity", value);
            }
        }
        public bool HasSmoke
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.HasSmoke;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.HasSmoke;
                return true;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_hasSmoke", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_hasSmoke", value);
            }
        }
        public float SmokeSpeed
        {
            get
            {
                if (_jetEngineScript != null) return _jetEngineScript.Data.SmokeSpeed;
                else if (_rocketEngineScript != null) return _rocketEngineScript.Data.SmokeSpeed;
                return -1f;
            }
            set
            {
                if (_jetEngineScript != null) UpdateField(_jetEngineScript.Data, "_smokeSpeed", value);
                else if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_smokeSpeed", value);
            }
        }
        public float? SmokeOffset
        {
            get
            {
                if (_rocketEngineScript != null) return _rocketEngineScript.Data.SmokeOffset;
                return null;
            }
            set
            {
                if (_rocketEngineScript != null) UpdateField(_rocketEngineScript.Data, "_smokeOffset", value);
            }
        }

        private void UpdateField(object data, string field, object val) => Traverse.Create(data).Field(field).SetValue(val);

        private object GetField(object data, string field) => Traverse.Create(data).Field(field).GetValue();
    }
}
