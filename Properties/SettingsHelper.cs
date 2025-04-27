using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml;

namespace BiblioGest.Properties
{
    public class Settings : ApplicationSettingsBase
    {
        private static Settings defaultInstance = (Settings)Synchronized(new Settings());

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("False")]
        public bool RememberUsername
        {
            get
            {
                return ((bool)(this["RememberUsername"]));
            }
            set
            {
                this["RememberUsername"] = value;
            }
        }

        [UserScopedSetting()]
        [DefaultSettingValue("")]
        public string LastUsername
        {
            get
            {
                return ((string)(this["LastUsername"]));
            }
            set
            {
                this["LastUsername"] = value;
            }
        }
    }

    // Simple Settings Management class for use in lieu of the full Settings implementation
    public class SettingsManager
    {
        private static Dictionary<string, object> _settings = new Dictionary<string, object>();
        private static string _settingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BiblioGest",
            "settings.xml");

        static SettingsManager()
        {
            LoadSettings();
        }

        public static T GetSetting<T>(string key, T defaultValue = default)
        {
            if (_settings.TryGetValue(key, out object value))
            {
                if (value is T typedValue)
                {
                    return typedValue;
                }
                
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }

        public static void SetSetting<T>(string key, T value)
        {
            _settings[key] = value;
        }

        public static void SaveSettings()
        {
            try
            {
                // Ensure the directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath));

                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("Settings");
                doc.AppendChild(root);

                foreach (var setting in _settings)
                {
                    XmlElement element = doc.CreateElement("Setting");
                    element.SetAttribute("Key", setting.Key);
                    element.SetAttribute("Type", setting.Value?.GetType().FullName ?? "null");
                    element.InnerText = setting.Value?.ToString() ?? string.Empty;
                    root.AppendChild(element);
                }

                doc.Save(_settingsPath);
            }
            catch (Exception ex)
            {
                // In a real app, we would log this error
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private static void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(_settingsPath);

                    foreach (XmlNode node in doc.SelectNodes("//Setting"))
                    {
                        string key = node.Attributes["Key"]?.Value;
                        string type = node.Attributes["Type"]?.Value;
                        string value = node.InnerText;

                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(type) && type != "null")
                        {
                            try
                            {
                                Type t = Type.GetType(type);
                                if (t == typeof(bool))
                                {
                                    _settings[key] = bool.Parse(value);
                                }
                                else if (t == typeof(int))
                                {
                                    _settings[key] = int.Parse(value);
                                }
                                else if (t == typeof(double))
                                {
                                    _settings[key] = double.Parse(value);
                                }
                                else if (t == typeof(DateTime))
                                {
                                    _settings[key] = DateTime.Parse(value);
                                }
                                else
                                {
                                    _settings[key] = value;
                                }
                            }
                            catch
                            {
                                _settings[key] = value;
                            }
                        }
                        else if (!string.IsNullOrEmpty(key))
                        {
                            _settings[key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // In a real app, we would log this error
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
        }
    }
}