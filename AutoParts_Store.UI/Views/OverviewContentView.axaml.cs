using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoParts_Store.UI.ViewModels;

namespace AutoParts_Store.UI.Views;

public partial class OverviewContentView : UserControl
{
    public OverviewContentView()
    {
        InitializeComponent();

        DataContext = new OverviewContentViewModel();
    }
}