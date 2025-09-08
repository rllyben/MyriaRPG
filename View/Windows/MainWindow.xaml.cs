using System.Windows;
using System.Windows.Media;
using MyriaLib.Models.Settings;
using MyriaRPG.Services;

namespace MyriaRPG.View.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            Navigation.SetNavigationFrame(Frame, 0);
        }

    }

}