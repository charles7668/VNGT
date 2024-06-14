using HandyControl.Controls;
using HandyControl.Tools;
using System.ComponentModel;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using VNGTTranslator.Configs;
using VNGTTranslator.Models;
using VNGTTranslator.TranslateProviders;
using FontFamily = System.Drawing.FontFamily;
using Window = System.Windows.Window;

namespace VNGTTranslator.SettingPages
{
    /// <summary>
    /// TranslateProviderStyleSetting.xaml 的互動邏輯
    /// </summary>
    public partial class TranslateProviderStyleSetting : INotifyPropertyChanged
    {
        public TranslateProviderStyleSetting(ITranslateProvider provider, AppConfig appConfig)
        {
            InitializeComponent();
            ProviderName = provider.ProviderName;
            _appConfig = appConfig;
            if (!_appConfig.TranslateTextStyles.ContainsKey(ProviderName))
            {
                _appConfig.TranslateTextStyles[ProviderName] = new DisplayTextStyle();
            }

            var fontList = new List<string>();
            var fonts = new InstalledFontCollection();
            foreach (FontFamily family in fonts.Families)
            {
                fontList.Add(family.Name);
            }

            FontList = fontList;
            DataContext = this;
        }

        private readonly AppConfig _appConfig;

        private readonly List<string> _fontList = new();

        public string ProviderName { get; set; }

        public string DisplayTitle => "Translate Source : " + ProviderName;

        public uint DisplayFontSize
        {
            get => _appConfig.TranslateTextStyles[ProviderName].FontSize;
            set
            {
                _appConfig.TranslateTextStyles[ProviderName] = _appConfig.TranslateTextStyles[ProviderName] with
                {
                    FontSize = value
                };
                OnPropertyChanged();
            }
        }

        public bool IsTextShadowEnabled
        {
            get => _appConfig.TranslateTextStyles[ProviderName].IsTextShadowEnabled;
            set
            {
                _appConfig.TranslateTextStyles[ProviderName] = _appConfig.TranslateTextStyles[ProviderName] with
                {
                    IsTextShadowEnabled = value
                };
                OnPropertyChanged();
            }
        }

        public string DisplayFontFamily
        {
            get => _appConfig.TranslateTextStyles[ProviderName].FontFamily;
            set
            {
                _appConfig.TranslateTextStyles[ProviderName] = _appConfig.TranslateTextStyles[ProviderName] with
                {
                    FontFamily = value
                };
                OnPropertyChanged();
            }
        }

        public List<string> FontList
        {
            get => _fontList;
            private init => SetField(ref _fontList, value);
        }

        public Brush TextColor
        {
            get => new SolidColorBrush(_appConfig.TranslateTextStyles[ProviderName].TextColor);
            private set
            {
                _appConfig.TranslateTextStyles[ProviderName] = _appConfig.TranslateTextStyles[ProviderName] with
                {
                    TextColor = ((SolidColorBrush)value).Color
                };
                OnPropertyChanged();
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

        private void BtnSelectTextColor_OnClick(object sender, RoutedEventArgs e)
        {
            ColorPicker? picker = SingleOpenHelper.CreateControl<ColorPicker>();
            var window = new PopupWindow
            {
                PopupElement = picker,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None,
                MinWidth = 0,
                MinHeight = 0,
                Title = "Select Color",
                Owner = Window.GetWindow(this)
            };
            picker.Confirmed += (_, args) =>
            {
                TextColor = new SolidColorBrush(args.Info);
                window.Close();
            };

            picker.Canceled += delegate
            {
                window.Close();
            };

            window.Show();
        }
    }
}