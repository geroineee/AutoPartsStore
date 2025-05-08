using AutoPartsStore.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPartsStore.Data
{
    public class TableDefinition
    {
        public string DisplayName { get; init; }
        public string DbName { get; init; }
        public Type TableType { get; set; }
        public Func<AutopartsStoreContext, IQueryable<object>> QueryBuilder { get; init; }
        public List<TableColumnInfo> Columns { get; init; }
    }
}
