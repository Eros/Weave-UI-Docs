using System.Text.RegularExpressions;
using Unity.Properties;
using UnityEngine;

namespace WeaveUI.Samples.SettingsMenu
{
    /// <summary>One rebindable action. The key button binds its text to <see cref="KeyText"/>.</summary>
    public sealed class KeyBinding : ViewModel
    {
        private KeyCode _key;
        private bool _listening;

        public KeyBinding(string action, KeyCode key) => (Action, _key) = (action, key);

        [CreateProperty]
        public string Action { get; }

        public KeyCode Key
        {
            get => _key;
            set
            {
                if (SetProperty(ref _key, value))
                {
                    Notify(nameof(KeyText));
                }
            }
        }

        /// <summary>True while waiting for the player to press the new key.</summary>
        public bool Listening
        {
            get => _listening;
            set
            {
                if (SetProperty(ref _listening, value))
                {
                    Notify(nameof(KeyText));
                }
            }
        }

        [CreateProperty]
        public string KeyText => _listening ? "Press a key…" : Pretty(_key);

        /// <summary>"LeftShift" → "Left Shift", "Alpha1" → "1".</summary>
        static string Pretty(KeyCode key)
        {
            var name = key.ToString();
            if (name.StartsWith("Alpha"))
            {
                return name.Substring(5);
            }
            return Regex.Replace(name, "(?<=[a-z])(?=[A-Z0-9])", " ");
        }
    }
}
