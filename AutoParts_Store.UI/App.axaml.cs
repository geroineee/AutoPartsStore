using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AutoParts_Store.UI.ViewModels;
using AutoParts_Store.UI.Views;

using AutoPartsStore.Data.Context;
using AutoPartsStore.Data;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace AutoParts_Store.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var db = new AutopartsStoreContext("S:\\source\\AutoPartsStore\\AutoPartsStore.Data\\dbsettings.json");
            
            Task.Run(() =>
            {
                using var tempCntxt = new AutopartsStoreContext("S:\\source\\AutoPartsStore\\AutoPartsStore.Data\\dbsettings.json");
                tempCntxt.Database.OpenConnection();
                tempCntxt.Database.CloseConnection();
            });

            ViewModelBase.Initialize( new AutoPartsStoreQueries(db), new AutoPartsStoreTables(db));

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}