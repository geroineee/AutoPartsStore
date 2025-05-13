using System;
using System.Collections.Generic;
using AutoPartsStore.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsStore.Data.Context;

public partial class AutopartsStoreContext : DbContext
{
    public AutopartsStoreContext(DbContextOptions<AutopartsStoreContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AvailableProductOnStorageView> AvailableProductOnStorageViews { get; set; }

    public virtual DbSet<Batch> Batches { get; set; }

    public virtual DbSet<BatchInfoView> BatchInfoViews { get; set; }

    public virtual DbSet<BatchItem> BatchItems { get; set; }

    public virtual DbSet<CustomPayment> CustomPayments { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerOrder> CustomerOrders { get; set; }

    public virtual DbSet<CustomerOrderItem> CustomerOrderItems { get; set; }

    public virtual DbSet<CustomerOrdersProductView> CustomerOrdersProductViews { get; set; }

    public virtual DbSet<CustomerPurchaseStatView> CustomerPurchaseStatViews { get; set; }

    public virtual DbSet<CustomerRefund> CustomerRefunds { get; set; }

    public virtual DbSet<Defect> Defects { get; set; }

    public virtual DbSet<DefectiveItemView> DefectiveItemViews { get; set; }

    public virtual DbSet<DeliveryTerm> DeliveryTerms { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SaleItem> SaleItems { get; set; }

    public virtual DbSet<SaleWithSupplierInfoView> SaleWithSupplierInfoViews { get; set; }

    public virtual DbSet<StorageCell> StorageCells { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<SupplierCategory> SupplierCategories { get; set; }

    public virtual DbSet<SupplierContract> SupplierContracts { get; set; }

    public virtual DbSet<SupplierOrder> SupplierOrders { get; set; }

    public virtual DbSet<SupplierOrderItem> SupplierOrderItems { get; set; }

    public virtual DbSet<SupplierProductStatView> SupplierProductStatViews { get; set; }

    public virtual DbSet<SupplierReturn> SupplierReturns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_unicode_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AvailableProductOnStorageView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("available_product_on_storage_view");

            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.BatchItemId).HasColumnName("batch_item_id");
            entity.Property(e => e.BiCellId).HasColumnName("bi_cell_id");
            entity.Property(e => e.CellName)
                .HasMaxLength(10)
                .HasColumnName("cell_name");
            entity.Property(e => e.CurrentQuantity).HasColumnName("current_quantity");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.BatchId).HasName("PRIMARY");

            entity.ToTable("batch", tb => tb.HasComment("Партия"));

            entity.HasIndex(e => e.BatchSupplierOrderId, "fk_batch_supplier_order");

            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.BatchDescription)
                .HasColumnType("text")
                .HasColumnName("batch_description");
            entity.Property(e => e.BatchSupplierOrderId).HasColumnName("batch_supplier_order_id");
            entity.Property(e => e.DeliveryDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("delivery_date");

            entity.HasOne(d => d.BatchSupplierOrder).WithMany(p => p.Batches)
                .HasForeignKey(d => d.BatchSupplierOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_batch_supplier_order");
        });

        modelBuilder.Entity<BatchInfoView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("batch_info_view");

            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.DeliveryDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("delivery_date");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.ProductQuantity).HasColumnName("product_quantity");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(100)
                .HasColumnName("supplier_name");
        });

        modelBuilder.Entity<BatchItem>(entity =>
        {
            entity.HasKey(e => e.BatchItemId).HasName("PRIMARY");

            entity.ToTable("batch_item", tb => tb.HasComment("Состав партии"));

            entity.HasIndex(e => e.BiBatchId, "bi_batch_id");

            entity.HasIndex(e => e.BiProductId, "bi_product_id");

            entity.HasIndex(e => e.BiCellId, "fk_batch_item_cell");

            entity.HasIndex(e => e.RemainingItem, "remaining_item");

            entity.Property(e => e.BatchItemId).HasColumnName("batch_item_id");
            entity.Property(e => e.BatchItemQuantity).HasColumnName("batch_item_quantity");
            entity.Property(e => e.BiBatchId).HasColumnName("bi_batch_id");
            entity.Property(e => e.BiCellId).HasColumnName("bi_cell_id");
            entity.Property(e => e.BiProductId).HasColumnName("bi_product_id");
            entity.Property(e => e.RemainingItem).HasColumnName("remaining_item");

            entity.HasOne(d => d.BiBatch).WithMany(p => p.BatchItems)
                .HasForeignKey(d => d.BiBatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("batch_item_ibfk_1");

            entity.HasOne(d => d.BiCell).WithMany(p => p.BatchItems)
                .HasForeignKey(d => d.BiCellId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_batch_item_cell");

            entity.HasOne(d => d.BiProduct).WithMany(p => p.BatchItems)
                .HasForeignKey(d => d.BiProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("batch_item_ibfk_2");
        });

        modelBuilder.Entity<CustomPayment>(entity =>
        {
            entity.HasKey(e => e.CustomPaymentId).HasName("PRIMARY");

            entity.ToTable("custom_payment", tb => tb.HasComment("Таможенный платеж"));

            entity.HasIndex(e => e.CpBatchId, "cp_batch_id");

            entity.Property(e => e.CustomPaymentId).HasColumnName("custom_payment_id");
            entity.Property(e => e.CpBatchId).HasColumnName("cp_batch_id");
            entity.Property(e => e.PaymentAmount)
                .HasPrecision(10, 2)
                .HasColumnName("payment_amount");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("payment_date");

            entity.HasOne(d => d.CpBatch).WithMany(p => p.CustomPayments)
                .HasForeignKey(d => d.CpBatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("custom_payment_ibfk_1");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PRIMARY");

            entity.ToTable("customer", tb => tb.HasComment("Клиент"));

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(100)
                .HasColumnName("customer_email");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasColumnName("customer_name");
            entity.Property(e => e.CustomerPatronymic)
                .HasMaxLength(50)
                .HasColumnName("customer_patronymic");
            entity.Property(e => e.CustomerPhone)
                .HasMaxLength(20)
                .HasColumnName("customer_phone");
            entity.Property(e => e.CustomerSurname)
                .HasMaxLength(50)
                .HasColumnName("customer_surname");
        });

        modelBuilder.Entity<CustomerOrder>(entity =>
        {
            entity.HasKey(e => e.CustomerOrderId).HasName("PRIMARY");

            entity.ToTable("customer_order");

            entity.HasIndex(e => e.CoCustomerId, "co_customer_id");

            entity.Property(e => e.CustomerOrderId).HasColumnName("customer_order_id");
            entity.Property(e => e.CoCustomerId).HasColumnName("co_customer_id");
            entity.Property(e => e.CustomerOrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("customer_order_date");
            entity.Property(e => e.CustomerOrderDescription)
                .HasColumnType("text")
                .HasColumnName("customer_order_description");
            entity.Property(e => e.CustomerOrderStatus).HasColumnName("customer_order_status");

            entity.HasOne(d => d.CoCustomer).WithMany(p => p.CustomerOrders)
                .HasForeignKey(d => d.CoCustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_order_ibfk_1");
        });

        modelBuilder.Entity<CustomerOrderItem>(entity =>
        {
            entity.HasKey(e => new { e.CoiProductId, e.CoiCustomerOrderId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("customer_order_item");

            entity.HasIndex(e => e.CoiCustomerOrderId, "coi_customer_order_id");

            entity.Property(e => e.CoiProductId).HasColumnName("coi_product_id");
            entity.Property(e => e.CoiCustomerOrderId).HasColumnName("coi_customer_order_id");
            entity.Property(e => e.CoiQuantity).HasColumnName("coi_quantity");

            entity.HasOne(d => d.CoiCustomerOrder).WithMany(p => p.CustomerOrderItems)
                .HasForeignKey(d => d.CoiCustomerOrderId)
                .HasConstraintName("customer_order_item_ibfk_1");

            entity.HasOne(d => d.CoiProduct).WithMany(p => p.CustomerOrderItems)
                .HasForeignKey(d => d.CoiProductId)
                .HasConstraintName("customer_order_item_ibfk_2");
        });

        modelBuilder.Entity<CustomerOrdersProductView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("customer_orders_product_view");

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.TotalOrdered)
                .HasPrecision(32)
                .HasColumnName("total_ordered");
        });

        modelBuilder.Entity<CustomerPurchaseStatView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("customer_purchase_stat_view");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasColumnName("customer_name");
            entity.Property(e => e.CustomerPatronymic)
                .HasMaxLength(50)
                .HasColumnName("customer_patronymic");
            entity.Property(e => e.CustomerSurname)
                .HasMaxLength(50)
                .HasColumnName("customer_surname");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.TotalPurchased)
                .HasPrecision(32)
                .HasColumnName("total_purchased");
            entity.Property(e => e.TotalSpent)
                .HasPrecision(42, 2)
                .HasColumnName("total_spent");
        });

        modelBuilder.Entity<CustomerRefund>(entity =>
        {
            entity.HasKey(e => e.CustomerRefundId).HasName("PRIMARY");

            entity.ToTable("customer_refund", tb => tb.HasComment("Возврат клиенту"));

            entity.HasIndex(e => e.CrSaleItemId, "cr_sale_item_id");

            entity.Property(e => e.CustomerRefundId).HasColumnName("customer_refund_id");
            entity.Property(e => e.CrQuantity).HasColumnName("cr_quantity");
            entity.Property(e => e.CrSaleItemId).HasColumnName("cr_sale_item_id");
            entity.Property(e => e.IsDefect).HasColumnName("is_defect");
            entity.Property(e => e.RefundAmount)
                .HasPrecision(12, 2)
                .HasColumnName("refund_amount");
            entity.Property(e => e.RefundDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("refund_date");

            entity.HasOne(d => d.CrSaleItem).WithMany(p => p.CustomerRefunds)
                .HasForeignKey(d => d.CrSaleItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_refund_ibfk_1");
        });

        modelBuilder.Entity<Defect>(entity =>
        {
            entity.HasKey(e => e.DefectId).HasName("PRIMARY");

            entity.ToTable("defect", tb => tb.HasComment("Брак"));

            entity.HasIndex(e => e.DefectBatchItemId, "defect_batch_item_id");

            entity.Property(e => e.DefectId).HasColumnName("defect_id");
            entity.Property(e => e.DefectBatchItemId).HasColumnName("defect_batch_item_id");
            entity.Property(e => e.DefectDescription)
                .HasColumnType("text")
                .HasColumnName("defect_description");
            entity.Property(e => e.DefectQuantity).HasColumnName("defect_quantity");
            entity.Property(e => e.DetectionDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("detection_date");

            entity.HasOne(d => d.DefectBatchItem).WithMany(p => p.Defects)
                .HasForeignKey(d => d.DefectBatchItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("defect_ibfk_1");
        });

        modelBuilder.Entity<DefectiveItemView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("defective_item_view");
        });

        modelBuilder.Entity<DeliveryTerm>(entity =>
        {
            entity.HasKey(e => new { e.DtProductId, e.DtSupplierId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("delivery_term", tb => tb.HasComment("Условия поставки"));

            entity.HasIndex(e => e.DtSupplierId, "dt_supplier_id");

            entity.Property(e => e.DtProductId).HasColumnName("dt_product_id");
            entity.Property(e => e.DtSupplierId).HasColumnName("dt_supplier_id");
            entity.Property(e => e.DeliveryDays).HasColumnName("delivery_days");
            entity.Property(e => e.DeliveryPrice)
                .HasPrecision(10, 2)
                .HasColumnName("delivery_price");

            entity.HasOne(d => d.DtProduct).WithMany(p => p.DeliveryTerms)
                .HasForeignKey(d => d.DtProductId)
                .HasConstraintName("delivery_term_ibfk_1");

            entity.HasOne(d => d.DtSupplier).WithMany(p => p.DeliveryTerms)
                .HasForeignKey(d => d.DtSupplierId)
                .HasConstraintName("delivery_term_ibfk_2");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PRIMARY");

            entity.ToTable("employee", tb => tb.HasComment("Сотрудник"));

            entity.HasIndex(e => e.EmployeePositionId, "fk_employee_position");

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.EmployeeEmail)
                .HasMaxLength(100)
                .HasColumnName("employee_email");
            entity.Property(e => e.EmployeeName)
                .HasMaxLength(50)
                .HasColumnName("employee_name");
            entity.Property(e => e.EmployeePatronymic)
                .HasMaxLength(50)
                .HasColumnName("employee_patronymic");
            entity.Property(e => e.EmployeePhone)
                .HasMaxLength(20)
                .HasColumnName("employee_phone");
            entity.Property(e => e.EmployeePositionId).HasColumnName("employee_position_id");
            entity.Property(e => e.EmployeeSurname)
                .HasMaxLength(50)
                .HasColumnName("employee_surname");
            entity.Property(e => e.FireDate).HasColumnName("fire_date");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");

            entity.HasOne(d => d.EmployeePosition).WithMany(p => p.Employees)
                .HasForeignKey(d => d.EmployeePositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_employee_position");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PRIMARY");

            entity.ToTable("position");

            entity.Property(e => e.PositionId).HasColumnName("position_id");
            entity.Property(e => e.PositionDescription)
                .HasColumnType("text")
                .HasColumnName("position_description");
            entity.Property(e => e.PositionName)
                .HasMaxLength(50)
                .HasColumnName("position_name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity.ToTable("product", tb => tb.HasComment("Товар"));

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductDescription)
                .HasColumnType("text")
                .HasColumnName("product_description");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.ProductSalePrice)
                .HasPrecision(10, 2)
                .HasColumnName("product_sale_price");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("PRIMARY");

            entity.ToTable("sale", tb => tb.HasComment("Продажа"));

            entity.HasIndex(e => e.SaleCustomerOrderId, "fk_sale_customer_order");

            entity.HasIndex(e => e.SaleCustomerId, "sale_customer_id");

            entity.HasIndex(e => e.SaleEmployeeId, "sale_employee_id");

            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.SaleCustomerId).HasColumnName("sale_customer_id");
            entity.Property(e => e.SaleCustomerOrderId).HasColumnName("sale_customer_order_id");
            entity.Property(e => e.SaleDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("sale_date");
            entity.Property(e => e.SaleEmployeeId).HasColumnName("sale_employee_id");

            entity.HasOne(d => d.SaleCustomer).WithMany(p => p.Sales)
                .HasForeignKey(d => d.SaleCustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_ibfk_1");

            entity.HasOne(d => d.SaleCustomerOrder).WithMany(p => p.Sales)
                .HasForeignKey(d => d.SaleCustomerOrderId)
                .HasConstraintName("fk_sale_customer_order");

            entity.HasOne(d => d.SaleEmployee).WithMany(p => p.Sales)
                .HasForeignKey(d => d.SaleEmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_ibfk_2");
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(e => e.SaleItemId).HasName("PRIMARY");

            entity.ToTable("sale_item", tb => tb.HasComment("Состав продажи"));

            entity.HasIndex(e => e.SiProductId, "fk_sale_item_product");

            entity.HasIndex(e => e.SiSaleId, "sale_item_ibfk_2");

            entity.HasIndex(e => e.SiBatchItemId, "si_batch_item_id");

            entity.Property(e => e.SaleItemId).HasColumnName("sale_item_id");
            entity.Property(e => e.SiBatchItemId).HasColumnName("si_batch_item_id");
            entity.Property(e => e.SiProductId).HasColumnName("si_product_id");
            entity.Property(e => e.SiQuantity).HasColumnName("si_quantity");
            entity.Property(e => e.SiSaleId).HasColumnName("si_sale_id");

            entity.HasOne(d => d.SiBatchItem).WithMany(p => p.SaleItems)
                .HasForeignKey(d => d.SiBatchItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_item_ibfk_1");

            entity.HasOne(d => d.SiProduct).WithMany(p => p.SaleItems)
                .HasForeignKey(d => d.SiProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sale_item_product");

            entity.HasOne(d => d.SiSale).WithMany(p => p.SaleItems)
                .HasForeignKey(d => d.SiSaleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sale_item_ibfk_2");
        });

        modelBuilder.Entity<SaleWithSupplierInfoView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("sale_with_supplier_info_view");

            entity.Property(e => e.ProductName).HasMaxLength(100);
            entity.Property(e => e.Profit).HasPrecision(21, 2);
            entity.Property(e => e.PurchasePrice).HasPrecision(10, 2);
            entity.Property(e => e.SaleDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp");
            entity.Property(e => e.SalePrice).HasPrecision(10, 2);
            entity.Property(e => e.SupplierName).HasMaxLength(100);
        });

        modelBuilder.Entity<StorageCell>(entity =>
        {
            entity.HasKey(e => e.CellId).HasName("PRIMARY");

            entity.ToTable("storage_cell", tb => tb.HasComment("Ячейка склада"));

            entity.HasIndex(e => e.CellName, "cell_name").IsUnique();

            entity.Property(e => e.CellId).HasColumnName("cell_id");
            entity.Property(e => e.CellName)
                .HasMaxLength(10)
                .HasColumnName("cell_name");
            entity.Property(e => e.Depth)
                .HasPrecision(5, 2)
                .HasColumnName("depth");
            entity.Property(e => e.Height)
                .HasPrecision(5, 2)
                .HasColumnName("height");
            entity.Property(e => e.LocationDescription)
                .HasMaxLength(100)
                .HasColumnName("location_description");
            entity.Property(e => e.MaxWeight)
                .HasComment("Вес в кг")
                .HasColumnName("max_weight");
            entity.Property(e => e.Width)
                .HasPrecision(5, 2)
                .HasColumnName("width");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PRIMARY");

            entity.ToTable("supplier", tb => tb.HasComment("Поставщик"));

            entity.HasIndex(e => e.SupplierCategoryId, "supplier_category_id");

            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.SupplierAddress)
                .HasMaxLength(150)
                .HasColumnName("supplier_address");
            entity.Property(e => e.SupplierCategoryId).HasColumnName("supplier_category_id");
            entity.Property(e => e.SupplierCountry)
                .HasMaxLength(50)
                .HasColumnName("supplier_country");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(100)
                .HasColumnName("supplier_name");

            entity.HasOne(d => d.SupplierCategory).WithMany(p => p.Suppliers)
                .HasForeignKey(d => d.SupplierCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("supplier_ibfk_1");
        });

        modelBuilder.Entity<SupplierCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity.ToTable("supplier_category", tb => tb.HasComment("Категория поставщика"));

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryDescription)
                .HasColumnType("text")
                .HasColumnName("category_description");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("category_name");
            entity.Property(e => e.ProvidesDiscount).HasColumnName("provides_discount");
            entity.Property(e => e.ProvidesGuarantee).HasColumnName("provides_guarantee");
        });

        modelBuilder.Entity<SupplierContract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PRIMARY");

            entity.ToTable("supplier_contract", tb => tb.HasComment("Контракт поставщика"));

            entity.HasIndex(e => e.ContractSupplierId, "contract_supplier_id");

            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            entity.Property(e => e.ContractNumber)
                .HasMaxLength(50)
                .HasColumnName("contract_number");
            entity.Property(e => e.ContractSupplierId).HasColumnName("contract_supplier_id");
            entity.Property(e => e.Discount)
                .HasPrecision(5, 2)
                .HasColumnName("discount");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Terms)
                .HasColumnType("text")
                .HasColumnName("terms");

            entity.HasOne(d => d.ContractSupplier).WithMany(p => p.SupplierContracts)
                .HasForeignKey(d => d.ContractSupplierId)
                .HasConstraintName("supplier_contract_ibfk_1");
        });

        modelBuilder.Entity<SupplierOrder>(entity =>
        {
            entity.HasKey(e => e.SupplierOrderId).HasName("PRIMARY");

            entity.ToTable("supplier_order", tb => tb.HasComment("Заказ поставщику"));

            entity.HasIndex(e => e.RecipientId, "recipient_id");

            entity.HasIndex(e => e.ManagerId, "supplier_order_ibfk_1");

            entity.Property(e => e.SupplierOrderId).HasColumnName("supplier_order_id");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.SupplierOrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("supplier_order_date");
            entity.Property(e => e.SupplierOrderDescription)
                .HasColumnType("text")
                .HasColumnName("supplier_order_description");
            entity.Property(e => e.SupplierOrderStatus).HasColumnName("supplier_order_status");

            entity.HasOne(d => d.Manager).WithMany(p => p.SupplierOrders)
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("supplier_order_ibfk_1");

            entity.HasOne(d => d.Recipient).WithMany(p => p.SupplierOrders)
                .HasForeignKey(d => d.RecipientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("supplier_order_ibfk_2");
        });

        modelBuilder.Entity<SupplierOrderItem>(entity =>
        {
            entity.HasKey(e => new { e.SoiSupplierOrderId, e.SoiProductId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("supplier_order_item", tb => tb.HasComment("Состав заказа"));

            entity.HasIndex(e => e.SoiProductId, "soi_product_id");

            entity.Property(e => e.SoiSupplierOrderId).HasColumnName("soi_supplier_order_id");
            entity.Property(e => e.SoiProductId).HasColumnName("soi_product_id");
            entity.Property(e => e.SoiQuantity).HasColumnName("soi_quantity");

            entity.HasOne(d => d.SoiProduct).WithMany(p => p.SupplierOrderItems)
                .HasForeignKey(d => d.SoiProductId)
                .HasConstraintName("supplier_order_item_ibfk_1");

            entity.HasOne(d => d.SoiSupplierOrder).WithMany(p => p.SupplierOrderItems)
                .HasForeignKey(d => d.SoiSupplierOrderId)
                .HasConstraintName("supplier_order_item_ibfk_2");
        });

        modelBuilder.Entity<SupplierProductStatView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("supplier_product_stat_view");

            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("category_name");
            entity.Property(e => e.DeliveryDays).HasColumnName("delivery_days");
            entity.Property(e => e.DeliveryPrice)
                .HasPrecision(10, 2)
                .HasColumnName("delivery_price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.SupplierCategoryId).HasColumnName("supplier_category_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(100)
                .HasColumnName("supplier_name");
            entity.Property(e => e.TotalDeliveries).HasColumnName("total_deliveries");
            entity.Property(e => e.TotalQuantity)
                .HasPrecision(32)
                .HasColumnName("total_quantity");
        });

        modelBuilder.Entity<SupplierReturn>(entity =>
        {
            entity.HasKey(e => e.SupplierReturnId).HasName("PRIMARY");

            entity.ToTable("supplier_return");

            entity.HasIndex(e => e.SrDefectId, "sr_defect_id");

            entity.Property(e => e.SupplierReturnId).HasColumnName("supplier_return_id");
            entity.Property(e => e.CompensationAmount)
                .HasPrecision(10, 2)
                .HasColumnName("compensation_amount");
            entity.Property(e => e.ReplacementCompensation)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("replacement_compensation");
            entity.Property(e => e.ReturnDate).HasColumnName("return_date");
            entity.Property(e => e.SrDefectId).HasColumnName("sr_defect_id");

            entity.HasOne(d => d.SrDefect).WithMany(p => p.SupplierReturns)
                .HasForeignKey(d => d.SrDefectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("supplier_return_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
