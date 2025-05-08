using AutoPartsStore.Data.Context;
using Microsoft.EntityFrameworkCore;

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
        public async Task<List<object>> GetSupplierProvidesProductAsync(int productId, int supplierCategory)
        {
            using (var _db = _dbContextFactory())
            {
                var res = from supplierProduct in _db.SupplierProductStatViews
                          where supplierProduct.ProductId == productId && supplierProduct.SupplierCategoryId == supplierCategory
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
        }

        /// <summary>
        /// Запрос 1) Получите перечень и общее число поставщиков определенной категории,
        ///         поставивших указанный товар в объеме, не менее заданного за определенный период.
        /// </summary>
        public async Task<List<object>> GetSupplierProvidesProductWithValueAsync(int productId, int supplierCategory, int value,
            DateTime startDate, DateTime endDate)
        {
            using (var _db = _dbContextFactory())
            {
                var res = from supplierProduct in _db.SupplierProductStatViews
                          where supplierProduct.ProductId == productId && supplierProduct.SupplierCategoryId == supplierCategory
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
                              Время_поставки = grouped.Key.DeliveryDays,
                              Всего_поставок = grouped.Count(),
                              Количество_поставленных_товаров_за_период = grouped.Sum(x => x.batchInfo.ProductQuantity)
                          };

                return await res.ToListAsync<object>();
            }
        }
    }
}
