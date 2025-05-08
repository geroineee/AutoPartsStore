using AutoParts_Store.UI.ViewModels;
using Avalonia.Controls;
using System;

namespace AutoParts_Store.UI.Views;

public partial class OverviewContentView : UserControl
{
    public OverviewContentView()
    {
        InitializeComponent();
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
