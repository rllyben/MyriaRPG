using System.Windows;
using MyriaLib.Models.Settings;
using MyriaRPG.Services;
using MyriaRPG.View.UserControls;

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
            Navigation.SetNavigationFrame(Frame, NavigationFrameType.Main);
            Canvas.Children.Add(gameWindow);
            gameWindow.Visibility = Visibility.Hidden;
            ApplyWindowMode(Settings.Current.VisualSettings.FullScreen);
        }

        public void ApplyWindowMode(bool fullScreen)
        {
            if (fullScreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                WindowState = WindowState.Normal;
            }
        }
    }

}