using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Model
{
    internal interface IDbAnswer : IDbBase
    {
        int Id { get; set; }
        int IdQuestion { get; set; }
        string Text { get; set; }
        bool IsCorrect { get; set; }
    }
}