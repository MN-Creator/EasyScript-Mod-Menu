using GTA;

namespace EasyScript
{
    internal class SettingsManager
    {
        private static SettingsManager _instance;
        public static SettingsManager Instance => _instance ?? (_instance = new SettingsManager());
        public static bool IsSaving = true;
        private readonly ScriptSettings _settings;
        private static readonly string _filepath = "Scripts/EasyScript.ini";

        public SettingsManager()
        {
            _settings = ScriptSettings.Load(_filepath);
        }

        public static void Save()
        {
            Instance._settings.Save();
            Logger.Log("Settings saved.");
        }

        /// <summary>
        /// Get a value from the settings file.
        /// </summary>
        /// <param name="save">Save the value in the file.</param>
        /// <returns>Value if found otherwise default</returns>
        public static T GetValue<T>(string section, string name, T defaultvalue, bool save = true)
        {
            if (!IsSaving) return defaultvalue;
            T value = Instance._settings.GetValue(section, name, defaultvalue);
            if (save)
            {
                SetValue(section, name, value);
            }
            return value;
        }

        public static void SetValue<T>(string section, string name, T value)
        {
            if (!IsSaving) return;
            Instance._settings.SetValue(section, name, value);
        }
    }
}
