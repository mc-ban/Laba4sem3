using System.Windows;

namespace CardGame.GUI
{
    public partial class SaveNameInputDialog : Window
    {
        public string SaveName => SaveNameTextBox.Text?.Trim();

        public SaveNameInputDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => SaveNameTextBox.Focus();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SaveName))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите название сохранения",
                              "Внимание",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}