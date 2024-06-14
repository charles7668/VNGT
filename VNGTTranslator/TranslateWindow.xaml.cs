using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using VNGTTranslator.Configs;
using VNGTTranslator.Hooker;
using VNGTTranslator.Models;
using VNGTTranslator.TranslateProviders;

namespace VNGTTranslator
{
    /// <summary>
    /// Interaction logic for TranslateWindow.xaml
    /// </summary>
    public partial class TranslateWindow : INotifyPropertyChanged
    {
        public TranslateWindow()
        {
            InitializeComponent();
            DataContext = this;
            _hooker = Program.ServiceProvider.GetRequiredService<IHooker>();
            _hooker.OnHookTextReceived += OnHookerTextReceived;
            _appConfig = Program.ServiceProvider.GetRequiredService<IAppConfigProvider>().GetAppConfig();

            RefreshDisplayUI();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = true;
        }

        private readonly AppConfig _appConfig;

        private readonly IHooker _hooker;
        private bool _isShowSourceText = true;

        private bool _isTransparent;
        private bool _pauseState;

        private FontFamily _sourceFontFamily = new("Arial");

        private uint _sourceFontSize = 15;
        private string _sourceText = "Wait source text";
        private Color _sourceTextColor = Colors.White;
        private string _translatedText = string.Empty;

        private List<TranslateProviderDataContext> _useTranslateProviderDataContexts;

        public Brush SourceTextColor
        {
            get => new SolidColorBrush(_sourceTextColor);
            set
            {
                _sourceTextColor = ((SolidColorBrush)value).Color;
                OnPropertyChanged();
            }
        }

        public bool IsShowSourceText
        {
            get => _isShowSourceText;
            set => SetField(ref _isShowSourceText, value);
        }

        public FontFamily SourceFontFamily
        {
            get => _sourceFontFamily;
            set => SetField(ref _sourceFontFamily, value);
        }

        public uint SourceFontSize
        {
            get => _sourceFontSize;
            set => SetField(ref _sourceFontSize, value);
        }

        public bool PauseState
        {
            get => _pauseState;
            set => SetField(ref _pauseState, value);
        }

        public bool IsTransparent
        {
            get => _isTransparent;
            set => SetField(ref _isTransparent, value);
        }

        public string SourceText
        {
            get => _sourceText;
            private set => SetField(ref _sourceText, value);
        }

        public string TranslatedText
        {
            get => _translatedText;
            set => SetField(ref _translatedText, value);
        }

        public List<TranslateProviderDataContext> UseTranslateProviderDataContexts
        {
            get => _useTranslateProviderDataContexts;
            set
            {
                _useTranslateProviderDataContexts = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!PauseState)
                _hooker.OnHookTextReceived -= OnHookerTextReceived;
            base.OnClosing(e);
        }

        private async Task OnHookerTextReceived(HookTextReceivedEventArgs e)
        {
            var data = new ReceivedHookData
            {
                Ctx = e.Ctx,
                Ctx2 = e.Ctx2,
                HookFunc = e.HookFunc,
                Data = e.Text,
                HookCode = e.HookCode
            };
            if (Program.SelectedHookItem == null || data.DisplayHookCode != Program.SelectedHookItem.DisplayHookCode)
            {
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                SourceText = e.Text;

                await Task.WhenAll(UseTranslateProviderDataContexts.Select(context => context.Translate(e.Text))
                    .ToArray());
            });
        }

        private void SetTranslateProviderDataContext()
        {
            TranslateProviderFactory translateProviderFactory =
                Program.ServiceProvider.GetRequiredService<TranslateProviderFactory>();
            var tempList = _appConfig.UsedTranslateProviderSet.Select(s =>
                new TranslateProviderDataContext(translateProviderFactory.GetProvider(s), _appConfig)).ToList();
            UseTranslateProviderDataContexts = tempList;
        }

        private void RefreshDisplayUI()
        {
            SetTranslateProviderDataContext();
            SetWindowColor();
            SetFontStyle();
        }

        private void SetFontStyle()
        {
            SourceFontFamily = new FontFamily(_appConfig.SourceTextStyle.FontFamily);
            SourceFontSize = _appConfig.SourceTextStyle.FontSize;
            SourceTextColor = new SolidColorBrush(_appConfig.SourceTextStyle.TextColor);
        }

        /// <summary>
        /// set background transparent or not
        /// </summary>
        private void SetWindowColor()
        {
            Background = IsTransparent
                ? new SolidColorBrush(Colors.Transparent)
                : new SolidColorBrush(_appConfig.TranslateWindowColor);
        }

        /// <summary>
        /// Set allow change size or not
        /// </summary>
        /// <param name="isAllow"></param>
        private void SetAllowChangeSize(bool isAllow)
        {
            if (isAllow)
            {
                BorderThickness = new Thickness(3);
                ResizeMode = ResizeMode.CanResizeWithGrip;
            }
            else
            {
                BorderThickness = new Thickness(0);
                ResizeMode = ResizeMode.NoResize;
            }
        }

        private void BtnLock_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowColor();
        }

        private void BtnAllowSizeChange_OnClick(object sender, RoutedEventArgs e)
        {
            SetAllowChangeSize(((ToggleButton)sender).IsChecked ?? false);
        }

        private void BtnPause_OnClick(object sender, RoutedEventArgs e)
        {
            if (!PauseState)
            {
                _hooker.OnHookTextReceived -= OnHookerTextReceived;
                PauseState = true;
            }
            else
            {
                _hooker.OnHookTextReceived += OnHookerTextReceived;
                PauseState = false;
            }
        }

        private void BtnReTranslate_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnSetting_OnClick(object sender, RoutedEventArgs e)
        {
            SettingWindow settingWindow = new()
            {
                Owner = this
            };
            settingWindow.ShowDialog();
            RefreshDisplayUI();
        }

        private void BtnHistory_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnShowSourceText_OnClick(object sender, RoutedEventArgs e)
        {
            IsShowSourceText = !IsShowSourceText;
        }

        private void BtnReadAloud_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnMinimizeWindow_OnClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnSelectProcess_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnSelectHookCode_OnClick(object sender, RoutedEventArgs e)
        {
            HookSelectWindow hookSelectWindow = new();
            hookSelectWindow.Show();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void BtnToggleOnTop_OnClick(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
        }

        public class TranslateProviderDataContext : INotifyPropertyChanged
        {
            public TranslateProviderDataContext(ITranslateProvider provider, AppConfig appConfig)
            {
                Provider = provider;
                _appConfig = appConfig;
                _providerStyle = appConfig.TranslateTextStyles.TryGetValue(provider.ProviderName,
                    out DisplayTextStyle displayTextStyle)
                    ? displayTextStyle
                    : new DisplayTextStyle();
            }

            private readonly AppConfig _appConfig;

            private readonly DisplayTextStyle _providerStyle;

            private string _translatedText = string.Empty;
            private ITranslateProvider Provider { get; }

            public Brush ProviderTextColor => new SolidColorBrush(_providerStyle.TextColor);

            public uint ProviderTextSize => _providerStyle.FontSize;

            public FontFamily ProviderFontFamily => new(_providerStyle.FontFamily);

            public string TranslatedText
            {
                get => _translatedText;
                set
                {
                    _translatedText = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            public async Task Translate(string text)
            {
                string translated = await Provider.TranslateAsync(text, LanguageConstant.Language.JAPANESE,
                    LanguageConstant.Language.CHINESE);
                TranslatedText = translated;
            }

            protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}