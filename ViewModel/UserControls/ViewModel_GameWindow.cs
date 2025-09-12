using MyriaRPG.Utils;
using MyriaRPG.View.Windows;
using MyriaRPG.View.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.UserControls
{
    public class ViewModel_GameWindow : BaseViewModel
    {
    public string Title { get; set; } = "Window";

        private double _left, _top, _width = 400, _height = 300;
        private int _zIndex;
        private Visibility _isVisible = Visibility.Visible;

        public double Left { get => _left; set { _left = value; OnPropertyChanged(); } }
        public double Top { get => _top; set { _top = value; OnPropertyChanged(); } }
        public double Width { get => _width; set { _width = value; OnPropertyChanged(); } }
        public double Height { get => _height; set { _height = value; OnPropertyChanged(); } }
        public int ZIndex { get => _zIndex; set { _zIndex = value; OnPropertyChanged(); } }
        public Visibility IsVisible { get => _isVisible; set { _isVisible = value; OnPropertyChanged(); } }

        // Commands that receive event args (delta, etc.)
        public ICommand CloseCommand { get; }
        public ICommand FocusCommand { get; }
        public ICommand DragDeltaCommand { get; }
        public ICommand ResizeDeltaCommand { get; }

        public ViewModel_GameWindow()
        {
            CloseCommand = new RelayCommand(() => IsVisible = Visibility.Hidden);
            //FocusCommand = new RelayCommand(() => ZIndex = WindowManager.BringToFront(this));

            // The parameter will be a small DTO we define below (DragDeltaArgs / ResizeDeltaArgs)
            DragDeltaCommand = new RelayCommand<DragDeltaArgs>(OnDragDelta);
            ResizeDeltaCommand = new RelayCommand<ResizeDeltaArgs>(OnResizeDelta);
        }

        private void OnDragDelta(DragDeltaArgs a)
        {
            Left += a.HorizontalChange;
            Top += a.VerticalChange;

            // optional: clamp inside parent bounds if supplied
            if (Left < 0 - (Width - 60)) Left = 0-(Width - 60);
            if (Top < 0) Top = 0;
            if (Left > 800) Left = 800;
            if (Top > 450) Top = 450;

            MainWindow.Instance.gameWindow.Margin = new Thickness(Left, Top, 0, 0);
        }

        private void OnResizeDelta(ResizeDeltaArgs a)
        {
            var minW = 200; var minH = 120;
            Width = Math.Max(Width + a.HorizontalChange, minW);
            Height = Math.Max(Height + a.VerticalChange, minH);
        }

    }

    // Small DTOs to carry event data in a VM-friendly way
    public record DragDeltaArgs(double HorizontalChange, double VerticalChange, double? HostWidth = null, double? HostHeight = null);
    public record ResizeDeltaArgs(double HorizontalChange, double VerticalChange);
}
