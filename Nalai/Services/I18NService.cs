using System.Globalization;
using Nalai.Resources;

namespace Nalai.Services;

public static class I18NService
{
    private static readonly List<Language> Languages =
    [
        new() { EnglishName = "English", NativeName = "English", Code = "en" },
        new() { EnglishName = "Chinese", NativeName = "简体中文", Code = "zh-hans" },
        new() { EnglishName = "Japanese", NativeName = "日本語", Code = "ja" }
    ];

    private static Language _currentLanguage;

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
        
        CurrentLanguage = Languages.Find(l => l.Code == cultureInfo.Name.Split('-')[0]) ?? Languages[0];
        
        Console.WriteLine(CurrentLanguage);
    }

    public static void SetLanguage(Language language)
    {
        CurrentLanguage = language;
    }
}

public class Language
{
    public string EnglishName { get; set; }
    public string NativeName { get; set; }
    public string Code { get; set; }
}