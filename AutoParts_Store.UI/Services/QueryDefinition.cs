using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoParts_Store.UI.Services
{
    public class QueryDefinition
    {
        public string DisplayName { get; set; } // Отображаемое имя запроса (например, "Поставщики по категории и товару")
        public string Description { get; set; } // Описание запроса
        public List<QueryVariation> Variations { get; set; } = new List<QueryVariation>();
    }

    public class QueryVariation
    {
        public string DisplayName { get; set; } // Отображаемое имя вариации (например, "С указанным объемом за период")
        public string Description { get; set; } // Описание вариации
        public Func<object[], Task<List<object>>> ExecutionFunction { get; set; } // Функция для выполнения запроса, принимает параметры object[]
        public List<QueryParameter> Parameters { get; set; } = new List<QueryParameter>(); //Список параметров, которые нужно будет ввести
    }

    public class QueryParameter
    {
        public string DisplayName { get; set; } // Отображаемое имя параметра (например, "ID товара")
        public string PropertyName { get; set; } // Имя свойства, в которое нужно сохранить значение
        public Type ParameterType { get; set; } // Тип параметра (int, DateTime, string и т.д.)
        public QueryParameterType InputType { get; set; } //Тип контрола для ввода
        public string? ReferenceTable { get; set; } //Если это ComboBox, то какая таблица является источником данных
    }

    public enum QueryParameterType
    {
        TextBox,
        ComboBox,
        DatePicker
    }
}