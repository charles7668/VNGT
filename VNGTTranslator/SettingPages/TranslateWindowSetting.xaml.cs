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
            var appConfig = _appConfigProvider.GetAppConfig();
            DataContext = this;

            var fontList = new List<string>();
            var fonts = new InstalledFontCollection();
            foreach (FontFamily family in fonts.Families)
            {
                fontList.Add(family.Name);
            }

            _selectedSourceFont = appConfig.SourceFontFamily;
            FontList = fontList;
        }

        private readonly IAppConfigProvider _appConfigProvider;

        private readonly List<string> _fontList = new();

        private string _selectedSourceFont;

        public string SelectedSourceFont
        {
            get => _selectedSourceFont;
            set
            {
                if (SetField(ref _selectedSourceFont, value))
                {
                    AppConfig appConfig = _appConfigProvider.GetAppConfig();
                    appConfig.SourceFontFamily = value;
                    _appConfigProvider.TrySaveAppConfig(out _);
                }
            }
        }

        public List<string> FontList
        {
            get => _fontList;
            private init => SetField(ref _fontList, value);
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
                AppConfig appConfig = _appConfigProvider.GetAppConfig();
                Color backupColor = appConfig.TranslateWindowColor;
                appConfig.TranslateWindowColor = args.Info;
                bool result = _appConfigProvider.TrySaveAppConfig(out string? err);
                if (!result)
                {
                    appConfig.TranslateWindowColor = backupColor;
                    MessageBox.Show(err, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

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

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}