using AutoPartsStore.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace AutoPartsStore.Data
{
    public class AutoPartsStoreTables
    {
        private readonly AutopartsStoreContext _db;

        public AutoPartsStoreTables(AutopartsStoreContext db)
        {
            _db = db;
        }
        public Dictionary<string, List<TableColumnInfo>> GetTableColumnsConfig()
        {
            return new Dictionary<string, List<TableColumnInfo>>
            {
                ["Поставщики"] = new List<TableColumnInfo>
                {
                    new() { DisplayName = "ID", PropertyName = "Id", IsVisible = false },
                    new() { DisplayName = "Название", PropertyName = "CompanyName" },
                    new() { DisplayName = "Телефон", PropertyName = "Phone" }
                },
                ["Товары"] = new List<TableColumnInfo>
                {
                    new() { DisplayName = "Артикул", PropertyName = "Article" },
                    new() { DisplayName = "Название", PropertyName = "Name" }
                }
            };
        }

        public Dictionary<string, string> AvailableTables => new()
        {
            ["Поставщики"] = "Suppliers",
            ["Товары"] = "Products",
            ["Заказы"] = "Orders"
        };

        public async Task<List<object>> GetTableDataAsync(string tableName)
        {
            return tableName switch
            {
                "Suppliers" => await _db.Suppliers.ToListAsync<object>(),
                "Products" => await _db.Products.ToListAsync<object>(),
                "Orders" => await _db.CustomerOrders.ToListAsync<object>(),
                _ => throw new ArgumentException("Неизвестная таблица")
            };
        }
    }
}
