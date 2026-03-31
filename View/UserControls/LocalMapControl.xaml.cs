using MyriaRPG.Systems.MapNode;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyriaRPG.View.UserControls
{
    public partial class LocalMapControl : UserControl
    {
        // ── Constants ──────────────────────────────────────────────────────────
        private const double SCALE_STEP = 0.15;
        private const double SCALE_MIN  = 0.20;
        private const double SCALE_MAX  = 3.00;

        // ── State ──────────────────────────────────────────────────────────────
        private double _scale    = 1.0;
        private Point  _dragOrigin;
        private double _dragTx, _dragTy;
        private bool   _dragging;

        public LocalMapControl()
        {
            InitializeComponent();
            SizeChanged += (_, __) => CenterOnCurrentNode();
        }

        // ── Dependency Properties ──────────────────────────────────────────────

        public IReadOnlyList<MapNodeVm> Nodes
        {
            get => (IReadOnlyList<MapNodeVm>)GetValue(NodesProperty);
            set => SetValue(NodesProperty, value);
        }
        public static readonly DependencyProperty NodesProperty =
            DependencyProperty.Register(nameof(Nodes), typeof(IReadOnlyList<MapNodeVm>),
                typeof(LocalMapControl), new PropertyMetadata(null, (d, _) => ((LocalMapControl)d).Redraw()));

        public IReadOnlyList<MapEdgeVm> Edges
        {
            get => (IReadOnlyList<MapEdgeVm>)GetValue(EdgesProperty);
            set => SetValue(EdgesProperty, value);
        }
        public static readonly DependencyProperty EdgesProperty =
            DependencyProperty.Register(nameof(Edges), typeof(IReadOnlyList<MapEdgeVm>),
                typeof(LocalMapControl), new PropertyMetadata(null, (d, _) => ((LocalMapControl)d).Redraw()));

        // ── Colors ─────────────────────────────────────────────────────────────
        private static readonly Color _colorCity    = Color.FromRgb(100, 75, 10);
        private static readonly Color _colorDungeon = Color.FromRgb(80,  20, 20);
        private static readonly Color _colorBoss    = Color.FromRgb(120, 10, 10);
        private static readonly Color _colorCave    = Color.FromRgb(55,  55, 65);
        private static readonly Color _colorForest  = Color.FromRgb(25,  65, 30);
        private static readonly Color _colorWorld   = Color.FromRgb(35,  40, 55);

        private static readonly Color _borderNormal  = Color.FromRgb(70,  70, 90);
        private static readonly Color _borderCurrent = Color.FromRgb(122, 180, 255);
        private static readonly Color _edgeColor     = Color.FromRgb(80,  80, 100);
        private static readonly Color _labelNormal   = Color.FromRgb(190, 190, 205);
        private static readonly Color _markerColor   = Color.FromRgb(122, 180, 255);

        // ── Public zoom API (called from Page buttons) ─────────────────────────
        public void ZoomIn()  => ApplyZoom(_scale + SCALE_STEP);
        public void ZoomOut() => ApplyZoom(_scale - SCALE_STEP);

        // ── Zoom logic ─────────────────────────────────────────────────────────
        /// <summary>
        /// Scales the map around <paramref name="pivot"/> (viewport coords).
        /// When pivot is null the viewport center is used.
        /// </summary>
        private void ApplyZoom(double newScale, Point? pivot = null)
        {
            newScale = Math.Clamp(newScale, SCALE_MIN, SCALE_MAX);
            double cx     = pivot?.X ?? Viewport.ActualWidth  / 2;
            double cy     = pivot?.Y ?? Viewport.ActualHeight / 2;
            double factor = newScale / _scale;

            TranslateXform.X = cx - (cx - TranslateXform.X) * factor;
            TranslateXform.Y = cy - (cy - TranslateXform.Y) * factor;
            ScaleXform.ScaleX = ScaleXform.ScaleY = newScale;
            _scale = newScale;
        }

        // ── Center the current-room node in the viewport ───────────────────────
        private void CenterOnCurrentNode()
        {
            if (Nodes == null) return;
            var current = Nodes.FirstOrDefault(n => n.IsCurrent);
            if (current == null || Viewport.ActualWidth <= 0) return;

            TranslateXform.X = Viewport.ActualWidth  / 2 - current.CenterX * _scale;
            TranslateXform.Y = Viewport.ActualHeight / 2 - current.CenterY * _scale;
        }

        // ── Drawing ────────────────────────────────────────────────────────────
        private void Redraw()
        {
            Layer.Children.Clear();

            // Reset zoom/pan for each new map
            ScaleXform.ScaleX = ScaleXform.ScaleY = 1.0;
            TranslateXform.X  = TranslateXform.Y  = 0;
            _scale = 1.0;

            var nodes = Nodes;
            var edges = Edges;
            if (nodes == null || nodes.Count == 0) return;

            // Size the canvas so hit-testing works across the full map area
            Layer.Width  = nodes.Max(n => n.X + n.Width)  + 30;
            Layer.Height = nodes.Max(n => n.Y + n.Height) + 30;

            // 1) Edges — behind nodes
            var edgePen = new SolidColorBrush(_edgeColor);
            foreach (var edge in edges ?? [])
            {
                Layer.Children.Add(new Line
                {
                    X1 = edge.X1, Y1 = edge.Y1,
                    X2 = edge.X2, Y2 = edge.Y2,
                    Stroke          = edgePen,
                    StrokeThickness = 2,
                    StrokeDashArray = new DoubleCollection { 5, 3 }
                });
            }

            // 2) Nodes
            foreach (var node in nodes)
            {
                var fillColor = node.Kind switch
                {
                    NodeKind.City    => _colorCity,
                    NodeKind.Dungeon => _colorDungeon,
                    NodeKind.Boss    => _colorBoss,
                    NodeKind.Cave    => _colorCave,
                    NodeKind.Forest  => _colorForest,
                    _                => _colorWorld
                };

                var rect = new Border
                {
                    Width           = node.Width,
                    Height          = node.Height,
                    Background      = new SolidColorBrush(fillColor),
                    BorderBrush     = new SolidColorBrush(node.IsCurrent ? _borderCurrent : _borderNormal),
                    BorderThickness = new Thickness(node.IsCurrent ? 2.5 : 1.5),
                    CornerRadius    = new CornerRadius(6)
                };
                Canvas.SetLeft(rect, node.X);
                Canvas.SetTop(rect,  node.Y);
                Layer.Children.Add(rect);

                var lbl = new TextBlock
                {
                    Text          = node.Label,
                    Foreground    = new SolidColorBrush(node.IsCurrent ? Colors.White : _labelNormal),
                    FontSize      = 11,
                    FontWeight    = node.IsCurrent ? FontWeights.Bold : FontWeights.Normal,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping  = TextWrapping.NoWrap,
                    Width         = node.Width - 8,
                    TextTrimming  = TextTrimming.CharacterEllipsis
                };
                Canvas.SetLeft(lbl, node.X + 4);
                Canvas.SetTop(lbl,  node.Y + (node.Height - 16) / 2);
                Layer.Children.Add(lbl);

                // ▶ marker left of current room node
                if (node.IsCurrent)
                {
                    var marker = new TextBlock
                    {
                        Text       = "▶",
                        Foreground = new SolidColorBrush(_markerColor),
                        FontSize   = 11
                    };
                    Canvas.SetLeft(marker, node.X - 16);
                    Canvas.SetTop(marker,  node.Y + (node.Height - 14) / 2);
                    Layer.Children.Add(marker);
                }
            }

            // Center on current node once layout is complete
            Dispatcher.InvokeAsync(CenterOnCurrentNode, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // ── Mouse: Pan ─────────────────────────────────────────────────────────
        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragging   = true;
            _dragOrigin = e.GetPosition(Viewport);
            _dragTx     = TranslateXform.X;
            _dragTy     = TranslateXform.Y;
            Viewport.CaptureMouse();
            Viewport.Cursor = Cursors.SizeAll;
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            var pos = e.GetPosition(Viewport);
            TranslateXform.X = _dragTx + (pos.X - _dragOrigin.X);
            TranslateXform.Y = _dragTy + (pos.Y - _dragOrigin.Y);
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragging = false;
            Viewport.ReleaseMouseCapture();
            Viewport.Cursor = Cursors.Hand;
        }

        private void Viewport_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            _dragging = false;
            Viewport.ReleaseMouseCapture();
            Viewport.Cursor = Cursors.Hand;
        }

        // ── Mouse: Zoom (wheel) ─────────────────────────────────────────────────
        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = e.Delta > 0 ? SCALE_STEP : -SCALE_STEP;
            ApplyZoom(_scale + delta, e.GetPosition(Viewport));
            e.Handled = true; // prevent page scroll
        }
    }
}
