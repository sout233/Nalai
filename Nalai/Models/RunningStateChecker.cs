namespace Nalai.Models;

public static class RunningStateChecker
{
    private static readonly FrameworkElement _globalSettingsHolder = new FrameworkElement();
    
        public static readonly DependencyProperty GlobalVariableProperty =
            DependencyProperty.Register(nameof(Status), typeof(string), typeof(RunningStateChecker),
                new PropertyMetadata(string.Empty, OnGlobalVariableChanged));

        public static string Status
        {
            get => (string)_globalSettingsHolder.GetValue(GlobalVariableProperty);
            set { _globalSettingsHolder.SetValue(GlobalVariableProperty, value); }
        }

        private static void OnGlobalVariableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 触发事件
            GlobalVariableChanged?.Invoke(null, e.NewValue);
        }

        public static event EventHandler<object> GlobalVariableChanged;
}