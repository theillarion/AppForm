using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Helper.Exceptions
{
    public class FactoryException : Exception
    {
        public FactoryException(string message) : base(message) { }
    }
}
