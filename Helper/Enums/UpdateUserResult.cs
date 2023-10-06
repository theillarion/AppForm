using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xk7.Services;

namespace Xk7.Helper.Enums
{
    internal enum UpdateUserResult
    {
        Success,
        UserNotExists,
        LoginIsBusy,
        Unknown
    }
    internal static class EnumStrings
    {
        public static string notFoundString = "Not found error";
        public static string[] UpdateUserResult =
        {
            UICultureService.GetProperty("Success")?? notFoundString,
            UICultureService.GetProperty("ErrorUserNotExists")?? notFoundString,
            UICultureService.GetProperty("ErrorUserExists")?? notFoundString,
            UICultureService.GetProperty("UnknownError")?? notFoundString
        };
    };
}
