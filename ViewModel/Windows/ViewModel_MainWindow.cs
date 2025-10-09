using MyriaRPG.Utils;
using MyriaRPG.ViewModel.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Windows
{
    public class ViewModel_MainWindow : BaseViewModel
    {
        private int _with;
        private int _height;
        public int With 
        { 
            get { return _with; }
            set
            {
                _with = value;
                OnPropertyChanged(nameof(With));
            }
            
        }
        public int Height 
        { 
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged(nameof(With));
            }

        }

    }

}
