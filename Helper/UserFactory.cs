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
    public static class UserFactory
    {
        private static bool IsSameOrSubClass(Type potentialBase, Type potentialDescendant)
        {
            return potentialDescendant.IsSubclassOf(potentialBase)
                   || potentialDescendant == potentialBase;
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
                    p.SetValue(item, row[c], null);
                }
            }
        }
        public static IDbUser? FromDataRow<T>(DataRow? row) where T : new()
        {
            if (!typeof(IDbUser).IsAssignableFrom(typeof(T)))
                throw new FactoryException($"Type {typeof(T)} is not a descendant of type {typeof(IDbUser)}");
            var user = new T();
            SetItemFromRow(user, row);

            return user as IDbUser;
        }
    }
}
