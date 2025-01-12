using System.Globalization;

namespace Nalai.Helpers;

public static class I18NHelper
{
    public static readonly List<Language> AvailableLanguages =
    [
        new() { EnglishName = "English", NativeName = "English", Code = "en" },
        new() { EnglishName = "Chinese", NativeName = "简体中文", Code = "zh-hans" },
        new() { EnglishName = "Japanese", NativeName = "日本語", Code = "ja" }
    ];

    private static Language _currentLanguage = AvailableLanguages[0];

    public static Language CurrentLanguage
    {
        get => _currentLanguage;
        private set
        {
            _currentLanguage = value;
            I18NExtension.Culture = new CultureInfo(_currentLanguage.Code);
        }
    }

    public static string GetTranslation(string key)
    {
        return I18NExtension.Translate(key) ?? string.Empty;
    }

    public static void SetLanguageBySystemCulture()
    {
        var cultureInfo = CultureInfo.CurrentUICulture;
        Console.WriteLine(cultureInfo.Name);

        CurrentLanguage = AvailableLanguages.Find(l => l.Code == cultureInfo.Name.Split('-')[0]) ??
                          AvailableLanguages[0];
    }

    public static void SetLanguageByCode(string code)
    {
        CurrentLanguage = AvailableLanguages.Find(l => l.Code == code) ?? AvailableLanguages[0];
    }

    public static void SetLanguage(Language value)
    {
        CurrentLanguage = value;
    }

    public static void SetLanguageByNativeName(string value)
    {
        CurrentLanguage = AvailableLanguages.Find(l => l.NativeName == value) ?? AvailableLanguages[0];
    }

    public static string GetCodeByNativeName(string value)
    {
        return AvailableLanguages.Find(l => l.NativeName == value)?.Code ?? string.Empty;
    }
}

public class Language
{
    public string EnglishName { get; set; }
    public string NativeName { get; set; }
    public string Code { get; set; }
}