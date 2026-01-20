using System.Windows.Controls;
using MyriaRPG.Services;
using MyriaRPG.ViewModel.UserControls;

namespace MyriaRPG.View.UserControls
{
    /// <summary>
    /// Interaktionslogik für GameWindow.xaml
    /// </summary>
    public partial class GameWindow : UserControl
    {
        public GameWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModel_GameWindow();
            Navigation.SetNavigationFrame(Frame, 3);
        }

    }

}
