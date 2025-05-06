using AutoPartsStore.Data.Context;
using AutoPartsStore.Data;
using Microsoft.EntityFrameworkCore;
using AutoPartsStore.Data.Models;
using System.Linq;
using System.Linq.Expressions;

public class AutoPartsStoreTables
{
    private readonly AutopartsStoreContext _db;

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
        DbName = "Suppliers",
        Columns = new List<TableColumnInfo>
        {
            new("ID", "SupplierId", isId: true, isVisible: false),
            new("Название", "SupplierName"),
            new TableColumnInfo(
                displayName: "Категория",
                propertyName: "CategoryName",
                referenceTable: "SupplierCategories",
                referenceIdColumn: "CategoryId",
                foreignKeyProperty: "SupplierCategoryId"
            ),
            new("Адрес", "SupplierAddress"),
            new("Активен", "IsActive")
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
        Columns = new List<TableColumnInfo>
        {
            new("ID", "CategoryId", isId: true, isVisible:false),
            new("Название", "CategoryName"),
            new("Дает гарантию", "ProvidesGuarantee"),
            new("Описание", "CategoryDescription")
        },
        QueryBuilder = db => db.SupplierCategories
            .Select(c => new
            {
                c.CategoryId,
                c.CategoryName,
                c.ProvidesGuarantee,
                c.CategoryDescription
            })
    },

