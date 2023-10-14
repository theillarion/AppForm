using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xk7.Model
{
    internal interface IDbQuestion : IDbBase
    {
        int Id { get; set; }
        int IdTest { get; set; }
        uint IdType { get; set; }
        int OrdinalNumber { get; set; }
        string Text { get; set; }
        bool HasImage { get; set; }
    }
    internal interface IDbQuestionWithImage : IDbQuestion
    {
        string Path { get; set; }
        string LocalPath { get; set; }
    }
}