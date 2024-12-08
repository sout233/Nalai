using System.ComponentModel;
using System.Globalization;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Input;
using Nalai.ViewModels.Windows;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Antelcat.I18N.Attributes;
using MenuItem = Wpf.Ui.Controls.MenuItem;

namespace Nalai.Views.Windows
{

    public partial class MainWindow : INavigationWindow
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow(
            MainWindowViewModel viewModel,
            IPageService pageService,
            INavigationService navigationService
        )
        {
            ViewModel = viewModel;
            DataContext = this;

            SystemThemeWatcher.Watch(this);

            InitializeComponent();
            SetPageService(pageService);

            navigationService.SetNavigationControl(RootNavigation);
            //Closing += OnClosed;
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => RootNavigation.SetPageService(pageService);

        public void ShowWindow() => Show();

        public void CloseWindow() => OnClosed();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        private void OnClosed()
        {
            Hide();
            //base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            // Application.Current.Shutdown();
            Console.WriteLine("Closing MainWindow");
        }

        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        private void FluentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //I18NExtension.Culture = new CultureInfo("en");
        }
        //private NotifyIcon notifyIcon = null;

    
    }
}
