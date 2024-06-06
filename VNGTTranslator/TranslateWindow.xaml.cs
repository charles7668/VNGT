using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace VNGTTranslator
{
    /// <summary>
    /// Interaction logic for TranslateWindow.xaml
    /// </summary>
    public partial class TranslateWindow : Window
    {
        public TranslateWindow()
        {
            InitializeComponent();
            SetBackgroundTransparent(false);
        }

        private readonly string _backgroundColor = "#7F000000";

        /// <summary>
        /// set background transparent or not
        /// </summary>
        /// <param name="isTransparent"></param>
        private void SetBackgroundTransparent(bool isTransparent)
        {
            Background = isTransparent
                ? new SolidColorBrush(Colors.Transparent)
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString(_backgroundColor));
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
            SetBackgroundTransparent(((ToggleButton)sender).IsChecked ?? false);
        }

        private void BtnAllowSizeChange_OnClick(object sender, RoutedEventArgs e)
        {
            SetAllowChangeSize(((ToggleButton)sender).IsChecked ?? false);
        }

        private void BtnPause_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnReTranslate_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnSetting_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnHistory_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BtnShowSourceText_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
    }
}