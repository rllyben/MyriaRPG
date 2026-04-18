using System.Windows.Controls;
using MyriaRPG.ViewModel.Pages.Game;

namespace MyriaRPG.View.Pages.Game
{
    public partial class Page_Room : Page
    {
        public Page_Room()
        {
            InitializeComponent();
            var vm = new ViewModel_PageRoom();
            DataContext = vm;

            vm.Log.CollectionChanged += (_, _) =>
            {
                Dispatcher.BeginInvoke(() => LogScrollViewer.ScrollToEnd());
            };
        }
    }
}
