using MyriaRPG.Systems.MapNode.MapEdge;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MyriaRPG.View.UserControls
{
    public partial class LocalMapControl : UserControl
    {
        public LocalMapControl() { InitializeComponent(); SizeChanged += (_, __) => Redraw(); }

        public LocalMapSnapshot Snapshot
        {
            get => (LocalMapSnapshot)GetValue(SnapshotProperty);
            set => SetValue(SnapshotProperty, value);
        }
        public static readonly DependencyProperty SnapshotProperty =
            DependencyProperty.Register(nameof(Snapshot), typeof(LocalMapSnapshot), typeof(LocalMapControl),
                new PropertyMetadata(null, (_, __) => ((LocalMapControl)_).Redraw()));

        public string? CurrentPoiId
        {
            get => (string?)GetValue(CurrentPoiIdProperty);
            set => SetValue(CurrentPoiIdProperty, value);
        }
        public static readonly DependencyProperty CurrentPoiIdProperty =
            DependencyProperty.Register(nameof(CurrentPoiId), typeof(string), typeof(LocalMapControl),
                new PropertyMetadata(null, (_, __) => ((LocalMapControl)_).Redraw()));

        public Brush RoadBrush { get; set; } = new SolidColorBrush(Color.FromRgb(164, 132, 100));

        void Redraw()
        {
            Layer.Children.Clear();
            if (Snapshot is null) return;

            double W = ActualWidth <= 0 ? 600 : ActualWidth;
            double H = ActualHeight <= 0 ? 400 : ActualHeight;

            // 1) exits (as diamonds on the border or wherever you place them)
            foreach (var ex in Snapshot.Exits)
            {
                var p = new Point(ex.X * W, ex.Y * H);
                var diamond = new Polygon
                {
                    Points = new PointCollection(new[]{
                        new Point(p.X, p.Y-10), new Point(p.X+10, p.Y),
                        new Point(p.X, p.Y+10), new Point(p.X-10, p.Y)}),
                    Fill = Brushes.SandyBrown,
                    Stroke = Brushes.SaddleBrown,
                    StrokeThickness = 1.5
                };
                Layer.Children.Add(diamond);

                var label = new TextBlock { Text = ex.Label, Foreground = Brushes.SandyBrown, FontSize = 12 };
                Canvas.SetLeft(label, p.X + 12); Canvas.SetTop(label, p.Y - 8);
                Layer.Children.Add(label);
            }

            // 2) POIs (icons + labels)
            foreach (var poi in Snapshot.Pois)
            {
                var p = new Point(poi.X * W, poi.Y * H);
                var radius = 12.0;
                var fill = Brushes.SkyBlue;
                var stroke = Brushes.White;

                // pick a color per kind (replace with images later)
                if (poi.Kind == PoiKind.Smith) fill = Brushes.Orange;
                if (poi.Kind == PoiKind.Healer) fill = Brushes.LightGreen;
                if (poi.Kind == PoiKind.Vendor) fill = Brushes.Khaki;
                if (poi.Kind == PoiKind.Trainer) fill = Brushes.Plum;
                if (poi.Kind == PoiKind.Portal) fill = Brushes.MediumPurple;

                var dot = new Ellipse { Width = radius * 2, Height = radius * 2, Fill = fill, Stroke = stroke, StrokeThickness = 2 };
                Canvas.SetLeft(dot, p.X - radius); Canvas.SetTop(dot, p.Y - radius);
                Layer.Children.Add(dot);

                var label = new TextBlock
                {
                    Text = poi.Label,
                    Foreground = Brushes.White,
                    FontSize = 12
                };
                Canvas.SetLeft(label, p.X + radius + 4); Canvas.SetTop(label, p.Y - 8);
                Layer.Children.Add(label);

                // highlight current poi
                if (poi.Id == CurrentPoiId)
                {
                    var ring = new Ellipse { Width = radius * 2 + 10, Height = radius * 2 + 10, Stroke = Brushes.Cyan, StrokeThickness = 2, Opacity = 0.85 };
                    Canvas.SetLeft(ring, p.X - radius - 5); Canvas.SetTop(ring, p.Y - radius - 5);
                    Layer.Children.Add(ring);
                }

            }

        }

    }

}

