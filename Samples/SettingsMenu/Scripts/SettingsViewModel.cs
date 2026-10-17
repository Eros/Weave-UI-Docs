using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Scripting;

namespace WeaveUI.Samples.SettingsMenu
{
    /// <summary>The saved settings: plain serialisable fields, so JsonUtility can store them and compare two copies.</summary>
    [Serializable]
    public sealed class SettingsData
    {
        public float master = 80, music = 60, effects = 75, voice = 90;
        public bool mute;
        public int resolution = 1, windowMode, quality = 2;
        public bool vsync = true;
        public float frameCap = 144, fieldOfView = 90, brightness = 50;
        public float sensitivity = 40;
        public bool invertY, vibration = true;
        public bool subtitles = true;
        public int subtitleSize = 1, colourFilter;
        public bool lightTheme, reduceMotion;

        public SettingsData Clone() => (SettingsData)MemberwiseClone();

        public string ToJson() => JsonUtility.ToJson(this);
    }

    public sealed class SettingsViewModel : ViewModel
    {
        const string PrefsKey = "weave-ui-sample-settings";

        private SettingsData _applied;
        private SettingsData _edit;
        private string _status = "Changes preview live. Apply keeps them; Revert undoes them.";

        public SettingsViewModel()
        {
            var saved = PlayerPrefs.GetString(PrefsKey, "");
            _applied = string.IsNullOrEmpty(saved) ? new SettingsData() : JsonUtility.FromJson<SettingsData>(saved);
            _edit = _applied.Clone();
        }

        // Audio

        [CreateProperty] public float Master { get => _edit.master; set => Set(ref _edit.master, value, nameof(MasterText)); }
        [CreateProperty] public string MasterText => Percent(_edit.master);
        [CreateProperty] public float Music { get => _edit.music; set => Set(ref _edit.music, value, nameof(MusicText)); }
        [CreateProperty] public string MusicText => Percent(_edit.music);
        [CreateProperty] public float Effects { get => _edit.effects; set => Set(ref _edit.effects, value, nameof(EffectsText)); }
        [CreateProperty] public string EffectsText => Percent(_edit.effects);
        [CreateProperty] public float Voice { get => _edit.voice; set => Set(ref _edit.voice, value, nameof(VoiceText)); }
        [CreateProperty] public string VoiceText => Percent(_edit.voice);
        [CreateProperty] public bool Mute { get => _edit.mute; set => Set(ref _edit.mute, value, nameof(AudioEnabled)); }

        /// <summary>The volume sliders are disabled while muted.</summary>
        [CreateProperty] public bool AudioEnabled => !_edit.mute;

        // Video

        [CreateProperty] public int Resolution { get => _edit.resolution; set => Set(ref _edit.resolution, value); }
        [CreateProperty] public int WindowMode { get => _edit.windowMode; set => Set(ref _edit.windowMode, value); }
        [CreateProperty] public int Quality { get => _edit.quality; set => Set(ref _edit.quality, value); }
        [CreateProperty] public bool VSync { get => _edit.vsync; set => Set(ref _edit.vsync, value, nameof(FrameCapEnabled)); }
        [CreateProperty] public bool FrameCapEnabled => !_edit.vsync;
        [CreateProperty] public float FrameCap { get => _edit.frameCap; set => Set(ref _edit.frameCap, Mathf.Round(value), nameof(FrameCapText)); }
        [CreateProperty] public string FrameCapText => $"{_edit.frameCap:0} fps";
        [CreateProperty] public float FieldOfView { get => _edit.fieldOfView; set => Set(ref _edit.fieldOfView, Mathf.Round(value), nameof(FieldOfViewText)); }
        [CreateProperty] public string FieldOfViewText => $"{_edit.fieldOfView:0}°";
        [CreateProperty] public float Brightness { get => _edit.brightness; set => Set(ref _edit.brightness, value, nameof(BrightnessText)); }
        [CreateProperty] public string BrightnessText => Percent(_edit.brightness);

        // Controls

        [CreateProperty] public float Sensitivity { get => _edit.sensitivity; set => Set(ref _edit.sensitivity, value, nameof(SensitivityText)); }
        [CreateProperty] public string SensitivityText => $"{_edit.sensitivity / 20:0.0}×";
        [CreateProperty] public bool InvertY { get => _edit.invertY; set => Set(ref _edit.invertY, value); }
        [CreateProperty] public bool Vibration { get => _edit.vibration; set => Set(ref _edit.vibration, value); }

