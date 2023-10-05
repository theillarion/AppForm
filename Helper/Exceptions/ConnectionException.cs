using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Helper.Exceptions
{
    public class ConnectionException : Exception
    {
        public ConnectionException(string message) : base(message) { }
    }
}
