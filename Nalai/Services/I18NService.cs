using System.Globalization;

namespace Nalai.Services;

public static class I18NService
{
    public enum Language
    {
        English,
        Chinese,
        Japanese,
    }

    private static readonly Dictionary<Language, string> Languages = new()
    {
        { Language.English, "en" },
        { Language.Chinese, "zh-hans" },
        { Language.Japanese, "ja" },
    };

    private static Language _currentLanguage;

    public static Language CurrentLanguage
    {
        get => _currentLanguage;
        private set
        {
            _currentLanguage = value;
            I18NExtension.Culture = new CultureInfo(Languages[value]);
        }
    }

    public static void SetLanguageByIndex(int index)
    {
        CurrentLanguage = (Language)index;
    }

    public static int GetLanguageIndex(Language currentLanguage)
    {
        return Languages.Values.ToList().IndexOf(Languages[currentLanguage]);
    }
}