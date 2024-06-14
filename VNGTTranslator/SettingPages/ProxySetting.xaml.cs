using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using VNGTTranslator.Configs;
using VNGTTranslator.Models;
using VNGTTranslator.Network;

namespace VNGTTranslator.SettingPages
{
    /// <summary>
    /// ProxySetting.xaml 的互動邏輯
    /// </summary>
    public partial class ProxySetting : INotifyPropertyChanged
    {
        public ProxySetting()
        {
            InitializeComponent();
            _appConfig = Program.ServiceProvider.GetRequiredService<IAppConfigProvider>().GetAppConfig().Clone();
            DataContext = this;
        }

        private readonly AppConfig _appConfig;

        public bool IsUseProxy
        {
            get => _appConfig.IsUseProxy;
            set
            {
                _appConfig.IsUseProxy = value;
                OnPropertyChanged();
            }
        }

        public bool IsUseSystemProxy
        {
            get => _appConfig.IsUseSystemProxy;
            set
            {
                _appConfig.IsUseSystemProxy = value;
                OnPropertyChanged();
            }
        }

        public string ProxyAddress
        {
            get => _appConfig.ProxyAddress;
            set
            {
                _appConfig.ProxyAddress = value;
                OnPropertyChanged();
            }
        }

        public string ProxyPort
        {
            get => _appConfig.ProxyPort;
            set
            {
                _appConfig.ProxyPort = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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

        private void BtnSaveSetting_OnClick(object sender, RoutedEventArgs e)
        {
            IAppConfigProvider appConfigProvider = Program.ServiceProvider.GetRequiredService<IAppConfigProvider>();
            appConfigProvider.GetAppConfig().Update(_appConfig);
            INetworkService networkService = Program.ServiceProvider.GetRequiredService<INetworkService>();
            networkService.UpdateProxySettingUsingAppConfig();
            Result saveSuccess = appConfigProvider.TrySaveAppConfig();
            MessageBox.Show(saveSuccess ? "Save Success" : saveSuccess.ErrorMessage);
        }
    }
}