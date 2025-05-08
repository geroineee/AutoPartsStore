using AutoParts_Store.UI.Services;
using AutoParts_Store.UI.ViewModels;
using AutoParts_Store.UI.Views;
using AutoPartsStore.Data;
using AutoPartsStore.Data.Context;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

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

            string defaultConnectionString = "Server=MySQL-8.2;Database=autoparts_store;User=guest;Password=";

            // Setup Dependency Injection
            var services = new ServiceCollection();
            services.AddSingleton<IAuthenticationService, AuthenticationService>(provider =>
            {
                var dbContextFactory = provider.GetRequiredService<IDbContextFactory<AutopartsStoreContext>>();
                return new AuthenticationService(dbContextFactory);
            });

            services.AddDbContextFactory<AutopartsStoreContext>(options =>
                options.UseMySql(defaultConnectionString, new MySqlServerVersion(new Version(8, 2, 0))));

            var serviceProvider = services.BuildServiceProvider();

            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<AutopartsStoreContext>>();
            var authService = serviceProvider.GetRequiredService<IAuthenticationService>();

            ViewModelBase.Initialize(
                new AutoPartsStoreQueries(() => dbContextFactory.CreateDbContext()),
                new AutoPartsStoreTables(() => dbContextFactory.CreateDbContext())
            );

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(authService, () => dbContextFactory.CreateDbContext()),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