    // Таблица Товары (Products)
    new TableDefinition
    {
        DisplayName = "Товары",
        DbName = "Products",
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
        Columns = new List<TableColumnInfo>
        {
            new("ID", "BatchItemId", isId: true, isVisible: false),
            new("Номер партии", "BatchId", referenceTable: "Batches", referenceIdColumn: "BatchId", foreignKeyProperty: "BiBatchId"),
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
        Columns = new List<TableColumnInfo>
        {
            new("ID", "SaleId", isId: true, isVisible: false),
            new("Клиент", "CustomerSurname", referenceTable: "Customers", referenceIdColumn: "CustomerId", foreignKeyProperty: "SaleCustomerId"),
            new("Сотрудник", "EmployeeSurname", referenceTable: "Employees", referenceIdColumn: "EmployeeId", foreignKeyProperty: "SaleEmployeeId"),
            new("Дата продажи", "SaleDate"),
            new("Сумма", "SaleTotalAmount")
        },
        QueryBuilder = db => db.Sales
            .Include(s => s.SaleCustomer)
            .Include(s => s.SaleEmployee)
            .Select(s => new
            {
                s.SaleId,
                CustomerSurname = s.SaleCustomer.CustomerSurname + " " + s.SaleCustomer.CustomerName + " " + s.SaleCustomer.CustomerPatronymic,
                EmployeeSurname = s.SaleEmployee.EmployeeSurname + " " + s.SaleEmployee.EmployeeName + " " + s.SaleEmployee.EmployeePatronymic,
                s.SaleDate,
                s.SaleTotalAmount
            })
    },

    // Таблица Состав продажи (SaleItems)
    new TableDefinition
    {
        DisplayName = "Состав продажи",
        DbName = "SaleItems",
        Columns = new List<TableColumnInfo>
        {
            new("Номер продажи", "SaleItemId", isId: true),
            new("Товар", "ProductName", referenceTable: "Products", referenceIdColumn: "ProductId", foreignKeyProperty: "SiBatchItemId"),
            new("Количество", "SiQuantity")
        },
        QueryBuilder = db => db.SaleItems
            .Include(si => si.SiBatchItem)
            .ThenInclude(bi => bi.BiProduct)
            .Select(si => new
            {
                si.SaleItemId,
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
        Columns = new List<TableColumnInfo>
        {
            new("ID", "SupplierOrderId", isId: true, isVisible: false),
            new("Менеджер", "EmployeeSurname",  referenceTable: "Employees", referenceIdColumn: "EmployeeId", foreignKeyProperty: "ManagerId"),
            new("Поставщик", "SupplierName", referenceTable: "Suppliers", referenceIdColumn: "SupplierId", foreignKeyProperty: "RecipientId"),
            new("Дата заказа", "SupplierOrderDate"),
            new("Ожидаемая дата", "ExpectedDeliveryDate"),
            new("Сумма", "SupplierOrderTotalAmount"),
            new("Статус", "SupplierOrderStatus")
        },
        QueryBuilder = db => db.SupplierOrders
            .Include(so => so.Manager)
            .Include(so => so.Recipient)
            .Select(so => new
            {
                so.SupplierOrderId,
                EmployeeSurname = so.Manager.EmployeeSurname + " " + so.Manager.EmployeeName + " " + so.Manager.EmployeePatronymic,
                so.Recipient.SupplierName,
                so.SupplierOrderDate,
                so.ExpectedDeliveryDate,
                so.SupplierOrderTotalAmount,
                so.SupplierOrderStatus,
                so.RecipientId
            })
    },

    // Таблица Состав заказов поставщикам (SupplierOrderItems)
    new TableDefinition
    {
        DisplayName = "Состав заказов поставщикам",
        DbName = "SupplierOrderItems",
        Columns = new List<TableColumnInfo>
        {
            new("Номер заказа", "SoiSupplierOrderId", referenceTable: "SupplierOrders", referenceIdColumn: "SoiSupplierOrderId", isId: true),
            new("Товар", "ProductName", referenceTable: "Products", referenceIdColumn: "ProductId", foreignKeyProperty: "SoiProductId"),
            new("Количество", "SoiQuantity")
        },
        QueryBuilder = db => db.SupplierOrderItems
            .Include(soi => soi.SoiProduct)
            .Select(soi => new
            {
                soi.SoiSupplierOrderId,
                soi.SoiProduct.ProductName,
                soi.SoiQuantity,
                soi.SoiProductId
            })
    },

    // Таблица Условия поставки (DeliveryTerms)
    new TableDefinition
    {
        DisplayName = "Условия поставки",
        DbName = "DeliveryTerms",
        Columns = new List<TableColumnInfo>
        {
            new("Поставщик", "SupplierName", referenceTable: "Suppliers", referenceIdColumn: "SupplierId", foreignKeyProperty: "DtSupplierId", isId: true),
            new("Товар", "ProductName", referenceTable: "Products", referenceIdColumn: "ProductId", foreignKeyProperty: "DtProductId"),
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
        Columns = new List<TableColumnInfo>
        {
            new("Товар", "ProductName", referenceTable: "Products", referenceIdColumn: "ProductId", foreignKeyProperty: "StockBatchItemId", isId: true),
            new("Ячейка", "CellName", referenceTable: "StorageCells", referenceIdColumn: "StorageCellId", foreignKeyProperty: "StockStorageCellId"),
            new("Количество", "StockItemQuantity")
        },
        QueryBuilder = db => db.StockItems
            .Include(si => si.StockBatchItem)
            .ThenInclude(bi => bi.BiProduct)
            .Select(si => new
            {
                si.StockBatchItem.BiProduct.ProductName,
                si.StockStorageCellId,
                si.StockStorageCell.CellName,
                si.StockItemQuantity,
                si.StockBatchItemId
            })
    },

    // Таблица Возвраты клиентов (CustomerRefunds)
    new TableDefinition
    {
        DisplayName = "Возвраты клиентов",
        DbName = "CustomerRefunds",
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
                SupplierName = sc.ContractSupplier.SupplierName,
                ContractSupplierId = sc.ContractSupplierId
            })
    }
};

    public Dictionary<string, string> AvailableTables =>
        TableDefinitions.ToDictionary(t => t.DisplayName, t => t.DbName);

    public List<TableColumnInfo> GetColumns(string tableName) =>
        TableDefinitions.First(t => t.DbName == tableName).Columns;

    public async Task<List<object>> GetTableDataAsync(string tableName)
    {
        var definition = TableDefinitions.FirstOrDefault(t => t.DisplayName == tableName)
            ?? throw new ArgumentException("Unknown table");

        return await definition.QueryBuilder(_db).ToListAsync();
    }

    public async Task<List<object>> SearchInTableAsync(string tableName, string columnDisplayName, string searchValue)
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

    public async Task DeleteTableItemAsync(string tableDisplayName, object selectedItem)
    {
        if (selectedItem == null)
            throw new ArgumentNullException(nameof(selectedItem));

        // 1. Находим определение таблицы
        var tableDef = TableDefinitions.First(t => t.DisplayName == tableDisplayName);

        // 2. Получаем DbSet по имени таблицы из контекста
        var dbSetProperty = _db.GetType().GetProperty(tableDef.DbName);
        if (dbSetProperty == null)
            throw new ArgumentException($"DbSet '{tableDef.DbName}' not found in DbContext");

        var dbSet = (IQueryable)dbSetProperty.GetValue(_db);

        // 3. Получаем ID (ищем свойство, заканчивающееся на "Id")
        var idProp = selectedItem.GetType().GetProperties()
            .FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));

