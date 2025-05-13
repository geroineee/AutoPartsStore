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
        /// Запрос 1.1) Получите перечень и общее число поставщиков определенной категории, поставляющих указанный вид товара.
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
        /// Запрос 1.2) Получите перечень и общее число поставщиков определенной категории,
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


        /// <summary>
        /// Запрос 	4) Получите перечень, объем и номер ячейки для всех деталей, хранящихся на складе. 
        /// </summary>
        public async Task<List<object>> GetCells()
        {
            using var db = _dbContextFactory();
            var res = from view in db.AvailableProductOnStorageViews
                      group view by new { view.CellName, view.ProductName } into g
                      select new
                      {
                          Ячейка = g.Key.CellName,
                          Товар = g.Key.ProductName,
                          Количество = g.Sum(x => x.CurrentQuantity)
                      };

            return await res.ToListAsync<object>();
        }

        /// <summary>
        /// Запрос 5.1) Получите десять самых продаваемых деталей в порядке возрастания.
        /// </summary>
        public async Task<List<object>> GetTopSellingProductsAsync()
        {
            using var db = _dbContextFactory();

            var result = await (
                from si in db.SaleItems
                join p in db.Products on si.SiProductId equals p.ProductId
                group si by new { p.ProductId, p.ProductName } into g
                orderby g.Sum(x => x.SiQuantity) ascending
                select new
                {
                    Название_товара = g.Key.ProductName,
                    Объем_продаж = g.Sum(x => x.SiQuantity)
                }
            ).Take(10).ToListAsync<object>();

            return result;
        }

        /// <summary>
        /// Запрос 5.2) Получите десять самых дешевых поставщиков для каждой детали.
        /// </summary>
        public async Task<List<object>> GetCheapestSuppliersAsync()
        {
            using var db = _dbContextFactory();

            var result = await (
                from view in db.SupplierProductStatViews
                group view by view.ProductId into g
                let minPrice = g.Min(x => x.DeliveryPrice)
                select new
                {
                    Название_товара = g.First().ProductName,
                    Поставщик = g.First(x => x.DeliveryPrice == minPrice).SupplierName,
                    Цена = minPrice
                }
            ).Take(10).ToListAsync<object>();

            return result;
        }

        /// <summary>
        /// Запрос 6) Получите среднее число продаж на месяц по выбранному виду деталей.
        /// </summary>
        public async Task<object> GetAverageMonthlySalesAsync(int productId)
        {
            using var db = _dbContextFactory();

            var monthlySales = await (
                from saleItem in db.SaleItems
                where saleItem.SiProductId == productId
                join sale in db.Sales on saleItem.SaleItemId equals sale.SaleId
                join product in db.Products on saleItem.SiProductId equals product.ProductId
                group saleItem by new { sale.SaleDate.Year, sale.SaleDate.Month, product.ProductName } into g
                select new
                {
                    Товар = g.Key.ProductName,
                    Месяц = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Продано = g.Sum(x => x.SiQuantity)
                }
            ).ToListAsync();

            double average = monthlySales.Any() ? monthlySales.Average(x => x.Продано) : 0;

            return new { Товар = monthlySales.First(x => x.Товар is not null).Товар, Среднее_количество_продаж = average };
        }

        /// <summary>
        /// Запрос 7.1: Получите долю товара конкретного поставщика в процентах, деньгах, единицах от всего оборота магазина.
        /// Включает название товара.
        /// </summary>
        public async Task<List<object>> GetSupplierContributionAsync(int supplierId, DateTime startDate, DateTime endDate)
        {
            using var db = _dbContextFactory();

            // Группируем по товару и поставщику
            var query = from sale in db.SaleWithSupplierInfoViews
                        where sale.SaleDate >= startDate && sale.SaleDate <= endDate
                        group sale by new
                        {
                            sale.ProductId,
                            sale.ProductName,
                            sale.SupplierId,
                            sale.SupplierName
                        } into g
                        where g.Key.SupplierId == supplierId
                        select new
                        {
                            Товар = g.Key.ProductName,
                            Количество = g.Sum(x => x.SaleQuantity),
                            Выручка = g.Sum(x => x.SalePrice * x.SaleQuantity),
                            Доля_по_единицам = (
                                (double)g.Sum(x => x.SaleQuantity) / (
                                    from all in db.SaleWithSupplierInfoViews
                                    where all.SaleDate >= startDate && all.SaleDate <= endDate
                                    select all
                                ).Sum(x => x.SaleQuantity) * 100
                            ),
                            Доля_по_выручке = (
                                g.Sum(x => x.SalePrice * x.SaleQuantity) / (
                                    from all in db.SaleWithSupplierInfoViews
                                    where all.SaleDate >= startDate && all.SaleDate <= endDate
                                    select all
                                ).Sum(x => x.SalePrice * x.SaleQuantity) * 100
                            )
                        };

            return await query.Cast<object>().ToListAsync();
        }

        /// <summary>
        /// Запрос 7.2: Получите прибыль магазина за указанный период с учетом закупочных цен.
        /// </summary>
        public async Task<object> GetStoreProfitAsync(DateTime startDate, DateTime endDate)
        {
            using var db = _dbContextFactory();

            var result = await (
                from sale in db.SaleWithSupplierInfoViews
                where sale.SaleDate >= startDate && sale.SaleDate <= endDate
                group sale by 1 into g // группировка для агрегации
                select new
                {
                    Общая_выручка = g.Sum(x => x.SalePrice * x.SaleQuantity),
                    Общие_закупки = g.Sum(x => x.PurchasePrice * x.SaleQuantity),
                    Прибыль = g.Sum(x => x.Profit)
                }
            ).FirstOrDefaultAsync();

            return result ?? new
            {
                Общая_выручка = 0m,
                Общие_закупки = 0m,
                Прибыль = 0m
            };
        }

        /// <summary>
        /// Запрос 8) Получите долю наклладных расходов в процентах от объема продаж за указанный период.
        /// </summary>
        public async Task<object> GetOverheadRatioAsync(DateTime startDate, DateTime endDate)
        {
            using var db = _dbContextFactory();

            // 1. Сумма таможенных платежей за период
            var totalCustomsPayments = await (
                from cp in db.CustomPayments
                where cp.PaymentDate >= startDate && cp.PaymentDate <= endDate
                select cp.PaymentAmount
            ).SumAsync();

            // 2. Общая выручка от продаж за этот период
            var totalRevenue = await (
                from s in db.Sales
                where s.SaleDate >= startDate && s.SaleDate <= endDate
                join si in db.SaleItems on s.SaleId equals si.SiSaleId
                join p in db.Products on si.SiProductId equals p.ProductId
                select si.SiQuantity * p.ProductSalePrice
            ).SumAsync();

            // 3. Процент таможенных платежей от выручки
            double customsPercent = totalRevenue == 0 ? 0 : (double)(totalCustomsPayments / totalRevenue) * 100;

            return new
            {
                Объем_продаж = totalRevenue,
                Таможенные_платежи = totalCustomsPayments,
                Доля_в_процентах = $"{customsPercent:F2}%"
            };
        }

        /// <summary>
        /// Запрос 9) Получите перечень и общее количество непроданного товара на складе за определенный период 
        ///         и его объем от общего товара в процентах.
        /// </summary>
        public async Task<List<object>> GetUnsoldStockReportAsync(DateTime startDate, DateTime endDate)
        {
            using var db = _dbContextFactory();

            // 1. Получаем все поставленные товары за период
            var deliveredQuery = from bi in db.BatchItems
                                 join b in db.Batches on bi.BiBatchId equals b.BatchId
                                 where b.DeliveryDate >= startDate && b.DeliveryDate <= endDate
                                 group bi by new { bi.BiProductId } into g
                                 select new
                                 {
                                     ProductId = g.Key.BiProductId,
                                     TotalDelivered = g.Sum(x => x.BatchItemQuantity)
                                 };

            var delivered = await deliveredQuery.ToListAsync();

            if (!delivered.Any())
                return new List<object>();

            // 2. Проданные товары за тот же период
            var soldQuery = from s in db.SaleWithSupplierInfoViews
                            where s.SaleDate >= startDate && s.SaleDate <= endDate
                            group s by new { s.ProductId } into g
                            select new
                            {
                                g.Key.ProductId,
                                TotalSold = g.Sum(x => x.SaleQuantity)
                            };

            var sold = await soldQuery.ToListAsync();

            // 3. Считаем непроданный товар
            var unsoldProducts = delivered.Select(d =>
            {
                int soldQty = sold.FirstOrDefault(s => s.ProductId == d.ProductId)?.TotalSold ?? 0;
                return new
                {
                    d.ProductId,
                    d.TotalDelivered,
                    Sold = soldQty,
                    Unsold = d.TotalDelivered - soldQty
                };
            }).Where(u => u.Unsold > 0).ToList();

            // 4. Общий объём поставок
            int totalDelivered = delivered.Sum(x => x.TotalDelivered);
            int totalUnsold = unsoldProducts.Sum(x => x.Unsold);

            double percentage = totalDelivered == 0 ? 0 : (double)totalUnsold / totalDelivered * 100;

            // 5. Финальная проекция с названиями товаров
            var result = await (
                from p in db.Products
                join up in unsoldProducts on p.ProductId equals up.ProductId
                select new
                {
                    Товар = p.ProductName,
                    Поставлено = up.TotalDelivered,
                    Продано = up.Sold,
                    Остаток = up.Unsold,
                    Процент_от_общего = $"{percentage:F2}%"
                }
            ).ToListAsync<object>();

            return result;
        }

        /// <summary>
        /// Запрос 10) Получите перечень и общее количество бракованного товара, пришедшего за определенный период 
        ///         и список поставщиков, поставивших товар.
        /// </summary>
        public async Task<List<object>> GetDefectiveItemsAsync(DateTime startDate, DateTime endDate)
        {
            using var db = _dbContextFactory();

            var defectiveQuery = from defect in db.Defects
                                 where defect.DetectionDate >= startDate && defect.DetectionDate <= endDate
                                 join bi in db.BatchItems on defect.DefectBatchItemId equals bi.BatchItemId
                                 join b in db.Batches on bi.BiBatchId equals b.BatchId
                                 join p in db.Products on bi.BiProductId equals p.ProductId
                                 join so in db.SupplierOrders on b.BatchSupplierOrderId equals so.SupplierOrderId
                                 join s in db.Suppliers on so.RecipientId equals s.SupplierId
                                 select new
                                 {
                                     Товар = p.ProductName,
                                     Поставщик = s.SupplierName,
                                     Дата_обнаружения = defect.DetectionDate,
                                     Количество = defect.DefectQuantity
                                 };

            return await defectiveQuery.ToListAsync<object>();
        }

        /// <summary>
        /// Запрос 11) Получите перечень, общее количество и стоимость товара, реализованного за конкретный день.
        /// </summary>
        public async Task<List<object>> GetDailySalesReportAsync(DateTime saleDate)
        {
            using var db = _dbContextFactory();

            // Фильтруем по дате продажи и группируем по ProductId и ProductName
            var dailySalesQuery = from s in db.SaleWithSupplierInfoViews
                                  where s.SaleDate.Date == saleDate.Date
                                  group s by new { s.ProductId, s.ProductName } into g
                                  select new
                                  {
                                      Товар = g.Key.ProductName,
                                      Количество = g.Sum(x => x.SaleQuantity),
                                      Сумма = g.Sum(x => x.SaleQuantity * x.SalePrice)
                                  };

            return await dailySalesQuery.ToListAsync<object>();
        }

        /// <summary>
        /// Запрос 12) Получите кассовый отчет за определенный период.
        /// </summary>
        public async Task<object> GetCashReportAsync(DateTime startDate, DateTime endDate)
        {
            using var db = _dbContextFactory();

            // 1. Выручка от продаж
            var salesQuery = from sale in db.Sales
                             where sale.SaleDate >= startDate && sale.SaleDate <= endDate
                             join item in db.SaleItems on sale.SaleId equals item.SiSaleId
                             join product in db.Products on item.SiProductId equals product.ProductId
                             select new { Quantity = item.SiQuantity, Price = product.ProductSalePrice };

            var salesData = await salesQuery.ToListAsync();

            decimal totalRevenue = salesData.Sum(x => x.Quantity * x.Price);

            // 2. Возвраты
            var refundsQuery = from refund in db.CustomerRefunds
                               where refund.RefundDate >= startDate && refund.RefundDate <= endDate
                               select refund.RefundAmount;

            var refundAmounts = await refundsQuery.ToListAsync();
            decimal totalRefunds = refundAmounts.Sum();

            decimal netIncome = totalRevenue - totalRefunds;

            return new
            {
                Общая_выручка = totalRevenue,
                Возвраты = totalRefunds,
                Чистый_доход = netIncome
            };
        }

        /// <summary>
        /// Запрос 13) Получите скорость оборота денежных средств, вложенных в товар (как быстро товар продается).
        /// </summary>
        public async Task<List<object>> GetProductTurnoverRateAsync(DateTime startDate, DateTime endDate)
        {
            using var db = _dbContextFactory();

            // 1. Выручка от продажи товаров за период (с использованием представления или таблиц)
            var salesQuery = from sale in db.SaleWithSupplierInfoViews
                             where sale.SaleDate >= startDate && sale.SaleDate <= endDate
                             group sale by new { sale.ProductId, sale.ProductName } into g
                             select new
                             {
                                 g.Key.ProductId,
                                 g.Key.ProductName,
                                 SalesRevenue = g.Sum(x => x.SaleQuantity * x.SalePrice),
                                 PurchaseCost = g.Sum(x => x.SaleQuantity * x.PurchasePrice)
                             };

            var salesData = await salesQuery.ToListAsync();

            if (!salesData.Any())
                return new List<object>();

            // 2. Средняя стоимость запасов за период (на основе среднего остатка и закупочной цены)
            var avgStockValueQuery = from bi in db.BatchItems
                                     join p in db.Products on bi.BiProductId equals p.ProductId
                                     join b in db.Batches on bi.BiBatchId equals b.BatchId
                                     where b.DeliveryDate <= endDate
                                     group new { bi, p } by new { bi.BiProductId, p.ProductName } into g
                                     select new
                                     {
                                         ProductId = g.Key.BiProductId,
                                         ProductName = g.Key.ProductName,
                                         AvgInventoryValue = g.Average(x => x.bi.RemainingItem * x.p.ProductSalePrice)
                                     };

            var avgStockValues = await avgStockValueQuery.ToDictionaryAsync(x => x.ProductId);

            // 3. Рассчитываем оборачиваемость
            var turnoverList = new List<dynamic>();

            foreach (var item in salesData)
            {
                decimal? avgInventory = avgStockValues.TryGetValue(item.ProductId, out var valueGroup)
                    ? valueGroup.AvgInventoryValue
                    : item.PurchaseCost; // если нет данных о среднем остатке — используем себестоимость

                decimal? turnover = avgInventory == 0 ? 0 : item.SalesRevenue / avgInventory;

                turnoverList.Add(new
                {
                    Товар = item.ProductName,
                    Выручка = item.SalesRevenue,
                    Средний_товарный_остаток = avgInventory,
                    Оборачиваемость = $"{turnover:F2} раз(а)"
                });
            }

            return turnoverList.Cast<object>().ToList();
        }

        /// <summary>
        /// Запрос 14) Получите перечень пустых ячеек и их обьем.
        /// </summary>
        public async Task<List<object>> GetEmptyStorageCellsAsync()
        {
            using var db = _dbContextFactory();

            var emptyCellsQuery = from cell in db.StorageCells
                                  where !db.BatchItems.Any(bi => bi.BiCellId == cell.CellId)
                                  select new
                                  {
                                      Ячейка = cell.CellName,
                                      Описание = cell.LocationDescription,
                                      Ширина = cell.Width.ToString() + " м",
                                      Высота = cell.Height.ToString() + " м",
                                      Глубина = cell.Depth.ToString() + " м",
                                      Объем = (cell.Width * cell.Height * cell.Depth).ToString() + " м³"
                                  };

            return await emptyCellsQuery.ToListAsync<object>();
        }

        /// <summary>
        /// Запрос 15) Получите перечень и общее количество заявок от покупателей на ожидаемый товар,
        /// подсчитайте, на какую сумму даны заявки.
        /// </summary>
        public async Task<List<object>> GetCustomerOrderRequestsAsync()
        {
            using var db = _dbContextFactory();

            // Заявки на ожидаемый товар
            var requestQuery = from order in db.CustomerOrders
                               where order.CustomerOrderStatus == false
                               join customer in db.Customers on order.CoCustomerId equals customer.CustomerId
                               join coi in db.CustomerOrderItems on order.CustomerOrderId equals coi.CoiCustomerOrderId
                               join product in db.Products on coi.CoiProductId equals product.ProductId
                               select new
                               {
                                   Товар = product.ProductName,
                                   Клиент = $"{customer.CustomerName} {customer.CustomerSurname} {customer.CustomerPatronymic}",
                                   Количество = coi.CoiQuantity,
                                   Сумма = coi.CoiQuantity * product.ProductSalePrice
                               };

            return await requestQuery.ToListAsync<object>();
        }
    }
}