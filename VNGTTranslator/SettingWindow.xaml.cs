using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace VNGTTranslator
{
    /// <summary>
    /// SettingWindow.xaml 的互動邏輯
    /// </summary>
    public sealed partial class SettingWindow : Window, INotifyPropertyChanged
    {
        public SettingWindow()
        {
            InitializeComponent();
            DataContext = this;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Topmost = true;
        }

        private Uri _navigateTo = new("SettingPages/About.xaml", UriKind.Relative);

        public Uri NavigateTo
        {
            get => _navigateTo;
            private set => SetField(ref _navigateTo, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SideMenuItemTranslateWindowSetting_OnSelected(object sender, RoutedEventArgs e)
        {
            NavigateTo = new Uri("SettingPages/TranslateWindowSetting.xaml", UriKind.Relative);
        }

        private void SideMenuItemAbout_OnSelected(object sender, RoutedEventArgs e)
        {
            NavigateTo = new Uri("SettingPages/About.xaml", UriKind.Relative);
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

        private void SideMenuItemTranslateSetting_OnSelected(object sender, RoutedEventArgs e)
        {
            NavigateTo = new Uri("SettingPages/TranslateSetting.xaml", UriKind.Relative);
        }
    }
}