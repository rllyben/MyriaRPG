using System.Windows;
using System.Windows.Input;
using MyriaLib.Models.Settings;
using MyriaRPG.Services;
using MyriaRPG.View.Pages.Game;
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

            PreviewKeyDown += OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Block game shortcuts when the ingame overlay is open
            if (gameWindow.Visibility == Visibility.Visible)
                return;

            if (Navigation.IsInFight)
                Page_Fight.Current?.HandleKey(e);
            else
                Page_Game.Current?.HandleKey(e);
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
