using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using VNGTTranslator.Hooker;

namespace VNGTTranslator
{
    /// <summary>
    /// HookSelectWindow.xaml 的互動邏輯
    /// </summary>
    public partial class HookSelectWindow : Window, INotifyPropertyChanged
    {
        public HookSelectWindow()
        {
            InitializeComponent();
            Topmost = true;
            DataContext = this;
        }

        private readonly Dictionary<string, int> _hookDataIndexCache = new();

        private string _addHookCodeInput = string.Empty;

        private IHooker _hooker = null!;

        private int _selectIndex = -1;

        public BindingList<ReceivedHookData> HookDataList { get; set; } = new();

        public int SelectIndex
        {
            get => _selectIndex;
            set => SetField(ref _selectIndex, value);
        }

        public string AddHookCodeInput
        {
            get => _addHookCodeInput;
            set => SetField(ref _addHookCodeInput, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnTextReceived(HookTextReceivedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                int currentIndex = SelectIndex;
                var data = new ReceivedHookData
                {
                    Ctx = e.Ctx,
                    Ctx2 = e.Ctx2,
                    HookFunc = e.HookFunc,
                    Data = e.Text,
                    HookCode = e.HookCode
                };
                if (!_hookDataIndexCache.TryGetValue(data.DisplayHookCode, out int value))
                {
                    HookDataList.Add(data);
                    _hookDataIndexCache[data.DisplayHookCode] = HookDataList.Count - 1;
                }
                else
                {
                    HookDataList[value] = data;
                }

                SelectIndex = currentIndex;
            }, DispatcherPriority.DataBind);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _hooker.HookTextReceived -= OnTextReceived;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _hooker = Program.ServiceProvider.GetRequiredService<IHooker>();
            _hooker.HookTextReceived += OnTextReceived;
        }

        private void BtnAddHook_OnClick(object sender, RoutedEventArgs e)
        {
            InputDrawer.IsOpen = true;
        }

        private void BtnConfirm_OnClick(object sender, RoutedEventArgs e)
        {
            Program.SelectedHookItem = SelectIndex == -1 ? null : HookDataList[SelectIndex];

            Close();
        }

        private void BtnAddHookConfirmHook_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_hooker.InsertHook(Program.PID, AddHookCodeInput))
            {
                MessageBox.Show("insert hook code failed");
                return;
            }

            InputDrawer.IsOpen = false;
        }

        private void BtnAddHookClose_OnClick(object sender, RoutedEventArgs e)
        {
            InputDrawer.IsOpen = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
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

        private void MenuItemCopyHookCode_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectIndex == -1 || string.IsNullOrEmpty(HookDataList[SelectIndex].HookCode))
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Clipboard.SetText(HookDataList[SelectIndex].HookCode);
            }, DispatcherPriority.Background);
        }
    }
}