        /// <summary>Key bindings apply as soon as they're changed, so they aren't part of Apply/Revert.</summary>
        public IReadOnlyList<KeyBinding> Bindings { get; } = new[]
        {
            new KeyBinding("Move forward", KeyCode.W),
            new KeyBinding("Move back", KeyCode.S),
            new KeyBinding("Strafe left", KeyCode.A),
            new KeyBinding("Strafe right", KeyCode.D),
            new KeyBinding("Jump", KeyCode.Space),
            new KeyBinding("Sprint", KeyCode.LeftShift),
            new KeyBinding("Interact", KeyCode.E),
            new KeyBinding("Inventory", KeyCode.Tab),
        };

        // Accessibility

        [CreateProperty] public bool Subtitles { get => _edit.subtitles; set => Set(ref _edit.subtitles, value); }
        [CreateProperty] public int SubtitleSize { get => _edit.subtitleSize; set => Set(ref _edit.subtitleSize, value); }
        [CreateProperty] public int ColourFilter { get => _edit.colourFilter; set => Set(ref _edit.colourFilter, value); }
        [CreateProperty] public bool LightTheme { get => _edit.lightTheme; set => Set(ref _edit.lightTheme, value); }
        [CreateProperty] public bool ReduceMotion { get => _edit.reduceMotion; set => Set(ref _edit.reduceMotion, value); }

        // Footer

        [CreateProperty]
        public bool HasChanges => _edit.ToJson() != _applied.ToJson();

        [CreateProperty]
        public string Status => HasChanges ? "You have unsaved changes." : _status;

        /// <summary>click="Apply" in Settings.uxml. Handler names are looked up on the view model first, then the screen.</summary>
        [Preserve]
        public void Apply()
        {
            _applied = _edit.Clone();
            PlayerPrefs.SetString(PrefsKey, _applied.ToJson());
            PlayerPrefs.Save();
            _status = "Settings saved.";
            NotifyAll();
        }

        [Preserve]
        public void Revert()
        {
            _edit = _applied.Clone();
            _status = "Changes reverted.";
            NotifyAll();
        }

        public void RestoreDefaults()
        {
            _edit = new SettingsData();
            _status = "Defaults restored. Apply to keep them.";
            NotifyAll();
        }

        /// <summary>A change="QualityChanged" handler from the UXML: the new value and, with [Previous], the old one are injected.</summary>
        [Preserve]
        public void QualityChanged(string quality, [Previous] string was) => SetStatus($"Quality: {was} → {quality}");

        void SetStatus(string status)
        {
            _status = status;
            Notify(nameof(Status));
        }

        /// <summary>Sets a field of the working copy, then notifies the property, its dependants and the footer.</summary>
        void Set<T>(ref T field, T value, string dependant = null, [System.Runtime.CompilerServices.CallerMemberName] string property = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            Notify(property);
            if (dependant != null)
            {
                Notify(dependant);
            }
            Notify(nameof(HasChanges));
            Notify(nameof(Status));
        }

        /// <summary>After Apply, Revert or Restore defaults every value may have changed.</summary>
        void NotifyAll()
        {
            foreach (var property in AllProperties)
            {
                Notify(property);
            }
        }

        static string Percent(float value) => $"{value:0}%";

        private static readonly string[] AllProperties =
        {
            nameof(Master), nameof(MasterText), nameof(Music), nameof(MusicText), nameof(Effects), nameof(EffectsText),
            nameof(Voice), nameof(VoiceText), nameof(Mute), nameof(AudioEnabled), nameof(Resolution), nameof(WindowMode),
            nameof(Quality), nameof(VSync), nameof(FrameCapEnabled), nameof(FrameCap), nameof(FrameCapText),
            nameof(FieldOfView), nameof(FieldOfViewText), nameof(Brightness), nameof(BrightnessText), nameof(Sensitivity),
            nameof(SensitivityText), nameof(InvertY), nameof(Vibration), nameof(Subtitles), nameof(SubtitleSize),
            nameof(ColourFilter), nameof(LightTheme), nameof(ReduceMotion), nameof(HasChanges), nameof(Status),
        };
    }
}
