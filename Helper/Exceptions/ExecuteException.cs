using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Helper.Exceptions
{
    internal class ExecuteException : Exception
    {
        public ExecuteException(string message) : base(message) { }
    }
}
