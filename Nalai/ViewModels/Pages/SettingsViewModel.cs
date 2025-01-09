using System.Collections.ObjectModel;
using System.Reflection;
using Nalai.Services;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Nalai.ViewModels.Pages
{
    public partial class SettingsViewModel(INavigationService navigationService) : ObservableObject, INavigationAware
    {
        private bool _isInitialized;

        [ObservableProperty] private string _appVersion = String.Empty;

        #region Languages

        [ObservableProperty] private ObservableCollection<string> _comboBoxLanguages =
        [
            "English",
            "简体中文",
            "日本語",
        ];

        [ObservableProperty] private string _selectedLanguageName = string.Empty;

        partial void OnSelectedLanguageNameChanged(string value)
        {
            I18NService.SetLanguage(value);
        }

        #endregion

        #region Themes

        [ObservableProperty] private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        [ObservableProperty] private ObservableCollection<ApplicationTheme> _comboBoxThemes =
        [
            ApplicationTheme.Light,
            ApplicationTheme.Dark
        ];

        partial void OnCurrentThemeChanged(ApplicationTheme value)
        {
            ApplicationThemeManager.Apply(value);
        }

        #endregion

        #region Navigation

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom()
        {
        }

        [RelayCommand]
        private void NavigateForward(Type type)
        {
            _ = navigationService.NavigateWithHierarchy(type);
        }

        [RelayCommand]
        private void NavigateBack()
        {
            _ = navigationService.GoBack();
        }

        #endregion

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"Nalai - {GetAssemblyVersion()}";
            SelectedLanguage = I18NService.CurrentLanguage;

            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                   ?? string.Empty;
        }

        [RelayCommand]
        private void OnChangeTheme(string parameter)
        {
            switch (parameter)
            {
                case "theme_light":
                    if (CurrentTheme == ApplicationTheme.Light)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Light);
                    CurrentTheme = ApplicationTheme.Light;

                    break;

                default:
                    if (CurrentTheme == ApplicationTheme.Dark)
                        break;

                    ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                    CurrentTheme = ApplicationTheme.Dark;

                    break;
            }
            //Console.WriteLine(ThemeColor);
        }

        [RelayCommand]
        private void OnOuterLink()
        {
        }
    }
}