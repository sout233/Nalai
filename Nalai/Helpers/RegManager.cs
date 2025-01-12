using System.IO;
using Microsoft.Win32;
using Nalai.Models;
using Newtonsoft.Json;

namespace Nalai.Helpers;

public static class RegManager
{
    private const string FirefoxExtensionId = "nalai-ext@sout.eu.org";
    private const string ChromeExtensionId = "chrome-extension://nhliacfehfhgideomolhjnnfabmmeiag/";

    public static string GetTrueAppDir()
    {
        var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var trueAppDir = Path.GetDirectoryName(appPath)!;
        return trueAppDir;
    }

    public static string GetTrueAppFullPath()
    {
        var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        return appPath;
    }

    public static void RegisterChromeNativeMessagingConfig()
    {
        try
        {
            var config = new ChromeNativeMessagingConfig("org.eu.sout.nalai", "A native messaging host for Nalai",
                Path.Combine(GetTrueAppDir(), "Nalai.Launcher.exe"), "stdio", [ChromeExtensionId]);
            var configFilePath = Path.Combine(GetTrueAppDir(), "Configs", "org.eu.sout.nalai_chrome.json");

            if (!File.Exists(configFilePath))
            {
                Directory.CreateDirectory(Path.Combine(GetTrueAppDir(), "Configs"));
                File.Create(configFilePath).Close();
            }

            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));

            var regLoc =
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Google\Chrome\NativeMessagingHosts\org.eu.sout.nalai",
                    true) ??
                Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Google\Chrome\NativeMessagingHosts\org.eu.sout.nalai");
            regLoc.SetValue(null, configFilePath);
        }
        catch (Exception e)
        {
            NalaiMsgBox.Show(e.ToString(), "Error");
        }
    }

    public static void RegisterFirefoxNativeMessagingConfig()
    {
        try
        {
            var config = new FirefoxNativeMessagingConfig("nalai_ext_firefox", "A native messaging host for Nalai",
                Path.Combine(GetTrueAppDir(), "Nalai.Launcher.exe"), "stdio", [FirefoxExtensionId]);
            var configFilePath =
                Path.Combine(GetTrueAppDir(), "Configs", "nalai_ext_firefox.json");

            if (!File.Exists(configFilePath))
            {
                Directory.CreateDirectory(Path.Combine(GetTrueAppDir(), "Configs"));
                File.Create(configFilePath).Close();
            }

            File.WriteAllText(configFilePath, JsonConvert.SerializeObject(config, Formatting.Indented));

            var regLoc =
                Registry.CurrentUser.OpenSubKey(@"Software\Mozilla\NativeMessagingHosts\nalai_ext_firefox", true) ??
                Registry.CurrentUser.CreateSubKey(@"Software\Mozilla\NativeMessagingHosts\nalai_ext_firefox");
            regLoc.SetValue(null, configFilePath);
        }
        catch (Exception e)
        {
            NalaiMsgBox.Show(e.ToString(), "Error");
        }
    }
    
    public static void RegStartWithWindows()
    {
        try
        {
            var appPath = GetTrueAppFullPath();
            var regLoc =
                Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true) ??
                Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
        
            // 设置注册表值，确保路径正确指向你的exe文件
            regLoc.SetValue("Nalai", $"\"{appPath}\"");
        }
        catch (Exception e)
        {
            NalaiMsgBox.Show(e.ToString(), "Error");
        }
    }
    
    public static void UnRegStartWithWindows()
    {
        try
        {
            var regLoc =
                Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true) ??
                Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            regLoc.DeleteValue("Nalai", false);
        }
        catch (Exception e)
        {
            NalaiMsgBox.Show(e.ToString(), "Error");
        }
    }
}