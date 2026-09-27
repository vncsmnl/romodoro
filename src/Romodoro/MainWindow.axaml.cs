using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Romodoro.Shell;

namespace Romodoro;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

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
