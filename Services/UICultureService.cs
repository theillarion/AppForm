using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Services
{
    static class UICultureService
    {
        public static List<CultureInfo> GetAllCultures() => CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(c => c.Name.Length > 0 && !c.Equals(CultureInfo.InvariantCulture))
                .Where(c => c.Equals(CultureInfo.CurrentUICulture) || c.Parent.Equals(CultureInfo.CurrentUICulture))
                .ToList();
        public static string? GetProperty(string propertyName)
        {
            var rm = new ResourceManager("Xk7.Resources.Resource", typeof(App).Assembly);
            return rm.GetString(propertyName);
        }
        public static string? GetProperty(string propertyName, CultureInfo cultureInfo)
        {
            var rm = new ResourceManager("Xk7.Resources.Resource", typeof(App).Assembly);
            return rm.GetString(propertyName, cultureInfo);
        }
        public static void SetCulture(CultureInfo cultureInfo)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
        public static CultureInfo GetDefaultCulture() => System.Threading.Thread.CurrentThread.CurrentUICulture;
    }
}
