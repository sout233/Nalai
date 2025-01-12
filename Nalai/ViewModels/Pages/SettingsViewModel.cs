using System.Collections.ObjectModel;
using System.Reflection;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Services;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Nalai.ViewModels.Pages
{
    public partial class SettingsViewModel(INavigationService navigationService) : ObservableObject, INavigationAware
    {
        private bool _isInitialized;

        [ObservableProperty] private string _appVersion = string.Empty;

        #region Languages

        [ObservableProperty] private ObservableCollection<string> _comboBoxLanguages = [];

        [ObservableProperty] private string _selectedLanguageName = "English";

        partial void OnSelectedLanguageNameChanged(string value)
        {
            ConfigHelper.GlobalConfig.General.Language = I18NHelper.GetCodeByNativeName(value);
        }

        #endregion

        #region Themes

        [ObservableProperty] private ObservableCollection<ApplicationTheme> _comboBoxThemes =
        [
            ApplicationTheme.Light,
            ApplicationTheme.Dark,
            ApplicationTheme.HighContrast
        ];
        
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

        #region StartWithWindows

        [ObservableProperty] private bool _isStartWithWindows;

        partial void OnIsStartWithWindowsChanged(bool value)
        {
            if (value)
                RegManager.RegStartWithWindows();
            else
                RegManager.UnRegStartWithWindows();
        }
        

        #endregion

        private void InitializeViewModel()
        {
            // CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"Nalai - {GetAssemblyVersion()}";
            SelectedLanguageName = I18NHelper.CurrentLanguage.NativeName;
            ComboBoxLanguages = new ObservableCollection<string>(I18NHelper.AvailableLanguages.Select(x => x.NativeName));

            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                   ?? string.Empty;
        }

        [RelayCommand]
        private void OnOuterLink()
        {
        }
    }
}