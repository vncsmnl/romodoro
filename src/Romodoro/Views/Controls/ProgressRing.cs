using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Romodoro.Views.Controls;

public sealed class ProgressRing : Control
{
    public static readonly StyledProperty<double> ProgressProperty = AvaloniaProperty.Register<ProgressRing, double>(nameof(Progress), 1);
    public double Progress { get => GetValue(ProgressProperty); set => SetValue(ProgressProperty, value); }
    static ProgressRing() => AffectsRender<ProgressRing>(ProgressProperty);
    public override void Render(DrawingContext context)
    {
        var center = Bounds.Center; var radius = Math.Min(Bounds.Width, Bounds.Height) / 2 - 7;
        var pen = new Pen(Brushes.DimGray, 8, lineCap: PenLineCap.Round);
        context.DrawEllipse(null, pen, center, radius, radius);
        if (Progress <= 0) return;
        var geometry = new StreamGeometry();
        using (var g = geometry.Open())
        {
            var start = new Point(center.X, center.Y - radius);
            var angle = 360 * Progress;
            var end = new Point(center.X + radius * Math.Sin(angle * Math.PI / 180), center.Y - radius * Math.Cos(angle * Math.PI / 180));
            g.BeginFigure(start, false);
            g.ArcTo(end, new Size(radius, radius), 0, angle > 180, SweepDirection.Clockwise);
        }
        context.DrawGeometry(null, new Pen(Brushes.Coral, 8, lineCap: PenLineCap.Round), geometry);
    }
}
