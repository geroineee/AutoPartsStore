using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsStore.Data
{
    public class TableColumnInfo
    {
        public string DisplayName { get; set; }
        public string PropertyName { get; set; }
        public bool IsVisible { get; set; } = true;
    }
}
