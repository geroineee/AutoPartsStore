namespace AutoPartsStore.Data
{
    public class TableColumnInfo
    {
        public string DisplayName { get; }
        public string PropertyName { get; }
        public bool IsVisible { get; } = true;
        public bool IsId { get; } = false;
        public string ReferenceTable { get; }
        public string ReferenceDisplayColumn { get; }
        public string ReferenceIdColumn { get; }

        // Добавлено для хранения имени свойства внешнего ключа
        public string ForeignKeyProperty { get; }

        public TableColumnInfo(
            string displayName,
            string propertyName,
            bool isVisible = true,
            bool isId = false,
            string referenceTable = null,
            string referenceDisplayColumn = null,
            string referenceIdColumn = null,
            string foreignKeyProperty = null) // новый параметр
        {
            DisplayName = displayName;
            PropertyName = propertyName;
            IsVisible = isVisible;
            IsId = isId;
            ReferenceTable = referenceTable;
            ReferenceDisplayColumn = referenceDisplayColumn;
            ReferenceIdColumn = referenceIdColumn;
            ForeignKeyProperty = foreignKeyProperty ?? propertyName; // По умолчанию PropertyName, если не указано
        }
    }
}
