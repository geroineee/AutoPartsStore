using AutoPartsStore.Data.Context;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AutoPartsStore.Data
{
    public class AutoPartsStoreQueries
    {
        private readonly Func<AutopartsStoreContext> _dbContextFactory;

        public AutoPartsStoreQueries(Func<AutopartsStoreContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        public AutoPartsStoreQueries()
        {
            throw new Exception("Необходимо указать контекст базы данных.");
        }

        /// <summary>
        /// Запрос 1) Получите перечень и общее число поставщиков определенной категории, поставляющих указанный вид товара.
        /// </summary>
        public async Task<List<object>> GetSupplierProvidesProductAsync(int productId)
        {
            using var _db = _dbContextFactory();

            var res = from supplierProduct in _db.SupplierProductStatViews
                      where supplierProduct.ProductId == productId
                      select new
                      {
                          Имя_поставщика = supplierProduct.SupplierName,
                          Название_товара = supplierProduct.ProductName,
                          Цена = supplierProduct.DeliveryPrice,
                          Срок_доставки = supplierProduct.DeliveryDays,
                          Всего_поставок = supplierProduct.TotalDeliveries,
                          Количество_поставленных_товаров = supplierProduct.TotalQuantity
                      };

            return await res.ToListAsync<object>();
        }

        /// <summary>
        /// Запрос 1) Получите перечень и общее число поставщиков определенной категории,
        ///         поставивших указанный товар в объеме, не менее заданного за определенный период.
        /// </summary>
        public async Task<List<object>> GetSupplierProvidesProductWithValueAsync(int productId, int value,
            DateTime startDate, DateTime endDate)
        {
            using var _db = _dbContextFactory();

            var res = from supplierProduct in _db.SupplierProductStatViews
                      where supplierProduct.ProductId == productId
                      join batchInfo in _db.BatchInfoViews on
                            new { supplierProduct.SupplierId, supplierProduct.ProductId } equals
                            new { batchInfo.SupplierId, batchInfo.ProductId }
                      where batchInfo.DeliveryDate >= startDate && batchInfo.DeliveryDate <= endDate
                      group new { supplierProduct, batchInfo } by new
                      {
                          supplierProduct.SupplierId,
                          supplierProduct.SupplierName,
                          supplierProduct.CategoryName,
                          supplierProduct.ProductName,
                          supplierProduct.DeliveryPrice,
                          supplierProduct.DeliveryDays
                      } into grouped
                      where grouped.Sum(x => x.batchInfo.ProductQuantity) >= value
                      select new
                      {
                          Имя_поставщика = grouped.Key.SupplierName,
                          Название_товара = grouped.Key.ProductName,
                          Категория = grouped.Key.CategoryName,
                          Цена_поставки = grouped.Key.DeliveryPrice,
                          Всего_поставок = grouped.Count(),
                          Количество_поставленных_товаров_за_период = grouped.Sum(x => x.batchInfo.ProductQuantity)
                      };

            return await res.ToListAsync<object>();
        }

        /// <summary>
        /// Запрос 2) Получите сведения о конкретном виде деталей: какими поставщиками поставляется, их расценки, время поставки.
        /// </summary>
        public async Task<List<object>> GetDetailProductInfo(int productId)
        {
            using var db = _dbContextFactory();
            var res = from view in db.SupplierProductStatViews
                      where view.ProductId == productId
                      select new
                      {
                          Название_товара = view.ProductName,
                          Поставщик = view.SupplierName,
                          Цена_поставки = view.DeliveryPrice,
                          Время_поставки = view.DeliveryDays,
                      };
            return await res.ToListAsync<object>();
        }

        /// <summary>
        /// Запрос 3) Получите перечень и общее число покупателей,
        /// купивших указанный вид товара за некоторый период либо сделавших покупку товара в объеме, не менее указанного
        /// </summary>
        public async Task<List<object>> GetCustomerBoughtProduct(int productId, DateTime startDate, DateTime endDate, int? minQuantity)
        {
            using var db = _dbContextFactory();
            var res = from c in db.Customers
                      join sale in db.Sales on c.CustomerId equals sale.SaleCustomerId
                      join si in db.SaleItems on sale.SaleId equals si.SaleItemId
                      join product in db.Products on si.SiProductId equals product.ProductId
                      where product.ProductId == productId &&
                      sale.SaleDate >= startDate &&
                      sale.SaleDate <= endDate
                      group si by new
                      {
                          c.CustomerId,
                          c.CustomerName,
                          c.CustomerSurname,
                          c.CustomerPatronymic,
                          product.ProductName,
                          sale.SaleDate
                      } into gr
                      select new
                      {
                          Имя = gr.Key.CustomerName,
                          Фамилия = gr.Key.CustomerSurname,
                          Отчество = gr.Key.CustomerPatronymic,
                          Товар = gr.Key.ProductName,
                          Дата_покупки = gr.Key.SaleDate,
                          Количество_товара = gr.Sum(x => x.SiQuantity)
                      };
            
            if (minQuantity.HasValue)
            {
                res = res.Where(x => x.Количество_товара >= minQuantity.Value);
            }

            return await res.ToListAsync<object>();
        }
    }
}
