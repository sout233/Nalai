using System;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using Nalai.Models;
using Wpf.Ui.Appearance;

namespace Nalai.Helpers
{
    public static class ConfigHelper
    {
        public static SettingsConfig GlobalConfig { get; private set; } = new();

        private static readonly string ConfigPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                , "Nalai", "Configs", "NalaiAppConfig.json");

        public static void LoadConfig()
        {
            try
            {
                // 确保目录存在
                var configDir = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // 如果文件不存在，则创建默认配置并保存
                if (!File.Exists(ConfigPath))
                {
                    SaveConfig();
                    return;
                }

                // 读取并反序列化配置
                var json = File.ReadAllText(ConfigPath);
                var config = JsonConvert.DeserializeObject<SettingsConfig>(json);
                if (config != null)
                {
                    GlobalConfig = config;
                }

                Console.WriteLine($@"Config loaded from: {ConfigPath}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading configuration: {ex.Message}");
                NalaiMsgBox.Show($"Error loading configuration: {ex.Message}");
            }
        }

        public static void SaveConfig()
        {
            try
            {
                // 确保目录存在
                var configDir = Path.GetDirectoryName(ConfigPath) ?? "Config";
                if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // 序列化并保存配置
                var jsonConfig = JsonConvert.SerializeObject(GlobalConfig, Formatting.Indented);
                File.WriteAllText(ConfigPath, jsonConfig);

                Console.WriteLine($@"Config saved to: {ConfigPath}");
            }
            catch (Exception ex)
            {
                NalaiMsgBox.Show($"Error saving configuration: {ex.Message}");
            }
        }

        public static void ApplyConfig()
        {
            // General
            var general = GlobalConfig.General;

            if (general.IsStartWithWindows)
            {
                RegManager.RegStartWithWindows();
            }
            else
            {
                RegManager.UnRegStartWithWindows();
            }

            I18NHelper.SetLanguageByCode(general.Language);

            // Appearance
            var appearance = GlobalConfig.Appearance;

            ApplicationThemeManager.Apply(appearance.Theme);

            // Download
            var download = GlobalConfig.Download;
        }
    }
}