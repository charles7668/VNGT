using HandyControl.Controls;
using HandyControl.Tools;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.CompilerServices;
using System.Windows;
using VNGTTranslator.Configs;
using Color = System.Windows.Media.Color;
using MessageBox = System.Windows.MessageBox;
using Window = System.Windows.Window;

namespace VNGTTranslator.SettingPages
{
    /// <summary>
    /// TranslateWindowSetting.xaml 的互動邏輯
    /// </summary>
    public partial class TranslateWindowSetting : INotifyPropertyChanged
    {
        public TranslateWindowSetting()
        {
            InitializeComponent();
            _appConfigProvider = Program.ServiceProvider.GetRequiredService<IAppConfigProvider>();
            _appConfig = _appConfigProvider.GetAppConfig().Clone();
            DataContext = this;

            var fontList = new List<string>();
            var fonts = new InstalledFontCollection();
            foreach (FontFamily family in fonts.Families)
            {
                fontList.Add(family.Name);
            }

            FontList = fontList;
        }

        private readonly AppConfig _appConfig;

        private readonly IAppConfigProvider _appConfigProvider;

        private readonly List<string> _fontList = new();

        public string SelectedTranslateFont
        {
            get => _appConfig.TranslateFontFamily;
            set
            {
                _appConfig.TranslateFontFamily = value;
                OnPropertyChanged();
            }
        }

        public string SelectedSourceFont
        {
            get => _appConfig.SourceFontFamily;
            set
            {
                _appConfig.SourceFontFamily = value;
                OnPropertyChanged();
            }
        }

        public Color SourceTextColor
        {
            get => _appConfig.SourceTextColor;
            private set
            {
                _appConfig.SourceTextColor = value;
                OnPropertyChanged();
            }
        }

        public Color TranslateTextColor
        {
            get => _appConfig.TranslateTextColor;
            private set
            {
                _appConfig.TranslateTextColor = value;
                OnPropertyChanged();
            }
        }

        public uint TranslateFontSize
        {
            get => _appConfig.TranslateFontSize;
            set
            {
                _appConfig.TranslateFontSize = value;
                OnPropertyChanged();
            }
        }

        public bool SourceTextShadowEnabled
        {
            get => _appConfig.SourceTextShadowEnabled;
            set
            {
                _appConfig.SourceTextShadowEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool TranslateTextShadowEnabled
        {
            get => _appConfig.TranslateTextShadowEnabled;
            set
            {
                _appConfig.TranslateTextShadowEnabled = value;
                OnPropertyChanged();
            }
        }

        public uint SourceFontSize
        {
            get => _appConfig.SourceFontSize;
            set
            {
                _appConfig.SourceFontSize = value;
                OnPropertyChanged();
            }
        }

        public List<string> FontList
        {
            get => _fontList;
            private init => SetField(ref _fontList, value);
        }

        public Color TranslateWindowColor
        {
            get => _appConfig.TranslateWindowColor;
            private set
            {
                _appConfig.TranslateWindowColor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void BtnBackgroundColorSelect_OnClick(object sender, RoutedEventArgs e)
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
                TranslateWindowColor = args.Info;
                window.Close();
            };

            picker.Canceled += delegate
            {
                window.Close();
            };

            window.Show();
        }

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

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            _appConfigProvider.GetAppConfig().Update(_appConfig);
            MessageBox.Show(!_appConfigProvider.TrySaveAppConfig(out string err) ? err : "Save Success");
        }

        private void BtnSelectSourceTextColor_OnClick(object sender, RoutedEventArgs e)
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
                SourceTextColor = args.Info;
                window.Close();
            };

            picker.Canceled += delegate
            {
                window.Close();
            };

            window.Show();
        }

        private void BtnSelectTranslateTextColor_OnClick(object sender, RoutedEventArgs e)
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
                TranslateTextColor = args.Info;
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