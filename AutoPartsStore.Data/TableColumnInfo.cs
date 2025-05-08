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

        /// <summary> Является ли частью составного ключа </summary>
        public bool IsCompositeKey { get; set; } = false;

        /// <summary> Будет ли редактироваться запись </summary>
        public bool IsEditable { get; set; } = true;

        /// <summary> Будет ли редактироваться при создании </summary>
        public bool IsCreationEditable { get; set; } = true;

        /// <summary> Отображать ли в форме создания/редактирования </summary>
        public bool IsVisibleInEdit { get; set; } = true;

        public TableColumnInfo(
            string displayName,
            string propertyName,
            bool isVisible = true,
            bool isId = false,
            string referenceTable = null,
            string referenceIdColumn = null,
            string foreignKeyProperty = null,
            bool isCompositeKey = false,
            bool isEditable = true,
            bool isCreationEditable = true,
            bool isVisibleInEdit = true)
        {
            DisplayName = displayName;
            PropertyName = propertyName;
            IsVisible = isVisible;
            IsId = isId;
            ReferenceTable = referenceTable;
            ReferenceIdColumn = referenceIdColumn;
            ForeignKeyProperty = foreignKeyProperty ?? propertyName;
            IsCompositeKey = isCompositeKey;
            IsEditable = isEditable;
            IsCreationEditable = isCreationEditable;
            IsVisibleInEdit = isVisibleInEdit;
        }
    }
}
