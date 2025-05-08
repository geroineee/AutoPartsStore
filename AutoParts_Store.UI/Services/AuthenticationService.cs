using AutoParts_Store.UI.ViewModels;
using AutoPartsStore.Data.Context;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Threading.Tasks;

namespace AutoParts_Store.UI.Services
{
    public interface IAuthenticationService
    {
        Task<bool> AuthenticateAsync(string username, string password);
        string GenerateConnectionString(string username, string password, string database);
        void UpdateDbContextFactory(string connectionString);
        Func<AutopartsStoreContext> GetDbContextFactoryFunc();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private string _currentConnectionString;
        private Func<AutopartsStoreContext> _dbContextFactoryFunc;
        private readonly IDbContextFactory<AutopartsStoreContext> _dbContextFactory;

        public AuthenticationService(IDbContextFactory<AutopartsStoreContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _dbContextFactoryFunc = () => _dbContextFactory.CreateDbContext();
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            string generatedConnectionString = GenerateConnectionString(username, password, "autoparts_store");

            var dbContextOptionsBuilder = new DbContextOptionsBuilder<AutopartsStoreContext>();
            dbContextOptionsBuilder.UseMySql(generatedConnectionString, new MySqlServerVersion(new Version(8, 2, 0)));

            using (var context = new AutopartsStoreContext(dbContextOptionsBuilder.Options))
            {
                try
                {
                    await context.Database.OpenConnectionAsync();
                    await context.Database.CloseConnectionAsync();
                    _currentConnectionString = generatedConnectionString;
                    UpdateDbContextFactory(_currentConnectionString);
                    return true;
                }
                catch (MySqlException)
                {
                    return false;
                }
            }
        }

        public string GenerateConnectionString(string username, string password, string database)
        {
            return $"Server=MySQL-8.2;User={username};Password={password};Database={database}";
        }

        public void UpdateDbContextFactory(string connectionString)
        {
            _currentConnectionString = connectionString;
            _dbContextFactoryFunc = () =>
            {
                var dbContextOptionsBuilder = new DbContextOptionsBuilder<AutopartsStoreContext>();
                dbContextOptionsBuilder.UseMySql(_currentConnectionString, new MySqlServerVersion(new Version(8, 2, 0)));
                return new AutopartsStoreContext(dbContextOptionsBuilder.Options);
            };

            ViewModelBase.UpdateDbContextFactoryFunc(_dbContextFactoryFunc);
        }

        public Func<AutopartsStoreContext> GetDbContextFactoryFunc()
        {
            return _dbContextFactoryFunc;
        }
    }

}
