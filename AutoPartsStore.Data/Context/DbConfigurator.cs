using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutoPartsStore.Data.Context
{
    class DbConfigurator
    {
        public string ConnectionString { get; set; } = "";
    }

    public partial class AutopartsStoreContext : DbContext
    {
        //private readonly string _connectionString;

        //public AutopartsStoreContext(string filePath)
        //{
        //    ConfigurationBuilder builder = new();
        //    builder.SetBasePath(Path.GetDirectoryName(filePath) ?? throw new Exception("Некорректный путь к файлу конфигурации бд."));
        //    builder.AddJsonFile(Path.GetFileName(filePath));

        //    var config = builder.Build();

        //    _connectionString = config.GetConnectionString("DefaultConnection")
        //        ?? throw new Exception($"Ошибка чтения json файла конфигурации бд: {Path.GetFileName(filePath)}.");
        //}
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseMySql(_connectionString, ServerVersion.Parse("8.2.0-mysql"));
        //}
    }
}
