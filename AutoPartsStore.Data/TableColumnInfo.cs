using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsStore.Data
{
    public class TableColumnInfo(
        string displayName,
        string propertyName,
        bool isVisible = true,
        bool isId = false,
        string referenceTable = null,
        string referenceDisplayColumn = null,
        string referenceIdColumn = null)
    {
        public string DisplayName { get; } = displayName;
        public string PropertyName { get; } = propertyName;
        public bool IsVisible { get; } = isVisible;
        public bool IsId { get; } = isId;
        public string ReferenceTable { get; } = referenceTable;
        public string ReferenceDisplayColumn { get; } = referenceDisplayColumn;
        public string ReferenceIdColumn { get; } = referenceIdColumn;
    }
}
