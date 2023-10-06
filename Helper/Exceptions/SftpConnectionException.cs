using System;

namespace Xk7.Helper.Exceptions
{
    public class SftpConnectionException : Exception
    {
        public SftpConnectionException(string message) : base(message) { }
    }
}