using System;

namespace Xk7.Model
{
    public interface IDbTest : IDbBase
    {
        public static string DbFormatDateTime = "yyyy-MM-dd hh:mm:ss";
        int Id { get; set; }
        string Tittle { get; set; }
        string? Description { get; set; }
        DateTime UtcDateTimeCreated { get; set; }
        string WhoCreated { get; set; }
        bool HasImage { get; set; }
    }
    public interface IDbTestWithImage : IDbTest
    {
        string Path { get; set; }
        string LocalPath { get; set; }
    }
}