        if (idProp == null)
            throw new InvalidOperationException("ID property not found in selected item");

        var idValue = idProp.GetValue(selectedItem);

        // 4. Находим и удаляем сущность
        var entityType = dbSet.ElementType;
        var entity = await _db.FindAsync(entityType, idValue);

        if (entity == null)
            throw new InvalidOperationException("Entity not found in database");

        _db.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(string tableName, object updatedItem)
    {
        // 1. Находим определение таблицы
        var tableDef = TableDefinitions.First(t => t.DbName == tableName);

        // 2. Получаем DbSet
        var dbSetProperty = _db.GetType().GetProperty(tableName);
        if (dbSetProperty == null)
            throw new ArgumentException($"Table {tableName} not found");

        var dbSet = (IQueryable)dbSetProperty.GetValue(_db);

        // 3. Находим ID (ищем свойство *Id)
        var idProp = updatedItem.GetType().GetProperties()
            .FirstOrDefault(p => p.Name.EndsWith("Id"));

        if (idProp == null)
            throw new InvalidOperationException("ID property not found");

        var idValue = idProp.GetValue(updatedItem);

        // 4. Находим существующую сущность
        var entity = await _db.FindAsync(dbSet.ElementType, idValue);
        if (entity == null)
            throw new InvalidOperationException("Entity not found");

        // 5. Копируем значения из updatedItem в entity
        foreach (var prop in updatedItem.GetType().GetProperties())
        {
            if (prop.Name == "Id") continue; // Пропускаем ID

            var entityProp = entity.GetType().GetProperty(prop.Name);
            if (entityProp == null || !entityProp.CanWrite) continue;

            var value = prop.GetValue(updatedItem);
            entityProp.SetValue(entity, value);
        }

        // 6. Сохраняем
        await _db.SaveChangesAsync();
    }

    public async Task<object> GetItemByIdAsync(string tableName, object id)
    {
        var tableDef = TableDefinitions.First(t => t.DbName == tableName);

        var dbSetProperty = _db.GetType().GetProperty(tableName);
        if (dbSetProperty == null) throw new ArgumentException($"Таблица {tableName} не найдена");

        var dbSet = (IQueryable)dbSetProperty.GetValue(_db);

        var idColumn = tableDef.Columns.First(c => c.IsId || c.PropertyName.EndsWith("Id"));
        var idPropertyName = idColumn.PropertyName;

        var parameter = Expression.Parameter(dbSet.ElementType, "x");

        var idProperty = Expression.Property(parameter, idPropertyName);

        var equals = Expression.Equal(idProperty, Expression.Constant(id));
        var lambda = Expression.Lambda(equals, parameter);

        var whereMethod = typeof(Queryable).GetMethods()
            .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
            .MakeGenericMethod(dbSet.ElementType);

        var filteredQuery = (IQueryable)whereMethod.Invoke(
            null,
            new object[] { dbSet, lambda });

        var firstOrDefaultAsyncMethod = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .First(m => m.Name == "FirstOrDefaultAsync" && m.GetParameters().Length == 2)
            .MakeGenericMethod(dbSet.ElementType);

        var task = (Task)firstOrDefaultAsyncMethod.Invoke(
            null,
            [filteredQuery, default(CancellationToken)]);

        await task.ConfigureAwait(false);

        var resultProperty = task.GetType().GetProperty("Result");
        return resultProperty.GetValue(task);
    }
}