using System.Text;
using System.Windows;
using Streebog_Hash.Hash;

namespace Streebog_Hash
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private void ExecuteBtn_Click(object sender, RoutedEventArgs e)
        {
            string input = InputTextTB.Text;

            Streebog hash = new Streebog();

            Hash512TB.Text = hash.GetHash512(input);
            Hash256TB.Text = hash.GetHash256(input);
        }

        private void InputTextTB_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!InputTextTB.Text.Equals(string.Empty))
                ExecuteBtn.IsEnabled = true;
            else
                ExecuteBtn.IsEnabled = false;
        }
    }
}
