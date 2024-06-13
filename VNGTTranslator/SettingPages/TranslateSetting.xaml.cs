using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        public List<string> TranslateSourceList { get; set; } =
        [
            "test1", "test2", "test2", "test2", "test2", "test2", "test2", "test2", "test2"
        ];

        private void BtnSetting_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                // 從按鈕獲取對應的項目
                var item = TranslateSourceItemsControl.ItemContainerGenerator.ItemFromContainer(button);
                // 獲取索引
                int index = TranslateSourceItemsControl.Items.IndexOf(item);
                MessageBox.Show($"Button index: {index}");
            }
        }
    }
}