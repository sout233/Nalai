﻿using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nalai.CoreConnector;
using Nalai.Models;
using Nalai.Services;
using Nalai.ViewModels.Pages;
using Nalai.ViewModels.Windows;
using Nalai.Views.Pages;
using Nalai.Views.Windows;
using Wpf.Ui;

namespace Nalai
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c =>
            {
                c.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location));
            })
            .ConfigureServices((context, services) =>
            {
                services.AddHostedService<ApplicationHostService>();

                // Page resolver service
                services.AddSingleton<IPageService, PageService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();

                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();

                services.AddSingleton<DashboardPage>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<DataPage>();
                services.AddSingleton<DataViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
            }).Build();

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T;
        }

        private TaskbarIcon? _taskbarIcon;

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            _host.Start();

            var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(DebuggableAttribute), false);
            if (attributes.Length > 0)
            {
                var debuggableAttribute = (DebuggableAttribute)attributes[0];
                if (debuggableAttribute.IsJITTrackingEnabled)
                {
                    Console.WriteLine("Debug模式不启动Nalai Core");
                }
                else
                {
                    Console.WriteLine("已在Release模式下启动Nalai Core");
                    Task.Run(CoreService.StartAsync);
                }
            }

            // 创建托盘图标
            _taskbarIcon = (TaskbarIcon)FindResource("NalaiTrayIcon");


            Task.Run(CoreTask.SyncAllTasksFromCore);
            RunningStateChecker.Start();

            Task.Run(EventApiService.Run);
        }

        private void ShowWindow(object sender, RoutedEventArgs e)
        {
            // 显示或激活主窗口
            if (MainWindow == null)
            {
                var vm = GetService<MainWindowViewModel>();
                var ps = GetService<IPageService>();
                var ns = GetService<INavigationService>();
                var window = new MainWindow(vm, ps, ns);
                window.Show();
            }
            else
            {
                MainWindow.Show();
                MainWindow.Activate();
            }

            if (MainWindow is INavigationWindow navWindow)
            {
                navWindow.Navigate(typeof(DashboardPage));
            }
        }

        private void ShowSettings(object sender, RoutedEventArgs e)
        {
            if (MainWindow == null)
            {
                var vm = GetService<MainWindowViewModel>();
                var ps = GetService<IPageService>();
                var ns = GetService<INavigationService>();
                var window = new MainWindow(vm, ps, ns);
                window.Show();
            }
            else
            {
                MainWindow.Show();
                MainWindow.Activate();
            }

            if (MainWindow is INavigationWindow navWindow)
            {
                navWindow.Navigate(typeof(SettingsPage));
            }
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            Current.Shutdown();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(DebuggableAttribute), false);
            if (attributes.Length > 0)
            {
                var debuggableAttribute = (DebuggableAttribute)attributes[0];
                if (debuggableAttribute.IsJITTrackingEnabled)
                {
                    Console.WriteLine("Debug模式不关闭Nalai Core");
                }
                else
                {
                    Console.WriteLine("已在Release模式下关闭Nalai Core");
                    CoreService.SendExitMsg();
                }
            }

            await _host.StopAsync();
            RunningStateChecker.Stop();
            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}