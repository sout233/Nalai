using System.IO;
using Microsoft.Win32;
using Nalai.Models;
using Newtonsoft.Json;

namespace Nalai.Helpers;

public static class RegManager
{
    private const string FirefoxExtensionId = "nalai-ext@sout.eu.org";
    private const string ChromeExtensionId = "chrome-extension://nhliacfehfhgideomolhjnnfabmmeiag/";

    public static void RegisterChromeNativeMessagingConfig()
    {
        try
        {
            var config = new ChromeNativeMessagingConfig("org.eu.sout.nalai", "A native messaging host for Nalai",
                Path.Combine(Environment.CurrentDirectory, "Nalai.Launcher.exe"), "stdio", [ChromeExtensionId]);
            var configFilePath = Path.Combine(Environment.CurrentDirectory, "Configs", "org.eu.sout.nalai_chrome.json");

            if (!File.Exists(configFilePath))
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Configs"));
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
                Path.Combine(Environment.CurrentDirectory, "Nalai.Launcher.exe"), "stdio", [FirefoxExtensionId]);
            var configFilePath =
                Path.Combine(Environment.CurrentDirectory, "Configs", "nalai_ext_firefox.json");

            if (!File.Exists(configFilePath))
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Configs"));
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
            var regLoc =
                Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true) ??
                Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            regLoc.SetValue("Nalai", Path.Combine(Environment.CurrentDirectory, "Nalai.Launcher.exe"));
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