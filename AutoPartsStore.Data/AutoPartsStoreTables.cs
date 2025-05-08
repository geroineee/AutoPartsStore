using AutoPartsStore.Data;
using AutoPartsStore.Data.Context;
using AutoPartsStore.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class AutoPartsStoreTables
{
    private readonly AutopartsStoreContext _db;

    private static readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

    public AutoPartsStoreTables(AutopartsStoreContext db)
    {
        _db = db;
    }

    public static readonly List<TableDefinition> TableDefinitions = new()
    {
        // Таблица Поставщики (Suppliers)
        new TableDefinition
        {
            DisplayName = "Поставщики",
            DbName = nameof(_db.Suppliers),
            TableType = typeof(Supplier),
            Columns = new List<TableColumnInfo>
            {
                new("ID", nameof(Supplier.SupplierId), isId: true, isVisible: false),
                new("Название", nameof(Supplier.SupplierName)),
                new TableColumnInfo(
                    displayName: "Категория",
                    propertyName: nameof(SupplierCategory.CategoryName),
                    referenceTable: nameof(_db.SupplierCategories),
                    referenceIdColumn: nameof(SupplierCategory.CategoryId),
                    foreignKeyProperty: nameof(Supplier.SupplierCategoryId)
                ),
                new("Адрес", nameof(Supplier.SupplierAddress)),
                new("Активен", nameof(Supplier.IsActive))
            },
            QueryBuilder = db => db.Suppliers
                .Include(s => s.SupplierCategory)
                .Select(s => new
                {
                    s.SupplierId,
                    s.SupplierName,
                    s.SupplierCategoryId,
                    s.SupplierCategory.CategoryName,
                    s.SupplierAddress,
                    s.IsActive
                })
        },

        // Таблица Категории поставщиков (SupplierCategories)
        new TableDefinition
        {
            DisplayName = "Категории поставщиков",
            DbName = "SupplierCategories",
            TableType = typeof(SupplierCategory),
            Columns = new List<TableColumnInfo>
            {
                new("ID", "CategoryId", isId: true, isVisible:false),
                new("Название", "CategoryName"),
                new("Дает гарантию", "ProvidesGuarantee"),
                new("Дает скидку", "ProvidesDiscount"),
                new("Описание", "CategoryDescription")
            },
            QueryBuilder = db => db.SupplierCategories
                .Select(c => new
                {
                    c.CategoryId,
                    c.CategoryName,
                    c.ProvidesGuarantee,
                    c.ProvidesDiscount,
                    c.CategoryDescription
                })
        },

        // Таблица Товары (Products)
        new TableDefinition
        {
            DisplayName = "Товары",
            DbName = "Products",
            TableType = typeof(Product),
            Columns = new List<TableColumnInfo>
            {
                new("ID", "ProductId", isId: true, isVisible:false),
                new("Наименование", "ProductName"),
                new("Цена продажи", "ProductSalePrice"),
                new("Описание", "ProductDescription")
            },
            QueryBuilder = db => db.Products
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.ProductSalePrice,
                    p.ProductDescription
                })
        },

        // Таблица Партии (Batches)
        new TableDefinition
        {
            DisplayName = "Партии",
            DbName = "Batches",
            TableType = typeof(Batch),
            Columns = new List<TableColumnInfo>
            {
                new("Номер партии", "BatchId", isId: true),
                new("Поставщик", "SupplierName", referenceTable: "Suppliers", referenceIdColumn: "SupplierId", foreignKeyProperty: "BatchSupplierId"),
                new("Дата доставки", "DeliveryDate"),
                new("Описание", "BatchDescription")
            },
            QueryBuilder = db => db.Batches
                .Include(b => b.BatchSupplier)
                .Select(b => new
                {
                    b.BatchId,
                    b.BatchSupplier.SupplierName,
                    b.DeliveryDate,
                    b.BatchDescription,
                    BatchSupplierId = b.BatchSupplierId
                })
        },

        // Таблица Состав партии (BatchItems)
        new TableDefinition
        {
            DisplayName = "Состав партии",
            DbName = "BatchItems",
            TableType = typeof(BatchItem),
            Columns = new List<TableColumnInfo>
            {
                new("ID", "BatchItemId", isId: true, isVisible: false),
                new("Номер партии", "BatchId", referenceTable: nameof(_db.Batches), referenceIdColumn: nameof(Batch.BatchId), foreignKeyProperty: "BiBatchId"),
                new("Товар", "ProductName", referenceTable: "Products", referenceIdColumn: "ProductId", foreignKeyProperty: "BiProductId"),
                new("Количество", "BatchItemQuantity"),
                new("Остаток", "RemainingItem")
            },
            QueryBuilder = db => db.BatchItems
                .Include(bi => bi.BiBatch)
                .Include(bi => bi.BiProduct)
                .Select(bi => new
                {
                    bi.BatchItemId,
                    bi.BiBatch.BatchId,
                    bi.BiBatchId,
                    bi.BiProduct.ProductName,
                    bi.BatchItemQuantity,
                    bi.RemainingItem,
                    bi.BiProductId
                })
        },

        // Таблица Клиенты (Customers)
        new TableDefinition
        {
            DisplayName = "Клиенты",
            DbName = "Customers",
            TableType = typeof(Customer),
            Columns = new List<TableColumnInfo>
            {
                new("ID", "CustomerId", isId: true, isVisible: false),
                new("Имя", "CustomerName"),
                new("Фамилия", "CustomerSurname"),
                new("Отчество", "CustomerPatronymic"),
                new("Телефон", "CustomerPhone"),
                new("Email", "CustomerEmail")
            },
            QueryBuilder = db => db.Customers
                .Select(c => new
                {
                    c.CustomerId,
                    c.CustomerName,
                    c.CustomerSurname,
                    c.CustomerPatronymic,
                    c.CustomerPhone,
                    c.CustomerEmail
                })
        },

        // Таблица Продажи (Sales)
        new TableDefinition
        {
            DisplayName = "Продажи",
            DbName = "Sales",
            TableType = typeof(Sale),
            Columns = new List<TableColumnInfo>
            {
                new("Номер продажи", "SaleId", isId: true),
                new("Клиент", "CustomerSurname", referenceTable: "Customers", referenceIdColumn: "CustomerId", foreignKeyProperty: "SaleCustomerId"),
                new("Сотрудник", "EmployeeSurname", referenceTable: "Employees", referenceIdColumn: "EmployeeId", foreignKeyProperty: "SaleEmployeeId"),
                new("Дата продажи", "SaleDate"),
                new("Сумма", "SaleTotalAmount")
            },
            QueryBuilder = db => db.Sales
                .Include(s => s.SaleCustomer)
                .Include(s => s.SaleEmployee)
                .Include(s => s.SaleItems)
                    .ThenInclude(si => si.SiBatchItem) 
                    .ThenInclude(bi => bi.BiProduct)
                .Select(s => new
                {
                    s.SaleId,
                    CustomerSurname = s.SaleCustomer.CustomerSurname + " " + s.SaleCustomer.CustomerName + " " + s.SaleCustomer.CustomerPatronymic,
                    EmployeeSurname = s.SaleEmployee.EmployeeSurname + " " + s.SaleEmployee.EmployeeName + " " + s.SaleEmployee.EmployeePatronymic,
                    s.SaleDate,
                    SaleTotalAmount = s.SaleItems.Sum(si => si.SiQuantity * si.SiBatchItem.BiProduct.ProductSalePrice)
                })
        },

        // Таблица Состав продажи (SaleItems)
        new TableDefinition
        {
            DisplayName = "Состав продажи",
            DbName = "SaleItems",
            TableType = typeof(SaleItem),
            Columns = new List<TableColumnInfo>
            {
                new("ID", "SaleItemId", isId: true, isVisible: false),
                new("Номер продажи", "SaleId",  referenceTable: "Sales", referenceIdColumn: "SaleId", foreignKeyProperty: "SiSaleId"),
                new("Товар", "ProductName", referenceTable: "Products", referenceIdColumn: "ProductId", foreignKeyProperty: "SiBatchItemId"),
                new("Количество", "SiQuantity")
            },
            QueryBuilder = db => db.SaleItems
                .Include(si => si.SiSale)
                .Include(si => si.SiBatchItem)
                .ThenInclude(bi => bi.BiProduct)
                .Select(si => new
                {
                    si.SaleItemId,
                    si.SiSaleId,
                    si.SiSale.SaleId,
                    si.SiBatchItem.BiProduct.ProductName,
                    si.SiQuantity,
                    ProductPrice = si.SiBatchItem.BiProduct.ProductSalePrice
                })            
        },

        // Таблица Сотрудники (Employees)
        new TableDefinition
        {
            DisplayName = "Сотрудники",
            DbName = "Employees",
            TableType = typeof(Employee),
            Columns = new List<TableColumnInfo>
            {
                new("ID", "EmployeeId", isId: true, isVisible: false),
                new("Имя", "EmployeeName"),
                new("Фамилия", "EmployeeSurname"),
                new("Отчество", "EmployeePatronymic"),
                new("Должность", "EmployeePosition"),
                new("Телефон", "EmployeePhone"),
                new("Email", "EmployeeEmail"),
                new("Дата найма", "HireDate"),
                new("Дата увольнения", "FireDate")
            },
            QueryBuilder = db => db.Employees
                .Select(e => new
                {
                    e.EmployeeId,
                    e.EmployeeName,
                    e.EmployeeSurname,
                    e.EmployeePatronymic,
                    e.EmployeePosition,
                    e.EmployeePhone,
                    e.EmployeeEmail,
                    e.HireDate,
                    e.FireDate
                })
        },

        // Таблица Заказы поставщикам (SupplierOrders)
        new TableDefinition
        {
            DisplayName = "Заказы поставщикам",
            DbName = "SupplierOrders",
            TableType = typeof(SupplierOrder),
            Columns = new List<TableColumnInfo>
            {
                new("Номер заказа", "SupplierOrderId", isId: true),
                new("Менеджер", "EmployeeSurname",  referenceTable: "Employees", referenceIdColumn: "EmployeeId", foreignKeyProperty: "ManagerId"),
                new("Поставщик", "SupplierName", referenceTable: "Suppliers", referenceIdColumn: "SupplierId", foreignKeyProperty: "RecipientId"),
                new("Дата заказа", "SupplierOrderDate"),
                new("Ожидаемая дата", "ExpectedDeliveryDate"),
                new("Статус", "SupplierOrderStatus"),
                new("Сумма", "SupplierOrderTotalAmount"),
            },
            QueryBuilder = db => db.SupplierOrders
                .Include(so => so.Manager)
                .Include(so => so.Recipient)
                .Include(so => so.SupplierOrderItems)
                    .ThenInclude(soi => soi.SoiProduct)
                .Select(so => new
                {
                    so.SupplierOrderId,
                    EmployeeSurname = so.Manager.EmployeeSurname + " " + so.Manager.EmployeeName + " " + so.Manager.EmployeePatronymic,
                    so.Recipient.SupplierName,
                    so.SupplierOrderDate,
                    so.ExpectedDeliveryDate,
                    SupplierOrderTotalAmount = so.SupplierOrderItems.Sum(soi => soi.SoiQuantity * soi.SoiProduct.ProductSalePrice),
                    so.SupplierOrderStatus,
                    so.RecipientId
                })
        },

        // Таблица Состав заказов поставщикам (SupplierOrderItems)
        new TableDefinition
        {
            DisplayName = "Состав заказов поставщикам",
            DbName = "SupplierOrderItems",
            TableType = typeof(SupplierOrderItem),
            Columns = new List<TableColumnInfo>
            {
                //new("Поставщик", "SupplierName", referenceTable: "Suppliers", referenceIdColumn: "SupplierId", foreignKeyProperty: "SoiSupplierOrderId", isEditable: false),
                new("Номер заказа", "SupplierOrderId", referenceTable: "SupplierOrders", referenceIdColumn: "SupplierOrderId", foreignKeyProperty: "SoiSupplierOrderId", isId: true, isCompositeKey: true),
                new("Товар", "ProductName", referenceTable: "Products", referenceIdColumn: "ProductId", foreignKeyProperty: "SoiProductId", isId: true, isCompositeKey: true),
                new("Количество", "SoiQuantity")
            },
            QueryBuilder = db => db.SupplierOrderItems
                .Include(soi => soi.SoiSupplierOrder)
                    .ThenInclude(so => so.Recipient)
                .Include(soi => soi.SoiProduct)
                .Select(soi => new
                {
                    soi.SoiSupplierOrder.SupplierOrderId,
                    soi.SoiSupplierOrder.Recipient.SupplierName,
                    soi.SoiProduct.ProductName,
                    soi.SoiQuantity,
                    soi.SoiSupplierOrderId,
                    soi.SoiProductId,
                })
        },

        // Таблица Условия поставки (DeliveryTerms)
        new TableDefinition
        {
            DisplayName = "Условия поставки",
            DbName = "DeliveryTerms",
            TableType = typeof(DeliveryTerm),
            Columns = new List<TableColumnInfo>
            {
                new("Поставщик", "SupplierName",
                    referenceTable: "Suppliers",
                    referenceIdColumn: "SupplierId",
                    foreignKeyProperty: "DtSupplierId",
                    isId: true,
                    isCompositeKey: true),
                new("Товар", "ProductName",
                    referenceTable: "Products",
                    referenceIdColumn: "ProductId",
                    foreignKeyProperty: "DtProductId",
                    isId: true,
                    isCompositeKey: true),
                new("Цена доставки", "DeliveryPrice"),
                new("Срок доставки (дни)", "DeliveryDays")
            },
            QueryBuilder = db => db.DeliveryTerms
                .Include(dt => dt.DtSupplier)
                .Include(dt => dt.DtProduct)
                .Select(dt => new
                {
                    dt.DtSupplier.SupplierName,
                    dt.DtProduct.ProductName,
                    dt.DeliveryPrice,
                    dt.DeliveryDays,
                    dt.DtSupplierId,
                    dt.DtProductId
                })
        },

        // Таблица Ячейки склада (StorageCells)
        new TableDefinition
        {
            DisplayName = "Ячейки склада",
            DbName = "StorageCells",
            TableType = typeof(StorageCell),
            Columns = new List<TableColumnInfo>
            {
                new("ID", "CellId", isId: true, isVisible: false),
                new("Название", "CellName"),
                new("Описание", "LocationDescription"),
                new("Ширина", "Width"),
                new("Глубина", "Depth"),
                new("Высота", "Height"),
                new("Макс. вес", "MaxWeight")
            },
            QueryBuilder = db => db.StorageCells
                .Select(sc => new
                {
                    sc.CellId,
                    sc.CellName,
                    sc.LocationDescription,
                    sc.Width,
                    sc.Depth,
                    sc.Height,
                    sc.MaxWeight
                })
        },

        // Таблица Товары на складе (StockItems)
        new TableDefinition
        {
            DisplayName = "Товары на складе",
            DbName = "StockItems",
            TableType = typeof(StockItem),
            Columns = new List<TableColumnInfo>
            {
                new("Название ячейки", "CellName",
                    isEditable:false,
                    referenceTable: "StorageCells",
                    referenceIdColumn: "CellId",
                    foreignKeyProperty: "StockStorageCellId"),
                new("Номер ячейки", "CellId",
                    referenceTable: "StorageCells",
                    referenceIdColumn: "CellId",
                    foreignKeyProperty: "StockStorageCellId",
                    isId: true,
                    isCompositeKey: true),
                new("Товар", "ProductName",
                    referenceTable: "Products",
                    referenceIdColumn: "ProductId",
                    foreignKeyProperty: "StockBatchItemId",
                    isId: true,
                    isCompositeKey: true),
                
                new("Количество", "StockItemQuantity")
            },
            QueryBuilder = db => db.StockItems
                .Include(si => si.StockBatchItem)
                    .ThenInclude(bi => bi.BiProduct)
                .Include(si => si.StockStorageCell)
                .Select(si => new
                {
                    si.StockBatchItem.BiProduct.ProductName,
                    si.StockStorageCell.CellId,
                    si.StockStorageCell.CellName,
                    si.StockItemQuantity,
                    si.StockBatchItemId,
                    si.StockStorageCellId
                })
        },

        // Таблица Возвраты клиентов (CustomerRefunds)
        new TableDefinition
        {
            DisplayName = "Возвраты клиентов",
            DbName = "CustomerRefunds",
            TableType = typeof(CustomerRefund),
            Columns = new List<TableColumnInfo>
            {
                new("ID", "CustomerRefundId", isId: true, isVisible: false),
                new("Товар", "ProductName", referenceTable: "Products", referenceIdColumn: "ProductId", foreignKeyProperty: "CrSaleItemId"),
                new("Количество", "CrQuantity"),
                new("Дата возврата", "RefundDate"),
                new("Сумма возврата", "RefundAmount"),
                new("Бракованный", "IsDefect")
            },
            QueryBuilder = db => db.CustomerRefunds
                .Include(cr => cr.CrSaleItem)
                .ThenInclude(si => si.SiBatchItem)
                .ThenInclude(bi => bi.BiProduct)
                .Select(cr => new
                {
                    cr.CustomerRefundId,
                    cr.CrSaleItem.SiBatchItem.BiProduct.ProductName,
                    cr.CrQuantity,
                    cr.RefundDate,
                    cr.RefundAmount,
                    cr.IsDefect
                })
        },

        // Таблица Брак (Defects)
        new TableDefinition
        {
            DisplayName = "Брак",
            DbName = "Defects",
            TableType = typeof(Defect),
            Columns = new List<TableColumnInfo>
            {
                new("ID", "DefectId", isId: true, isVisible: false),
                new("Номер партии", "BatchItemId", referenceTable: "BatchItems", referenceIdColumn: "BatchItemId", foreignKeyProperty: "DefectBatchItemId"),
                new("Товар", "ProductName", referenceTable: "Products", referenceIdColumn: "ProductId", foreignKeyProperty: "DefectBatchItemId"),
                new("Количество", "DefectQuantity"),
                new("Дата обнаружения", "DetectionDate"),
                new("Описание", "DefectDescription")
            },
            QueryBuilder = db => db.Defects
                .Include(d => d.DefectBatchItem)
                .ThenInclude(bi => bi.BiProduct)
                .Select(d => new
                {
                    d.DefectId,
                    d.DefectBatchItem.BatchItemId,
                    d.DefectBatchItem.BiProduct.ProductName,
                    d.DefectQuantity,
                    d.DetectionDate,
                    d.DefectDescription
                })
        },

        // Таблица Контракты поставщиков (SupplierContracts)
        new TableDefinition
        {
            DisplayName = "Контракты поставщиков",
            DbName = "SupplierContracts",
            TableType = typeof(SupplierContract),
            Columns = new List<TableColumnInfo>
            {
                new("ID", "ContractId", isId: true, isVisible: false),
                new TableColumnInfo(
                    displayName: "Поставщик",
                    propertyName: "SupplierName",
                    referenceTable: "Suppliers",
                    referenceIdColumn: "SupplierId",
                    foreignKeyProperty: "ContractSupplierId"
                ),
                new("Номер контракта", "ContractNumber"),
                new("Дата начала", "StartDate"),
                new("Дата окончания", "EndDate"),
                new("Условия", "Terms"),
                new("Скидка", "Discount")
            },
            QueryBuilder = db => db.SupplierContracts
                .Include(sc => sc.ContractSupplier)
                .Select(sc => new
                {
                    sc.ContractId,
                    sc.ContractNumber,
                    sc.StartDate,
                    sc.EndDate,
                    sc.Terms,
                    sc.Discount,
                    sc.ContractSupplier.SupplierName,
                    sc.ContractSupplierId
                })
        },
        new TableDefinition()
        {
            DisplayName = "Заказы клиентов",
            DbName = nameof(_db.CustomerOrders),
            TableType = typeof(SupplierContract),
            Columns = new List<TableColumnInfo>
            {
                new("Номер заказа", "CustomerOrderId", isId: true, isVisible: false),
                new TableColumnInfo(
                    displayName: "Клиент",
                    propertyName: "CustomerSurname",
                    referenceTable: "Customers",
                    referenceIdColumn: "CustomerId",
                    foreignKeyProperty: "CoCustomerId"
                ),
                new("Дата заказа", "CustomerOrderDate"),
                new("Описание", "CustomerOrderDescription"),
                new("Статус", "CustomerOrderStatus")
            },
            QueryBuilder = db => db.CustomerOrders
                .Include(co => co.CoCustomer)
                .Select(co => new
                {
                    co.CustomerOrderId,
                    CustomerSurname = co.CoCustomer.CustomerName + " " + co.CoCustomer.CustomerSurname + " " + co.CoCustomer.CustomerPatronymic,
                    co.CustomerOrderDate,
                    co.CustomerOrderDescription,
                    co.CustomerOrderStatus
                })
        }
    };

    public Dictionary<string, string> AvailableTables =>
        TableDefinitions.ToDictionary(t => t.DisplayName, t => t.DbName);

    public List<TableColumnInfo> GetColumns(string tableName) =>
        TableDefinitions.First(t => t.DbName == tableName).Columns;

    public async Task<List<object>> GetTableDataAsync(string tableName)
    {
        await _asyncLock.WaitAsync();
        try
        {
            var definition = TableDefinitions.FirstOrDefault(t => t.DisplayName == tableName)
                ?? throw new ArgumentException("Unknown table");

            return await definition.QueryBuilder(_db).ToListAsync();
        }
        finally
        {
            _asyncLock.Release();
        }
    }

    public async Task<List<object>> SearchInTableAsync(string tableName, string columnDisplayName, string searchValue)
    {
        await _asyncLock.WaitAsync();
        try
        {
            var definition = TableDefinitions.FirstOrDefault(t => t.DisplayName == tableName)
            ?? throw new ArgumentException("Unknown table");

            var columnInfo = definition.Columns.FirstOrDefault(c => c.DisplayName == columnDisplayName);
            if (columnInfo == null)
                throw new ArgumentException($"Column '{columnDisplayName}' not found in table '{tableName}'");

            var query = definition.QueryBuilder(_db).AsQueryable();
            var parameter = Expression.Parameter(query.ElementType, "x");

            var property = Expression.Property(parameter, columnInfo.PropertyName);

            var toStringCall = Expression.Call(property, "ToString", null, null);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var containsCall = Expression.Call(toStringCall, containsMethod, Expression.Constant(searchValue));

            var lambda = Expression.Lambda(containsCall, parameter);

            var whereCall = Expression.Call(
                typeof(Queryable),
                "Where",
                [query.ElementType],
                query.Expression,
                lambda);

            var filteredQuery = query.Provider.CreateQuery(whereCall);
            return await ((IQueryable<object>)filteredQuery).ToListAsync();
        }
        finally
        {
            _asyncLock.Release();
        }
    }

    public async Task DeleteTableItemAsync(string tableDisplayName, object selectedItem)
    {
        await _asyncLock.WaitAsync();
        try
        {
            if (selectedItem == null)
                throw new ArgumentNullException(nameof(selectedItem));

            // 1. Находим определение таблицы
            var tableDef = TableDefinitions.First(t => t.DisplayName == tableDisplayName);

            if (tableDef == null)
                throw new ArgumentException("Table definition not found");

            // 2. Получаем DbSet по имени таблицы из контекста
            var dbSetProperty = _db.GetType().GetProperty(tableDef.DbName);
            if (dbSetProperty == null)
                throw new ArgumentException($"DbSet '{tableDef.DbName}' not found in DbContext");

            var dbSet = (IQueryable)dbSetProperty.GetValue(_db);
            var entityType = dbSet.ElementType;

            // 3. Incorporate FindOriginalEntity logic
            object entity = await FindOriginalEntityAsync(tableDef, selectedItem);

            if (entity == null)
                throw new InvalidOperationException("Entity not found in database");

            _db.Remove(entity);
            await _db.SaveChangesAsync();
        }
        finally
        {
            _asyncLock.Release();
        }
    }

    public async Task<object> FindOriginalEntityAsync(TableDefinition tableDef, object selectedItem)
    {
        // Получаем все колонки, которые являются частью ключа (Id или составного ключа)
        var keyColumns = tableDef.Columns
            .Where(c => c.IsId || c.IsCompositeKey)
            .ToList();

        if (!keyColumns.Any())
        {
            // Если нет явных ключевых колонок, ищем по свойству, заканчивающемуся на Id
            keyColumns = tableDef.Columns
                .Where(c => c.PropertyName.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!keyColumns.Any()) return null!;
        }

        // Если ключ составной - собираем все значения
        if (keyColumns.Count > 1 || keyColumns.Any(c => c.IsCompositeKey))
        {
            var keyValues = new Dictionary<string, object>();
            foreach (var keyColumn in keyColumns)
            {
                var prop = selectedItem.GetType().GetProperty(keyColumn.ForeignKeyProperty);
                if (prop == null) return null!;

                var value = prop.GetValue(selectedItem);
                if (value == null) return null!;

                keyValues[keyColumn.ForeignKeyProperty] = value;
            }

            return await GetCompositeKeyItemAsync(tableDef.DbName, keyValues);
        }
        else
        {
            // Обычный одиночный ключ
            var idColumn = keyColumns.First();
            var idProperty = selectedItem.GetType().GetProperty(idColumn.PropertyName);
            if (idProperty == null) return null!;

            var idValue = idProperty.GetValue(selectedItem);
            if (idValue == null) return null!;

            return await GetItemByIdAsync(tableDef.DbName, idValue);
        }
    }

    public async Task UpdateItemAsync(string tableName, object updatedItem)
    {
        await _asyncLock.WaitAsync();
        try
        {
            // 1. Находим определение таблицы
            var tableDef = TableDefinitions.First(t => t.DbName == tableName);

            // 2. Используем FindOriginalEntityAsync для поиска существующей сущности
            var entity = await FindOriginalEntityAsync(tableDef, updatedItem);

            if (entity == null)
            {
                throw new InvalidOperationException("Entity not found");
            }

            // 3. Копируем значения из updatedItem в entity
            var keyColumns = tableDef.Columns
                .Where(c => c.IsId || c.IsCompositeKey)
                .ToList();

            foreach (var prop in updatedItem.GetType().GetProperties())
            {
                // Пропускаем ключевые свойства
                if (keyColumns.Any(k => k.PropertyName == prop.Name))
                    continue;

                var entityProp = entity.GetType().GetProperty(prop.Name);
                if (entityProp == null || !entityProp.CanWrite) continue;

                var value = prop.GetValue(updatedItem);
                entityProp.SetValue(entity, value);
            }

            await _db.SaveChangesAsync();
        }
        finally
        {
            _asyncLock.Release();
        }
    }

    public async Task<object> GetItemByIdAsync(string tableName, object id)
    {
        try
        {
            var tableDef = TableDefinitions.First(t => t.DbName == tableName);

            var dbSetProperty = _db.GetType().GetProperty(tableName);
            if (dbSetProperty == null) throw new ArgumentException($"Таблица {tableName} не найдена");

            var dbSet = (IQueryable)dbSetProperty.GetValue(_db)!;

            var idColumn = tableDef.Columns.First(c => c.IsId || c.PropertyName.EndsWith("Id"));
            var idPropertyName = idColumn.PropertyName;

            var parameter = Expression.Parameter(dbSet?.ElementType!, "x");

            var idProperty = Expression.Property(parameter, idPropertyName);

            var equals = Expression.Equal(idProperty, Expression.Constant(id));
            var lambda = Expression.Lambda(equals, parameter);

            var whereMethod = typeof(Queryable).GetMethods()
                .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
                .MakeGenericMethod(dbSet?.ElementType!);

            var filteredQuery = (IQueryable)whereMethod.Invoke(
                null,
                [dbSet, lambda])!;

            var firstOrDefaultAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .First(m => m.Name == "FirstOrDefaultAsync" && m.GetParameters().Length == 2)
                .MakeGenericMethod(dbSet.ElementType);

            var task = (Task)firstOrDefaultAsyncMethod.Invoke(
                null,
                [filteredQuery, default(CancellationToken)])!;

            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null!;
        }
    }

    public async Task<object> GetCompositeKeyItemAsync(string tableName, Dictionary<string, object> keyValues)
    {
        try
        {
            var tableDef = TableDefinitions.First(t => t.DbName == tableName);

            var dbSetProperty = _db.GetType().GetProperty(tableName) ?? throw new ArgumentException($"Таблица {tableName} не найдена");
            var dbSet = (IQueryable)dbSetProperty.GetValue(_db)!;
            var parameter = Expression.Parameter(dbSet.ElementType, "x");

            // Строим условие WHERE для всех ключевых полей
            Expression? whereCondition = null;
            foreach (var kvp in keyValues)
            {
                var property = Expression.Property(parameter, kvp.Key);
                var equals = Expression.Equal(property, Expression.Constant(kvp.Value));

                whereCondition = whereCondition == null
                    ? equals
                    : Expression.AndAlso(whereCondition, equals);
            }

            if (whereCondition == null)
                throw new InvalidOperationException("Cant build search conditions");

            var lambda = Expression.Lambda(whereCondition, parameter);

            var whereMethod = typeof(Queryable).GetMethods()
                .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
                .MakeGenericMethod(dbSet.ElementType);

            var filteredQuery = (IQueryable)whereMethod.Invoke(
                null,
                [dbSet, lambda])!;

            var firstOrDefaultAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
                .GetMethods()
                .First(m => m.Name == "FirstOrDefaultAsync" && m.GetParameters().Length == 2)
                .MakeGenericMethod(dbSet.ElementType);

            var task = (Task)firstOrDefaultAsyncMethod.Invoke(
                null,
                [filteredQuery, default(CancellationToken)])!;

            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null!;
        }
    }

    public async Task AddNewItemAsync(string tableName, object newItem)
    {
        await _asyncLock.WaitAsync();
        try
        {
            // 1. Получаем DbSet
            var dbSetProperty = _db.GetType().GetProperty(tableName);
            if (dbSetProperty == null)
                throw new ArgumentException($"Table {tableName} not found");

            var dbSet = dbSetProperty.GetValue(_db);

            // 2. Добавляем новую сущность
            var addMethod = dbSet.GetType().GetMethod("Add");
            addMethod.Invoke(dbSet, new[] { newItem });

            // 3. Сохраняем изменения
            await _db.SaveChangesAsync();
        }
        finally
        {
            _asyncLock.Release();
        }
    }

}