using System.Windows;
using System.Windows.Media;
using MyriaLib.Models.Settings;
using MyriaRPG.Services;
using MyriaRPG.View.UserControls;
using MyriaRPG.ViewModel.UserControls;

namespace MyriaRPG.View.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public GameWindow gameWindow = new GameWindow();
        public static MainWindow Instance { get; private set; }
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            Navigation.SetNavigationFrame(Frame, 0);
            Canvas.Children.Add(gameWindow);
            gameWindow.Visibility = Visibility.Hidden;
            this.WindowState = WindowState.Maximized;
        }

    }

}