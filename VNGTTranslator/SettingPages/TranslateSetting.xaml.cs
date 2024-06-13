using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VNGTTranslator.Configs;
using VNGTTranslator.Models;
using VNGTTranslator.TranslateProviders;

namespace VNGTTranslator.SettingPages
{
    /// <summary>
    /// TranslateSetting.xaml 的互動邏輯
    /// </summary>
    public partial class TranslateSetting : Page
    {
        public TranslateSetting()
        {
            InitializeComponent();
            DataContext = this;
            _appConfig = Program.ServiceProvider.GetRequiredService<IAppConfigProvider>().GetAppConfig().Clone();
            TranslateProviderFactory translateProviderFactory =
                Program.ServiceProvider.GetRequiredService<TranslateProviderFactory>();
            TranslateProviderList = new List<TranslateProviderBindingContext>();
            foreach (KeyValuePair<string, ITranslateProvider> translateProvider in translateProviderFactory
                         .CachedProviders)
            {
                TranslateProviderList.Add(new TranslateProviderBindingContext(translateProvider.Value, _appConfig));
            }
        }

        private readonly AppConfig _appConfig;

        public List<TranslateProviderBindingContext> TranslateProviderList { get; set; }

        private void BtnProviderCheck_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.DataContext is not TranslateProviderBindingContext context) return;
            context.IsChecked = !context.IsChecked;
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            IAppConfigProvider appConfigProvider = Program.ServiceProvider.GetRequiredService<IAppConfigProvider>();
            appConfigProvider.GetAppConfig().Update(_appConfig);
            MessageBox.Show(appConfigProvider.TrySaveAppConfig(out string err) ? "Save Success" : err);
        }

        private void BtnProviderSetting_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.DataContext is not TranslateProviderBindingContext context) return;
            TranslateProviderFactory translateProviderFactory =
                Program.ServiceProvider.GetRequiredService<TranslateProviderFactory>();
            TranslateProviderSettingWindow settingWindow =
                new(translateProviderFactory.GetProvider(context.ProviderName), _appConfig)
                {
                    Owner = Window.GetWindow(this),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
            settingWindow.ShowDialog();
            context.ProviderTextColor = new SolidColorBrush(_appConfig.TranslateTextStyles.TryGetValue(
                context.ProviderName,
                out DisplayTextStyle displayTextStyle)
                ? displayTextStyle.TextColor
                : new DisplayTextStyle().TextColor);
        }

        public class TranslateProviderBindingContext
            : INotifyPropertyChanged
        {
            public TranslateProviderBindingContext(ITranslateProvider provider, AppConfig appConfig)
            {
                Provider = provider;
                _appConfig = appConfig;
                _isChecked = appConfig.UsedTranslateProviderSet.Contains(provider.ProviderName);
                DisplayTextStyle style = appConfig.TranslateTextStyles.TryGetValue(provider.ProviderName,
                    out DisplayTextStyle displayTextStyle)
                    ? displayTextStyle
                    : new DisplayTextStyle();
                _providerTextColor = new SolidColorBrush(style.TextColor);
            }

            private readonly AppConfig _appConfig;

            private bool _isChecked;

            private Brush _providerTextColor;
            private ITranslateProvider Provider { get; }
            public string ProviderName => Provider.ProviderName;

            public Brush ProviderTextColor
            {
                get => _providerTextColor;
                set
                {
                    _providerTextColor = value;
                    OnPropertyChanged();
                }
            }

            public bool IsChecked
            {
                get => _isChecked;
                set
                {
                    if (value)
                        _appConfig.UsedTranslateProviderSet.Add(Provider.ProviderName);
                    else
                        _appConfig.UsedTranslateProviderSet.Remove(Provider.ProviderName);
                    SetField(ref _isChecked, value);
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value))
                    return;
                field = value;
                OnPropertyChanged(propertyName);
            }
        }
    }
}