using Avalonia.Controls;
using Avalonia.Input;

namespace Romodoro;
public partial class MainWindow : Window
{
    public MainWindow() { InitializeComponent(); PointerPressed += (_, e) => { if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed) BeginMoveDrag(e); }; }
}
