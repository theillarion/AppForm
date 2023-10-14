using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xk7.Model;
using Xk7.Helper;
using Xk7.Helper.Exceptions;

namespace Xk7.Helper
{
    public static class Factory
    {
        private static bool IsSameOrSubClass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
        }
        private static object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                    return null;
                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }
        private static void SetItemFromRow<T>(T? item, DataRow? row) where T : new()
        {
            if (item == null || row == null)
                throw new FactoryException("Argument is null");
            foreach (DataColumn c in row.Table.Columns)
            {
                var p = item.GetType().GetProperty(c.ColumnName);

                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, ChangeType(row[c], p.PropertyType), null);
                }
            }
        }
        public static T? FromDataRow<T>(DataRow? row) where T : new()
        {
            var obj = new T();
            SetItemFromRow(obj, row);

            return obj;
        }
    }
}