namespace AutoPartsStore.Data
{
    public class TableColumnInfo
    {
        /// <summary> Отображаемое имя </summary>
        public string DisplayName { get; }
        /// <summary> Соответствующее отображаемое свойство анонимного типа </summary>
        public string PropertyName { get; }
        /// <summary> Отображение </summary>
        public bool IsVisible { get; } = true;
        /// <summary> Является ли ID </summary>
        public bool IsId { get; } = false;
        /// <summary> Название ссылаемой таблицы </summary>
        public string ReferenceTable { get; }
        /// <summary> Название Id колонки в ссылаемой таблице </summary>
        public string ReferenceIdColumn { get; }
        /// <summary> FK в данной таблице </summary>
        public string ForeignKeyProperty { get; }

        public TableColumnInfo(
            string displayName,
            string propertyName,
            bool isVisible = true,
            bool isId = false,
            string referenceTable = null,
            string referenceIdColumn = null,
            string foreignKeyProperty = null)
        {
            DisplayName = displayName;
            PropertyName = propertyName;
            IsVisible = isVisible;
            IsId = isId;
            ReferenceTable = referenceTable;
            ReferenceIdColumn = referenceIdColumn;
            ForeignKeyProperty = foreignKeyProperty ?? propertyName; // По умолчанию PropertyName, если не указано
        }
    }
}
