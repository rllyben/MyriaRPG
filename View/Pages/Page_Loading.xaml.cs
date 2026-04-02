using System.Windows.Controls;
using MyriaRPG.ViewModel.Pages;

namespace MyriaRPG.View.Pages
{
    public partial class Page_Loading : Page
    {
        public Page_Loading()
        {
            InitializeComponent();
            DataContext = new ViewModel_LoadingPage();
        }
    }
}
