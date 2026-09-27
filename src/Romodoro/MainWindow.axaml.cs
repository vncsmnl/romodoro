using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Romodoro.Shell;

namespace Romodoro;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel? _viewModel;
    private DispatcherTimer? _sizeAnimation;

    public MainWindow() => InitializeComponent();

    public MainWindow(MainWindowViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.PropertyChanged += ViewModel_OnPropertyChanged;
        Closed += (_, _) =>
        {
            viewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
            _sizeAnimation?.Stop();
        };
    }

    private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.WindowWidth) && _viewModel is { } viewModel)
            AnimateWindowSize(viewModel.WindowWidth, viewModel.WindowHeight, viewModel.MinimumWindowWidth, viewModel.MinimumWindowHeight);
    }

    private void AnimateWindowSize(double width, double height, double minWidth, double minHeight)
    {
        _sizeAnimation?.Stop();
        var startWidth = Width;
        var startHeight = Height;
        var elapsed = global::System.Diagnostics.Stopwatch.StartNew();
        MinWidth = MinHeight = 0;

        var animation = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _sizeAnimation = animation;
        animation.Tick += (_, _) =>
        {
            var progress = Math.Min(elapsed.Elapsed.TotalMilliseconds / 240, 1);
            var eased = 1 - Math.Pow(1 - progress, 3);
            Width = startWidth + (width - startWidth) * eased;
            Height = startHeight + (height - startHeight) * eased;
            if (progress < 1) return;

            animation.Stop();
            Width = width;
            Height = height;
            MinWidth = minWidth;
            MinHeight = minHeight;
        };
        animation.Start();
    }

    private void TitleBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void TitleBar_OnDoubleTapped(object? sender, TappedEventArgs e) => TogglePill();

    private void Pill_OnDoubleTapped(object? sender, TappedEventArgs e) => TogglePill();

    private void TogglePill()
    {
        if (DataContext is MainWindowViewModel viewModel)
            viewModel.TogglePill();
    }

    private void ResizeGrip_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control { Tag: string edgeName } &&
            Enum.TryParse<WindowEdge>(edgeName, out var edge) &&
            e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
            BeginResizeDrag(edge, e);
    }

    private void MinimizeButton_OnClick(object? sender, RoutedEventArgs e) =>
        WindowState = WindowState.Minimized;

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e) => Close();
}
