using System;
using System.Text;
using Xk7.Helper.Enums;

namespace Xk7.Helper.Converts
{
    public static class Converts
    {
        public static string ConvertByteArrayToString(byte[] array) => Encoding.UTF8.GetString(array);

        public static string ConvertStringToHeshString(string value)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(value);
            var hashBytes = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes);
        }
        internal static AddImageResult ConvertEnum(LoadFileResult fileResult)
        {
            if ((uint)fileResult > (uint)LoadFileResult.Success)
                return (AddImageResult)(fileResult + 1);
            return AddImageResult.Success;
        }
        internal static AddImageResult ConvertEnum(AddTestResult fileResult)
        {
            if ((uint)fileResult < (uint)AddTestResult.Unknown)
                return (AddImageResult)fileResult;
            return AddImageResult.Unknown;
        }
    }
}
