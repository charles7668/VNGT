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
        }

        private void BtnLock_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(((ToggleButton)sender).IsChecked ?? false))
            {
                Background = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                BrushConverter brushConverter = new();
                var brush = (Brush?)brushConverter.ConvertFromString("#7F000000");
                if (brush != null)
                    Background = brush;
            }
        }

        private void BtnAllowSizeChange_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}