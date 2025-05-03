using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AutoParts_Store.UI.ViewModels;
using System;

namespace AutoParts_Store.UI.Views;

public partial class OverviewContentView : UserControl
{
    public OverviewContentView()
    {
        InitializeComponent();

        DataContext = new OverviewContentViewModel();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is OverviewContentViewModel vm)
        {
            vm.AttachDataGrid(tableDataGrid);
        }
    }
}