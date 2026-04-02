using System.Windows;

namespace MyriaRPG.View.Windows
{
    public partial class Window_InitError : Window
    {
        public string FailedStep { get; }
        public string ErrorMessage { get; }

        public Window_InitError(string failedStep, string errorMessage)
        {
            FailedStep = failedStep;
            ErrorMessage = errorMessage;
            InitializeComponent();
            DataContext = this;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
